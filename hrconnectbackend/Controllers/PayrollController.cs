using hrconnectbackend.Helper;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Models;
using hrconnectbackend.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace hrconnectbackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PayrollController : ControllerBase
    {
        private readonly IPayrollServices _payrollService;

        public PayrollController(IPayrollServices payrollService)
        {
            _payrollService = payrollService;
        }

        // Get all payroll records
        [HttpGet]
        public async Task<IActionResult> GetAllPayrolls()
        {
            return Ok(await _payrollService.GetAllAsync());
        }

        // Get payroll record by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPayrollById(int id)
        {
            var payroll = await _payrollService.GetByIdAsync(id);
            if (payroll == null) return NotFound();
            return Ok(payroll);
        }

        [HttpPost("all-employees")]
        public async Task<IActionResult> GeneratePayrollAllEmployees([FromQuery] string period1, [FromQuery] string period2)
        {
            try
            {
                await _payrollService.GeneratePayrollForAllEmployees(DateTime.Parse(period1), DateTime.Parse(period2));

                return Ok(new ApiResponse(true, $"Payroll for all employees successfully generated"));
            }
            catch (Exception ex) 
            {
                return StatusCode(500, ex);
            }
        }

        // Update payroll payment status
        [HttpPut("{id}")]
        public async Task<ActionResult<Payroll>> UpdatePayrollStatus(int id, [FromBody] string status)
        {
            var updatedPayroll = await _payrollService.UpdatePayrollStatus(id, status);
            if (updatedPayroll == null) return NotFound();
            return Ok(updatedPayroll);
        }

        // Delete payroll record
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeletePayroll(int id)
        {
            var payroll = await _payrollService.GetByIdAsync(id);

            if (payroll == null) return NotFound();

            await _payrollService.DeleteAsync(payroll);

            return NoContent();
        }
    }
}
