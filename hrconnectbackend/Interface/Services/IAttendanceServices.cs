using hrconnectbackend.Interface.Repositories;
using hrconnectbackend.Models;

namespace hrconnectbackend.Interface.Services
{
    public interface IAttendanceServices : IGenericRepository<Attendance>
    {
        Task ClockIn(int id);
        Task ClockOut(int id);
        Task<Attendance> GetDailyAttendanceByEmployeeId(int id);
        Task<List<Attendance>> GetAttendanceByEmployeeId(int id);
        Task<List<Attendance>> GetMonthlyAttendanceByEmployeeId(int id);
        Task<List<Attendance>> GetRangeAttendanceByEmployeeId(int id, DateTime start, DateTime end);
    }
}
