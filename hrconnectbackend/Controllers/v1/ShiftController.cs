using AutoMapper;
using hrconnectbackend.Helper;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Models;
using hrconnectbackend.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace hrconnectbackend.Controllers.v1
{
    [Authorize]
    [ApiController]
    [Route("api/v{version:apiVersion}/shift")]
    [ApiVersion("1.0")]
    public class ShiftController : ControllerBase
    {
        private readonly IShiftServices _shiftServices;
        private readonly IMapper _mapper;
        private readonly AuthenticationServices _authenticationServices;

        public ShiftController(IShiftServices shiftServices, IMapper mapper, AuthenticationServices authenticationServices)
        {
            _shiftServices = shiftServices;
            _mapper = mapper;
            _authenticationServices = authenticationServices;
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetShiftById(int id)
        {
            try
            {
                var shift = await _shiftServices.GetByIdAsync(id);
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
            var shifts = await _shiftServices.GetAllAsync();

            return Ok(shifts);
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

                var employee = await _shiftServices.GetAllAsync();

                bool shiftExisted = employee.Where(a => a.EmployeeShiftId == shiftDTO.EmployeeShiftId && a.DaysOfWorked == shiftDTO.DaysOfWorked).Any();

                if (shiftExisted)
                {
                    return Conflict(new ApiResponse(false, "Day of worked already existed"));
                }

                var createdShift = await _shiftServices.AddAsync(new Shift
                {
                    EmployeeShiftId = shiftDTO.EmployeeShiftId,
                    DaysOfWorked = BodyRequestCorrection.CapitalLowerCaseName(shiftDTO.DaysOfWorked),
                    TimeIn = TimeSpan.Parse(shiftDTO.TimeIn),
                    TimeOut = TimeSpan.Parse(shiftDTO.TimeOut)
                });

                return Ok(new ApiResponse<Shift>(success: true, message: $"Shift by Employee with ID {shiftDTO} created successfully!", data: createdShift));
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
                var shiftExist = await _shiftServices.GetByIdAsync(id);

                if (shiftExist == null)
                {
                    throw new KeyNotFoundException($"No shift found with Id {id}");
                }

                if (shift == null)
                {
                    throw new ArgumentNullException(nameof(shift));
                }

                await _shiftServices.UpdateAsync(shift);
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
                var shift = await _shiftServices.GetByIdAsync(id);

                if (shift == null)
                {
                    throw new KeyNotFoundException($"No shift found for employee with ID {id}");
                }

                await _shiftServices.DeleteAsync(shift);
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

            try
            {
                var shifts = await _shiftServices.GetEmployeeShifts(employeeId);

                var mappedShift = _mapper.Map<List<ShiftDTO>>(shifts);

                return Ok(new ApiResponse<List<ShiftDTO>>(true, $"Shifts by Employee with id: {employeeId} retrieved successfully.", mappedShift));
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

        [Authorize]
        [HttpGet("employee-shift/{employeeId}")]
        public async Task<IActionResult> GetEmployeeShift(int employeeId)
        {

            string[] admins = {
                "Admin", "HR"
            };

            try
            {
                var validateUser = _authenticationServices.ValidateUser(User, employeeId, admins);

                if (validateUser != null){
                    return validateUser;
                }

                var shifts = await _shiftServices.GetEmployeeShifts(employeeId);

                var mappedShift = _mapper.Map<List<ShiftDTO>>(shifts);

                return Ok(new ApiResponse<List<ShiftDTO>>(true, $"Shifts by Employee with id: {employeeId} retrieved successfully.", mappedShift));
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

        [Authorize(Roles ="Admin,HR")]
        [HttpGet("shift-today/{employeeId}")]
        public async Task<IActionResult> HasShiftToday(int employeeId)
        {
            

            try
            {
                var hasShift = await _shiftServices.HasShiftToday(employeeId);
                return Ok(hasShift);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
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
