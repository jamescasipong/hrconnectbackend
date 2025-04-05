using hrconnectbackend.Data;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Models;
using hrconnectbackend.Repository;
using Microsoft.EntityFrameworkCore;

namespace hrconnectbackend.Services.Clients
{
    public class LeaveBalanceServices(DataContext context)
        : GenericRepository<LeaveBalance>(context), ILeaveBalanceServices
    {
        public async Task<List<LeaveBalance>> GetLeaveBalanceByEmployeeId(int employeeId)
        {
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == employeeId);
            var leaveBalances = await _context.LeaveBalances.Where(l => l.EmployeeId == employeeId).ToListAsync();

            if (employee == null)
            {
                throw new KeyNotFoundException($"Employee with ID: {employeeId} not found.");
            }

            return leaveBalances;
        }

        public async Task AddOrUpdateLeaveBalance(LeaveBalance leaveBalance)
        {

            var existingBalance = await _context.LeaveBalances
                .FirstOrDefaultAsync(l => l.EmployeeId == leaveBalance.EmployeeId && l.LeaveType == leaveBalance.LeaveType);

            if (existingBalance != null)
            {
                existingBalance.TotalLeaves = leaveBalance.TotalLeaves;
                existingBalance.UsedLeaves = leaveBalance.UsedLeaves;
            }
            else
            {
                _context.LeaveBalances.Add(leaveBalance);
            }

            await _context.SaveChangesAsync();        }
    }
}
