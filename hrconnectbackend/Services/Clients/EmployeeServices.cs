using hrconnectbackend.Constants;
using hrconnectbackend.Data;
using hrconnectbackend.Exceptions;
using hrconnectbackend.Helper;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Interface.Services.Clients;
using hrconnectbackend.Models;
using hrconnectbackend.Models.DTOs;
using hrconnectbackend.Models.DTOs.AuthDTOs;
using hrconnectbackend.Models.EmployeeModels;
using hrconnectbackend.Repository;
using Microsoft.EntityFrameworkCore;

namespace hrconnectbackend.Services.Clients
{
    public class EmployeeServices(
        DataContext context,
        IAboutEmployeeServices aboutEmployeeService,
        IAttendanceServices attendanceService,
        IUserAccountServices userAccountService,
        ILogger<EmployeeServices> logger,
        IShiftServices shiftService
        )
        : GenericRepository<Employee>(context), IEmployeeServices
    {

        public async Task<List<Employee>> GetEmployeeByDepartment(int deptId, int? pageIndex, int? pageSize)
        {

            logger.LogInformation("Getting employees by department id: {DepartmentId}", deptId);

            var department = await _context.Departments.FindAsync(deptId);


            if (department == null)
            {
                throw new KeyNotFoundException($"No department found with an id {deptId}");
            }

            var employees = await _context.Employees.Where(e => e.EmployeeDepartmentId == deptId).ToListAsync();

            var employeesPagination = GetEmployeesPagination(employees, pageIndex, pageSize);

            return employeesPagination;
        }

        public async Task<List<Employee>> GetSubordinates(int employeeId)
        {
            var employee = await _context.Employees.Where(a => a.Id == employeeId).FirstOrDefaultAsync();

            if (employee == null) return new List<Employee>();
            // Get the department ID where the employee is supervisor
            var subordinates = await _context.Employees.Include(a => a.AboutEmployee).Where(a => a.EmployeeDepartmentId == employee.EmployeeDepartmentId && a.Id != employee.Id).ToListAsync();

            return subordinates;
        }

        public async Task<Employee?> GetEmployeeByEmail(string email)
        {
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == email);

            if (employee == null)
            {
                throw new KeyNotFoundException($"No employee found with an email {email}");
            }

            return employee;
        }


        public async Task<List<Employee>> GenerateEmployeesWithEmail(List<GenerateEmployeeDto> employeesDto, int orgId)
        {
            var employeesCreated = new List<Employee>();

            foreach (var employee in employeesDto)
            {
                using var transaction = await _context.Database.BeginTransactionAsync(); // Begin transaction per employee
                try
                {
                    var existingEmployee = await _context.Employees
                        .FirstOrDefaultAsync(e => e.Email.ToString().Trim() == employee.Email.Trim());

                    if (existingEmployee != null)
                    {
                        logger.LogWarning("Employee with email {Email} already exists.", employee.Email);
                        continue;
                    }

                    string password = Generator.GeneratePassword();
                    string username = Generator.GenerateUsername();

                    var userAccount = new UserAccount
                    {
                        UserName = username,
                        Password = password,
                        Email = employee.Email,
                        ChangePassword = true
                    };

                    await userAccountService.AddAsync(userAccount);

                    var employeeEntity = new Employee
                    {
                        Email = employee.Email,
                        OrganizationId = orgId,
                        PositionId = null,
                        Status = "offline",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = null
                    };

                    await AddAsync(employeeEntity);

                    AboutEmployee aboutEmployee = employeeEntity.CreateAboutEmployee(firstName: null, lastName: null);
                    EducationBackground educationBackground = aboutEmployee.CreateEducationBackground();

                    await aboutEmployeeService.AddAsync(aboutEmployee);

                    await attendanceService.AddAsync(new Attendance
                    {
                        EmployeeId = employeeEntity.Id,
                        ClockIn = TimeOnly.FromDateTime(DateTime.Now).ToTimeSpan(),
                        ClockOut = TimeOnly.FromDateTime(DateTime.Now).ToTimeSpan(),
                        DateToday = DateTime.Now
                    });

                    await aboutEmployeeService.AddEducationBackgroundAsync(educationBackground);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync(); // Commit after all operations

                    employeesCreated.Add(employeeEntity);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(); // Rollback this iteration
                    logger.LogError(ex, "Error creating employee with email {Email}. Rolling back.", employee.Email);
                    // Optionally continue to next employee or rethrow
                }
            }

            return employeesCreated;
        }


