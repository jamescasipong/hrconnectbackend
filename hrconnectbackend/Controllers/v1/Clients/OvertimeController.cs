using AutoMapper;
using hrconnectbackend.Helper;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Interface.Services.Clients;
using hrconnectbackend.Models;
using hrconnectbackend.Models.DTOs;
using hrconnectbackend.Models.Requests;
using hrconnectbackend.Models.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hrconnectbackend.Controllers.v1.Clients
{
    [Authorize]
    [ApiController]
    [Route("api/v{version:apiVersion}/overtime")]
    [ApiVersion("1.0")]
    public class OvertimeController(
        IOTApplicationServices oTApplicationServices,
        IAttendanceServices attendanceServices,
        ISupervisorServices supervisorServices,
        IMapper mapper)
        : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CreateOtApplication(CreateOtApplicationDto dtoApplication)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse(false, ModelState.ToString()));
                }

                var supervisor = await supervisorServices.GetEmployeeSupervisor(dtoApplication.EmployeeId);

                if (supervisor == null)
                {
                    return BadRequest(new ApiResponse(false, $"Unable to process. You don't have a supervisor."));
                }

                var attendance = await attendanceServices.GetAllAsync();

                var attendanceExist = attendance.FirstOrDefault(a => a.Equals(dtoApplication));

                if (attendanceExist == null)
                {
                    return BadRequest(new ApiResponse(false, $"Unable to process. You don't have an attendance for the specified date."));
                }

                if (attendanceExist.ClockOut == null)
                {
                    return BadRequest(new ApiResponse(false, $"You are not clocked out."));
                }

                var otApplication = await oTApplicationServices.GetAllAsync();

                var newOTApplication = new OtApplication
                {
                    EmployeeId = dtoApplication.EmployeeId,
                    SupervisorId = supervisor.Id,
                    Date = dtoApplication.Date,
                    StartTime = TimeOnly.FromTimeSpan(attendanceExist.ClockOut.Value).AddMinutes(1),
                    EndTime = dtoApplication.EndTime,
                    Reasons = dtoApplication.Reasons,
                    Status = "Pending",
                };

                await oTApplicationServices.AddAsync(newOTApplication);

                return Ok(new ApiResponse(false, $"OT Application created successfully!"));
            }
            catch (Exception)
            {
                return StatusCode(500, $"Internal Server Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> RetrieveOTApplication([FromQuery] int? pageIndex, [FromQuery] int? pageSize)
        {
            var oTApplications = new List<OtApplication>();

            try
            {
                var otApplication = await oTApplicationServices.GetAllAsync();

                var mappedOTApplication = mapper.Map<List<ReadOtApplicationDto>>(otApplication);

                if (!mappedOTApplication.Any())
                {
                    return Ok(new ApiResponse<List<ReadOtApplicationDto>?>(false, $"OT Application not found.", mappedOTApplication));
                }

                if (pageIndex != null && pageSize != null)
                {
                    otApplication = oTApplicationServices.GetOTPagination(otApplication, pageIndex.Value, pageSize.Value);
                }

                return Ok(new ApiResponse<List<ReadOtApplicationDto>?>(false, $"OT Application retrieved successfully!", mappedOTApplication));
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{oTApplicationId:int}")]
        public async Task<IActionResult> RetrieveOTApplication(int oTApplicationId, int? pageIndex, int? pageSize)
        {
            try
            {
                var otApplication = await oTApplicationServices.GetByIdAsync(oTApplicationId);

                if (otApplication == null)
                {
                    return NotFound(new ApiResponse(false, $"OT Application with id: {oTApplicationId} not found."));
                }

                var mappedOTApplication = mapper.Map<ReadOtApplicationDto>(otApplication);

                return Ok(new ApiResponse<ReadOtApplicationDto?>(false, $"OT Application with id: {otApplication} retrieved successfully!", mappedOTApplication));
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse(false, $"Internal Server Error"));
            }
        }

        [HttpPut("{oTApplicationId:int}")]
        public async Task<IActionResult> UpdateOTApplication(int oTApplicationId, UpdateOtApplicationDto otApplicationDTO)
        {
            try
            {
                var overtime = await oTApplicationServices.GetByIdAsync(oTApplicationId);

                if (overtime == null)
                {
                    return NotFound(new ApiResponse(false, $"OT Application with {oTApplicationId} not found."));
                }

                overtime.Date = otApplicationDTO.Date;
                overtime.Status = otApplicationDTO.Status;
                overtime.StartTime = otApplicationDTO.StartTime;
                overtime.EndTime = otApplicationDTO.EndTime;
                overtime.Reasons = otApplicationDTO.Reasons;

                await oTApplicationServices.UpdateAsync(overtime);

                return Ok(new ApiResponse(true, $"OT Application with id: {oTApplicationId} updated successfully!"));
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse(false, $"Internal Server Error"));
            }
        }

        [HttpDelete("{otApplicationId:int}")]
        public async Task<IActionResult> DeleteOTApplication(int otApplicationId)
        {
            try
            {
                var otApplication = await oTApplicationServices.GetByIdAsync(otApplicationId);

                if (otApplication == null)
                {
                    return NotFound(new ApiResponse(false, $"OT Application with id: {otApplicationId} not found."));
                }

                await oTApplicationServices.DeleteAsync(otApplication);

                return Ok(new ApiResponse(false, $"OT Application with id: {otApplicationId} deleted successfully!"));
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse(false, $"Internal Server Error"));
            }
        }

        [HttpGet("employee/{employeeId:int}")]
        public async Task<IActionResult> RetrieveEmployeeOT(int employeeId, int? pageIndex, int? pageSize)
        {
            try
            {
                var employeeOt = await oTApplicationServices.GetOTByEmployee(employeeId, pageIndex, pageSize);

                return Ok(employeeOt);
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

        [HttpGet("range-date")]
        public async Task<IActionResult> RetrieveOTByDate(DateOnly startDate, DateOnly endDate, int? pageIndex, int? pageSize)
        {
            var otApplications = await oTApplicationServices.GetOTByDate(startDate, endDate, pageIndex, pageSize);

            var mappedOTApplications = mapper.Map<List<OtApplication>>(otApplications);
            try
            {
                return Ok(new ApiResponse<List<OtApplication>?>(false, $"OT Applications retrieved successfully!", mappedOTApplications));
            }
            catch (KeyNotFoundException ex)
            {
                return Ok(new ApiResponse<List<OtApplication>?>(false, ex.Message, mappedOTApplications));
            }
            catch (Exception)
            {
                return StatusCode(500, $"Internal Server Error");
            }
        }

        [HttpGet("supervisor/{supervisorId:int}")]
        public async Task<IActionResult> RetrieveOTBySupervisor(int supervisorId, int? pageIndex, int? pageSize)
        {
            var otApplications = await oTApplicationServices.GetOTBySupervisor(supervisorId, pageIndex, pageSize);

            var mappedOTApplications = mapper.Map<List<OtApplication>>(otApplications);
            try
            {
                return Ok(new ApiResponse<List<OtApplication>?>(false, $"OT Applications retrieved successfully!", mappedOTApplications));
            }
            catch (KeyNotFoundException ex)
            {
                return Ok(new ApiResponse<List<OtApplication>?>(false, ex.Message, mappedOTApplications));
            }
            catch (Exception)
            {
                return StatusCode(500, $"Internal Server Error");
            }
        }

        [HttpPut("{oTApplicationId:int}/approve")]
        public async Task<IActionResult> ApproveOT(int oTApplicationId)
        {
            try
            {
                await oTApplicationServices.ApproveOT(oTApplicationId);

                return Ok(new ApiResponse(true, $"OT Application with id: {oTApplicationId} has been approved!"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse(false, ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse(true, $"Internal Server Error"));
            }
        }

        [HttpPut("{oTApplicationId:int}/reject")]
        public async Task<IActionResult> RejectOT(int oTApplicationId)
        {
            try
            {
                await oTApplicationServices.RejectOT(oTApplicationId);

                return Ok(new ApiResponse(true, $"OT Application with id: {oTApplicationId} has been rejected!"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse(false, ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse(true, $"Internal Server Error"));
            }
        }
    }
}
