using hrconnectbackend.Interface.Repositories;

namespace hrconnectbackend.Interface.Services.Clients
{
    public interface IAttendanceServices : IGenericRepository<Attendance>
    {
        Task ClockIn(int id);
        Task ClockOut(int id);
        Task<Attendance> GetDailyAttendanceByEmployeeId(int id);
        Task<PagedResponse<IEnumerable<Attendance>>> GetAllAttendanceByOrganization(int organizationId, PaginationParams paginationParams);
        Task<PagedResponse<IEnumerable<Attendance>>> GetAttendanceByEmployeeId(int id, PaginationParams paginationParams);
        Task<PagedResponse<IEnumerable<Attendance>>> GetRangeAttendanceByEmployeeId(int id, DateIntervalParam dateIntervalParam, PaginationParams paginationParams);
        Task<PagedResponse<IEnumerable<Attendance>>> GetMonthlyAttendanceByEmployeeId(int id, PaginationParams paginationParams);
        Task<bool> HasClockedIn(int employeeId);
        Task<bool> HasClockedOut(int employeeId);
        Task<dynamic> EmployeeAttendanceStatsByDeptSpecificOrToday(int departmentId, DateTime? specificDate);
        Task<dynamic> EmployeeAttendanceStatsByShiftSpecificOrToday(int shiftId, DateTime? specificDate);
        Task<dynamic> EmployeeAttendanceStatsSpecificOrToday(DateTime? specificDate);
    }
}
