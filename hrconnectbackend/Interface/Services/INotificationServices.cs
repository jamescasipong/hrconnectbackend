using hrconnectbackend.Interface.Repositories;
using hrconnectbackend.Models;

namespace hrconnectbackend.Interface.Services;

public interface INotificationServices : IGenericRepository<Notifications>
{
    Task<List<Notifications>> GetNotificationsByEmployeeId(int id, int? pageIndex, int? pageSize);
    List<Notifications> NotifcationPagination(List<Notifications> notifications, int? pageIndex, int? pageSize);
}