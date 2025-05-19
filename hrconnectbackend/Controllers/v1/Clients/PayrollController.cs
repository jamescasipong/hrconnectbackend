using hrconnectbackend.Constants;
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

        [Authorize(Policy = "Premium")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPayrollById(int id)
        {
            var payroll = await payrollService.GetByIdAsync(id);

            return Ok(payroll);
        }
        [Authorize(Roles = "Admin,HR")]
        [HttpPost("all-employees")]
        public async Task<IActionResult> GeneratePayrollAllEmployees([FromQuery] string period1, [FromQuery] string period2)
        {

            await payrollService.GeneratePayrollForAllEmployees(DateTime.Parse(period1), DateTime.Parse(period2));

            return Ok(new SuccessResponse("Payroll generated successfully!"));
        }

        [Authorize(Roles = "Admin,HR")]
        [HttpPut("{id}")]
        public async Task<ActionResult<Payroll>> UpdatePayrollStatus(int id, [FromBody] string status)
        {
            var updatedPayroll = await payrollService.UpdatePayrollStatus(id, status);

            return Ok(new SuccessResponse("Payroll status updated successfully!"));
        }

        [Authorize(Roles = "Admin,HR")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeletePayroll(int id)
        {
            var payroll = await payrollService.GetByIdAsync(id);

            await payrollService.DeleteAsync(payroll);

            return Ok(new SuccessResponse("Payroll deleted successfully!"));
        }
    }
}
