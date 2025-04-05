using hrconnectbackend.Interface.Repositories;
using hrconnectbackend.Models;

namespace hrconnectbackend.Interface.Services.Clients
{
    public interface IAttendanceCertificationServices: IGenericRepository<AttendanceCertification>
    {
        Task ApproveCertification(int id);
        Task RejectCertification(int id);
    }
}
