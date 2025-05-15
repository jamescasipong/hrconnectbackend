using hrconnectbackend.Interface.Repositories;
using hrconnectbackend.Models;
using hrconnectbackend.Models.DTOs;

namespace hrconnectbackend.Interface.Services;

public interface INotificationServices : IGenericRepository<Notifications>
{
    Task<List<UserNotification>> GetNotificationsByEmployeeId(int id, int? pageIndex, int? pageSize);
    List<UserNotification> UserNotificationsPagination(List<UserNotification> notifications, int? pageIndex, int? pageSize);
    List<Notifications> NotificationPagination(List<Notifications> notifications, int? pageIndex, int? pageSize);
    Task CreateUserNotification(CreateNotificationDto createNotificationDto, int organizationId, int employeeId);
}