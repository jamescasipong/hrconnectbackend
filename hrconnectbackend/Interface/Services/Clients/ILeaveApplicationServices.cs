using hrconnectbackend.Interface.Repositories;
using hrconnectbackend.Models;

namespace hrconnectbackend.Interface.Services;

public interface ILeaveApplicationServices : IGenericRepository<LeaveApplication>
{
    Task RejectLeave(int id);
    Task ApproveLeave(int id);
    Task<List<LeaveApplication>> GetLeaveBySupervisor(int supervisorId);
    Task<List<LeaveApplication>> GetLeaveByEmployee(int employeeId);
    Task<LeaveApplication> RequestLeave(LeaveApplication leaveApplication);
    Task<List<LeaveApplication>> GetLeavePagination(int page, int pageSize, int? employeeId);
}