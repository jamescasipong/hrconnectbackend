using hrconnectbackend.Data;
using hrconnectbackend.IRepositories;
using hrconnectbackend.Models;
using Microsoft.EntityFrameworkCore;


namespace hrconnectbackend.Repositories;

public class LeavesApprovalRepositories
{
    private readonly DataContext _context;

    public LeavesApprovalRepositories(DataContext context)
    {
        _context = context;
    }

    public async Task CreateLeaveApproval(LeaveApproval leaveApproval)
    {
        await _context.LeaveApprovals.AddAsync(leaveApproval);
        await _context.SaveChangesAsync();

    }

    public Task<LeaveApproval> DeleteLeaveApproval(int id)
    {
        throw new NotImplementedException();
    }

    public async Task<LeaveApproval> GetLeaveApproval(int id)
    {
        return await _context.LeaveApprovals.FirstOrDefaultAsync(e => e.LeaveApprovalId == id);
    }

    public async Task<ICollection<LeaveApplication>> GetLeaveApplicationsByAproverId(int id)
    {
        return await _context.LeaveApplications.Where(e => e.LeaveApproval.SupervisorId == id).ToListAsync();
    }

    public async Task<ICollection<LeaveApproval>> GetLeaveApprovals()
    {
        return await _context.LeaveApprovals.ToListAsync();
    }

    public async Task UpdateLeaveApproval(LeaveApproval leaveApproval)
    {
        _context.LeaveApprovals.Update(leaveApproval);
        await _context.SaveChangesAsync();
    }





}