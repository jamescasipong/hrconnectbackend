using System.Threading.Tasks;
using Amazon.Runtime.Internal.Util;
using hrconnectbackend.Data;
using hrconnectbackend.IRepositories;
using hrconnectbackend.Models;
using Microsoft.EntityFrameworkCore;

namespace hrconnectbackend.Repositories
{
    public class AttendanceRepositories : GenericRepository<Attendance>, IAttendanceRepositories
    {

        public AttendanceRepositories(DataContext _context) : base(_context)
        {

        }

        public Task ApproveAttendance(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<string> ClockIn(int id)
        {
            if (_context.Employees.Where(e => e.Id == id).FirstOrDefault() == null)
            {
                return "Employee not found!";
            }


            if (GetDailyAttendanceByEmployeeId(id).Result == null)
            {
                var employee = await _context.Attendances.Where(a => a.EmployeeId == id).FirstOrDefaultAsync();

                var attendance = new Attendance
                {
                    EmployeeId = id,
                    DateToday = DateOnly.FromDateTime(DateTime.Now),
                    ClockIn = new TimeOnly(DateTime.Now.Hour, DateTime.Now.Minute)
                };

                AddAsync(attendance);

                return "Clocked in successfully!";
            }

            return "Already clocked in!";
        }

        public async Task<string> ClockOut(int id)
        {
            if (_context.Employees.Where(e => e.Id == id).FirstOrDefault() == null)
            {
                return "Employee not found!";
            }


            if (GetDailyAttendanceByEmployeeId(id).Result == null)
            {
                return "You have not clocked in yet!";
            }
            else
            {
                if (GetDailyAttendanceByEmployeeId(id).Result.ClockOut != null)
                {
                    return "You have already clocked out!";
                }

                var employee = await _context.Attendances.Where(a => a.EmployeeId == id).FirstOrDefaultAsync();

                employee.ClockOut = new TimeOnly(DateTime.Now.Hour, DateTime.Now.Minute);

                UpdateAsync(employee);

                return "Clocked out successfully!";
            }
        }

        public Task FileAttendance(int id, DateOnly date)
        {
            var employee = GetByIdAsync(id).Result;

            employee.DateToday = date;

            return UpdateAsync(employee);
        }

        public async Task<Attendance> GetDailyAttendanceByEmployeeId(int id)
        {
            var attendances = await _context.Attendances.ToListAsync();

            var dailyAttendance = attendances.Where(a => a.EmployeeId == id && a.DateToday == DateOnly.FromDateTime(DateTime.Now)).FirstOrDefault();

            return dailyAttendance;
        }



        public async Task<ICollection<Attendance>> GetMonthlyAttendanceByEmployeeId(int id)
        {
            var attendances = await _context.Attendances.ToListAsync();

            var monthlyAttendances = attendances.Where(a => a.EmployeeId == id && a.DateToday.Month == DateTime.Now.Month).ToList();

            return monthlyAttendances;
        }


        public async Task<ICollection<Attendance>> GetRangeAttendanceByEmployeeId(int id, DateOnly start, DateOnly end)
        {
            var attendances = await _context.Attendances.ToListAsync();

            var rangeAttendances = attendances
            .Where(a => a.EmployeeId == id && a.DateToday >= start && a.DateToday <= end).ToList();

            return rangeAttendances;
        }

        public Task RejectAttendance(int id)
        {
            throw new NotImplementedException();
        }
    }
}
