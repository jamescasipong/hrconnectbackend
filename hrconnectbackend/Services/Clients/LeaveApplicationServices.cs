using hrconnectbackend.Data;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Models;
using hrconnectbackend.Repository;
using Microsoft.EntityFrameworkCore;

namespace hrconnectbackend.Services.Clients;

public class LeaveApplicationServices(
    DataContext context,
    ILogger<UserAccount> logger,
    IAttendanceServices attendanceServices)
    : GenericRepository<LeaveApplication>(context), ILeaveApplicationServices
{
    private async Task<LeaveApplication> GetLeaveApplicationByIdAsync(int id)
    {
        var leaveApplication = await GetByIdAsync(id);
        if (leaveApplication == null)
        {
            logger.LogWarning($"Leave application with ID {id} not found.");
            throw new ArgumentException("Leave application not found.");
        }
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
        if (employee == null)
        {
            logger.LogWarning($"Employee with ID {leaveApplication.EmployeeId} not found.");
            throw new ArgumentException("Employee not found.");
        }

        leaveApplication.Status = "Pending"; // Default status
        await AddAsync(leaveApplication);
        logger.LogInformation($"Leave application created for Employee ID {leaveApplication.EmployeeId}");

        return leaveApplication;
    }

    public async Task ApproveLeave(int id)
    {
        var leaveApplication = await GetLeaveApplicationByIdAsync(id);
        using var transaction = await _context.Database.BeginTransactionAsync();

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
            throw new KeyNotFoundException($"Leave application with ID: {id} not found.");
        }

        leaveApplication.Status = "Rejected";
        await UpdateAsync(leaveApplication);
        logger.LogInformation($"Leave application ID {id} rejected.");
    }

    public async Task<List<LeaveApplication>> GetLeaveBySupervisor(int supervisorId)
    {
        var leaveApplication = await GetAllAsync();

        if (leaveApplication == null || leaveApplication.Count == 0)
        {
            logger.LogWarning($"Leave application not found");
            throw new ArgumentException("Leave application not found.");
        }

        return leaveApplication.Where(l => l.SupervisorId == supervisorId).ToList();

    }

    public async Task<List<LeaveApplication>> GetLeavePagination(int page, int pageSize, int? employeeId)
    {
        var leaveApplication = new List<LeaveApplication>();

        if (pageSize <= 0)
        {
            logger.LogWarning($"Page number must be greater than zero.");
            throw new ArgumentOutOfRangeException("Page number must be greater than zero.");
        }

        if (page <= 0)
        {
            logger.LogWarning($"Size must be greater than zero.");
            throw new ArgumentOutOfRangeException("Size must be greater than zero.");
        }

        if (employeeId == null)
        {
            leaveApplication = await _context.LeaveApplications.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        }
        else
        {
            leaveApplication = await _context.LeaveApplications.Where(l => l.EmployeeId == employeeId).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        }


        if (leaveApplication.Count == 0)
        {
            throw new ArgumentException("Leave application not found.");
        }

        return leaveApplication;
    }

    public async Task<List<LeaveApplication>> GetLeaveByEmployee(int employeeId)
    {
        var employee = await _context.Employees.FindAsync(employeeId);
        var leaveApplication = await _context.LeaveApplications.Where(l => l.EmployeeId == employeeId).ToListAsync();

        if (employee == null)
        {
            logger.LogWarning($"Leave application with ID: {employeeId} not found.");
            throw new KeyNotFoundException($"Leave application with ID: {employeeId} not found.");
        }

        if (!leaveApplication.Any())
        {
            logger.LogWarning($"Leave application by employee: {employee.Id} not found.");
            throw new KeyNotFoundException($"Leave application with ID: {employeeId} not found.");
        }

        return leaveApplication;
    }
}
