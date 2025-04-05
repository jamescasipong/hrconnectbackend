using hrconnectbackend.Data;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Models;
using hrconnectbackend.Repository;

namespace hrconnectbackend.Services.Clients
{
    public class AttendanceCertificationServices(DataContext context, IAttendanceServices attendanceServices)
        : GenericRepository<AttendanceCertification>(context), IAttendanceCertificationServices
    {
        public async Task ApproveCertification(int id)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                var certification = await GetByIdAsync(id);

                if (certification == null)
                {
                    throw new KeyNotFoundException("No certification created yet!");
                }

                certification.Status = "Approved";
                await UpdateAsync(certification);

                var attendance = _context.Attendances.Where(a => a.EmployeeId == certification.EmployeeId && a.DateToday == certification.Date).FirstOrDefault();

                if (attendance == null)
                {
                    var newAttendance = new Attendance
                    {
                        EmployeeId = certification.EmployeeId,
                        ClockIn = certification.ClockIn,
                        ClockOut = certification.ClockOut,
                        DateToday = certification.Date,
                    };

                    await attendanceServices.AddAsync(newAttendance);
                }
                else
                {
                    attendance.ClockIn = certification.ClockIn;
                    attendance.ClockOut = certification.ClockOut;

                    await attendanceServices.UpdateAsync(attendance);
                }

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception("We have to roll back");
            }
            
        }

        public async Task RejectCertification(int id)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                var certification = await GetByIdAsync(id);

                if (certification == null)
                {
                    throw new KeyNotFoundException("No certification created yet!");
                }

                certification.Status = "Rejected";
                await UpdateAsync(certification);

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception(ex.Message);
            }
        }
    }
}
