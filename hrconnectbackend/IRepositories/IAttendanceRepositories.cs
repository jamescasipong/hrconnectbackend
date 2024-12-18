using hrconnectbackend.Models;

namespace hrconnectbackend.IRepositories
{
    public interface IAttendanceRepositories
    {
        Task<Attendance> getAttendance(int id);
        Task<List<Attendance>> GetAttendances();
    }
}
