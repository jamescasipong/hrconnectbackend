using System.Linq.Expressions;
using hrconnectbackend.Constants;
using hrconnectbackend.Data;
using hrconnectbackend.Exceptions;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Interface.Services.Clients;
using hrconnectbackend.Models;
using hrconnectbackend.Repository;
using Microsoft.EntityFrameworkCore;

namespace hrconnectbackend.Services.Clients;

public class LeaveApplicationServices(
    DataContext context,
    ILogger<UserAccount> logger,
    IPaginatedService<LeaveApplication> paginatedService)
    : GenericRepository<LeaveApplication>(context), ILeaveApplicationServices
{
    private async Task<LeaveApplication> GetLeaveApplicationByIdAsync(int id)
    {
        var leaveApplication = await GetByIdAsync(id);

        return leaveApplication;
    }

    private bool IsValidLeaveType(string leaveType)
    {
        var validLeaveTypes = new HashSet<string> { "Annual", "Sick", "Maternity", "Paternity" };
        return validLeaveTypes.Contains(leaveType);
    }

    public async Task<LeaveApplication> RequestLeave(LeaveApplication leaveApplication)
    {
        var employee = await GetByIdAsync(leaveApplication.EmployeeId);

        leaveApplication.Status = "Pending"; // Default status
        await AddAsync(leaveApplication);
        logger.LogInformation($"Leave application created for Employee ID {leaveApplication.EmployeeId}");

        return leaveApplication;
    }

    public async Task ApproveLeave(int id)
    {
        var leaveApplication = await GetLeaveApplicationByIdAsync(id);

        if (leaveApplication == null)
        {
            throw new KeyNotFoundException($"Leave application with ID: {id} not found.");
        }

        leaveApplication.Status = "Approved";
        await UpdateAsync(leaveApplication);

        logger.LogInformation($"Leave application ID {id} approved.");
    }

    public async Task RejectLeave(int id)
    {
        var leaveApplication = await GetLeaveApplicationByIdAsync(id);

        if (leaveApplication == null)
        {
            throw new NotFoundException(ErrorCodes.LeaveNotFound, $"Leave application with ID: {id} not found.");
        }

        leaveApplication.Status = "Rejected";
        await UpdateAsync(leaveApplication);
        logger.LogInformation($"Leave application ID {id} rejected.");
    }

    public async Task<PagedResponse<IEnumerable<LeaveApplication>>> GetLeaveBySupervisor(int supervisorId, PaginationParams paginationParams, string? searchTerm = null)
    {
        Expression<Func<LeaveApplication, bool>> filter = l => l.SupervisorId == supervisorId &&
            (string.IsNullOrEmpty(searchTerm) || l.Reason.Contains(searchTerm));

        var paginatedResponse = await paginatedService.GetPaginatedAsync(
            paginationParams,
            filter,
            orderBy: q => q.OrderBy(l => l.AppliedDate));

        return paginatedResponse;
    }

    public async Task<PagedResponse<IEnumerable<LeaveApplication>>> GetLeaveByEmployee(int employeeId, PaginationParams paginationParams, string? searchTerm = null)
    {
        Expression<Func<LeaveApplication, bool>> filter = l => l.EmployeeId == employeeId &&
            (string.IsNullOrEmpty(searchTerm) || l.Reason.Contains(searchTerm));

        var paginatedResponse = await paginatedService.GetPaginatedAsync(
            paginationParams,
            filter,
            orderBy: q => q.OrderBy(l => l.AppliedDate));

        return paginatedResponse;
    }

    public async Task<PagedResponse<IEnumerable<LeaveApplication>>> GetLeaveByOrganization(int organizationId, PaginationParams paginationParams, string searchTerm)
    {
        Expression<Func<LeaveApplication, bool>> filter = l => l.OrganizationId == organizationId &&
            (string.IsNullOrEmpty(searchTerm) || l.Reason.Contains(searchTerm));

        var paginatedResponse = await paginatedService.GetPaginatedAsync(
            paginationParams,
            filter,
            orderBy: q => q.OrderBy(l => l.AppliedDate));

        return paginatedResponse;
    }

    public async Task<PagedResponse<IEnumerable<LeaveApplication>>> GetLeaveByDepartment(int employeeDepartmentId, PaginationParams paginationParams, string searchTerm)
    {
        Expression<Func<LeaveApplication, bool>> filter = l => l.Employee!.EmployeeDepartmentId == employeeDepartmentId &&
            (string.IsNullOrEmpty(searchTerm) || l.Reason.Contains(searchTerm));

        var paginatedResponse = await paginatedService.GetPaginatedAsync(
            paginationParams,
            filter,
            orderBy: q => q.OrderBy(l => l.AppliedDate));

        return paginatedResponse;
    }
}
