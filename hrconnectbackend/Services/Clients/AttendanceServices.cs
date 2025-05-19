using AutoMapper;
using hrconnectbackend.Constants;
using hrconnectbackend.Data;
using hrconnectbackend.Exceptions;
using hrconnectbackend.Interface.Services.Clients;
using hrconnectbackend.Models;
using hrconnectbackend.Models.DTOs;
using hrconnectbackend.Models.EmployeeModels;
using hrconnectbackend.Repository;
using Microsoft.EntityFrameworkCore;

namespace hrconnectbackend.Services.Clients
{
    public class AttendanceServices(DataContext context, IMapper mapper, IPaginatedService<Attendance> paginatedServiceAttendance, IPaginatedService<AttendanceCertification> paginatedServiceCertification)
        : GenericRepository<Attendance>(context), IAttendanceServices, IAttendanceCertificationServices
    {
        public async Task ClockIn(int employeeId)
        {
            var employee = await _context.Employees.FindAsync(employeeId);
            var dayToday = DateTime.UtcNow;

            // Convert clock-out time to TimeSpan
            var timeClockedIn = dayToday.TimeOfDay; // This gives us the TimeSpan of the current time

            var employeeShift = await _context.Shifts.Where(e => e.EmployeeShiftId == employeeId).ToListAsync();
            var hasShift = employeeShift.Any(shift => dayToday.ToString("dddd") == shift.DaysOfWorked);

            if (employee == null)
            {
                throw new NotFoundException(ErrorCodes.EmployeeNotFound, $"Employee with id: {employeeId} not found.");
            }

            if (!hasShift)
            {
                throw new ConflictException(ErrorCodes.EmployeeNoShift, $"Employee with ID {employeeId} has no shift today. Cannot clock in.");
            }

            var hasClockedIn = await HasClockedIn(employeeId);

            if (hasClockedIn)
            {
                throw new ConflictException(ErrorCodes.AlreadyClockedIn, $"Employee with ID {employeeId} has already clocked in.");
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
                DateToday = DateTime.UtcNow,
                ClockIn = timeClockedIn,
                ClockOut = null,
                LateClockIn = lateLeave,
            };

            await AddAsync(newAttendance);
        }

        public async Task ClockOut(int employeeId)
        {
            var employee = await _context.Employees.FindAsync(employeeId);
            var attendance = await _context.Attendances.FirstOrDefaultAsync(a => DateOnly.FromDateTime(a.DateToday) == DateOnly.FromDateTime(DateTime.UtcNow) && a.EmployeeId == employeeId);
            var employeeShift = await _context.Shifts.Where(e => e.EmployeeShiftId == employeeId).ToListAsync();

            if (attendance == null)
            {
                throw new NotFoundException(ErrorCodes.AttendanceNotFound, $"No attendance record found for employee ID {employeeId} on {DateTime.UtcNow:yyyy-MM-dd}. No clock-in yet.");
            }

            if (employee == null)
            {
                throw new NotFoundException(ErrorCodes.EmployeeNotFound, $"Employee with ID {employeeId} not found.");
            }

            var dayToday = DateTime.UtcNow;
            var clockIn = attendance.ClockIn;
            var hasShift = employeeShift.Any(shift => dayToday.ToString("dddd") == shift.DaysOfWorked);


            if (!hasShift)
            {
                throw new ConflictException(ErrorCodes.EmployeeNoShift, $"Employee with ID {employeeId} has no shift today. Cannot clock out.");
            }

            // Convert clock-out time to TimeSpan
            var timeClockedOut = dayToday.TimeOfDay; // This gives us the TimeSpan of the current time

            // Calculate working hours as the difference between clock-in and clock-out
            attendance.ClockOut = timeClockedOut;
            attendance.WorkingHours = (decimal)(timeClockedOut - clockIn).TotalHours;

            var hasClockedIn = await HasClockedIn(employeeId);

            if (!hasClockedIn)
            {
                throw new ConflictException(ErrorCodes.AttendanceNotClockedIn, $"Employee with ID {employeeId} has not clocked in yet.");
            }

            var hasClockedOut = await HasClockedOut(employeeId);

            if (hasClockedOut)
            {
                throw new ConflictException(ErrorCodes.AlreadyClockedIn, $"Employee with ID {employeeId} has already clocked out.");
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

        public async Task<dynamic> EmployeeAttendanceStatsByDeptSpecificOrToday(int departmentId, DateTime? specificDate)
        {
            var employees = await _context.Employees.Where(e => e.EmployeeDepartmentId == departmentId).ToListAsync();

            return await EmployeeAttendanceStats(employees, specificDate);
        }

        public async Task<dynamic> EmployeeAttendanceStatsByShiftSpecificOrToday(int shiftId, DateTime? specificDate)
        {
            var employees = await _context.Shifts.Where(s => s.EmployeeShiftId == shiftId).Select(e => e.Employee!).ToListAsync();

            if (!employees.Any()) throw new NotFoundException(ErrorCodes.EmployeeNotFound, $"No employees found for shift ID {shiftId}.");

            return await EmployeeAttendanceStats(employees, specificDate);
        }

        public async Task<dynamic> EmployeeAttendanceStatsSpecificOrToday(DateTime? specificDate)
        {
            var employees = await _context.Employees.ToListAsync();

            return await EmployeeAttendanceStats(employees, specificDate);
        }

        private async Task<dynamic> EmployeeAttendanceStats(List<Employee> employees, DateTime? specificDate)
        {
            var today = DateTime.UtcNow.Date;
            var shifts = await _context.Shifts.ToListAsync();
            List<Attendance> attendances;

            if (specificDate != null)
            {
                attendances = await _context.Attendances.Where(a => a.DateToday.Date == specificDate.Value.Date).ToListAsync();
            }
            else
            {
                attendances = await _context.Attendances.Where(a => a.DateToday.Date == today).ToListAsync();
            }

            int absent = 0, present = 0, hasNotClockedIn = 0, late = 0, offWork = 0;

            var absentsId = new List<ReadEmployeeDto>();
            var presentsId = new List<ReadEmployeeDto>();
            var hasNotClockedInId = new List<ReadEmployeeDto>();
            var latesId = new List<ReadEmployeeDto>();
            var offWorksId = new List<ReadEmployeeDto>();


            foreach (var employee in employees)
            {
                var employeeShift = shifts.FirstOrDefault(s => s.EmployeeShiftId == employee.Id);
                var employeeAttendance = attendances.FirstOrDefault(a => a.EmployeeId == employee.Id);

                if (employeeShift == null)
                {
                    offWork++; // No assigned shift
                    offWorksId.Add(mapper.Map<ReadEmployeeDto>(employee));
                    continue;
                }

                var shiftStartTime = employeeShift.TimeIn;
                var currentTime = TimeOnly.FromDateTime(DateTime.UtcNow).ToTimeSpan();

                if (employeeAttendance == null)
                {
                    // If the employee has a shift but hasn't clocked in after 5 hours
                    if ((currentTime - shiftStartTime).TotalHours <= 5)
                    {
                        hasNotClockedIn++;
                        hasNotClockedInId.Add(mapper.Map<ReadEmployeeDto>(employee));
                    }
                    else
                    {
                        absent++;
                        absentsId.Add(mapper.Map<ReadEmployeeDto>(employee));
                    }
                }
                else
                {
                    var clockInTime = employeeAttendance.ClockIn;
                    //var hoursLate = (clockInTime - shiftStartTime).TotalHours;

                    if (clockInTime > shiftStartTime)
                    {
                        late++;
                        latesId.Add(mapper.Map<ReadEmployeeDto>(employee));
                    }
                    else
                    {
                        present++;
                        presentsId.Add(mapper.Map<ReadEmployeeDto>(employee));
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


        public async Task<PagedResponse<IEnumerable<Attendance>>> GetAttendanceByEmployeeId(int id, PaginationParams paginationParams)
        {
            var attendancePagination = await paginatedServiceAttendance.GetPaginatedAsync(
                paginationParams,
                a => a.EmployeeId == id,
                orderBy: q => q.OrderBy(a => a.DateToday));

            return attendancePagination;
        }

        public async Task<Attendance> GetDailyAttendanceByEmployeeId(int employeeId)
        {
            var attendance = await _context.Attendances.FirstOrDefaultAsync(a => a.EmployeeId == employeeId && DateOnly.FromDateTime(a.DateToday) == DateOnly.FromDateTime(DateTime.UtcNow));

            if (attendance == null)
            {
                throw new NotFoundException(ErrorCodes.AttendanceNotFound, $"No attendance record found for employee ID {employeeId} on {DateTime.UtcNow:yyyy-MM-dd}.");
            }

            return attendance;
        }

        public async Task<PagedResponse<IEnumerable<Attendance>>> GetMonthlyAttendanceByEmployeeId(int id, PaginationParams paginationParams)
        {
            if (id <= 0)
                throw new BadRequestException("Invalid employee ID", nameof(id));

            var paginatedResponse = await paginatedServiceAttendance.GetPaginatedAsync(
                paginationParams,
                a => a.EmployeeId == id && DateOnly.FromDateTime(a.DateToday) == DateOnly.FromDateTime(DateTime.UtcNow),
                orderBy: q => q.OrderBy(a => a.DateToday));

            return paginatedResponse;
        }


        public async Task<PagedResponse<IEnumerable<Attendance>>> GetRangeAttendanceByEmployeeId(int id, DateIntervalParam dateIntervalParam, PaginationParams paginationParams)
        {
            if (id <= 0)
            {
                throw new BadRequestException(ErrorCodes.InvalidEmployeeData, "Invalid employee ID");
            }

            if (dateIntervalParam.StartDate > dateIntervalParam.EndDate)
            {
                throw new BadRequestException(ErrorCodes.InvalidAttendanceData, "Start date cannot be greater than end date.");
            }

            var attendanceRecordPaginations = await paginatedServiceAttendance.GetPaginatedAsync(
                paginationParams,
                a => a.EmployeeId == id && DateOnly.FromDateTime(a.DateToday) >= DateOnly.FromDateTime(dateIntervalParam.StartDate) && DateOnly.FromDateTime(a.DateToday) <= DateOnly.FromDateTime(dateIntervalParam.EndDate),
                orderBy: q => q.OrderBy(a => a.DateToday));


            return attendanceRecordPaginations;
        }


        // Conditional Methods
        public async Task<bool> HasClockedIn(int employeeId)
        {
            var attendance = await _context.Attendances.FirstOrDefaultAsync(a => a.EmployeeId == employeeId && DateOnly.FromDateTime(a.DateToday) == DateOnly.FromDateTime(DateTime.UtcNow));

            var hasClockedIn = attendance != null;

            return hasClockedIn;
        }

        public async Task<bool> HasClockedOut(int employeeId)
        {
            var attendance = await _context.Attendances.FirstOrDefaultAsync(a => a.EmployeeId == employeeId && DateOnly.FromDateTime(a.DateToday) == DateOnly.FromDateTime(DateTime.UtcNow) && a.ClockOut != null);

            var hasClockedOut = attendance != null;

            return hasClockedOut;
        }

        public Task<PagedResponse<IEnumerable<Attendance>>> GetAllAttendanceByOrganization(int organizationId, PaginationParams paginationParams)
        {
            var paginatedResponse = paginatedServiceAttendance.GetPaginatedAsync(
                paginationParams,
                a => a.Employee!.OrganizationId == organizationId,
                orderBy: q => q.OrderBy(a => a.DateToday));

            return paginatedResponse;
        }

        public async Task CreateCertification(AttendanceCertification attendanceCertification)
        {
            var newCertification = new AttendanceCertification
            {
                EmployeeId = attendanceCertification.EmployeeId,
                SupervisorId = attendanceCertification.SupervisorId,
                Status = attendanceCertification.Status,
                Date = attendanceCertification.Date,
                TenantId = attendanceCertification.TenantId,
                ClockIn = attendanceCertification.ClockIn,
                ClockOut = attendanceCertification.ClockOut,
                Reason = attendanceCertification.Reason,
                DateCreated = DateTime.UtcNow,
                OrganizationId = attendanceCertification.OrganizationId
            };

            await _context.AttendanceCertifications.AddAsync(newCertification);
            await _context.SaveChangesAsync();
        }

        public async Task<AttendanceCertification> GetCertificationById(int id)
        {
            var certification = await _context.AttendanceCertifications.FirstOrDefaultAsync(c => c.Id == id);

            if (certification == null)
            {
                throw new NotFoundException(ErrorCodes.AttendanceNotFound, $"No certification found with ID {id}.");
            }

            return certification;
        }

        public async Task<PagedResponse<IEnumerable<AttendanceCertification>>> GetAllCertifications(int organizationId, PaginationParams paginationParams, DateIntervalParam? dateIntervalParam = null)
        {
            if (dateIntervalParam != null)
            {
                var certifications = await paginatedServiceCertification.GetPaginatedAsync(
                    paginationParams,
                    c => c.OrganizationId == organizationId && c.Date >= dateIntervalParam.StartDate && c.Date <= dateIntervalParam.EndDate,
                    orderBy: q => q.OrderBy(c => c.Date));

                return certifications;
            }

            var allCertifications = await paginatedServiceCertification.GetPaginatedAsync(
                paginationParams,
                c => c.OrganizationId == organizationId,
                orderBy: q => q.OrderBy(c => c.Date));

            return allCertifications;
        }

        public Task<PagedResponse<IEnumerable<AttendanceCertification>>> GetCertificationsByEmployeeId(int employeeId, PaginationParams paginationParams)
        {
            var certifications = paginatedServiceCertification.GetPaginatedAsync(
                paginationParams,
                c => c.EmployeeId == employeeId,
                orderBy: q => q.OrderBy(c => c.Date));

            return certifications;
        }

        public Task<PagedResponse<IEnumerable<AttendanceCertification>>> GetCertificationsByStatus(int organizationId, PaginationParams paginationParams, string status)
        {
            var certifications = paginatedServiceCertification.GetPaginatedAsync(
                paginationParams,
                c => c.OrganizationId == organizationId && c.Status == status,
                orderBy: q => q.OrderBy(c => c.Date));

            return certifications;
        }

        public async Task UpdateCertification(AttendanceCertification attendanceCertification)
        {
            var certification = await _context.AttendanceCertifications.FindAsync(attendanceCertification.Id);

            if (certification == null)
            {
                throw new NotFoundException(ErrorCodes.AttendanceNotFound, $"No certification found with ID {attendanceCertification.Id}.");
            }

            certification.EmployeeId = attendanceCertification.EmployeeId;
            certification.SupervisorId = attendanceCertification.SupervisorId;
            certification.Status = attendanceCertification.Status;
            certification.Date = attendanceCertification.Date;
            certification.TenantId = attendanceCertification.TenantId;
            certification.ClockIn = attendanceCertification.ClockIn;
            certification.ClockOut = attendanceCertification.ClockOut;
            certification.Reason = attendanceCertification.Reason;
            certification.DateCreated = DateTime.UtcNow;

            _context.AttendanceCertifications.Update(certification);
            await SaveChangesAsync();
        }

        public async Task DeleteCertification(int id)
        {
            var certification = await _context.AttendanceCertifications.FindAsync(id);

            if (certification == null)
            {
                throw new NotFoundException(ErrorCodes.AttendanceNotFound, $"No certification found with ID {id}.");
            }

            _context.AttendanceCertifications.Remove(certification);
            await SaveChangesAsync();
        }

        public async Task ApproveCertification(int id)
        {
            var certification = await _context.AttendanceCertifications.FindAsync(id);

            if (certification == null)
            {
                throw new NotFoundException(ErrorCodes.AttendanceNotFound, $"No certification found with ID {id}.");
            }

            certification.Status = "Approved";
            _context.AttendanceCertifications.Update(certification);
            await SaveChangesAsync();
        }

        public async Task RejectCertification(int id)
        {
            var certification = await _context.AttendanceCertifications.FindAsync(id);

            if (certification == null)
            {
                throw new NotFoundException(ErrorCodes.AttendanceNotFound, $"No certification found with ID {id}.");
            }

            certification.Status = "Rejected";
            _context.AttendanceCertifications.Update(certification);
            await SaveChangesAsync();
        }
    }
}
