﻿using System.Linq;
using System.Threading.Tasks;
using Amazon.Runtime.Internal.Util;
using AutoMapper;
using hrconnectbackend.Data;
using hrconnectbackend.Helper.CustomExceptions;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Models;
using hrconnectbackend.Models.DTOs;
using hrconnectbackend.Repository;
using Microsoft.EntityFrameworkCore;

namespace hrconnectbackend.Repositories
{
    public class AttendanceServices : GenericRepository<Attendance>, IAttendanceServices
    {
        private readonly IMapper _mapper;

        public AttendanceServices(DataContext _context, IMapper mapper) : base(_context)
        {
            _mapper = mapper;
        }

        public async Task ClockIn(int employeeId)
        {
            var employee = await _context.Employees.FindAsync(employeeId);
            var dayToday = DateTime.Now;

            // Convert clock-out time to TimeSpan
            var timeClockedIn = dayToday.TimeOfDay; // This gives us the TimeSpan of the current time

            var employeeShift = await _context.Shifts.Where(e => e.EmployeeShiftId == employeeId).ToListAsync();
            var hasShift = employeeShift.Any(shift => dayToday.ToString("dddd") == shift.DaysOfWorked);

            if (employee == null)
            {
                throw new KeyNotFoundException($"Employee with id: {employeeId} not found.");
            }

            if (!hasShift)
            {
                throw new InvalidOperationException($"Employee with ID {employeeId} has no shift today. Cannot clock in.");
            }

            var hasClockedIn = await HasClockedIn(employeeId);

            if (hasClockedIn)
            {
                throw new ConflictException($"Employee with ID {employeeId} has already clocked in.");
            }

            TimeSpan lateLeave = TimeSpan.Zero;

            foreach (var shift in employeeShift)
            {
                if (dayToday.ToString("dddd") == shift.DaysOfWorked)
                {
                    // Early leave calculation
                    if (shift.TimeIn < timeClockedIn)
                    {
                        lateLeave = timeClockedIn - shift.TimeIn;
                    }
                }
            }

            var newAttendance = new Attendance
            {
                EmployeeId = employeeId,
                DateToday = DateTime.Now,
                ClockIn = timeClockedIn,
                ClockOut = null,
                LateClockIn = lateLeave,
            };

            await AddAsync(newAttendance);
        }

        public async Task ClockOut(int employeeId)
        {
            var employee = await _context.Employees.FindAsync(employeeId);
            var attendance = await _context.Attendances.FirstOrDefaultAsync(a => DateOnly.FromDateTime(a.DateToday) == DateOnly.FromDateTime(DateTime.Now) && a.EmployeeId == employeeId);
            var employeeShift = await _context.Shifts.Where(e => e.EmployeeShiftId == employeeId).ToListAsync();

            if (attendance == null)
            {
                throw new KeyNotFoundException($"No attendance record found for employee ID {employeeId} on {DateTime.Now:yyyy-MM-dd}. No clock-in yet.");
            }

            if (employee == null)
            {
                throw new KeyNotFoundException($"Employee with ID {employeeId} not found.");
            }

            var dayToday = DateTime.Now;
            var clockIn = attendance.ClockIn;
            var hasShift = employeeShift.Any(shift => dayToday.ToString("dddd") == shift.DaysOfWorked);


            if (!hasShift)
            {
                throw new InvalidOperationException($"Employee with ID {employeeId} has no shift today. Cannot clock in.");
            }

            // Convert clock-out time to TimeSpan
            var timeClockedOut = dayToday.TimeOfDay; // This gives us the TimeSpan of the current time

            // Calculate working hours as the difference between clock-in and clock-out
            attendance.ClockOut = timeClockedOut;
            attendance.WorkingHours = (decimal)(timeClockedOut - clockIn).TotalHours;

            var hasClockedIn = await HasClockedIn(employeeId);

            if (!hasClockedIn)
            {
                throw new ArgumentException($"Employee with ID: {employeeId} has not clocked in yet.");
            }

            var hasClockedOut = await HasClockedOut(employeeId);


            if (hasClockedOut)
            {
                throw new ArgumentException($"Employee with ID {employeeId} has already clocked out.");
            }


            foreach (var shift in employeeShift)
            {

                if (dayToday.ToString("dddd") == shift.DaysOfWorked)
                {
                    // Early leave calculation
                    if (shift.TimeOut > timeClockedOut)
                    {
                        attendance.EarlyLeave = shift.TimeOut - timeClockedOut;
                    }
                    else
                    {
                        attendance.EarlyLeave = TimeSpan.Zero;
                    }

                }
            }

            await UpdateAsync(attendance);
        }

