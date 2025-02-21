using hrconnectbackend.Data;
using hrconnectbackend.Helper;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Models;
using hrconnectbackend.Models.DTOs;
using hrconnectbackend.Repository;
using Microsoft.EntityFrameworkCore;
using ZstdSharp.Unsafe;


namespace hrconnectbackend.Repositories
{
    public class EmployeeServices : GenericRepository<Employee>, IEmployeeServices
    {
        private readonly IAboutEmployeeServices _aboutEmployeeService;
        private readonly IAttendanceServices _attendanceService;
        private readonly IUserAccountServices _userAccountService;
        private readonly IDepartmentServices _departmentServices;
        private readonly ILogger<EmployeeServices> _logger;

        public EmployeeServices(DataContext context, IAboutEmployeeServices aboutEmployeeService, IAttendanceServices attendanceService, IUserAccountServices userAccountService, ILogger<EmployeeServices> logger, IDepartmentServices departmentServices)
            : base(context)
        {
            _aboutEmployeeService = aboutEmployeeService;
            _attendanceService = attendanceService;
            _userAccountService = userAccountService;
            _departmentServices = departmentServices;
            _logger = logger;
        }


        public async Task<List<Employee>> GetEmployeeByDepartment(int deptId, int? pageIndex, int? pageSize)
        {
            var department = await _context.Departments.FindAsync(deptId);


            if (department == null)
            {
                throw new KeyNotFoundException($"No department found with an id {deptId}");
            }

            var employees = await _context.Employees.Where(e => e.DepartmentId == deptId).ToListAsync();

            var employeesPagination = GetEmployeesPagination(employees, pageIndex, pageSize);

            return employeesPagination;
        }

        public async Task<List<Employee>> GetSubordinates(int empSupervisorId)
        {
            return await _context.Supervisors.Where(s => s.Id == empSupervisorId).SelectMany(s => s.Subordinates ?? new List<Employee>()).ToListAsync();
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

        public async Task CreateEmployee(CreateEmployeeDTO employee)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Check if the employee is null and throw an exception
                if (employee == null)
                {
                    _logger.LogWarning("Attempted to create an employee with null data.");
                    throw new ArgumentNullException(nameof(employee), "Employee data cannot be null.");
                }

                // Check if an employee with the same email already exists
                var existingEmployee = (await GetAllAsync())
                                       .FirstOrDefault(e => e.Email == employee.Email);

                if (!EmailServices.IsValidEmail(employee.Email))
                {
                    _logger.LogWarning("Invalid email format provided: {Email}", employee.Email);
                    throw new ArgumentException("Invalid email format", nameof(employee.Email));
                }

                // If an employee with the same email exists, throw an exception
                if (existingEmployee != null)
                {
                    _logger.LogWarning("Employee with email {Email} already exists.", employee.Email);
                    throw new InvalidOperationException("An employee with the same email already exists.");
                }

                // Check if the status is valid
                if (employee.Status != "Online" && employee.Status != "Offline")
                {
                    _logger.LogWarning("Invalid status provided: {Status}. Expected 'Online' or 'Offline'.", employee.Status);
                    throw new ArgumentException("Invalid status provided: {Status}. Expected 'Online' or 'Offline'.");
                }

                string password = "";

                if (employee.Password != null)
                {
                    password = BCrypt.Net.BCrypt.HashPassword(employee.Password);
                }
                else
                {
                    password = PasswordGenerator.GeneratePassword(12, true);
                }

                // Create the employee entity
                var employeeEntity = new Employee
                {
                    FirstName = employee.FirstName,
                    LastName = employee.LastName,
                    Email = employee.Email,
                    IsAdmin = employee.IsAdmin,
                    Status = employee.Status,
                };

                bool departmentHasValue = employee.DepartmentId != null && employee.DepartmentId.HasValue;

                if (departmentHasValue)
                {
                    var department = await _departmentServices.GetByIdAsync(employee.DepartmentId.Value);

                    if (department != null)
                    {
                        employeeEntity.DepartmentId = employee.DepartmentId ?? null;
                    }
                }

                employeeEntity.CreatedAt = DateOnly.FromDateTime(DateTime.Now);
                employeeEntity.UpdatedAt = DateOnly.FromDateTime(DateTime.Now);

                // Add the employee entity to the database
                await AddAsync(employeeEntity);

                string userName = $"{employeeEntity.FirstName[0].ToString().ToUpper()}"+$"{employeeEntity.LastName.ToLower()}";

                // Create associated records
                await _userAccountService.AddAsync(new UserAccount
                {
                    UserId = employeeEntity.Id,
                    VerificationCode = null,
                    EmailVerified = false,
                    SMSVerified = false,
                    UserName = userName,
                    Password = password,
                    Email = employee.Email
                });

                await _aboutEmployeeService.AddAsync(new AboutEmployee
                {
                    EmployeeInfoId = employeeEntity.Id,
                    ProfilePicture = "https://via.placeholder.com/250",
                    Address = "No address",
                    BirthDate = DateOnly.FromDateTime(DateTime.Now),
                    Age = 0
                });

                await _attendanceService.AddAsync(new Attendance
                {
                    EmployeeId = employeeEntity.Id,
                    ClockIn = TimeOnly.FromDateTime(DateTime.Now).ToTimeSpan(),
                    ClockOut = TimeOnly.FromDateTime(DateTime.Now).ToTimeSpan(),
                    DateToday = DateTime.Now
                });

                await _aboutEmployeeService.AddEducationBackgroundAsync(new EducationBackground
                {
                    UserId = employeeEntity.Id,
                    InstitutionName = "No institution name",
                    Degree = "No degree",
                    FieldOfStudy = "No field of study",
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now,
                    GPA = 0.0
                });



                // Commit the transaction
                await transaction.CommitAsync();

                // Log success and return a success message
                _logger.LogInformation("Employee created successfully: {EmployeeId}", employeeEntity.Id);
            }
            catch (Exception ex)
            {
                // Rollback the transaction on error
                await transaction.RollbackAsync();

                // Log the exception
                _logger.LogError(ex, "Error occurred while creating employee.");

                // Throw the error after logging
                throw new InvalidOperationException("An error occurred while processing your request.", ex);
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


    }



}
