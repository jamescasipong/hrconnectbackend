using System.Security.Claims;
using AutoMapper;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Interface.Services.Clients;
using hrconnectbackend.Models.DTOs;
using hrconnectbackend.Models.DTOs.AuthDTOs;
using hrconnectbackend.Models.EmployeeModels;
using hrconnectbackend.Models.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hrconnectbackend.Controllers.v1.Clients
{
    
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
        public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeDto employee, bool? createAccount)
        {
            if (employee == null)
            {
                logger.LogWarning("Received null data for employee creation.");
                return BadRequest("Employee data cannot be null.");
            }
            
            try
            {
                // Call the CreateEmployeeAsync method
                await employeeService.CreateEmployee(employee);

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

        [Authorize(Roles ="Admin, HR")]
        [HttpGet]
        public async Task<IActionResult> GetEmployees([FromQuery] int? pageIndex, [FromQuery] int? pageSize)
        {
            try
            {
                var employees = new List<Employee>();

                // Check if pagination parameters are provided
                if (pageIndex == null || pageSize == null)
                {
                    employees = await employeeService.GetAllAsync();
                }
                else
                {
                    if (pageIndex <= 0)
                    {
                        return BadRequest(new ApiResponse(false, "Page index must be greater than 0"));
                    }

                    if (pageSize <= 0)
                    {
                        return BadRequest(new ApiResponse(false, "Page size must be greater than 0"));
                    }

                    // Apply pagination only if pageIndex and pageSize are not null
                    employees = await employeeService.RetrieveEmployees(pageIndex, pageSize);

                }

                if (!employees.Any())
                {
                    return NotFound(new ApiResponse(false, $"No employees exist"));
                }

                var employeesDTO = mapper.Map<List<ReadEmployeeDto>>(employees);

                return Ok(new ApiResponse<List<ReadEmployeeDto>?>(true, $"Employees retrieved successfully!", employeesDTO));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving employees");
                return StatusCode(500, new ApiResponse(false, "Internal server error"));
            }
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetEmployee()
        {
            // Check if the cookies are expired and delete them
            if (Request.Cookies.ContainsKey("token"))
            {
                var cookie = Request.Cookies["token"];
                if (cookie != null && DateTime.TryParse(cookie, out var cookieExpiryDate))
                {
                    if (cookieExpiryDate < DateTime.UtcNow)
                    {
                        Response.Cookies.Append("token", "", new CookieOptions
                        {
                            HttpOnly = true,  // Secure from JavaScript (prevent XSS)
                            SameSite = SameSiteMode.None, // Prevent CSRF attacks
                            Secure = true,
                            Path="/",
                            Expires = DateTime.UtcNow.AddMinutes(-1), // Cookie expires in 1 hour
                        });
                    }
                }
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null) return Unauthorized(new ApiResponse(false, $"User not authenticated"));

            var employee = await employeeService.GetEmployeeById(int.Parse(userId));

            if (employee == null)
            {
                return NotFound(new ApiResponse(false, $"Employee with an ID: {userId} not found."));
            }

            var userAccount = await userAccountServices.GetByIdAsync(int.Parse(userId));

            if (userAccount == null)
            {
                return NotFound(new ApiResponse(false, $"Employee account with an ID: {userId} not found."));
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
        public async Task<IActionResult> UpdateEmployee(int id, UpdateEmployeeDto employeeDTO)
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
                employee.Email = employee.Email;
                employee.Status = employee.Status;

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

        [HttpGet("department/{departmentId}")]
        public async Task<IActionResult> RetrieveEmployeeByDepartment(int deptId, int? pageIndex, int? pageSize)
        {
            try
            {
                var employeesByDept = await employeeService.GetEmployeeByDepartment(deptId, pageIndex, pageSize);

                var employeesMapped = mapper.Map<List<ReadEmployeeDto>>(employeesByDept);
                
                return Ok(new ApiResponse<List<ReadEmployeeDto>?>(false, $"Employees under a department {deptId} retrieved successfully.", employeesMapped));
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



