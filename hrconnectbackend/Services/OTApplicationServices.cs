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

    public async Task ApproveOT(int id)
    {
        var otApplication = await GetByIdAsync(id);
        if (otApplication == null)
        {
            _logger.LogWarning($"OT Application with id: {id} not found.");
            throw new KeyNotFoundException($"OT Application with id: {id} not found.");
        }

        otApplication.Status = "Approved";
        await UpdateAsync(otApplication);
    }

    public async Task RejectOT(int id)
    {
        var otApplication = await GetByIdAsync(id);
        if (otApplication == null)
        {
            _logger.LogWarning($"OT Application with id: {id} not found.");
            throw new KeyNotFoundException($"OT Application with id: {id} not found.");
        }

        otApplication.Status = "Rejected";
        await UpdateAsync(otApplication);
        _logger.LogInformation($"OT Application with ID {id} has been rejected.");
    }


    public async Task<List<OTApplication>> GetOTByDate(DateOnly startDate, DateOnly endDate, int? pageIndex, int? pageSize)
    {

        var otApplications = await _context.OTApplications
            .Where(a => DateOnly.FromDateTime(a.Date) >= startDate && DateOnly.FromDateTime(a.Date) <= endDate)
            .ToListAsync();

        if (!otApplications.Any())
        {
            throw new KeyNotFoundException("OT Application not found.");
        }

        if (pageIndex != null && pageSize != null && otApplications.Count < pageSize)
        {
            return GetOTPagination(otApplications, pageIndex.Value, pageSize.Value);
        }

        return otApplications;
    }

    public async Task<List<OTApplication>> GetOTBySupervisor(int supervisorId, int? pageIndex, int? pageSize)
    {
        var otApplication = await GetAllAsync();

        if (otApplication == null || otApplication.Count == 0)
        {
            throw new KeyNotFoundException($"Unable to process. Supervisor with id: {supervisorId} not found.");
        }

        var supervisorOT = otApplication.Where(ot => ot.SupervisorId == supervisorId).ToList();

        if (pageIndex != null && pageSize != null && otApplication.Count < pageSize)
        {
            return GetOTPagination(supervisorOT, pageIndex.Value, pageSize.Value);
        }

        return supervisorOT;
    }

    public async Task<List<OTApplication>> GetOTByEmployee(int employeeId, int? pageIndex, int? pageSize)
    {
        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == employeeId);
        var otApplication = await _context.OTApplications.Where(ot => ot.EmployeeId == employeeId).ToListAsync();

        if (employee == null)
        {
            throw new KeyNotFoundException($"No employee found with an id {employeeId}");
        }

        if (pageIndex != null && pageSize != null && otApplication.Count < pageSize)
        {
            return GetOTPagination(otApplication, pageIndex.Value, pageSize.Value);
        }

        return otApplication;
    }

    public List<OTApplication> GetOTPagination(List<OTApplication> otApplication, int pageIndex, int pageSize)
    {
        if (pageSize < 0)
        {
            throw new ArgumentException($"Page index must be higher than 0");
        }

        if (pageIndex < 0)
        {
            throw new ArgumentException($"Page size must be higher than 0");
        }

        return otApplication.Take((pageIndex - 1) * pageSize).Take(pageSize).ToList();
    }
}