        //Return Late and by Dept

        //Return Present and by Dept

        //Return NotClockedIn and by Dept

        //Return Late and by Dept

        public async Task<dynamic> EmployeeAttendanceStatsByDeptSpecificOrToday(int departmentId, DateTime? specificDate)
        {
            var employees = await _context.Employees.Where(e => e.DepartmentId == departmentId).ToListAsync();

            return await EmployeeAttendanceStats(employees, specificDate);
        }

        public async Task<dynamic> EmployeeAttendanceStatsByShiftSpecificOrToday(int shiftId, DateTime? specificDate)
        {
            var employees = await _context.Shifts.Where(s => s.EmployeeShiftId == shiftId).Select(e => e.Employee).ToListAsync();

            return await EmployeeAttendanceStats(employees, specificDate);
        }

        public async Task<dynamic> EmployeeAttendanceStatsSpecificOrToday(DateTime? specificDate)
        {
            var employees = await _context.Employees.ToListAsync();

            return await EmployeeAttendanceStats(employees, specificDate);
        }

        private async Task<dynamic> EmployeeAttendanceStats(List<Employee> employees, DateTime? specificDate)
        {
            var today = DateTime.Now.Date;
            var shifts = await _context.Shifts.ToListAsync();
            var attendances = new List<Attendance>();

            if (specificDate != null)
            {
                attendances = await _context.Attendances.Where(a => a.DateToday.Date == specificDate.Value.Date).ToListAsync();
            }
            else
            {
                attendances = await _context.Attendances.Where(a => a.DateToday.Date == today).ToListAsync();
            }

            int absent = 0, present = 0, hasNotClockedIn = 0, late = 0, offWork = 0;
            TimeSpan lateDuration = TimeSpan.Zero;

            var absentsId = new List<ReadEmployeeDTO>();
            var presentsId = new List<ReadEmployeeDTO>();
            var hasNotClockedInId = new List<ReadEmployeeDTO>();
            var latesId = new List<ReadEmployeeDTO>();
            var offWorksId = new List<ReadEmployeeDTO>();


            foreach (var employee in employees)
            {
                var employeeShift = shifts.FirstOrDefault(s => s.EmployeeShiftId == employee.Id);
                var employeeAttendance = attendances.FirstOrDefault(a => a.EmployeeId == employee.Id);

                if (employeeShift == null)
                {
                    offWork++; // No assigned shift
                    offWorksId.Add(_mapper.Map<ReadEmployeeDTO>(employee));
                    continue;
                }

                var shiftStartTime = employeeShift.TimeIn;
                var currentTime = TimeOnly.FromDateTime(DateTime.Now).ToTimeSpan();

                if (employeeAttendance == null)
                {
                    // If the employee has a shift but hasn't clocked in after 5 hours
                    if ((currentTime - shiftStartTime).TotalHours <= 5)
                    {
                        hasNotClockedIn++;
                        hasNotClockedInId.Add(_mapper.Map<ReadEmployeeDTO>(employee));
                    }
                    else
                    {
                        absent++;
                        absentsId.Add(_mapper.Map<ReadEmployeeDTO>(employee));
                    }
                }
                else
                {
                    var clockInTime = employeeAttendance.ClockIn;
                    //var hoursLate = (clockInTime - shiftStartTime).TotalHours;

                    if (clockInTime > shiftStartTime)
                    {
                        late++;
                        lateDuration = clockInTime - shiftStartTime;
                        latesId.Add(_mapper.Map<ReadEmployeeDTO>(employee));
                    }
                    else
                    {
                        present++;
                        presentsId.Add(_mapper.Map<ReadEmployeeDTO>(employee));
                    }

                }
            }

            return new
            {
                Absent = new
                {
                    quantity = absent,
                    employees = absentsId,
                },
                Present = new
                {
                    quantity = present,
                    employees = presentsId
                },
                NotClockedIn = new
                {
                    quantity = hasNotClockedIn,
                    employees = hasNotClockedInId
                },
                Late = new
                {
                    quantity = late,
                    employees = latesId,
                },
                OffWork = new
                {
                    quantity = offWork,
                    employees = offWorksId
                },
            };
        }


