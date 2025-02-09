using hrconnectbackend.Data;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Models;
using hrconnectbackend.Models.Enums;
using hrconnectbackend.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace hrconnectbackend.Repositories;

public class LeaveApplicationServices : GenericRepository<LeaveApplication>, ILeaveApplicationServices
{
    private readonly ILogger<UserAccount> _logger;
    private readonly IAttendanceServices _attendanceServices;

    public LeaveApplicationServices(DataContext context, ILogger<UserAccount> logger, IAttendanceServices attendanceServices) : base(context)
    {
        _logger = logger;
        _attendanceServices = attendanceServices;
    }

    private async Task<LeaveApplication> GetLeaveApplicationByIdAsync(int id)
    {
        var leaveApplication = await GetByIdAsync(id);
        if (leaveApplication == null)
        {
            _logger.LogWarning($"Leave application with ID {id} not found.");
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
        if (leaveApplication == null)
            throw new ArgumentNullException(nameof(leaveApplication), "Leave application cannot be null.");

        if (string.IsNullOrWhiteSpace(leaveApplication.Reason))
            throw new ArgumentException("Leave reason is required.");

        if (!IsValidLeaveType(leaveApplication.Type))
            throw new InvalidOperationException("Invalid leave type.");


        var employee = await GetByIdAsync(leaveApplication.EmployeeId);
        if (employee == null)
        {
            _logger.LogWarning($"Employee with ID {leaveApplication.EmployeeId} not found.");
            throw new ArgumentException("Employee not found.");
        }

        leaveApplication.Status = "Pending"; // Default status
        await AddAsync(leaveApplication);
        _logger.LogInformation($"Leave application created for Employee ID {leaveApplication.EmployeeId}");

        return leaveApplication;
    }

    public async Task ApproveLeave(int id)
    {
        var leaveApplication = await GetLeaveApplicationByIdAsync(id);
        using var transaction = await _context.Database.BeginTransactionAsync();

        if (leaveApplication == null)
        {
            _logger.LogWarning($"Employee with ID {id} not found.");
            throw new ArgumentException("Employee not found.");
        }

        leaveApplication.Status = "Approved";
        await UpdateAsync(leaveApplication);
        _logger.LogInformation($"Leave application ID {id} approved.");
    }

    public async Task RejectLeave(int id)
    {
        var leaveApplication = await GetLeaveApplicationByIdAsync(id);
        leaveApplication.Status = "Rejected";
        await UpdateAsync(leaveApplication);
        _logger.LogInformation($"Leave application ID {id} rejected.");
    }

    public async Task<List<LeaveApplication>> GetLeaveBySupervisor(int supervisorId)
    {
        var leaveApplication = await GetAllAsync();

        if (leaveApplication == null || leaveApplication.Count == 0)
        {
            _logger.LogWarning($"Leave application not found");
            throw new ArgumentException("Leave application not found.");
        }

        return leaveApplication.Where(l => l.SupervisorId == supervisorId).ToList();

    }

    public async Task<List<LeaveApplication>> GetLeavePagination(int page, int pageSize, int? employeeId)
    {
        var leaveApplication = new List<LeaveApplication>();

        if (pageSize <= 0)
        {
            _logger.LogWarning($"Page number must be greater than zero.");
            throw new ArgumentOutOfRangeException("Page number must be greater than zero.");
        }

        if (page <= 0)
        {
            _logger.LogWarning($"Size must be greater than zero.");
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
            _logger.LogWarning($"Employee not found.");
            throw new KeyNotFoundException($"No employee found with an id {employeeId}.");
        }

        if (leaveApplication.Count == 0)
        {
            _logger.LogWarning($"Leave application by employee: {employee.Id} not found.");
            throw new KeyNotFoundException($"No leave application found with an id {employeeId}.");
        }

        return leaveApplication;
    }
}
