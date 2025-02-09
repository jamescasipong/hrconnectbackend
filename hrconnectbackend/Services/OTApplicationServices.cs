using Microsoft.Extensions.Logging;
using hrconnectbackend.Models;
using hrconnectbackend.Data;
using hrconnectbackend.Repository;
using hrconnectbackend.Interface.Services;
using Microsoft.EntityFrameworkCore;

public class OTApplicationServices : GenericRepository<OTApplication>, IOTApplicationServices
{
    private readonly ILogger<OTApplicationServices> _logger;

    public OTApplicationServices(DataContext context, ILogger<OTApplicationServices> logger) : base(context)
    {
        _logger = logger;
    }

    public async Task<OTApplication> RequestOT(OTApplication otRequest)
    {
        // Validate the input here and any business logic you need
        if (otRequest.StartDate == null && otRequest.EndTime == null && otRequest.StartDate == null)
        {
            _logger.LogWarning("Attempted to create OT application with invalid data.");
            throw new InvalidOperationException("Invalid OT application data.");
        }

        // Add to the database
        await AddAsync(otRequest);
        _logger.LogInformation($"OT application created for Employee ID: {otRequest.EmployeeId}");
        return otRequest;
    }

    public async Task ApproveOT(int id)
    {
        var otApplication = await GetByIdAsync(id);
        if (otApplication == null)
        {
            _logger.LogWarning($"OT application with ID {id} not found.");
            throw new ArgumentException("OT application not found.");
        }

        otApplication.Status = "Approved";
        await UpdateAsync(otApplication);
        _logger.LogInformation($"OT Application with ID {id} has been approved.");
    }

    public async Task RejectOT(int id)
    {
        var otApplication = await GetByIdAsync(id);
        if (otApplication == null)
        {
            _logger.LogWarning($"OT application with ID {id} not found.");
            throw new ArgumentException("OT application not found.");
        }

        otApplication.Status = "Rejected";
        await UpdateAsync(otApplication);
        _logger.LogInformation($"OT Application with ID {id} has been rejected.");
    }

    public async Task<List<OTApplication>> GetOTByDate(string startDate, string endDate)
    {
        var parsedStartDate = DateOnly.Parse(startDate);
        var parsedEndDate = DateOnly.Parse(endDate);

        var otApplications = await _context.OTApplications
            .Where(a => a.StartDate >= parsedStartDate && a.StartDate <= parsedEndDate)
            .ToListAsync();

        if (otApplications == null || otApplications.Count == 0)
        {
            _logger.LogWarning($"OT application with an interval date between {startDate} and {endDate} not found.");
            throw new ArgumentException("OT Application not found.");
        }

        return otApplications;
    }

    public async Task<List<OTApplication>> GetOTBySupervisor(int supervisorId)
    {
        var otApplication = await GetAllAsync();

        if (otApplication == null || otApplication.Count == 0)
        {
            _logger.LogWarning($"OT Application with supervisor {supervisorId} not found.");
            throw new ArgumentException("OT Application not found.");
        }

        return otApplication.Where(ot => ot.SupervisorId == supervisorId).ToList();
    }

    public async Task<List<OTApplication>> GetOTPagination(int pageIndex, int pageSize, int? employeeId)
    {
        var otApplication = new List<OTApplication>();

        if (pageIndex <= 0)
        {
            throw new ArgumentOutOfRangeException("Page index must be greater than 0");
        }

        if (pageSize <= 0)
        {
            throw new ArgumentOutOfRangeException("Page size must be greater than 0");
        }

        if (employeeId == null)
        {
            otApplication = await _context.OTApplications.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
        }
        else
        {
            var employeeOT = await _context.OTApplications.Where(ot => ot.EmployeeId == employeeId).ToListAsync();
            otApplication = employeeOT.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        }

        return otApplication;
    }

    public async Task<List<OTApplication>> GetOTByEmployee(int employeeId)
    {
        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == employeeId);
        var otApplication = await _context.OTApplications.Where(ot => ot.EmployeeId == employeeId).ToListAsync();

        if (employee == null)
        {
            throw new KeyNotFoundException($"No employee found with an id {employeeId}");
        }

        return otApplication;
    }
}
