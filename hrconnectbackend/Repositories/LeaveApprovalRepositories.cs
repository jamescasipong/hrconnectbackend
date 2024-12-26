using hrconnectbackend.Data;
using hrconnectbackend.IRepositories;
using hrconnectbackend.Models;
using Microsoft.EntityFrameworkCore;

namespace hrconnectbackend.Repositories;

public class LeaveApprovalRepositories : GenericRepository<LeaveApproval>, ILeaveApprovalRepositories
{
    public LeaveApprovalRepositories(DataContext context) : base(context)
    {

    }

    public async Task ReponseLeave(int id, string response)
    {
        var leave = await _context.LeaveApplications.FirstOrDefaultAsync(l => l.LeaveApplicationId == id);
        leave.Status = response;
        await _context.SaveChangesAsync();
    }
}