using hrconnectbackend.Interface.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace hrconnectbackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationServices _notificationServices;
        public NotificationController(INotificationServices notificationServices)
        {
            _notificationServices = notificationServices;
        }
    }
}
