using hrconnectbackend.Data;
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
        IDepartmentServices departmentServices)
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
            return await _context.Employees
                .Include(a => a.EmployeeDepartment)
                .Where(e => e.Id == employeeId)
                .Select(a => a.EmployeeDepartment)
                .SelectMany(a => a.Employees)
                .ToListAsync();
        }

        public async Task<Employee> GetEmployeeByEmail(string email)
        {
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email.ToString().Trim() == email.ToString().Trim());

            if (employee == null)
            {
                throw new KeyNotFoundException($"No employee found with an email {email}");
            }

            return employee;
        }

        public async Task<List<Employee>> GenerateEmployeesWithEmail(List<GenerateEmployeeDTO> employeesDTO)
        {
            var employeesCreated = new List<Employee>();
            try
            {
                foreach (var employee in employeesDTO)
                {
                    var existingEmployee = await _context.Employees.FirstOrDefaultAsync(e => e.Email.ToString().Trim() == employee.Email.Trim());

                    if (existingEmployee != null)
                    {
                        logger.LogWarning("Employee with email {Email} already exists.", employee.Email);
                        continue;
                    }

                    var userAccount = new UserAccount
                    {
                        Email = employee.Email,
                        ChangePassword = true
                    };

                    var employeeEntity = new Employee
                    {
                        Email = employee.Email,
                        TenantId = 1,
                        PositionId = 1,
                        Status = "offline",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = null
                    };

                    await AddAsync(employeeEntity);

                    string password = Generator.GeneratePassword(12, true);
                    string username = Generator.GenerateUsername();

                    AboutEmployee aboutEmployee = employeeEntity.CreateAboutEmployee(firstName:null, lastName:null);
                    
                    EducationBackground educationBackground = aboutEmployee.CreateEducationBackground();

                    await userAccountService.AddAsync(new UserAccount
                    {
                        UserId = employeeEntity.Id,
                        EmailVerified = false,
                        SmsVerified = false,
                        UserName = username,
                        Password = password,
                        Email = employee.Email
                    });

                    await aboutEmployeeService.AddAsync(aboutEmployee);

                    await attendanceService.AddAsync(new Attendance
                    {
                        EmployeeId = employeeEntity.Id,
                        ClockIn = TimeOnly.FromDateTime(DateTime.Now).ToTimeSpan(),
                        ClockOut = TimeOnly.FromDateTime(DateTime.Now).ToTimeSpan(),
                        DateToday = DateTime.Now
                    });

                    await aboutEmployeeService.AddEducationBackgroundAsync(educationBackground);
                    

                    employeesCreated.Add(employeeEntity);

                }

                return employeesCreated;

            }
            catch (Exception ex)
            {
                throw new Exception(employeesDTO.Count + " employees could not be created. " + ex.Message);
            }
        }

        public async Task CreateEmployee(CreateEmployeeDTO employee)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Check if the employee is null and throw an exception
                if (employee == null)
                {
                    logger.LogWarning("Attempted to create an employee with null data.");
                    throw new ArgumentNullException(nameof(employee), "Employee data cannot be null.");
                }

                // Check if an employee with the same email already exists
                var existingEmployee = (await GetAllAsync())
                                       .FirstOrDefault(e => e.Email == employee.Email);

                // if (!EmailServices.IsValidEmail(employee.Email))
                // {
                //     _logger.LogWarning("Invalid email format provided: {Email}", employee.Email);
                //     throw new ArgumentException("Invalid email format", nameof(employee.Email));
                // }

                // If an employee with the same email exists, throw an exception
                if (existingEmployee != null)
                {
                    logger.LogWarning("Employee with email {Email} already exists.", employee.Email);
                    throw new InvalidOperationException("An employee with the same email already exists.");
                }

                string password = "";

                if (employee.Password != null)
                {
                    if (!Validator.IsValidPassword(employee.Password))
                    {
                        throw new ArgumentException("Invalid password format", nameof(employee.Password));
                    }

                    password = BCrypt.Net.BCrypt.HashPassword(employee.Password);
                }
                else
                {
                    password = Generator.GeneratePassword(12, true);
                }

                if (!Validator.IsValidEmail(employee.Email))
                {
                    throw new ArgumentException("Invalid email format", nameof(employee.Email));
                }
                // Create the employee entity
                var employeeEntity = new Employee
                {
                    Email = employee.Email,
                    Status = "offline",
                };

                var aboutEmployee = employeeEntity.CreateAboutEmployee(employee.FirstName, employee.FirstName);

                var education = aboutEmployee.CreateEducationBackground();
                

                employeeEntity.CreatedAt = DateTime.Now;
                employeeEntity.UpdatedAt = null;
                // Add the employee entity to the database
                await AddAsync(employeeEntity);

                string userName = $"{aboutEmployee.FirstName.CapitalLowerCaseName()}" + $"{aboutEmployee.LastName.CapitalLowerCaseName()}";

                // Create associated records
                await userAccountService.AddAsync(new UserAccount
                {
                    UserId = employeeEntity.Id,
                    EmailVerified = false,
                    SmsVerified = false,
                    UserName = userName,
                    Password = password,
                    Email = employee.Email
                });

                await aboutEmployeeService.AddAsync(aboutEmployee);

                await attendanceService.AddAsync(new Attendance
                {
                    EmployeeId = employeeEntity.Id,
                    ClockIn = TimeOnly.FromDateTime(DateTime.Now).ToTimeSpan(),
                    ClockOut = TimeOnly.FromDateTime(DateTime.Now).ToTimeSpan(),
                    DateToday = DateTime.Now
                });

                await aboutEmployeeService.AddEducationBackgroundAsync(education);
                
                // Commit the transaction
                await transaction.CommitAsync();

                // Log success and return a success message
                logger.LogInformation("Employee created successfully: {EmployeeId}", employeeEntity.Id);
            }
            catch (ArgumentNullException ex)
            {
                // Rollback the transaction on error
                await transaction.RollbackAsync();

                // Log the exception
                ThrowErrorType<ArgumentNullException>($"Error occurred while creating employee: {ex.Message}", log => logger.LogError(ex.Message));
            }
            catch (ArgumentException ex)
            {
                // Rollback the transaction on error
                await transaction.RollbackAsync();

                // Log the exception
                ThrowErrorType<ArgumentException>($"Error occurred while creating employee: {ex.Message}", log => logger.LogError(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                // Rollback the transaction on error
                await transaction.RollbackAsync();

                // Log the exception
                ThrowErrorType<InvalidOperationException>($"Error occurred while creating employee: {ex.Message}", log => logger.LogError(ex.Message));
            }
            catch (Exception ex)
            {
                // Rollback the transaction on error
                await transaction.RollbackAsync();

                // Log the exception
                ThrowErrorType<Exception>($"Error occurred while creating employee: {ex.Message}", log => logger.LogError(ex.Message));
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
            
            throw (T)errorType;
        }

        public async Task<List<Employee>> RetrieveEmployees(int? pageIndex, int? pageSize)
        {
            var employees = await _context.Employees.Include(e => e.AboutEmployee).Include(e => e.AboutEmployee.EducationBackground).Include(d => d.EmployeeDepartment).ToListAsync();

            var employeesPagination = GetEmployeesPagination(employees, pageIndex, pageSize);

            return employeesPagination;
        }

        public async Task<Employee> GetEmployeeById(int id)
        {
            var employee = await _context.Employees.Include(e => e.AboutEmployee).Include(e => e.AboutEmployee.EducationBackground).Include(d => d.EmployeeDepartment).FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null)
            {
                throw new KeyNotFoundException($"No employee found with an id {id}");
            }

            return employee;
        }
    }

}
