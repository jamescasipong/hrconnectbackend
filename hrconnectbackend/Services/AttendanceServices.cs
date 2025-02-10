﻿using System.Threading.Tasks;
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
                throw new KeyNotFoundException($"Employee with ID {employeeId} not found.");
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

        public async Task<dynamic> EmployeePresentStatsByDept(int departmentId)
        {
            var today = DateTime.Now.Date;
            var employees = await _context.Employees.Where(e => e.DepartmentId == departmentId).ToListAsync();
            var shifts = await _context.Shifts.ToListAsync();
            var attendances = await _context.Attendances.Where(a => a.DateToday == today).ToListAsync();

            int absent = 0, present = 0, hasNotClockedIn = 0, late = 0, offWork = 0;

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
                    employees = absentsId
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
                    employees = latesId
                },
                OffWork = new
                {
                    quantity = offWork,
                    employees = offWorksId
                },
            };
        }

        public async Task<dynamic> EmployeePresentStats()
        {
            var today = DateTime.Now;
            var employees = await _context.Employees.ToListAsync();
            var shifts = await _context.Shifts.ToListAsync();
            var attendances = await _context.Attendances.Where(a => a.DateToday.Date == today.Date).ToListAsync();

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


        public async Task<List<Attendance>> GetAttendanceByEmployeeId(int id)
        {
            var attendance = await _context.Attendances.Where(e => e.EmployeeId == id).ToListAsync();

            return attendance;
        }

        public async Task<Attendance> GetDailyAttendanceByEmployeeId(int employeeId)
        {
            var attendance = await _context.Attendances.FirstOrDefaultAsync(a => a.EmployeeId == employeeId && DateOnly.FromDateTime(a.DateToday) == DateOnly.FromDateTime(DateTime.Now));

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
                .Where(a => a.EmployeeId == id && DateOnly.FromDateTime(a.DateToday) >= DateOnly.FromDateTime(start) && DateOnly.FromDateTime(a.DateToday) <= DateOnly.FromDateTime(end))
                .OrderBy(a => a.DateToday)
                .ToListAsync();

            if (!attendanceRecords.Any())
                throw new KeyNotFoundException($"No attendance records found for employee with ID {id} in the given date range.");

            return attendanceRecords;
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

    }
}
