using hrconnectbackend.Helper;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Models;
using Microsoft.AspNetCore.Mvc;

namespace hrconnectbackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserSettingController : ControllerBase
    {
        private readonly IUserSettingsServices _userSettingsServices;
        public UserSettingController(IUserSettingsServices userSettingsServices)
        {
            _userSettingsServices = userSettingsServices;
        }

        [HttpPost("{employeeId}")]
        public async Task<IActionResult> AddUserSetting(int employeeId)
        {
            try
            {
                await _userSettingsServices.CreateDefaultSettings(employeeId);
                return Ok(new ApiResponse(true, "Success!"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch(InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPut("{employeeId}")]
        public async Task<IActionResult> ResetSettings(int employeeId)
        {
            try
            {
                await _userSettingsServices.ResetSettings(employeeId);

                return Ok(new ApiResponse(true, "Success!"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
