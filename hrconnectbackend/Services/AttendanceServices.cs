using System.Threading.Tasks;
using Amazon.Runtime.Internal.Util;
using hrconnectbackend.Data;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Models;
using hrconnectbackend.Repository;
using Microsoft.EntityFrameworkCore;

namespace hrconnectbackend.Repositories
{
    public class AttendanceServices : GenericRepository<Attendance>, IAttendanceServices
    {

        public AttendanceServices(DataContext _context) : base(_context)
        {

        }

        public async Task ClockIn(int employeeId)
        {
            var employee = await _context.Employees.FindAsync(employeeId);

            if (employee == null)
            {
                throw new KeyNotFoundException($"Employee with ID {employeeId} not found.");
            }

            var hasClockedIn = await HasClockedIn(employeeId);

            if (hasClockedIn)
            {
                throw new ArgumentException($"Employee with ID {employeeId} has already clocked in.");
            }

            var newAttendance = new Attendance
            {
                EmployeeId = employeeId,
                DateToday = DateOnly.FromDateTime(DateTime.Now),
                ClockIn = TimeOnly.FromDateTime(DateTime.Now).ToTimeSpan(),
                ClockOut = null,
            };

            await AddAsync(newAttendance);
        }

        public async Task ClockOut(int employeeId)
        {
            var employee = await _context.Employees.FindAsync(employeeId);
            var attendance = await _context.Attendances.FirstOrDefaultAsync(a => a.DateToday == DateOnly.FromDateTime(DateTime.Now) && a.EmployeeId == employeeId);

            if (employee == null)
            {
                throw new KeyNotFoundException($"Employee with ID {employeeId} not found.");
            }

            var hasClockedOut = await HasClockedIn(employeeId);

            if (attendance == null)
            {
                throw new KeyNotFoundException($"No attendance record found for employee ID {employeeId} on {DateTime.Now:yyyy-MM-dd}.");
            }

            if (hasClockedOut)
            {
                throw new ArgumentException($"Employee with ID {employeeId} has already clocked out.");
            }

            attendance.ClockOut = TimeOnly.FromDateTime(DateTime.Now).ToTimeSpan();
            await UpdateAsync(attendance);
        }

        public async Task<List<Attendance>> GetAttendanceByEmployeeId(int id)
        {
            var attendance = await _context.Attendances.Where(e => e.EmployeeId == id).ToListAsync();

            return attendance;
        }

        public async Task<Attendance> GetDailyAttendanceByEmployeeId(int employeeId)
        {
            var attendance = await _context.Attendances.FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.DateToday == DateOnly.FromDateTime(DateTime.Now));

            if (attendance == null)
            {
                throw new KeyNotFoundException($"No attendance records found for employee with ID {employeeId} today");
            }

            return attendance;
        }

        public async Task<List<Attendance>> GetMonthlyAttendanceByEmployeeId(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid employee ID", nameof(id));

            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            var attendanceRecords = await _context.Attendances
                .Where(a => a.EmployeeId == id && a.DateToday.Month == currentMonth && a.DateToday.Year == currentYear)
                .OrderBy(a => a.DateToday)
                .ToListAsync();

            if (!attendanceRecords.Any())
                throw new KeyNotFoundException($"No attendance records found for employee with ID {id} in the current month.");

            return attendanceRecords;
        }


        public async Task<List<Attendance>> GetRangeAttendanceByEmployeeId(int id, DateTime start, DateTime end)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid employee ID", nameof(id));

            if (start > end)
                throw new ArgumentException("Start date must be earlier than or equal to end date.");

            var attendanceRecords = await _context.Attendances
                .Where(a => a.EmployeeId == id && a.DateToday >= DateOnly.FromDateTime(start) && a.DateToday <= DateOnly.FromDateTime(end))
                .OrderBy(a => a.DateToday)
                .ToListAsync();

            if (!attendanceRecords.Any())
                throw new KeyNotFoundException($"No attendance records found for employee with ID {id} in the given date range.");

            return attendanceRecords;
        }



        // Conditional Methods
        private async Task<bool> HasClockedIn(int employeeId)
        {
            var attendance = await _context.Attendances.FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.DateToday == DateOnly.FromDateTime(DateTime.Now));

            return attendance != null;
        }

        private async Task<bool> HasClockedOut(int employeeId)
        {
            var attendance = await _context.Attendances.FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.DateToday == DateOnly.FromDateTime(DateTime.Now) && ClockOut != null);

            return attendance != null;
        }

    }
}
