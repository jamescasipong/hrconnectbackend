using hrconnectbackend.Data;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Models;
using hrconnectbackend.Repository;
using Microsoft.EntityFrameworkCore;

namespace hrconnectbackend.Repositories;

public class ShiftServices : GenericRepository<Shift>, IShiftServices
{
    public ShiftServices(DataContext context) : base(context)
    {

    }

    public async Task<List<Shift>> GetEmployeeShifts(int employeeId)
    {
        var employee = await _context.Employees.FindAsync(employeeId);

        if (employee == null)
        {
            throw new KeyNotFoundException($"No employee found with an id {employeeId}");
        }

        var employeeShifts = await _context.Shifts.Where(a => a.EmployeeShiftId == employeeId).ToListAsync();

        if (!employeeShifts.Any())
        {
            throw new KeyNotFoundException($"No shift found for an employee with an id {employeeId}");
        }

        return employeeShifts;
    }

    public async Task<bool> HasShiftToday(int employeeId)
    {

        var shifts = await GetAllAsync();

        if (shifts.Count == 0)
        {
            throw new KeyNotFoundException($"No shift today found for an employee with an id {employeeId}");
        }

        var shift = await _context.Shifts.Where(s => s.EmployeeShiftId == employeeId && s.DaysOfWorked.Contains(DateTime.Now.DayOfWeek.ToString())).FirstOrDefaultAsync();

        if (shift == null)
        {
            return false;
        }

        return true;
    }

}