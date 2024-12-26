using hrconnectbackend.Data;
using hrconnectbackend.IRepositories;
using hrconnectbackend.Models;
using Microsoft.EntityFrameworkCore;

namespace hrconnectbackend.Repositories;

public class SupervisorRepositories : GenericRepository<Supervisor>, ISupervisorRepositories
{

    public SupervisorRepositories(DataContext context) : base(context)
    {

    }

    public async Task<LeaveApplication> LeaveApprovalResponse(int id, string response)
    {
        var leave = await _context.LeaveApplications.FirstOrDefaultAsync(l => l.LeaveApplicationId == id);
        var leaveApproval = await _context.LeaveApprovals.FirstOrDefaultAsync(l => l.LeaveApplicationId == id);

        if (leave.Status != "Pending")
            return null;

        if (leave == null)
            return null;

        leave.Status = response;
        leaveApproval.Decision = response;

        await _context.SaveChangesAsync();

        return leave;
    }


    public async Task<List<Employee>> GetSuperVisorSubordinates(int id)
    {
        return await _context.Supervisors.Where(s => s.Id == id).SelectMany(s => s.Subordinates).ToListAsync();
    }

    public Task<LeaveApplication> GetLeaveApplicationById(int id)
    {
        return _context.LeaveApplications.FirstOrDefaultAsync(l => l.LeaveApplicationId == id);
    }

}