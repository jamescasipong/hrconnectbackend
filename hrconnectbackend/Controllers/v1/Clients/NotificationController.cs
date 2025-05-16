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

            await userNotificationServices.AddAsync(usernotification);

            return Ok(new SuccessResponse("Notification created successfully."));

        }

        [Authorize(Roles = "Admin")]
        [HttpPost("assing-notification/{employeeId:int}")]
        public async Task<IActionResult> CreateUserNotification(int employeeId, CreateNotificationDto notificationDTO)
        {
            var orgId = User.RetrieveSpecificUser("OrganizationId");

            await notificationServices.CreateUserNotification(notificationDTO, int.Parse(orgId), employeeId);

            return Ok(new SuccessResponse($"Notification created successfully."));
        }

        [Authorize]
        [HttpGet("my-notifications")]
        public async Task<IActionResult> RetrieveMyNotifications()
        {

            var employeeId = User.RetrieveSpecificUser(ClaimTypes.NameIdentifier);

            var employeeIdInt = TypeConverter.StringToInt(employeeId);

            var notifications = await userNotificationServices.GetNotificationByUserId(employeeIdInt);

            return Ok(new SuccessResponse<List<ReadUserNotificationDto>?>(mapper.Map<List<ReadUserNotificationDto>>(notifications), $"Notifications retrieved successfully."));

        }

        [Authorize]
        [HttpPut("mark-as-read/{notificationId:int}")]
        public async Task<IActionResult> MarkAsRead(int notificationId)
        {

            var notification = await userNotificationServices.GetUserNotificationById(notificationId);

            notification.IsRead = true;

            await userNotificationServices.UpdateAsync(notification);

            return Ok(new SuccessResponse<ReadUserNotificationDto?>(mapper.Map<ReadUserNotificationDto>(notification), $"Notification with id: {notificationId} marked as read successfully."));
        }

        [HttpGet("{notificationId:int}")]
        public async Task<IActionResult> RetrieveNotification(int notificationId)
        {

            var notification = await notificationServices.GetByIdAsync(notificationId);

            if (notification == null) return StatusCode(404, new ErrorResponse(ErrorCodes.NotificationNotFound, $"Notification with id: {notificationId} does not exist."));

            return Ok(new SuccessResponse<ReadNotificationsDto?>(mapper.Map<ReadNotificationsDto>(notification), $"Notification with id: {notificationId} retrieved successfully."));

        }

        [HttpGet]
        public async Task<IActionResult> RetrieveNotifications(int? pageIndex, int? pageSize)
        {

            var notifications = await notificationServices.GetAllAsync();

            var paginationaNotifications = notificationServices.NotificationPagination(notifications, pageIndex, pageSize);

            return Ok(new SuccessResponse<List<ReadNotificationsDto>?>(mapper.Map<List<ReadNotificationsDto>>(paginationaNotifications), $"Notifications retrieved successfully."));

        }

        [HttpPut("{notificationId:int}")]
        public async Task<IActionResult> UpdateNotification(int notificationId, CreateNotificationDto notificationDTO)
        {

            var notification = await notificationServices.GetByIdAsync(notificationId);

            if (notification == null)
            {
                return NotFound(new ErrorResponse(ErrorCodes.NotificationNotFound, $"Notification with id: {notificationId} does not exist."));
            }

            return Ok(new SuccessResponse<ReadNotificationsDto?>(mapper.Map<ReadNotificationsDto>(notification), $"Notification with id: {notificationId} updated successfully."));

        }

        [HttpGet("employee/{employeeId:int}")]
        public async Task<IActionResult> RetrieveNotificationByEmployee(int employeeId, int? pageIndex, int? pageSize)
        {
            List<UserNotification> employeeNotifications = await notificationServices.GetNotificationsByEmployeeId(employeeId, pageIndex, pageSize);

            var mappedNotifications = mapper.Map<List<ReadNotificationsDto>>(employeeNotifications);

            return Ok(new SuccessResponse<List<ReadNotificationsDto>?>(mappedNotifications, $"Notifications retrieved successfully."));

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

            return Ok(new SuccessResponse($"Notification with id: {notificationId} deleted successfully."));
        }
    }
}