        public async Task<List<Attendance>> GetAttendanceByEmployeeId(int id, int? pageIndex, int? pageSize)
        {
            var attendance = await _context.Attendances.Where(e => e.EmployeeId == id).ToListAsync();

            var attendancePagination = GetAttendancesPagination(attendance, pageIndex, pageSize);

            return attendancePagination;
        }

        public async Task<Attendance> GetDailyAttendanceByEmployeeId(int employeeId)
        {
            var attendance = await _context.Attendances.FirstOrDefaultAsync(a => a.EmployeeId == employeeId && DateOnly.FromDateTime(a.DateToday) == DateOnly.FromDateTime(DateTime.Now));

            if (attendance == null)
            {
                throw new KeyNotFoundException($"Daily attendance for employee {employeeId} not found.");
            }

            return attendance;
        }

        public async Task<List<Attendance>> GetMonthlyAttendanceByEmployeeId(int id, int? pageIndex, int? pageSize)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid employee ID", nameof(id));

            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            var attendanceRecords = await _context.Attendances
                .Where(a => a.EmployeeId == id && a.DateToday.Month == currentMonth && a.DateToday.Year == currentYear)
                .OrderBy(a => a.DateToday)
                .ToListAsync();

            var attendancePagination = GetAttendancesPagination(attendanceRecords, pageIndex, pageSize);

            if (!attendanceRecords.Any())
                throw new KeyNotFoundException($"No attendance records found for employee with ID {id} in the current month.");

            return attendancePagination;
        }


        public async Task<List<Attendance>> GetRangeAttendanceByEmployeeId(int id, DateTime start, DateTime end, int? pageIndex, int? pageSize)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid employee ID", nameof(id));

            if (start > end)
                throw new ArgumentException("Start date must be earlier than or equal to end date.");

            var attendanceRecords = await _context.Attendances
                .Where(a => a.EmployeeId == id && DateOnly.FromDateTime(a.DateToday) >= DateOnly.FromDateTime(start) && DateOnly.FromDateTime(a.DateToday) <= DateOnly.FromDateTime(end))
                .OrderBy(a => a.DateToday)
                .ToListAsync();

            var paginationAttendance = GetAttendancesPagination(attendanceRecords, pageIndex, pageSize);

            if (!attendanceRecords.Any())
                throw new KeyNotFoundException($"No attendance records found for employee with ID {id} in the given date range.");

            return paginationAttendance;
        }

        // Conditional Methods
        public async Task<bool> HasClockedIn(int employeeId)
        {
            var attendance = await _context.Attendances.FirstOrDefaultAsync(a => a.EmployeeId == employeeId && DateOnly.FromDateTime(a.DateToday) == DateOnly.FromDateTime(DateTime.Now));

            return attendance != null;
        }

        public async Task<bool> HasClockedOut(int employeeId)
        {
            var attendance = await _context.Attendances.FirstOrDefaultAsync(a => a.EmployeeId == employeeId && DateOnly.FromDateTime(a.DateToday) == DateOnly.FromDateTime(DateTime.Now) && a.ClockOut != null);

            return attendance != null;
        }

        public List<Attendance> GetAttendancesPagination(List<Attendance> attendances, int? pageIndex, int? pageSize)
        {
            if (pageSize.HasValue && pageSize.Value <= 0)
                throw new ArgumentOutOfRangeException("Page number must be greater than zero.");
            if (pageSize.HasValue && pageSize.Value <= 0)
                throw new ArgumentOutOfRangeException("Quantity must be greater than zero.");

            if (!pageIndex.HasValue || !pageSize.HasValue)
            {
                return attendances;
            }

            return attendances.Skip((pageIndex.Value - 1) * pageSize.Value).Take(pageSize.Value).ToList();
        }
    }
}
