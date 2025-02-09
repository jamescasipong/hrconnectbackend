using hrconnectbackend.Interface.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace hrconnectbackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserSettingController : ControllerBase
    {
        private readonly IUserSettingsServices _userSettingsServices;
        public UserSettingController(IUserSettingsServices userSettingsServices) {
            _userSettingsServices = userSettingsServices;
        }

        
    }
}
