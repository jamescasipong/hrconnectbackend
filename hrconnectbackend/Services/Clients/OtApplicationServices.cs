using hrconnectbackend.Models;
using hrconnectbackend.Data;
using hrconnectbackend.Repository;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Models.Requests;
using Microsoft.EntityFrameworkCore;

namespace hrconnectbackend.Services.Clients;

public class OtApplicationServices(DataContext context, ILogger<OtApplicationServices> logger)
    : GenericRepository<OtApplication>(context), IOTApplicationServices
{
    public async Task ApproveOT(int id)
    {
        var otApplication = await GetByIdAsync(id);
        if (otApplication == null)
        {
            logger.LogWarning($"OT Application with id: {id} not found.");
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
            logger.LogWarning($"OT Application with id: {id} not found.");
            throw new KeyNotFoundException($"OT Application with id: {id} not found.");
        }

        otApplication.Status = "Rejected";
        await UpdateAsync(otApplication);
        logger.LogInformation($"OT Application with ID {id} has been rejected.");
    }


    public async Task<List<OtApplication>> GetOTByDate(DateOnly startDate, DateOnly endDate, int? pageIndex, int? pageSize)
    {

        var otApplications = await _context.OtApplications
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

    public async Task<List<OtApplication>> GetOTBySupervisor(int supervisorId, int? pageIndex, int? pageSize)
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

    public async Task<List<OtApplication>> GetOTByEmployee(int employeeId, int? pageIndex, int? pageSize)
    {
        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == employeeId);
        var otApplication = await _context.OtApplications.Where(ot => ot.EmployeeId == employeeId).ToListAsync();

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

    public List<OtApplication> GetOTPagination(List<OtApplication> otApplication, int pageIndex, int pageSize)
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
