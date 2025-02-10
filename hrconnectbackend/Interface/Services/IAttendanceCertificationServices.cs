using hrconnectbackend.Interface.Repositories;
using hrconnectbackend.Models;

namespace hrconnectbackend.Interface.Services
{
    public interface IAttendanceCertificationServices: IGenericRepository<AttendanceCertification>
    {
        Task ApproveCertification(int id);
        Task RejectCertification(int id);
    }
}
