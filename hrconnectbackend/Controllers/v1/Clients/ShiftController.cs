﻿using System.Security.Claims;
using AutoMapper;
using hrconnectbackend.Helper;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Models;
using hrconnectbackend.Models.DTOs;
using hrconnectbackend.Models.Response;
using hrconnectbackend.Services.ExternalServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hrconnectbackend.Controllers.v1.Clients
{
    [Authorize]
    [ApiController]
    [Route("api/v{version:apiVersion}/shift")]
    [ApiVersion("1.0")]
    public class ShiftController(
        IShiftServices shiftServices,
        IMapper mapper,
        AuthenticationServices authenticationServices,
        ILogger<ShiftController> logger)
        : ControllerBase
    {
        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetShiftById(int id)
        {
            try
            {
                var shift = await shiftServices.GetByIdAsync(id);
                return Ok(shift);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllShifts()
        {
            var organizationId = User.FindFirstValue("organizationId")!;

            if (!int.TryParse(organizationId, out var orgId))
            {
                return BadRequest(new ApiResponse(false, "Invalid organization ID"));
            }

            var shifts = await shiftServices.GetAllAsync();

            var orgShifts = shifts.Where(a => a.OrganizationId == orgId).ToList();

            return Ok(orgShifts);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateShift(ShiftDTO shiftDTO)
        {
            try
            {
                if (!IsValidDayOfWorked(shiftDTO.DaysOfWorked))
                {
                    return BadRequest(new ApiResponse(false, "Days of worked is invalid"));
                }

                var employee = await shiftServices.GetAllAsync();

                bool shiftExisted = employee.Where(a => a.EmployeeShiftId == shiftDTO.EmployeeShiftId && a.DaysOfWorked == shiftDTO.DaysOfWorked).Any();

                if (shiftExisted)
                {
                    return Conflict(new ApiResponse(false, "Day of worked already existed"));
                }

                var createdShift = await shiftServices.AddAsync(new Shift
                {
                    EmployeeShiftId = shiftDTO.EmployeeShiftId,
                    DaysOfWorked = BodyRequestCorrection.CapitalLowerCaseName(shiftDTO.DaysOfWorked),
                    TimeIn = TimeSpan.Parse(shiftDTO.TimeIn),
                    TimeOut = TimeSpan.Parse(shiftDTO.TimeOut)
                });

                return Ok(new ApiResponse<Shift?>(success: true, message: $"Shift by Employee with ID {shiftDTO} created successfully!", data: createdShift));
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateShift(int id, [FromBody] Shift shift)
        {
            try
            {
                var shiftExist = await shiftServices.GetByIdAsync(id);

                if (shiftExist == null)
                {
                    throw new KeyNotFoundException($"No shift found with Id {id}");
                }

                if (shift == null)
                {
                    throw new ArgumentNullException(nameof(shift));
                }

                await shiftServices.UpdateAsync(shift);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteShift(int id)
        {
            try
            {
                var shift = await shiftServices.GetByIdAsync(id);

                if (shift == null)
                {
                    throw new KeyNotFoundException($"No shift found for employee with ID {id}");
                }

                await shiftServices.DeleteAsync(shift);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [Authorize]
        [HttpGet("my-shift")]
        public async Task<IActionResult> GetMyShift()
        {
            var employeeId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var organizationId = User.FindFirstValue("organizationId")!;

            if (!int.TryParse(organizationId, out var orgId))
            {
                return BadRequest(new ApiResponse(false, "Invalid organization ID"));
            }

            try
            {
                var shifts = await shiftServices.GetEmployeeShifts(employeeId, orgId);

                var mappedShift = mapper.Map<List<ShiftDTO>>(shifts);

                return Ok(new ApiResponse<List<ShiftDTO>?>(true, $"Shifts by Employee with id: {employeeId} retrieved successfully.", mappedShift));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse(false, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse(false, ex.Message));  // Handle unexpected errors
            }
        }


        [Authorize(Roles = "Admin")]
        [HttpGet("employee-shift/{employeeId}")]
        public async Task<IActionResult> GetEmployeeShift(int employeeId)
        {
            var organizationId = User.FindFirstValue("organizationId")!;

            if (!int.TryParse(organizationId, out var orgId))
            {
                return BadRequest(new ApiResponse(false, "Invalid organization ID"));
            }

            try
            {
                var shifts = await shiftServices.GetEmployeeShifts(employeeId, orgId);

                var mappedShift = mapper.Map<List<ShiftDTO>>(shifts);

                return Ok(new ApiResponse<List<ShiftDTO>?>(true, $"Shifts by Employee with id: {employeeId} retrieved successfully.", mappedShift));
            }

            catch (Exception ex)
            {
                if (ex is KeyNotFoundException)
                {
                    return NotFound(new ApiResponse(false, ex.Message));
                }

                if (ex is UnauthorizedAccessException)
                {
                    return Forbid();
                }

                return StatusCode(500, new ApiResponse(false, ex.Message));  // Handle unexpected errors
            }
        }

        [Authorize(Roles ="Admin,HR")]
        [HttpGet("shift-today/{employeeId}")]
        public async Task<IActionResult> HasShiftToday(int employeeId)
        {
            

            try
            {
                var hasShift = await shiftServices.HasShiftToday(employeeId);
                return Ok(hasShift);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "An internal error occurred" });
            }
        }

        [Authorize]
        [HttpGet("shift-today")]
        public async Task<IActionResult> HasShiftToday()
        {
            var user = User.FindFirstValue("EmployeeId")!;

            if (user == null) return Unauthorized();

            try
            {
                if (int.TryParse(user, out var employeeId)){
                    var hasShift = await shiftServices.HasShiftToday(employeeId);
                    return Ok(new ApiResponse<bool>(true, $"User has shift today", hasShift));
                }else{
                    return Unauthorized();
                }
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return StatusCode(500, new { error = "An internal error occurred" });
            }
        }

        private bool IsValidDayOfWorked(string name)
        {
            List<string> daysOfWorked = new List<string>
        {
            "Monday",
            "Tuesday",
            "Wednesday",
            "Thursday",
            "Friday",
            "Saturday",
            "Sunday"
        };

            return daysOfWorked.Any(a => a.ToLower() == name.ToLower());
        }
    }
    
}
