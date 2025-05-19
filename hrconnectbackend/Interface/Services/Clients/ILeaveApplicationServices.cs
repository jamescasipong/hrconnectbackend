using hrconnectbackend.Interface.Repositories;
using hrconnectbackend.Models;

namespace hrconnectbackend.Interface.Services;

public interface ILeaveApplicationServices : IGenericRepository<LeaveApplication>
{
    Task RejectLeave(int id);
    Task ApproveLeave(int id);
    Task<PagedResponse<IEnumerable<LeaveApplication>>> GetLeaveByOrganization(int organizationId, PaginationParams paginationParams, string searchTerm);
    Task<PagedResponse<IEnumerable<LeaveApplication>>> GetLeaveByDepartment(int departmentId, PaginationParams paginationParams, string searchTerm);
    Task<PagedResponse<IEnumerable<LeaveApplication>>> GetLeaveBySupervisor(int supervisorId, PaginationParams paginationParams, string searchTerm);
    Task<PagedResponse<IEnumerable<LeaveApplication>>> GetLeaveByEmployee(int employeeId, PaginationParams paginationParams, string searchTerm);
    Task<LeaveApplication> RequestLeave(LeaveApplication leaveApplication);
}