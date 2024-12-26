using hrconnectbackend.Data;
using hrconnectbackend.IRepositories;
using hrconnectbackend.Models;
using hrconnectbackend.Models.DTOs.Shifts;
using Microsoft.EntityFrameworkCore;

namespace hrconnectbackend.Repositories;

public class ShiftRepositories : GenericRepository<Shift>, IShiftRepositories
{
    public ShiftRepositories(DataContext context) : base(context)
    {

    }

    public async Task<string> AddShiftToEmployee(int employeeId, CreateShiftDTO shiftDTO)
    {
        var shift = new Shift
        {
            EmployeeShiftId = employeeId,
            DaysOfWorked = shiftDTO.DaysOfWorked,
            TimeIn = TimeOnly.Parse(shiftDTO.TimeIn),
            TimeOut = TimeOnly.Parse(shiftDTO.TimeOut)
        };

        await _context.Shifts.AddAsync(shift);
        await _context.SaveChangesAsync();

        return "Shift added successfully";
    }

    public Task<bool> CheckIfEmployeeHasShift(int employeeId, DateTime date)
    {
        throw new NotImplementedException();
    }

    public async Task<ICollection<Shift>> GetEmployeeShiftsById(int employeeId)
    {
        var shifts = await _context.Shifts.Where(s => s.EmployeeShiftId == employeeId).ToListAsync();

        return shifts;

    }

    public async Task<bool> HasShiftToday(int employeeId)
    {
        var shift = await _context.Shifts.Where(s => s.EmployeeShiftId == employeeId && s.DaysOfWorked.Contains(DateTime.Now.DayOfWeek.ToString())).FirstOrDefaultAsync();

        if (shift == null)
        {
            return false;
        }

        return true;
    }
    public Task RemoveShiftFromEmployee(int employeeId, int shiftId)
    {
        throw new NotImplementedException();
    }
}