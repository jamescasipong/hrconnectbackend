using hrconnectbackend.Models;

namespace hrconnectbackend.IRepositories;

public interface ILeaveApplicationRepositories : IGenericRepository<LeaveApplication>
{
    public Task<LeaveApplication> UpdateDate(int id, DateTime startDate, DateTime endDate);
    public Task<LeaveApplication> UpdateLeaveType(int id, string leaveType);
    public Task<LeaveApplication> UpdateLeaveReason(int id, string leaveReason);
}