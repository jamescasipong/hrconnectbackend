using hrconnectbackend.Models.EmployeeModels;

namespace hrconnectbackend.Interface.Services.Clients;

public interface ISupervisorServices
{
    Task<List<Employee>> GetAllSupervisors();
    Task<Employee> GetEmployeeSupervisor(int employeeId);
    Task<Employee> GetSupervisor(int employeeId);
    Task<List<Employee>> GetAllEmployeesByASupervisor(int supervisorId);
    Task<bool> IsSupervisor(int employeeId);
}