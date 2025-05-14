using hrconnectbackend.Constants;
using hrconnectbackend.Data;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Models;
using hrconnectbackend.Repository;
using Microsoft.EntityFrameworkCore;

namespace hrconnectbackend.Services.Clients;

public class ShiftServices(DataContext context) : GenericRepository<Shift>(context), IShiftServices
{
    public async Task<List<Shift>> GetEmployeeShifts(int employeeId, int orgId)
    {
        var employee = await _context.Employees.FindAsync(employeeId);


        if (employee == null)
        {
            throw new KeyNotFoundException($"No employee found with an id {employeeId}");
        }

        if (employee.OrganizationId != orgId)
        {
            throw new UnauthorizedAccessException($"Employee with id {employeeId} does not belong to the organization with id {orgId}");
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

        var shiftsForEmployee = await _context.Shifts.Where(s => s.EmployeeShiftId == employeeId).ToListAsync();
        var shift = shiftsForEmployee.FirstOrDefault(s => s.DaysOfWorked.Contains(DateTime.Now.DayOfWeek.ToString()));

        if (shift == null)
        {
            return false;
        }

        return true;
    }

    public async Task<List<Shift>> GenerateShiftForEmployee(int employeeId, int orgId)
    {
        List<string> employeeShifts = DaysOfWorked.DaysOfWorkedList;

        var employee = await _context.Employees.FindAsync(employeeId);

        if (employee == null)
        {
            throw new KeyNotFoundException($"No employee found with an id {employeeId}");
        }

        List<Shift> shifts = new List<Shift>();

        foreach (var empShift in employeeShifts)
        {
            var shift = new Shift
            {
                EmployeeShiftId = employeeId,
                DaysOfWorked = empShift,
                OrganizationId = orgId,
                TimeIn = new TimeSpan(9, 0, 0),
                TimeOut = new TimeSpan(17, 0, 0),
            };

            shifts.Add(shift);
        }

        try
        {

            await _context.Shifts.AddRangeAsync(shifts);
            await _context.SaveChangesAsync();


            return shifts;
        }

        catch (Exception ex)
        {
            throw new Exception($"Error creating shift: {ex.Message}", ex);
        }

    }

}