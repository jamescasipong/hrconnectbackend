using hrconnectbackend.Interface.Repositories;
using hrconnectbackend.Models;

namespace hrconnectbackend.Interface.Services;

public interface ISupervisorServices : IGenericRepository<Supervisor>
{
    Task<List<Employee>> GetEmployeesUnderASupervisor(int id);
    Task<Supervisor> GetSupervisorByEmployee(int employeeId);

}