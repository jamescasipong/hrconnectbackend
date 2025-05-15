using AutoMapper;
using hrconnectbackend.Constants;
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
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorResponse(ErrorCodes.InvalidRequestModel, "Invalid OT application data."));
            }

            var supervisor = await supervisorServices.GetEmployeeSupervisor(dtoApplication.EmployeeId);

            var attendance = await attendanceServices.GetAllAsync();

            var attendanceExist = attendance.FirstOrDefault(a => a.Equals(dtoApplication));

            if (attendanceExist == null)
            {
                return StatusCode(404, new ErrorResponse(ErrorCodes.AttendanceNotFound, $"Attendance not found."));
            }

            if (attendanceExist.ClockOut == null)
            {
                return StatusCode(400, new ErrorResponse(ErrorCodes.InvalidRequestModel, $"Clock out time is required."));
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

            return Ok(new SuccessResponse($"OT Application created successfully!"));

        }

        [HttpGet]
        public async Task<IActionResult> RetrieveOTApplication([FromQuery] int? pageIndex, [FromQuery] int? pageSize)
        {

            var otApplication = await oTApplicationServices.GetAllAsync();

            var mappedOTApplication = mapper.Map<List<ReadOtApplicationDto>>(otApplication);

            if (!mappedOTApplication.Any())
            {
                return Ok(new SuccessResponse<List<ReadOtApplicationDto>?>(mappedOTApplication, $"No OT Application found!"));
            }

            if (pageIndex != null && pageSize != null)
            {
                otApplication = oTApplicationServices.GetOTPagination(otApplication, pageIndex.Value, pageSize.Value);
            }

            return Ok(new SuccessResponse<List<ReadOtApplicationDto>?>(mappedOTApplication, $"OT Applications retrieved successfully!"));

        }

        [HttpGet("{oTApplicationId:int}")]
        public async Task<IActionResult> RetrieveOTApplication(int oTApplicationId, int? pageIndex, int? pageSize)
        {

            var otApplication = await oTApplicationServices.GetByIdAsync(oTApplicationId);

            if (otApplication == null)
            {
                return NotFound(new ErrorResponse(ErrorCodes.OTApplicationNotFound, $"OT Application with id: {oTApplicationId} not found."));
            }

            var mappedOTApplication = mapper.Map<ReadOtApplicationDto>(otApplication);

            return Ok(new SuccessResponse<ReadOtApplicationDto?>(mappedOTApplication, $"OT Application with id: {oTApplicationId} retrieved successfully!"));

        }

        [HttpPut("{oTApplicationId:int}")]
        public async Task<IActionResult> UpdateOTApplication(int oTApplicationId, UpdateOtApplicationDto otApplicationDTO)
        {

            var overtime = await oTApplicationServices.GetByIdAsync(oTApplicationId);

            if (overtime == null)
            {
                return NotFound(new ErrorResponse(ErrorCodes.OTApplicationNotFound, $"OT Application with id: {oTApplicationId} not found."));
            }

            overtime.Date = otApplicationDTO.Date;
            overtime.Status = otApplicationDTO.Status;
            overtime.StartTime = otApplicationDTO.StartTime;
            overtime.EndTime = otApplicationDTO.EndTime;
            overtime.Reasons = otApplicationDTO.Reasons;

            await oTApplicationServices.UpdateAsync(overtime);

            return Ok(new SuccessResponse<ReadOtApplicationDto?>(mapper.Map<ReadOtApplicationDto>(overtime), $"OT Application with id: {oTApplicationId} updated successfully!"));
        }

        [HttpDelete("{otApplicationId:int}")]
        public async Task<IActionResult> DeleteOTApplication(int otApplicationId)
        {

            var otApplication = await oTApplicationServices.GetByIdAsync(otApplicationId);

            if (otApplication == null)
            {
                return NotFound(new ErrorResponse(ErrorCodes.OTApplicationNotFound, $"OT Application with id: {otApplicationId} not found."));
            }

            await oTApplicationServices.DeleteAsync(otApplication);

            return Ok(new SuccessResponse($"OT Application with id: {otApplicationId} deleted successfully!"));
        }

        [HttpGet("employee/{employeeId:int}")]
        public async Task<IActionResult> RetrieveEmployeeOT(int employeeId, int? pageIndex, int? pageSize)
        {

            var employeeOt = await oTApplicationServices.GetOTByEmployee(employeeId, pageIndex, pageSize);

            return Ok(new SuccessResponse<List<OtApplication>?>(employeeOt, $"OT Applications retrieved successfully!"));

        }

        [HttpGet("range-date")]
        public async Task<IActionResult> RetrieveOTByDate(DateOnly startDate, DateOnly endDate, int? pageIndex, int? pageSize)
        {
            var otApplications = await oTApplicationServices.GetOTByDate(startDate, endDate, pageIndex, pageSize);

            var mappedOTApplications = mapper.Map<List<OtApplication>>(otApplications);

            return Ok(new SuccessResponse<List<OtApplication>?>(mappedOTApplications, $"OT Applications retrieved successfully!"));

        }

        [HttpGet("supervisor/{supervisorId:int}")]
        public async Task<IActionResult> RetrieveOTBySupervisor(int supervisorId, int? pageIndex, int? pageSize)
        {
            var otApplications = await oTApplicationServices.GetOTBySupervisor(supervisorId, pageIndex, pageSize);

            var mappedOTApplications = mapper.Map<List<OtApplication>>(otApplications);

            return Ok(new SuccessResponse<List<OtApplication>?>(mappedOTApplications, $"OT Applications retrieved successfully!"));

        }

        [HttpPut("{oTApplicationId:int}/approve")]
        public async Task<IActionResult> ApproveOT(int oTApplicationId)
        {

            await oTApplicationServices.ApproveOT(oTApplicationId);

            return Ok(new SuccessResponse($"OT Application with id: {oTApplicationId} has been approved!"));

        }

        [HttpPut("{oTApplicationId:int}/reject")]
        public async Task<IActionResult> RejectOT(int oTApplicationId)
        {

            await oTApplicationServices.RejectOT(oTApplicationId);

            return Ok(new SuccessResponse($"OT Application with id: {oTApplicationId} has been rejected!"));

        }
    }
}
