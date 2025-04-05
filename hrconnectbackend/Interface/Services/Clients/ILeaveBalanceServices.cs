using hrconnectbackend.Interface.Repositories;
using hrconnectbackend.Models;

namespace hrconnectbackend.Interface.Services
{
    public interface ILeaveBalanceServices: IGenericRepository<LeaveBalance>
    {
        Task<List<LeaveBalance>> GetLeaveBalanceByEmployeeId(int employeeId);
        Task AddOrUpdateLeaveBalance(LeaveBalance leaveBalance);
    }
}
