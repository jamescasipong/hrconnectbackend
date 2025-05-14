using System.Security.Claims;
using AutoMapper;
using hrconnectbackend.Constants;
using hrconnectbackend.Extensions;
using hrconnectbackend.Helper;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Interface.Services.Clients;
using hrconnectbackend.Models;
using hrconnectbackend.Models.DTOs;
using hrconnectbackend.Models.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hrconnectbackend.Controllers.v1.Clients
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    [Route("api/v{version:apiVersion}/notification")]
    [ApiVersion("1.0")]
    public class NotificationController(
        INotificationServices notificationServices,
        ILogger<NotificationController> logger,
        IMapper mapper,
        IUserNotificationServices userNotificationServices)
        : ControllerBase
    {
        // private IDistributedCache _distributedCache;


        [Authorize(Roles = "Admin")]
        [HttpPost("{employeeId:int}")]
        public async Task<IActionResult> CreateNotification(int employeeId, CreateNotificationDto notificationDTO)
        {
            try
            {
                var notification = mapper.Map<Notifications>(notificationDTO);

                var newNotification = await notificationServices.AddAsync(notification);

                var createUserNotificationDTO = new CreateUserNotificationDto
                {
                    EmployeeId = employeeId,
                    NotificationId = newNotification.Id,
                    IsRead = false,
                    Status = "Unread"
                };

                var usernotification = mapper.Map<UserNotification>(createUserNotificationDTO);

                var userNotifications = await userNotificationServices.AddAsync(usernotification);

                return Ok(new ApiResponse<ReadNotificationsDto?>(true, $"Notification created successfully.", mapper.Map<ReadNotificationsDto>(newNotification)));
            }
            catch (Exception ex)
            {
                logger.LogError($"Error in {nameof(CreateNotification)}: " + ex.Message);
                return StatusCode(500, new ApiResponse(false, $"Internal Server Error"));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("assing-notification/{employeeId:int}")]
        public async Task<IActionResult> CreateNotification(int employeeId, int notificationId, CreateNotificationDto notificationDTO)
        {

            var notification = mapper.Map<Notifications>(notificationDTO);

            await notificationServices.AddAsync(notification);

            var createUserNotificationDTO = new CreateUserNotificationDto
            {
                EmployeeId = employeeId,
                NotificationId = notification.Id,
                IsRead = false,
                Status = "Unread"
            };

            var usernotification = mapper.Map<UserNotification>(createUserNotificationDTO);

            await userNotificationServices.AddAsync(usernotification);

            return Ok(new ApiResponse<ReadNotificationsDto?>(true, $"Notification created successfully.", mapper.Map<ReadNotificationsDto>(notification)));

        }

        [Authorize]
        [HttpGet("my-notifications")]
        public async Task<IActionResult> RetrieveMyNotifications()
        {

            var employeeId = User.RetrieveSpecificUser(ClaimTypes.NameIdentifier);

            var employeeIdInt = TypeConverter.StringToInt(employeeId);

            var notifications = await userNotificationServices.GetNotificationByUserId(employeeIdInt);

            return Ok(new ApiResponse<List<ReadUserNotificationDto>?>(true, $"Notifications retrieved successfully.", mapper.Map<List<ReadUserNotificationDto>>(notifications)));

        }

        [Authorize]
        [HttpPut("mark-as-read/{notificationId:int}")]
        public async Task<IActionResult> MarkAsRead(int notificationId)
        {

            var notification = await userNotificationServices.GetUserNotificationById(notificationId);

            notification.IsRead = true;

            await userNotificationServices.UpdateAsync(notification);

            return Ok(new ApiResponse<ReadUserNotificationDto?>(true, $"User Notification with id: {notificationId} updated successfully.", mapper.Map<ReadUserNotificationDto>(notification)));
        }

        [HttpGet("{notificationId:int}")]
        public async Task<IActionResult> RetrieveNotification(int notificationId)
        {

            var notification = await notificationServices.GetByIdAsync(notificationId);

            if (notification == null) return StatusCode(404, new ErrorResponse(ErrorCodes.NotificationNotFound, $"Notification with id: {notificationId} does not exist."));

            return Ok(new ApiResponse<ReadNotificationsDto?>(true, $"Notification with id: {notificationId} retrieved successfully.", mapper.Map<ReadNotificationsDto>(notification)));

        }

        [HttpGet]
        public async Task<IActionResult> RetrieveNotifications(int? pageIndex, int? pageSize)
        {

            var notifications = await notificationServices.GetAllAsync();

            var paginationaNotifications = notificationServices.NotificationPagination(notifications, pageIndex, pageSize);

            return Ok(new ApiResponse<List<ReadNotificationsDto>?>(true, $"Notifications retrieved successfully.", mapper.Map<List<ReadNotificationsDto>>(paginationaNotifications)));

        }

        [HttpPut("{notificationId:int}")]
        public async Task<IActionResult> UpdateNotification(int notificationId, CreateNotificationDto notificationDTO)
        {

            var notification = await notificationServices.GetByIdAsync(notificationId);

            if (notification == null)
            {
                return NotFound(new ErrorResponse(ErrorCodes.NotificationNotFound, $"Notification with id: {notificationId} does not exist."));
            }

            return Ok(new ApiResponse<ReadNotificationsDto?>(false, $"Notification with id: {notificationId} retrieved successfully.", mapper.Map<ReadNotificationsDto>(notification)));

        }

        [HttpGet("employee/{employeeId:int}")]
        public async Task<IActionResult> RetrieveNotificationByEmployee(int employeeId, int? pageIndex, int? pageSize)
        {
            List<UserNotification> employeeNotifications = await notificationServices.GetNotificationsByEmployeeId(employeeId, pageIndex, pageSize);

            var mappedNotifications = mapper.Map<List<ReadNotificationsDto>>(employeeNotifications);

            return Ok(new ApiResponse<List<ReadNotificationsDto>?>(true, $"Notifications by employee with id: {employeeId} retrieved successfully.", mappedNotifications));

        }

        [HttpDelete("{notificationId:int}")]
        public async Task<IActionResult> DeleteNotification(int notificationId)
        {

            var notification = await notificationServices.GetByIdAsync(notificationId);

            if (notification == null)
            {
                return StatusCode(404, new ErrorResponse(ErrorCodes.NotificationNotFound, $"Notification with id: {notificationId} does not exist."));
            }

            await notificationServices.DeleteAsync(notification);

            return Ok(new ApiResponse(true, $"Notification with id: {notificationId} deleted successfully"));


        }
    }
}
