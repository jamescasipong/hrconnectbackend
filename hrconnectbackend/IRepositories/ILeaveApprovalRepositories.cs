using hrconnectbackend.Models;

namespace hrconnectbackend.IRepositories;

public interface ILeaveApprovalRepositories : IGenericRepository<LeaveApproval>
{
    Task ReponseLeave(int id, string response);
}