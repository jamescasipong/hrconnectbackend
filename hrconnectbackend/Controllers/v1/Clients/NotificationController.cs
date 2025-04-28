using System.Security.Claims;
using AutoMapper;
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
            try
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
                
                var userNotifications = await userNotificationServices.AddAsync(usernotification);

                return Ok(new ApiResponse<ReadNotificationsDto?>(true, $"Notification created successfully.", mapper.Map<ReadNotificationsDto>(notification)));
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse(false, $"Internal Server Error"));
            }   
        }

        [Authorize]
        [HttpGet("my-notifications")]
        public async Task<IActionResult> RetrieveMyNotifications()
        {
            try
            {
                var employeeId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                var notifications = await userNotificationServices.GetNotificationByUserId(employeeId);

                return Ok(new ApiResponse<List<ReadUserNotificationDto>?>(true, $"Notifications retrieved successfully.", mapper.Map<List<ReadUserNotificationDto>>(notifications)));
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse(false, $"Internal Server Error"));
            }
        }

        [Authorize]
        [HttpPut("mark-as-read/{notificationId:int}")]
        public async Task<IActionResult> MarkAsRead(int notificationId){
            try {
                var notification = await userNotificationServices.GetUserNotificationById(notificationId);

                if (notification == null) return NotFound(new ApiResponse(false, $"User Notification with id: {notificationId} not found"));

                notification.IsRead = true;

                await userNotificationServices.UpdateAsync(notification);

                return Ok(new ApiResponse(true, $"User Notification with Id: {notificationId} marked as read successfully"));
            }
            catch (Exception ex){
                logger.LogError(ex.Message);
                return BadRequest(new ApiResponse(false, $"Internal Server Error"));
            }

        }

        [HttpGet("{notificationId:int}")]
        public async Task<IActionResult> RetrieveNotification(int notificationId)
        {
            try
            {
                var notification = await notificationServices.GetByIdAsync(notificationId);

                if (notification == null) return NotFound(new ApiResponse(false, $"Notification with id: {notificationId} does not exist."));

                return Ok(new ApiResponse<ReadNotificationsDto?>(true, $"Notification with id: {notificationId} retrieved successfully.", mapper.Map<ReadNotificationsDto>(notification)));
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
                var notifications = await notificationServices.GetAllAsync();

                var paginationaNotifications = notificationServices.NotificationPagination(notifications, pageIndex, pageSize);

                return Ok(new ApiResponse<List<ReadNotificationsDto>?>(true, $"Notifications retrieved successfully.", mapper.Map<List<ReadNotificationsDto>>(paginationaNotifications)));
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse(false, $"Internal Server Error"));
            }
        }

        [HttpPut("{notificationId:int}")]
        public async Task<IActionResult> UpdateNotification(int notificationId, CreateNotificationDto notificationDTO)
        {
            try
            {
                var notification = await notificationServices.GetByIdAsync(notificationId);

                if (notification == null)
                {
                    return NotFound(new ApiResponse(false, $"Notification with id: {notificationId} does not exist."));
                }

                return Ok(new ApiResponse<ReadNotificationsDto?>(false, $"Notification with id: {notificationId} retrieved successfully.", mapper.Map<ReadNotificationsDto>(notification)));
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse(false, $"Internal Server Error"));
            }
        }

        [HttpGet("employee/{employeeId:int}")]
        public async Task<IActionResult> RetrieveNotificationByEmployee(int employeeId, int? pageIndex, int? pageSize)
        {
            List<UserNotification> employeeNotifications = await notificationServices.GetNotificationsByEmployeeId(employeeId, pageIndex, pageSize);

            var mappedNotifications = mapper.Map<List<ReadNotificationsDto>>(employeeNotifications);

            try
            {
                return Ok(new ApiResponse<List<ReadNotificationsDto>?>(true, $"Notifications by employee with id: {employeeId} retrieved successfully.", mappedNotifications));
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return Ok(new ApiResponse<List<ReadNotificationsDto>?>(false, $"{ex.Message}. Thus, it returned the original", mappedNotifications));
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
                var notification = await notificationServices.GetByIdAsync(notificationId);

                if (notification == null)
                {
                    return NotFound(new ApiResponse(false, $"Notification with id: {notificationId} does not exist."));
                }

                await notificationServices.DeleteAsync(notification);

                return Ok(new ApiResponse(true, $"Notification with id: {notificationId} deleted successfully"));
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse(false, $"Internal Server Error"));
            }
        }
    }
}
