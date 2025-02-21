using hrconnectbackend.Data;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Models;
using hrconnectbackend.Repository;
using Microsoft.EntityFrameworkCore;

namespace hrconnectbackend.Repositories;

public class SupervisorServices : GenericRepository<Supervisor>, ISupervisorServices
{

    public SupervisorServices(DataContext context) : base(context)
    {

    }

    public async Task<Supervisor> GetSupervisorByEmployee(int employeeId)
    {
        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == employeeId);

        if (employee == null)
        {
            throw new KeyNotFoundException($"Unable to process. Employee with id: {employeeId} not found.");
        }

        if (employee.Supervisor == null)
        {
            throw new ArgumentException($"Unable to process. Employee with id: {employeeId} has no supervisor");
        }

        return employee.Supervisor;
    }

    public async Task<List<Employee>> GetEmployeesUnderASupervisor(int id)
    {
        var supervisor = await GetByIdAsync(id);

        if (supervisor == null)
        {
            throw new KeyNotFoundException($"{id} is not a supervisor");
        }

        var supervisorEmployees = supervisor.Subordinates?.ToList() ?? new List<Employee>();

        return supervisorEmployees;
    }
}