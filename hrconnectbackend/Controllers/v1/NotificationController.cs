using System.Security.Claims;
using AutoMapper;
using hrconnectbackend.Helper;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Models;
using hrconnectbackend.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace hrconnectbackend.Controllers.v1
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    [Route("api/v{version:apiVersion}/notification")]
    [ApiVersion("1.0")]
    public class NotificationController : ControllerBase
    {
        // private IDistributedCache _distributedCache;

    
        private readonly INotificationServices _notificationServices;
        private readonly IUserNotificationServices _userNotificationServices;
        private readonly IMapper _mapper;
        public NotificationController(INotificationServices notificationServices,  IMapper mapper, IUserNotificationServices userNotificationServices)
        {
            _notificationServices = notificationServices;
            _mapper = mapper;
            _userNotificationServices = userNotificationServices;
        }

        [Authorize]
        [HttpGet("my-notifications")]
        public async Task<IActionResult> RetrieveMyNotifications()
        {
            try
            {
                var employeeId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                var notifications = await _userNotificationServices.GetNotificationByUserId(employeeId);

                return Ok(new ApiResponse<List<ReadUserNotificationDTO>>(true, $"Notifications retrieved successfully.", _mapper.Map<List<ReadUserNotificationDTO>>(notifications)));
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse(false, $"Internal Server Error"));
            }
        }


        [HttpGet("{notificationId:int}")]
        public async Task<IActionResult> RetrieveNotification(int notificationId)
        {
            try
            {
                var notification = await _notificationServices.GetByIdAsync(notificationId);

                if (notification == null) return NotFound(new ApiResponse(false, $"Notification with id: {notificationId} does not exist."));

                return Ok(new ApiResponse<ReadNotificationsDTO>(true, $"Notification with id: {notificationId} retrieved successfully.", _mapper.Map<ReadNotificationsDTO>(notification)));
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse(false, $"Internal Server Error"));
            }
        }

        [HttpGet]
        public async Task<IActionResult> RetrieveNotifications(int? pageIndex, int? pageSize)
        {
            try
            {
                var notifications = await _notificationServices.GetAllAsync();

                var paginationaNotifications = _notificationServices.NotificationPagination(notifications, pageIndex, pageSize);

                return Ok(new ApiResponse<List<ReadNotificationsDTO>>(true, $"Notifications retrieved successfully.", _mapper.Map<List<ReadNotificationsDTO>>(paginationaNotifications)));
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse(false, $"Internal Server Error"));
            }
        }

        [HttpPut("{notificationId:int}")]
        public async Task<IActionResult> UpdateNotification(int notificationId, CreateNotificationDTO notificationDTO)
        {
            try
            {
                var notification = await _notificationServices.GetByIdAsync(notificationId);

                if (notification == null)
                {
                    return NotFound(new ApiResponse(false, $"Notification with id: {notificationId} does not exist."));
                }

                return Ok(new ApiResponse<ReadNotificationsDTO>(false, $"Notification with id: {notificationId} retrieved successfully.", _mapper.Map<ReadNotificationsDTO>(notification)));
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse(false, $"Internal Server Error"));
            }
        }

        [HttpGet("employee/{employeeId:int}")]
        public async Task<IActionResult> RetrieveNotificationByEmployee(int employeeId, int? pageIndex, int? pageSize)
        {
            List<UserNotification> employeeNotifications = await _notificationServices.GetNotificationsByEmployeeId(employeeId, pageIndex, pageSize);

            var mappedNotifications = _mapper.Map<List<ReadNotificationsDTO>>(employeeNotifications);

            try
            {
                return Ok(new ApiResponse<List<ReadNotificationsDTO>>(true, $"Notifications by employee with id: {employeeId} retrieved successfully.", mappedNotifications));
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return Ok(new ApiResponse<List<ReadNotificationsDTO>>(false, $"{ex.Message}. Thus, it returned the original", mappedNotifications));
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse(false, $"Internal Server Error"));
            }
        }

        [HttpDelete("{notificationId:int}")]
        public async Task<IActionResult> DeleteNotification(int notificationId)
        {
            try
            {
                var notification = await _notificationServices.GetByIdAsync(notificationId);

                if (notification == null)
                {
                    return NotFound(new ApiResponse(false, $"Notification with id: {notificationId} does not exist."));
                }

                await _notificationServices.DeleteAsync(notification);

                return Ok(new ApiResponse(true, $"Notification with id: {notificationId} deleted successfully"));
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse(false, $"Internal Server Error"));
            }
        }
    }
}
