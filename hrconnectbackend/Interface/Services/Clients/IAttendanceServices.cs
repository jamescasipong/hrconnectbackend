using hrconnectbackend.Interface.Repositories;
using hrconnectbackend.Models;

namespace hrconnectbackend.Interface.Services
{
    public interface IAttendanceServices : IGenericRepository<Attendance>
    {
        Task ClockIn(int id);
        Task ClockOut(int id);
        Task<Attendance> GetDailyAttendanceByEmployeeId(int id);
        Task<List<Attendance>> GetAttendanceByEmployeeId(int id, int? pageIndex, int? pageSize);
        Task<List<Attendance>> GetMonthlyAttendanceByEmployeeId(int id, int? pageIndex, int? pageSize);
        Task<List<Attendance>> GetRangeAttendanceByEmployeeId(int id, DateTime start, DateTime end, int? pageIndex, int? pageSize);
        Task<bool> HasClockedIn(int employeeId);
        Task<bool> HasClockedOut(int employeeId);
        Task<dynamic> EmployeeAttendanceStatsByDeptSpecificOrToday(int departmentId, DateTime? specificDate);
        Task<dynamic> EmployeeAttendanceStatsByShiftSpecificOrToday(int shiftId, DateTime? specificDate);
        Task<dynamic> EmployeeAttendanceStatsSpecificOrToday(DateTime? specificDate);
    }
}
