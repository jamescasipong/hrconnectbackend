using AutoMapper;
using hrconnectbackend.Helper;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Models;
using hrconnectbackend.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace hrconnectbackend.Controllers.v1
{
    [Authorize]
    [ApiController]
    [Route("api/v{version:apiVersion}/overtime")]
    [ApiVersion("1.0")]
    public class OvertimeController : ControllerBase
    {
        private readonly IOTApplicationServices _oTApplicationServices;
        private readonly IMapper _mapper;
        private readonly IAttendanceServices _attendanceServices;
        private readonly IEmployeeServices _employeeServices;
        public OvertimeController(IOTApplicationServices oTApplicationServices, IAttendanceServices attendanceServices, IEmployeeServices employeeServices, IMapper mapper) 
        {
            _oTApplicationServices = oTApplicationServices;
            _mapper = mapper;
            _attendanceServices = attendanceServices;
            _employeeServices = employeeServices;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOTApplication(CreateOTApplicationDTO dtoApplication)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse(false, ModelState.ToString()));
                }

                var supervisor = await _employeeServices.GetByIdAsync(dtoApplication.EmployeeId);

                var attendance = await _attendanceServices.GetAllAsync();

                var attendanceExist = attendance.FirstOrDefault(a => a.Equals(dtoApplication));

                if (attendanceExist == null)
                {
                    return BadRequest(new ApiResponse(false, $"Unable to process. You don't have an attendance for the specified date."));
                }

                if (attendanceExist.ClockOut == null)
                {
                    return BadRequest(new ApiResponse(false, $"You are not clocked out."));
                }

                var otApplication = await _oTApplicationServices.GetAllAsync();

                var newOTApplication = new OTApplication
                {
                    EmployeeId = dtoApplication.EmployeeId,
                    SupervisorId = supervisor.SupervisorId ?? null,
                    Date = dtoApplication.Date,
                    StartTime = TimeOnly.FromTimeSpan(attendanceExist.ClockOut.Value).AddMinutes(1),
                    EndTime = dtoApplication.EndTime,
                    Reasons = dtoApplication.Reasons,
                    Status = "Pending",
                };

                await _oTApplicationServices.AddAsync(newOTApplication);

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
            var oTApplications = new List<OTApplication>();

            try
            {
                var otApplication = await _oTApplicationServices.GetAllAsync();

                var mappedOTApplication = _mapper.Map<List<ReadOTApplicationDTO>>(otApplication);

                if (!mappedOTApplication.Any())
                {
                    return Ok(new ApiResponse<List<ReadOTApplicationDTO>>(false, $"OT Application not found.", mappedOTApplication));
                }

                if (pageIndex != null && pageSize != null)
                {
                    otApplication = _oTApplicationServices.GetOTPagination(otApplication, pageIndex.Value, pageSize.Value);
                }

                return Ok(new ApiResponse<List<ReadOTApplicationDTO>>(false, $"OT Application retrieved successfully!", mappedOTApplication));
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
                var otApplication = await _oTApplicationServices.GetByIdAsync(oTApplicationId);

                if (otApplication == null)
                {
                    return NotFound(new ApiResponse(false, $"OT Application with id: {oTApplicationId} not found."));
                }

                var mappedOTApplication = _mapper.Map<ReadOTApplicationDTO>(otApplication);

                return Ok(new ApiResponse<ReadOTApplicationDTO>(false, $"OT Application with id: {otApplication} retrieved successfully!", mappedOTApplication));
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse(false, $"Internal Server Error"));
            }
        }

        [HttpPut("{oTApplicationId:int}")]
        public async Task<IActionResult> UpdateOTApplication(int oTApplicationId, UpdateOTApplicationDTO otApplicationDTO)
        {
            try
            {
                var overtime = await _oTApplicationServices.GetByIdAsync(oTApplicationId);

                if (overtime == null)
                {
                    return NotFound(new ApiResponse(false, $"OT Application with {oTApplicationId} not found."));
                }

                overtime.Date = otApplicationDTO.Date;
                overtime.Status = otApplicationDTO.Status;
                overtime.StartTime = otApplicationDTO.StartTime;
                overtime.EndTime = otApplicationDTO.EndTime;
                overtime.Reasons = otApplicationDTO.Reasons;

                await _oTApplicationServices.UpdateAsync(overtime);

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
                var otApplication = await _oTApplicationServices.GetByIdAsync(otApplicationId);

                if (otApplication == null)
                {
                    return NotFound(new ApiResponse(false, $"OT Application with id: {otApplicationId} not found."));
                }

                await _oTApplicationServices.DeleteAsync(otApplication);

                return Ok(new ApiResponse(false, $"OT Application with id: {otApplicationId} deleted successfully!"));
            }
            catch(Exception)
            {
                return StatusCode(500, new ApiResponse(false, $"Internal Server Error"));
            }
        }

        [HttpGet("employee/{employeeId:int}")]
        public async Task<IActionResult> RetrieveEmployeeOT(int employeeId, int? pageIndex, int? pageSize)
        {
            try
            {
                var employeeOt = await _oTApplicationServices.GetOTByEmployee(employeeId, pageIndex, pageSize);

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
            var otApplications = await _oTApplicationServices.GetOTByDate(startDate, endDate, pageIndex, pageSize);

            var mappedOTApplications = _mapper.Map<List<OTApplication>>(otApplications);
            try
            {
                return Ok(new ApiResponse<List<OTApplication>>(false, $"OT Applications retrieved successfully!", mappedOTApplications));
            }
            catch (KeyNotFoundException ex)
            {
                return Ok(new ApiResponse<List<OTApplication>>(false, ex.Message, mappedOTApplications));
            }
            catch (Exception)
            {
                return StatusCode(500, $"Internal Server Error");
            }
        }

        [HttpGet("supervisor/{supervisorId:int}")]
        public async Task<IActionResult> RetrieveOTBySupervisor(int supervisorId, int? pageIndex, int? pageSize)
        {
            var otApplications = await _oTApplicationServices.GetOTBySupervisor(supervisorId, pageIndex, pageSize);

            var mappedOTApplications = _mapper.Map<List<OTApplication>>(otApplications);
            try
            {
                return Ok(new ApiResponse<List<OTApplication>>(false, $"OT Applications retrieved successfully!", mappedOTApplications));
            }
            catch (KeyNotFoundException ex)
            {
                return Ok(new ApiResponse<List<OTApplication>>(false, ex.Message, mappedOTApplications));
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
                await _oTApplicationServices.ApproveOT(oTApplicationId);

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
                await _oTApplicationServices.RejectOT(oTApplicationId);

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
