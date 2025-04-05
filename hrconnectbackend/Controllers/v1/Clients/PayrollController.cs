using hrconnectbackend.Helper;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Models;
using hrconnectbackend.Models.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hrconnectbackend.Controllers.v1.Clients
{
    [Authorize]
    [ApiController]
    [Route("api/v{version:apiVersion}/payroll")]
    [ApiVersion("1.0")]
    public class PayrollController(IPayrollServices payrollService) : ControllerBase
    {
        // Get all payroll records
        [HttpGet]
        public async Task<IActionResult> GetAllPayrolls()
        {
            return Ok(await payrollService.GetAllAsync());
        }

        // Get payroll record by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPayrollById(int id)
        {
            var payroll = await payrollService.GetByIdAsync(id);
            if (payroll == null) return NotFound();
            return Ok(payroll);
        }

        [HttpPost("all-employees")]
        public async Task<IActionResult> GeneratePayrollAllEmployees([FromQuery] string period1, [FromQuery] string period2)
        {
            try
            {
                await payrollService.GeneratePayrollForAllEmployees(DateTime.Parse(period1), DateTime.Parse(period2));

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
            var updatedPayroll = await payrollService.UpdatePayrollStatus(id, status);
            if (updatedPayroll == null) return NotFound();
            return Ok(updatedPayroll);
        }

        // Delete payroll record
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeletePayroll(int id)
        {
            var payroll = await payrollService.GetByIdAsync(id);

            if (payroll == null) return NotFound();

            await payrollService.DeleteAsync(payroll);

            return NoContent();
        }
    }
}
