using hrconnectbackend.Data;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Interface.Services.Clients;
using hrconnectbackend.Models;
using hrconnectbackend.Models.EmployeeModels;
using hrconnectbackend.Repository;
using Microsoft.EntityFrameworkCore;

namespace hrconnectbackend.Services.Clients;

public class SupervisorServices(DataContext context): ISupervisorServices
{
    public async Task<List<Employee>> GetAllSupervisors()
    {
        var deparmentSups = await context.EmployeeDepartments.Include(a => a.Supervisor).Select(a => a.Supervisor).ToListAsync();

        return deparmentSups;
    }
    
    public async Task<Employee> GetEmployeeSupervisor(int employeeId)
    {
        var employeeDepartment = context.Employees.Where(a => a.Id == employeeId)
            .Include(a => a.EmployeeDepartment).Select(a => a.EmployeeDepartment!);

        if (!employeeDepartment.Any())
        {
            return null;
        }
        
        var employeeSupervisor = await employeeDepartment.Include(a => a.Supervisor).Select(a => a.Supervisor).FirstOrDefaultAsync();
        
        if (employeeSupervisor == null) return null;
        
        return employeeSupervisor;
    }

    public async Task<Employee> GetSupervisor(int employeeId)
    {
        var supervisor = await context.EmployeeDepartments.Include(a => a.Supervisor).Where(a => a.Id == employeeId).Select(a => a.Supervisor).FirstOrDefaultAsync();
        
        return supervisor;
    }

    public async Task<List<Employee>> GetAllEmployeesByASupervisor(int supervisorId)
    {
        var deparmentSups = await context.EmployeeDepartments.Include(a => a.Employees).Where(a => a.SupervisorId == supervisorId).SelectMany(a => a.Employees).ToListAsync();

        return deparmentSups;
    }

    public async Task<bool> IsSupervisor(int employeeId)
    {
        var employeeSupervisor = await context.EmployeeDepartments.Include(a => a.Supervisor).AnyAsync(a => a.Id == employeeId);
        
        return employeeSupervisor;
    }
}