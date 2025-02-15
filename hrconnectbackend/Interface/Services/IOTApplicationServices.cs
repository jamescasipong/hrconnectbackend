using hrconnectbackend.Interface.Repositories;
using hrconnectbackend.Models;

namespace hrconnectbackend.Interface.Services
{
    public interface IOTApplicationServices: IGenericRepository<OTApplication>
    {
        Task<List<OTApplication>> GetOTByEmployee(int employeeId, int? pageIndex, int? pageSize);
        Task<List<OTApplication>> GetOTByDate(DateOnly startDate, DateOnly endDate, int? pageIndex, int? pageSize);
        Task<List<OTApplication>> GetOTBySupervisor(int supervisorId, int? pageIndex, int? pageSize);
        List<OTApplication> GetOTPagination(List<OTApplication> oTApplications, int pageSize, int employeeId);
        Task ApproveOT(int id);
        Task RejectOT(int id);
    }
}