        public async Task CreateEmployee(CreateEmployeeDto employee, int orgId, bool? createAccount = false)
{
    await using var transaction = await _context.Database.BeginTransactionAsync();

    try
    {
        // Check if an employee with the same email already exists
        var existingEmployee = await _context.Employees
                                             .FirstOrDefaultAsync(e => e.Email == employee.Email);

        if (existingEmployee != null)
        {
            logger.LogWarning("Employee with email {Email} already exists.", employee.Email);
            throw new ConflictException(ErrorCodes.DuplicateEmail, "An employee with the same email already exists.");
        }

        // Handle password and user account creation
        string password = !string.IsNullOrEmpty(employee.Password) && employee.Password.IsValidPassword()
            ? BCrypt.Net.BCrypt.HashPassword(employee.Password)
            : Generator.GeneratePassword();

        if (!employee.Email.IsValidEmail())
        {
            throw new ValidationException("Invalid email format", nameof(employee.Email));
        }

        string userName = $"{employee.FirstName.CapitalLowerCaseName()}{employee.LastName.CapitalLowerCaseName()}";

        UserAccount? userAccount = null;

        if (createAccount.HasValue && createAccount.Value)
        {
            userAccount = await userAccountService.AddAsync(new UserAccount
            {
                EmailVerified = false,
                SmsVerified = false,
                UserName = userName,
                Password = password,
                Role = "Employee",
                Email = employee.Email
            });
        }

        // Create the employee entity and ensure DateTime is UTC
        var empEntity = new Employee
        {
            Email = employee.Email,
            Status = "offline",
            CreatedAt = DateTime.UtcNow,  // Ensure it's UTC
            UserId = (createAccount.HasValue && createAccount.Value) && userAccount != null ? userAccount?.UserId : null,
            OrganizationId = orgId,
            UpdatedAt = null
        };

        // Add employee to the database
        _context.Employees.Add(empEntity);
        await _context.SaveChangesAsync();  // Save to get the generated Id

        var employeeShifts = await shiftService.GenerateShiftForEmployee(empEntity.Id, orgId);

        // Create associated records for AboutEmployee
        var aboutEmployee = empEntity.CreateAboutEmployee(employee.FirstName, employee.LastName);
        var newAboutEmployee = await aboutEmployeeService.AddAsync(aboutEmployee);

        // Create Education Background for AboutEmployee
        var education = newAboutEmployee.CreateEducationBackground();
        await aboutEmployeeService.AddEducationBackgroundAsync(education);

        //// Create attendance record for the new employee
        //var attendance = new Attendance
        //{
        //    EmployeeId = empEntity.Id,
        //    ClockIn = TimeOnly.FromDateTime(DateTime.UtcNow).ToTimeSpan(),
        //    ClockOut = TimeOnly.FromDateTime(DateTime.UtcNow).ToTimeSpan(),
        //    DateToday = DateTime.UtcNow  // Ensure it's UTC
        //};
        //await attendanceService.AddAsync(attendance);

        // Commit the transaction
        await transaction.CommitAsync();

        // Log success and return a success message
        logger.LogInformation("Employee created successfully: {EmployeeId}", empEntity.Id);
    }
    catch (Exception ex)
    {
        // Rollback the transaction on error
        await transaction.RollbackAsync();
        throw; // Rethrow the exception after logging it
    }
}



        public List<Employee> GetEmployeesPagination(List<Employee> employees, int? pageIndex, int? pageSize)
        {
            // Ensure that pageIndex and pageSize have valid values
            if (pageIndex.HasValue && pageIndex.Value <= 0)
                throw new ArgumentOutOfRangeException("Page number must be greater than zero.");

            if (pageSize.HasValue && pageSize.Value <= 0)
                throw new ArgumentOutOfRangeException("Quantity must be greater than zero.");

            // If neither pageIndex nor pageSize is provided (both are null), return all employees
            if (!pageIndex.HasValue || !pageSize.HasValue)
            {
                return employees;
            }

            // Fetch the employees with pagination
            var paginationEmployees = employees.Skip((pageIndex.Value - 1) * pageSize.Value)
                                               .Take(pageSize.Value)
                                               .ToList();

            return paginationEmployees;
        }


        private static void ThrowErrorType<T>(string message, Action<string>? log) where T : Exception
        {
            log?.Invoke(message);
            var errorType = Activator.CreateInstance(typeof(T), message);
            
            throw ((T)errorType!);
        }

        public async Task<List<Employee>> RetrieveEmployees(int? pageIndex, int? pageSize)
        {
            var employees = await _context.Employees.Include(e => e.AboutEmployee).Include(e => e.AboutEmployee!.EducationBackground).Include(d => d.EmployeeDepartment).ToListAsync();

            var employeesPagination = GetEmployeesPagination(employees, pageIndex, pageSize);

            return employeesPagination;
        }

        public async Task<Employee?> GetEmployeeById(int id)
        {
            var employee = await _context.Employees.Include(e => e.AboutEmployee).Include(e => e.AboutEmployee!.EducationBackground).Include(d => d.EmployeeDepartment).FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null)
            {
                throw new KeyNotFoundException($"No employee found with an id {id}");
            }

            return employee;
        }
    }

}
