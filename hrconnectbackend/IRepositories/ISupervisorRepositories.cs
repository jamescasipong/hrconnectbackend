using hrconnectbackend.Models;

namespace hrconnectbackend.IRepositories;

public interface ISupervisorRepositories : IGenericRepository<Supervisor>
{
    Task<LeaveApplication> LeaveApprovalResponse(int id, string response);
    Task<LeaveApplication> GetLeaveApplicationById(int id);
}