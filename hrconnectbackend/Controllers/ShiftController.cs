using AutoMapper;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace hrconnectbackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShiftController : ControllerBase
    {
        private readonly IShiftServices _shiftServices;
        private readonly IMapper _mapper;

        public ShiftController(IShiftServices shiftServices, IMapper mapper)
        {
            _shiftServices = shiftServices;
            _mapper = mapper;
        }

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

        [HttpGet]
        public async Task<IActionResult> GetAllShifts()
        {
            var shifts = await _shiftServices.GetAllAsync();

            return Ok(shifts);
        }

        [HttpPost]
        public async Task<IActionResult> CreateShift([FromBody] Shift shift)
        {
            try
            {
                var createdShift = await _shiftServices.AddAsync(shift);
                return CreatedAtAction(nameof(GetShiftById), new { id = createdShift.Id }, createdShift);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

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

        [HttpGet("employee-shift/{employeeId}")]
        public async Task<IActionResult> GetEmployeeShift(int employeeId)
        {
            try
            {
                var shifts = await _shiftServices.GetEmployeeShifts(employeeId);

                return Ok(shifts);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");  // Handle unexpected errors
            }
        }


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
    }
}
