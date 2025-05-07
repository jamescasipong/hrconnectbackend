using System.Security.Claims;
using AutoMapper;
using hrconnectbackend.Attributes.Authorization.Requirements;
using hrconnectbackend.Exceptions;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Interface.Services.Clients;
using hrconnectbackend.Models;
using hrconnectbackend.Models.DTOs;
using hrconnectbackend.Models.DTOs.AuthDTOs;
using hrconnectbackend.Models.EmployeeModels;
using hrconnectbackend.Models.RequestModel;
using hrconnectbackend.Models.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hrconnectbackend.Controllers.v1.Clients
{
    // [Authorize]
    [ApiController]
    [Route("api/v{version:apiVersion}/employee")]
    [ApiVersion("1.0")]
    public class EmployeeController(
        IEmployeeServices employeeService,
        IMapper mapper,
        IAboutEmployeeServices aboutEmployeeServices,
        ILogger<EmployeeController> logger,
        IUserAccountServices userAccountServices
        )
        : ControllerBase
    {

        // [Authorize(Roles = "Admin,HR")]
        [HttpPost]
        public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeDto employee, bool? createAccount = false)
        {
            var organizationId = User.FindFirstValue("organizationId");

            logger.LogWarning("Organization ID: {organizationId}", organizationId);

            if (!int.TryParse(organizationId, out int orgId))
            {
                logger.LogWarning("Organization ID is not valid.");
                return BadRequest(new ApiResponse(false, "Invalid organization ID."));
            }

            if (employee == null)
            {
                logger.LogWarning("Received null data for employee creation.");
                return BadRequest("Employee data cannot be null.");
            }
            
            try
            {
                // Call the CreateEmployeeAsync method
                await employeeService.CreateEmployee(employee, orgId, createAccount);

                return Ok(new ApiResponse(true, $"Employee created successfully!"));  // Return a success message
            }
            catch (ArgumentNullException ex)
            {
                logger.LogError(ex, "Error creating employee due to null data.");
                return BadRequest(new ApiResponse(false, ex.Message));  // Return BadRequest for specific exceptions
            }
            catch (ArgumentException ex)
            {
                logger.LogError(ex, "Error creating employee: Invalid argument.");
                return BadRequest(new ApiResponse(false, ex.Message)); // Return BadRequest for invalid arguments
            }
            catch (InvalidOperationException ex)
            {
                logger.LogError(ex, "Error creating employee: Invalid operation.");
                return Conflict(new ApiResponse(false, ex.Message));  // Return Conflict for specific scenarios like existing employee
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred while creating employee.");
                return StatusCode(500, "Internal server error");  // Handle unexpected errors
            }
        }

        [Authorize(Roles = "Admin,HR")]
        [HttpPost("generate-employees")]
        public async Task<IActionResult> GenerateEmployees([FromBody] List<GenerateEmployeeDto> employees)
        {
            try
            {
                var emp = await employeeService.GenerateEmployeesWithEmail(employees);

                var empDto = mapper.Map<List<ReadEmployeeDto>>(emp);

                return Ok(new ApiResponse<List<ReadEmployeeDto>?>(true, "Employees generated successfully", empDto));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error generating employees");
                return StatusCode(500, new ApiResponse(false, "Internal server error"));
            }
        }

        [HttpPost("{employeeId}/create-account")]
        public async Task<IActionResult> CreateUserAccount (int employeeId, CreateUser userAccExistingEmployee)
        {
            try
            {
                var newAccount = new UserAccount
                {
                    UserName = userAccExistingEmployee.UserName,
                    Password = userAccExistingEmployee.Password,
                    Email = userAccExistingEmployee.Email,
                    Role = "Employee",
                };

                var user = await userAccountServices.CreateEmployeeUserAccount(newAccount, employeeId);
                if (user == null)
                {
                    return BadRequest(new ApiResponse(false, "Failed to create user account"));
                }
                return Ok(new ApiResponse(true, "User account created successfully"));
            }
            catch (Exception ex)
            {
                if (ex is CustomException)
                {
                    return BadRequest(new ApiResponse(false, ex.Message));
                }

                if (ex is ConflictException)
                {
                    return Conflict(new ApiResponse(false, ex.Message));
                }

                logger.LogError(ex, "Error creating user account");
                return StatusCode(500, new ApiResponse(false, "Internal server error"));
            }
        }

        // [Authorize(Roles ="Admin, HR")]
        // [UserRole("Owner")]
        [HttpGet]
        public async Task<IActionResult> GetEmployees([FromQuery] int? pageIndex, [FromQuery] int? pageSize)
        {
            var user = User;

            if (user == null)
            {
                logger.LogWarning("User is unauthenticated.");
                return Unauthorized();
            }

            if (!user.IsInRole("HR") && !user.IsInRole("Admin") && !user.IsInRole("Operator"))
            {
                logger.LogWarning("User is not in role 'HR' or in role 'Admin', 'Operator'.");
                return Forbid();
            }
            
            try
            {
                var employees = new List<Employee>();
                logger.LogInformation($"Getting employees for {pageIndex} and {pageSize}.");
                // Check if pagination parameters are provided
                if (pageIndex == null || pageSize == null)
                {
                    logger.LogWarning("Page index and Page size are required.");
                    employees = await employeeService.GetAllAsync();
                }
                else
                {
                    if (pageIndex <= 0)
                    {
                        logger.LogWarning("Page index is less than zero.");
                        return BadRequest(new ApiResponse(false, "Page index must be greater than 0"));
                    }

                    if (pageSize <= 0)
                    {
                        logger.LogWarning("Page size is less than zero.");
                        return BadRequest(new ApiResponse(false, "Page size must be greater than 0"));
                    }
                        
                    // Apply pagination only if pageIndex and pageSize are not null
                    logger.LogInformation($"Getting employees for {pageIndex} and {pageSize}.");
                    employees = await employeeService.RetrieveEmployees(pageIndex, pageSize);

                }

                if (!employees.Any())
                {
                    logger.LogWarning("No employees found.");
                    return NotFound(new ApiResponse(false, $"No employees exist"));
                }
                
                var employeesDto = mapper.Map<List<ReadEmployeeDto>>(employees);
                
                return Ok(new ApiResponse<List<ReadEmployeeDto>?>(true, $"Employees retrieved successfully!", employeesDto));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving employees");
                return StatusCode(500, new ApiResponse(false, "Internal server error"));
            }
        }
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetEmployee()
        {
            var userId = User.FindFirstValue("EmployeeId");

            if (userId == null) return Unauthorized(new ApiResponse(false, $"User not authenticated"));

            var employee = await employeeService.GetEmployeeById(int.Parse(userId));

            if (employee == null)
            {
                return NotFound(new ApiResponse(false, $"Employee with an ID: {userId} not found."));
            }

            var userAccount = await userAccountServices.GetByIdAsync(int.Parse(userId));

            if (userAccount == null)
            {
                return Ok(new ApiResponse(false, $"Employee account with an ID: {userId} not found."));
            }

            // var aboutEmployeeDTO = _mapper.Map<ReadAboutEmployeeDTO>(employee.AboutEmployee);

            var employeeDTO = mapper.Map<ReadEmployeeDto>(employee);

            // employeeDTO.AboutEmployee = aboutEmployeeDTO;


            return Ok(new ApiResponse<ReadEmployeeDto?>(true, $"Employee with an ID: {userId} retrieved successfully!", employeeDTO));
        }
        [Authorize]
        [HttpGet("education/me")]
        public async Task<IActionResult> GetEducation(){
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null) return Unauthorized(new ApiResponse(false, $"User not authenticated"));

            var employee = await aboutEmployeeServices.GetEmployeeEducationBackgroundAsync(int.Parse(userId));


            var employeeDTO = mapper.Map<List<EducationBackgroundDto>>(employee);
        

            return Ok(new ApiResponse<List<EducationBackgroundDto>?>(true, $"Employee with an ID: {userId} retrieved successfully!", employeeDTO));
        }

        [Authorize]
        [HttpGet("about/me")]
        public async Task<IActionResult> GetAboutMe()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null) return Unauthorized(new ApiResponse(false, $"User not authenticated"));

            var employee = await aboutEmployeeServices.GetByIdAsync(int.Parse(userId));

            if (employee == null)
            {
                return NotFound(new ApiResponse(false, $"Employee with an ID: {userId} not found."));
            }

            var employeeDTO = mapper.Map<CreateAboutEmployeeDto>(employee);

            return Ok(new ApiResponse<CreateAboutEmployeeDto?>(true, $"Employee with an ID: {userId} retrieved successfully!", employeeDTO));
        }

        [Authorize]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetEmployee(int id)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

                bool isAdmin = userRoles.Contains("Admin") || userRoles.Contains("HR");

                if (currentUserId == null) return Unauthorized(new ApiResponse(false, $"User not authenticated"));

                if (!isAdmin && currentUserId != id.ToString()){
                    return Forbid();
                }

                var employee = await employeeService.GetByIdAsync(id);

                if (employee == null)
                {
                    return NotFound(new ApiResponse(false, $"Employee with an ID: {id} not found."));
                }

                var employeeDTO = mapper.Map<ReadEmployeeDto>(employee);

                return Ok(new ApiResponse<ReadEmployeeDto?>(true, $"Employee with an ID: {id} retrieved successfully!", employeeDTO));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving an employee with ID: {id}", id);
                return StatusCode(500, new ApiResponse(false, "Internal server error"));
            }
        }

        [Authorize(Roles ="Admin,HR")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateEmployee(int id, UpdateEmployeeDto employeeDto)
        {
            var employee = await employeeService.GetByIdAsync(id);

            try
            {
                if (employee == null)
                {
                    return NotFound(new ApiResponse(false, $"Employee with an ID: {id} not found."));
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // employee.FirstName = employee.FirstName;
                // employee.LastName = employee.LastName;
                employee.Email = employeeDto.Email;
                employee.Status = employeeDto.Status;

                await employeeService.UpdateAsync(employee);

                return Ok(new ApiResponse(true, $"Employee with an ID: {id} updated successfully"));
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Error updating an employee with ID {id}", id);
                return StatusCode(500, new ApiResponse(false, "Internal server error"));
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var employee = await employeeService.GetByIdAsync(id);

            try
            {
                if (employee == null)
                {
                    return NotFound(new ApiResponse(false, $"Employee with ID: {id} not found."));
                }

                await employeeService.DeleteAsync(employee);

                return Ok(new ApiResponse(true, $"Employee with ID: {id} deleted successfully!"));
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Error deleting an employee with ID {id}", id);
                return StatusCode(500, new ApiResponse(false, "Internal server error"));
            }
        }

        [Authorize]
        [HttpGet("department/{departmentId}")]
        public async Task<IActionResult> RetrieveEmployeeByDepartment(int departmentId, int? pageIndex, int? pageSize)
        {
            try
            {
                var employeesByDept = await employeeService.GetEmployeeByDepartment(departmentId, pageIndex, pageSize);

                var employeesMapped = mapper.Map<List<ReadEmployeeDto>>(employeesByDept);
                
                return Ok(new ApiResponse<List<ReadEmployeeDto>?>(false, $"Employees under a department {departmentId} retrieved successfully.", employeesMapped));
            }
            catch (KeyNotFoundException ex)
            {
                return Ok(new ApiResponse(false, ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse(false, $"Internal Server Error"));
            }
        }

        [Authorize]
        [HttpGet("my-subordinates")]
        public async Task<IActionResult> RetrieveSubordinates()
        {

            var employeeId = User.FindFirstValue("EmployeeId");

            if (employeeId == null) return Unauthorized();

            if (!int.TryParse(employeeId, out var parsedEmployeeId))
            {
                return Unauthorized();
            }

            try
            {
                var subordinates = await employeeService.GetSubordinates(parsedEmployeeId);

                var subordinateRead = mapper.Map<List<ReadEmployeeDto>>(subordinates);

                return Ok(subordinateRead);
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        [Authorize]
        [HttpPut("update-username/{accountId:int}")]
        public async Task<IActionResult> ChangeUserName(int accountId, string name)
        {
            var user = await userAccountServices.GetByIdAsync(accountId);

            try
            {
                if (user == null) return NotFound(new ApiResponse(false, $"Employee account with account ID: {accountId} not found."));

                user.UserName = name;

                await userAccountServices.UpdateAsync(user);
                return Ok(new ApiResponse(true, $"Employee's account username with account ID: {accountId} changed to {name} successfully!"));

            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Error updating an employee's account username with account ID {id}", accountId);
                return StatusCode(500, new ApiResponse(false, "Internal server error"));
            }
        }
        
    }

}



