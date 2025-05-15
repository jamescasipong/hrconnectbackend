using System.Security.Claims;
using AutoMapper;
using hrconnectbackend.Attributes.Authorization.Requirements;
using hrconnectbackend.Constants;
using hrconnectbackend.Exceptions;
using hrconnectbackend.Extensions;
using hrconnectbackend.Helper;
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
            var organizationId = User.RetrieveSpecificUser("organizationId");

            int organizationIdInt = TypeConverter.StringToInt(organizationId);

            if (employee == null)
            {
                logger.LogWarning("Received null data for employee creation.");
                return BadRequest("Employee data cannot be null.");
            }

            // Call the CreateEmployeeAsync method
            await employeeService.CreateEmployee(employee, organizationIdInt, createAccount);

            return Ok(new ApiResponse(true, $"Employee created successfully!"));  // Return a success message

        }

        [Authorize(Roles = "Admin,HR")]
        [HttpPost("generate-employees")]
        public async Task<IActionResult> GenerateEmployees([FromBody] List<GenerateEmployeeDto> employees)
        {
            var organizationId = User.RetrieveSpecificUser("organizationId");
            int organizationIdInt = TypeConverter.StringToInt(organizationId);

            var emp = await employeeService.GenerateEmployeesWithEmail(employees, organizationIdInt);

            var empDto = mapper.Map<List<ReadEmployeeDto>>(emp);

            return Ok(new ApiResponse<List<ReadEmployeeDto>?>(true, "Employees generated successfully", empDto));

        }

        [HttpPost("{employeeId}/create-account")]
        public async Task<IActionResult> CreateUserAccount(int employeeId, CreateUser userAccExistingEmployee)
        {

            var newAccount = new UserAccount
            {
                UserName = userAccExistingEmployee.UserName,
                Password = userAccExistingEmployee.Password,
                Email = userAccExistingEmployee.Email,
                Role = "Employee",
            };

            var user = await userAccountServices.CreateEmployeeUserAccount(newAccount, employeeId);

            return Ok(new ApiResponse(true, "User account created successfully"));

        }

        [Authorize(Roles = "Admin, HR")]
        // [UserRole("Owner")]
        [HttpGet]
        public async Task<IActionResult> GetEmployees([FromQuery] int? pageIndex, [FromQuery] int? pageSize)
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

                // Apply pagination only if pageIndex and pageSize are not null
                logger.LogInformation($"Getting employees for {pageIndex} and {pageSize}.");
                employees = await employeeService.RetrieveEmployees(pageIndex, pageSize);

            }

            if (!employees.Any())
            {
                logger.LogWarning("No employees found.");
                return StatusCode(404, new ErrorResponse(ErrorCodes.EmployeeNotFound, "No employees found."));
            }

            var employeesDto = mapper.Map<List<ReadEmployeeDto>>(employees);

            return Ok(new SuccessResponse<List<ReadEmployeeDto>>(employeesDto, $"Employees retrieved successfully!"));

        }
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetEmployee()
        {
            var userId = User.RetrieveSpecificUser("EmployeeId");

            var employee = await employeeService.GetEmployeeById(int.Parse(userId));

            if (employee == null)
            {
                return StatusCode(404, new ErrorResponse(ErrorCodes.EmployeeNotFound, $"Employee with ID: {userId} not found."));
            }

            var userAccount = await userAccountServices.GetByIdAsync(int.Parse(userId));

            if (userAccount == null)
            {
                return StatusCode(404, new ErrorResponse(ErrorCodes.UserNotFound, $"User account with an ID: {userId} not found."));
            }

            // var aboutEmployeeDTO = _mapper.Map<ReadAboutEmployeeDTO>(employee.AboutEmployee);

            var employeeDTO = mapper.Map<ReadEmployeeDto>(employee);


            return Ok(new SuccessResponse<ReadEmployeeDto?>(employeeDTO, $"Employee with an ID: {userId} retrieved successfully!"));
        }
        [Authorize]
        [HttpGet("education/me")]
        public async Task<IActionResult> GetEducation()
        {
            var userId = User.RetrieveSpecificUser(ClaimTypes.NameIdentifier);

            if (userId == null) return StatusCode(401, new ErrorResponse(ErrorCodes.Unauthorized, "User not authenticated"));

            var employee = await aboutEmployeeServices.GetEmployeeEducationBackgroundAsync(int.Parse(userId));

            var employeeDTO = mapper.Map<List<EducationBackgroundDto>>(employee);

            return Ok(new SuccessResponse<List<EducationBackgroundDto>>(employeeDTO, $"Employee with an ID: {userId} retrieved successfully!"));
        }

        [Authorize]
        [HttpGet("about/me")]
        public async Task<IActionResult> GetAboutMe()
        {
            var userId = User.RetrieveSpecificUser(ClaimTypes.NameIdentifier);

            var employee = await aboutEmployeeServices.GetByIdAsync(int.Parse(userId));

            if (employee == null)
            {
                return StatusCode(404, new ErrorResponse(ErrorCodes.EmployeeNotFound, $"Employee with ID: {userId} not found."));
            }

            var employeeDTO = mapper.Map<CreateAboutEmployeeDto>(employee);

            return Ok(new SuccessResponse<CreateAboutEmployeeDto?>(employeeDTO, $"Employee with an ID: {userId} retrieved successfully!"));
        }

        [Authorize(Roles = "Admin,HR")]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetEmployee(int id)
        {

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            if (currentUserId == null) return StatusCode(401, new ErrorResponse(ErrorCodes.Unauthorized, "User not authenticated"));

            var employee = await employeeService.GetByIdAsync(id);

            if (employee == null)
            {
                return StatusCode(404, new ErrorResponse(ErrorCodes.EmployeeNotFound, $"Employee with ID: {id} not found."));
            }

            var employeeDTO = mapper.Map<ReadEmployeeDto>(employee);

            return Ok(new SuccessResponse<ReadEmployeeDto?>(employeeDTO, $"Employee with an ID: {id} retrieved successfully!"));
        }

        [Authorize(Roles = "Admin,HR")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateEmployee(int id, UpdateEmployeeDto employeeDto)
        {
            var employee = await employeeService.GetByIdAsync(id);

            if (employee == null)
            {
                return StatusCode(404, new ErrorResponse(ErrorCodes.EmployeeNotFound, $"Employee with ID: {id} not found."));
            }

            if (!ModelState.IsValid)
            {
                return StatusCode(400, new ErrorResponse(ErrorCodes.InvalidEmployeeData, "Invalid employee data."));
            }

            // employee.FirstName = employee.FirstName;
            // employee.LastName = employee.LastName;
            employee.Email = employeeDto.Email;
            employee.Status = employeeDto.Status;

            await employeeService.UpdateAsync(employee);

            return Ok(new ApiResponse(true, $"Employee with an ID: {id} updated successfully"));

        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var employee = await employeeService.GetByIdAsync(id);


            if (employee == null)
            {
                return StatusCode(404, new ErrorResponse(ErrorCodes.EmployeeNotFound, $"Employee with ID: {id} not found."));
            }

            await employeeService.DeleteAsync(employee);

            return Ok(new SuccessResponse($"Employee with ID: {id} deleted successfully!"));
        }

        [Authorize]
        [HttpGet("department/{departmentId}")]
        public async Task<IActionResult> RetrieveEmployeeByDepartment(int departmentId, int? pageIndex, int? pageSize)
        {

            var employeesByDept = await employeeService.GetEmployeeByDepartment(departmentId, pageIndex, pageSize);

            var employeesMapped = mapper.Map<List<ReadEmployeeDto>>(employeesByDept);

            return Ok(new ApiResponse<List<ReadEmployeeDto>?>(false, $"Employees under a department {departmentId} retrieved successfully.", employeesMapped));

        }

        [Authorize]
        [HttpGet("my-subordinates")]
        public async Task<IActionResult> RetrieveSubordinates()
        {

            var employeeId = User.RetrieveSpecificUser("EmployeeId");

            if (!int.TryParse(employeeId, out var parsedEmployeeId))
            {
                return StatusCode(400, new ErrorResponse(ErrorCodes.EmployeeNotFound, "Invalid employee ID."));
            }


            var subordinates = await employeeService.GetSubordinates(parsedEmployeeId);

            var subordinateRead = mapper.Map<List<ReadEmployeeDto>>(subordinates);

            return Ok(new SuccessResponse<List<ReadEmployeeDto>>(subordinateRead, $"Subordinates retrieved successfully"));

        }

        [Authorize]
        [HttpPut("update-username/{accountId:int}")]
        public async Task<IActionResult> ChangeUserName(int accountId, string name)
        {
            var user = await userAccountServices.GetByIdAsync(accountId);

            try
            {
                if (user == null) return NotFound(new ErrorResponse(ErrorCodes.UserNotFound, $"Employee account with account ID: {accountId} not found."));

                user.UserName = name;

                await userAccountServices.UpdateAsync(user);
                return Ok(new SuccessResponse($"Employee's account username with account ID: {accountId} changed to {name} successfully!"));

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating an employee's account username with account ID {id}", accountId);
                return StatusCode(500, new ApiResponse(false, "Internal server error"));
            }
        }

    }

}



