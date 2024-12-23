using hrconnectbackend.Models;

namespace hrconnectbackend.IRepositories
{
    public interface IAttendanceRepositories : IGenericRepository<Attendance>
    {
        Task<string> ClockIn(int id);
        Task<string> ClockOut(int id);
        Task FileAttendance(int id, DateOnly date);
        Task<Attendance> GetDailyAttendanceByEmployeeId(int id);
        Task<ICollection<Attendance>> GetMonthlyAttendanceByEmployeeId(int id);
        Task<ICollection<Attendance>> GetRangeAttendanceByEmployeeId(int id, DateOnly start, DateOnly end);
        Task ApproveAttendance(int id);
        Task RejectAttendance(int id);

    }
}
