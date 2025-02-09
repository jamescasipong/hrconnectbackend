using hrconnectbackend.Interface.Repositories;
using hrconnectbackend.Models;

namespace hrconnectbackend.Interface.Services
{
    public interface IOTApplicationServices: IGenericRepository<OTApplication>
    {
        Task<List<OTApplication>> GetOTByEmployee(int id);
        Task<List<OTApplication>> GetOTByDate(string startDate, string endDate);
        Task<List<OTApplication>> GetOTBySupervisor(int supervisorId);
        Task<List<OTApplication>> GetOTPagination(int pageIndex, int pageSize, int? employeeId);
        Task ApproveOT(int id);
        Task RejectOT(int id);
    }
}
