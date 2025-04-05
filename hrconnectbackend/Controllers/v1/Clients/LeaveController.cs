using AutoMapper;
using hrconnectbackend.Helper;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Models;
using hrconnectbackend.Models.DTOs;
using hrconnectbackend.Models.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hrconnectbackend.Controllers.v1.Clients;

[Authorize]
[ApiController]
[Route("api/v{version:apiVersion}/leave")]
[ApiVersion("1.0")]
public class LeaveController(
    ILeaveApplicationServices leaveServices,
    ILogger<LeaveController> logger,
    ILeaveBalanceServices leaveBalanceServices,
    AuthenticationServices authenticationServices,
    IMapper mapper)
    : ControllerBase
{
    [Authorize]
    [HttpPost("applications")]
    public async Task<IActionResult> CreateLeave([FromBody] CreateLeaveApplicationDto leaveRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var validateUser = authenticationServices.ValidateUser(User, leaveRequest.EmployeeId);

            if (validateUser != null){
                return validateUser;
            }

            var newLeaveApplication = new LeaveApplication
            {
                EmployeeId = leaveRequest.EmployeeId,
                SupervisorId = leaveRequest.SupervisorId ?? null,
                Type = leaveRequest.Type,
                StartDate = DateOnly.Parse(leaveRequest.StartDate),
                Reason = leaveRequest.Reason,
                AppliedDate = DateOnly.FromDateTime(DateTime.Now)
            };

            var createdLeave = await leaveServices.RequestLeave(newLeaveApplication);

            return Ok(new ApiResponse(true, $"Leave application created successfully!"));
        }
        catch (ArgumentNullException ex){
            return BadRequest(new ApiResponse(false, ex.Message));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ApiResponse(false, ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating an employee.");
            return StatusCode(500, "Internal server error");
        }
    }

    [Authorize(Roles ="Admin,HR")]
    [HttpGet("applications")]
    public async Task<IActionResult> GetAllLeaves([FromQuery] int? pageIndex, [FromQuery] int? pageSize)
    {
        try
        {
            List<LeaveApplication> leaves;

            // Check if pagination parameters are provided
            if (pageIndex == null || pageSize == null)
            {
                // If no pagination parameters, fetch all leaves
                leaves = await leaveServices.GetAllAsync();
            }
            else
            {
                // Validate pageIndex and pageSize
                if (pageIndex <= 0)
                {
                    return BadRequest(new ApiResponse(false, "Page index must be greater than 0"));
                }

                if (pageSize <= 0)
                {
                    return BadRequest(new ApiResponse(false, "Page size must be greater than 0"));
                }

                // Fetch all leaves and apply pagination
                leaves = await leaveServices.GetAllAsync();
                leaves = leaves.Skip((pageIndex.Value - 1) * pageSize.Value)
                    .Take(pageSize.Value)
                    .ToList();
            }

            // If no leaves are found, return an empty list
            if (!leaves.Any())
            {
                return Ok(new ApiResponse<List<LeaveApplication>>(true, "No leave applications found.", new List<LeaveApplication>()));
            }

            // Map the leave data to DTOs
            var leaveDTO = mapper.Map<List<ReadLeaveApplicationDto>>(leaves);

            // Return the leave data in the response
            return Ok(new ApiResponse<List<ReadLeaveApplicationDto>>(true, "Leave applications retrieved successfully!", leaveDTO));
        }
        catch (Exception ex)
        {
            // Log the error and return internal server error
            logger.LogError(ex, "Error retrieving leave applications.");
            return StatusCode(500, "Internal server error");
        }
    }

    
    [Authorize]
    [HttpGet("applications/{id:int}")]
    public async Task<IActionResult> GetLeaveById(int id)
    {
        try
        {
            var admins = new string[] {
                "Admin", "HR"
            };

            var leave = await leaveServices.GetByIdAsync(id);

            if (leave == null)
                return NotFound(new ApiResponse(false, $"Leave application with ID: {id} not found."));

            var validateUser = authenticationServices.ValidateUser(User, leave.EmployeeId, admins);

            if (validateUser != null){
                return validateUser;
            }


            var leaveDTO = mapper.Map<ReadLeaveApplicationDto>(leave);

            return Ok(new ApiResponse<ReadLeaveApplicationDto>(true, $"Leave application with ID: {id} retrieved successfully!", leaveDTO));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving a leave application with ID: {id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [Authorize]
    [HttpPut("applications/{id:int}")]
    public async Task<IActionResult> UpdateLeaveDates(int id, [FromBody] UpdateLeaveApplicationDto leaveRequest)
    {
        var admins = new string[]{
            "Admin", "HR"
        };

        if (leaveRequest == null)
            return BadRequest("Body request not found.");

        try
        {
            var leaveApplication = await leaveServices.GetByIdAsync(id);

            if (leaveApplication == null)
            {
                return NotFound(new ApiResponse(false, $"Leave application with ID: {id} not found."));
            }

            var validateUser = authenticationServices.ValidateUser(User, leaveApplication.EmployeeId, admins);

            if (validateUser != null){
                return validateUser;
            }

            leaveApplication.Type = leaveRequest.Type;
            leaveApplication.Status = leaveRequest.Status;
            leaveApplication.Reason = leaveRequest.Reason;
            leaveApplication.StartDate = leaveApplication.StartDate;

            await leaveServices.UpdateAsync(leaveApplication);

            return Ok(new ApiResponse(true, $"Leave application with ID: {id} updated successfully!"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating a leave application with ID: {id}", id);
            return StatusCode(500, "Internal server error");
        }
    }    

    [Authorize]
    [HttpDelete("applications/{id:int}")]
    public async Task<IActionResult> DeleteLeave(int id)
    {
        var admins = new []{
            "Admin", "HR"
        };
        try
        {
            var leave = await leaveServices.GetByIdAsync(id);

            if (leave == null)
            {
                return NotFound(new ApiResponse(false, $"Leave application with ID: {id} not found."));
            }

            var validateUser = authenticationServices.ValidateUser(User, leave.EmployeeId, admins);

            if (validateUser != null){
                return validateUser;
            }

            await leaveServices.DeleteAsync(leave);

            return Ok(new ApiResponse(true, $"Leave application with ID: {id} deleted successfully!"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting a leave application with ID: {id}", id);
            return StatusCode(500, "Internal server error");
        }
    }
    [Authorize]
    [HttpGet("applications/employee/{employeeId:int}")]
    public async Task<IActionResult> GetLeaveApplicationByEmp(int employeeId, [FromQuery] int? pageIndex, [FromQuery] int? pageSize)
    {
        var admins = new string[] { "Admin", "HR" };
        
        // Validate the user's permissions
        authenticationServices.ValidateUser(User, employeeId, admins);
        

        try
        {
            List<LeaveApplication> employeeLeaveApplication;

            if (pageIndex == null || pageSize == null)
            {
                // Fetch all leave applications for the employee if pagination is not provided
                employeeLeaveApplication = await leaveServices.GetLeaveByEmployee(employeeId);
            }
            else
            {
                // Validate pageIndex and pageSize
                if (pageIndex <= 0)
                {
                    return BadRequest(new ApiResponse(false, "Page index must be greater than 0"));
                }

                if (pageSize <= 0)
                {
                    return BadRequest(new ApiResponse(false, "Page size must be greater than 0"));
                }

                // Fetch leave applications with pagination
                employeeLeaveApplication = await leaveServices.GetLeaveByEmployee(employeeId);
                employeeLeaveApplication = employeeLeaveApplication
                                            .Skip((pageIndex.Value - 1) * pageSize.Value)
                                            .Take(pageSize.Value)
                                            .ToList();
            }

            if (!employeeLeaveApplication.Any())
            {
                return NotFound(new ApiResponse(false, "No leave applications found for this employee."));
            }

            // Map the data to the DTO
            var employeeLeaveApplicationDto = mapper.Map<List<ReadLeaveApplicationDto>>(employeeLeaveApplication);

            return Ok(new ApiResponse<List<ReadLeaveApplicationDto>>(true, $"Leave application by employee with ID: {employeeId} retrieved successfully!", employeeLeaveApplicationDto));
        }
        catch (KeyNotFoundException ex)
        {
            // Handle case where the leave application is not found
            return NotFound(new ApiResponse(false, ex.Message));
        }
        catch (Exception ex)
        {
            // Log and handle unexpected errors
            logger.LogError(ex, "Error retrieving leave applications for employee with ID: {employeeId}", employeeId);
            return StatusCode(500, "Internal server error");
        }
    }

    
    [Authorize(Roles ="HR,Admin")]
    [HttpPut("applications/approve/{id:int}")]
    public async Task<IActionResult> ApproveLeave(int id)
    {
        try
        {
            await leaveServices.ApproveLeave(id);

            return Ok(new ApiResponse(true, $"Leave application with ID: {id} approved successfully!"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse(false, ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error approving a leave application with ID: {id}", id);
            return StatusCode(500, "Internal server error");
        }
    }
    [Authorize(Roles ="HR,Admin")]
    [HttpPut("applications/reject/{id:int}")]
    public async Task<IActionResult> RejectLeave(int id)
    {
        try
        {
            await leaveServices.RejectLeave(id);

            return Ok(new ApiResponse(false, $"Leave application with ID: {id} rejected successfully!"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse(false, ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error approving a leave application with ID: {id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [Authorize("Admin,HR")]
    [HttpGet("balances")]
    public async Task<IActionResult> GetAllLeaveBalances([FromQuery] int? pageIndex, [FromQuery] int? pageSize)
    {
        try
        {
            // Fetch all leave balances
            var leaves = await leaveBalanceServices.GetAllAsync();

            // If pagination is requested, apply pagination logic
            if (pageIndex.HasValue && pageSize.HasValue && pageIndex.Value > 0 && pageSize.Value > 0)
            {
                leaves = leaves.Skip((pageIndex.Value - 1) * pageSize.Value)
                    .Take(pageSize.Value)
                    .ToList();
            }

            // If no leave balances are found
            if (!leaves.Any())
            {
                return Ok(new ApiResponse<List<LeaveBalance>>(true, "No leave balances found.", new List<LeaveBalance>()));
            }

            // Return the leave balances
            return Ok(new ApiResponse<List<LeaveBalance>>(success: true, message: "Leave balances retrieved successfully!", data: leaves));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving leave balances");
            return StatusCode(500, "Internal server error");
        }
    }


    [Authorize]
    [HttpGet("balances/{employeeId}")]
    public async Task<IActionResult> GetLeaveBalanceByEmployeeId(int employeeId)
    {
        var admins = new string []{
            "Admin", "HR"
        };
        
        var validateUser = authenticationServices.ValidateUser(User, employeeId, admins);

        if (validateUser != null){
            return validateUser;
        }

        try
        {
            var balances = await leaveBalanceServices.GetLeaveBalanceByEmployeeId(employeeId);

            if (!balances.Any()) return Ok(new ApiResponse<List<LeaveBalance>>(true, $"Leave balances by employee with ID: {employeeId} retrieved successfully!", balances));

            return Ok(new ApiResponse<List<LeaveBalance>>(true, $"Leave balances by employee with ID: {employeeId} retrieved successfully!", balances));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse(false, ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving leave balances for employee with ID: {employeeId}", employeeId);
            return StatusCode(500, "Internal server error");
        }
    }

    [Authorize]
    [HttpPost("balances")]
    public async Task<ActionResult<LeaveBalance>> AddOrUpdateLeaveBalance([FromBody] LeaveBalance leaveBalance)
    {
        await leaveBalanceServices.AddOrUpdateLeaveBalance(leaveBalance);
        return Ok();
    }
}
