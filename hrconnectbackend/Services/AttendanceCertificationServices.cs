using hrconnectbackend.Data;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Models;
using hrconnectbackend.Repository;

namespace hrconnectbackend.Services
{
    public class AttendanceCertificationServices: GenericRepository<AttendanceCertification>, IAttendanceCertificationServices
    {
        public AttendanceCertificationServices(DataContext _context): base(_context) { }
    }
}
