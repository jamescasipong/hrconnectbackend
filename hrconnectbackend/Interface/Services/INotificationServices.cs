using hrconnectbackend.Interface.Repositories;
using hrconnectbackend.Models;

namespace hrconnectbackend.Interface.Services;

public interface INotificationServices : IGenericRepository<Notifications>
{
    Task<List<Notifications>> GetNotificationsByEmployeeId(int id);
    Task<List<Notifications>> NotifcationPagination(int pageIndex, int pageSize, int? employeeId);
}