using hrconnectbackend.Interface.Repositories;
using hrconnectbackend.Models;
using hrconnectbackend.Models.Requests;

namespace hrconnectbackend.Interface.Services
{
    public interface IOTApplicationServices: IGenericRepository<OtApplication>
    {
        Task<List<OtApplication>> GetOTByEmployee(int employeeId, int? pageIndex, int? pageSize);
        Task<List<OtApplication>> GetOTByDate(DateOnly startDate, DateOnly endDate, int? pageIndex, int? pageSize);
        Task<List<OtApplication>> GetOTBySupervisor(int supervisorId, int? pageIndex, int? pageSize);
        List<OtApplication> GetOTPagination(List<OtApplication> oTApplications, int pageSize, int employeeId);
        Task ApproveOT(int id);
        Task RejectOT(int id);
    }
}
