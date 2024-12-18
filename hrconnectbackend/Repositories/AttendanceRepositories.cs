using hrconnectbackend.Data;
using hrconnectbackend.IRepositories;
using hrconnectbackend.Models;
using Microsoft.EntityFrameworkCore;

namespace hrconnectbackend.Repositories
{
    public class AttendanceRepositories : IAttendanceRepositories
    {
        private readonly DataContext _dataContext;

        public AttendanceRepositories(DataContext dataContext) {
            _dataContext = dataContext;
        }


        public async Task<Attendance> getAttendance(int id)
        {
            return await _dataContext.Attendances
        .Include(a => a.Employee)
        .FirstOrDefaultAsync(a => a.AttendanceId == id);
        }

        public async Task<List<Attendance>> GetAttendances()
        {
            return await _dataContext.Attendances.Include(a => a.Employee).ToListAsync();
        }
    }
}
