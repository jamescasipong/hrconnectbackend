using hrconnectbackend.Interface.Repositories;
using hrconnectbackend.Models;

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

    public interface IAttendanceCertificationServices
    {
        Task CreateCertification(AttendanceCertification attendanceCertification);
        Task<AttendanceCertification> GetCertificationById(int id);
        Task<PagedResponse<IEnumerable<AttendanceCertification>>> GetAllCertifications(int organizationId, PaginationParams paginationParams, DateIntervalParam? dateIntervalParam = null);
        Task<PagedResponse<IEnumerable<AttendanceCertification>>> GetCertificationsByEmployeeId(int employeeId, PaginationParams paginationParams);
        Task<PagedResponse<IEnumerable<AttendanceCertification>>> GetCertificationsByStatus(int organizationId, PaginationParams paginationParams, string status);
        Task UpdateCertification(AttendanceCertification attendanceCertification);
        Task DeleteCertification(int id);
        Task ApproveCertification(int id);
        Task RejectCertification(int id);
    }
}
