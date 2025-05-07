using hrconnectbackend.Data;
using hrconnectbackend.Interface.Services.Clients;
using hrconnectbackend.Models.EmployeeModels;
using Microsoft.EntityFrameworkCore;

namespace hrconnectbackend.Services.Clients;

public class SupervisorServices(DataContext context): ISupervisorServices
{
    public async Task<ICollection<Employee?>> GetAllSupervisors()
    {
        var departmentSupervisors = await context.EmployeeDepartments.ToListAsync();
        List<Employee?> supervisors = new List<Employee?>();

        foreach (var department in departmentSupervisors)
        {
            var employee = await context.Employees.FindAsync(department.SupervisorId);

            if (employee == null) continue;

            supervisors.Add(employee);
        }

        return supervisors;
    }
    
    public async Task<Employee?> GetEmployeeSupervisor(int employeeId)
    {
        var employeeDepartment = context.Employees.Where(a => a.Id == employeeId)
            .Include(a => a.EmployeeDepartment).Select(a => a.EmployeeDepartment!);

        if (!employeeDepartment.Any())
        {
            return null;
        }

        var supervisor = await context.Employees.FindAsync(employeeDepartment.Select(a => a.SupervisorId));
                
        return supervisor;
    }

    public async Task<Employee?> GetSupervisor(int employeeId)
    {
        var supervisorDepartment = await context.EmployeeDepartments.FirstOrDefaultAsync(a => a.SupervisorId == employeeId);

        if (supervisorDepartment == null) return null;

        var supervisor = await context.Employees.FirstOrDefaultAsync(a => a.Id == supervisorDepartment.SupervisorId);
        
        return supervisor;
    }

    public async Task<List<Employee>> GetAllEmployeesByASupervisor(int supervisorId)
    {
        var departmentSupervisors = await context.EmployeeDepartments.Include(a => a.Employees).Where(a => a.SupervisorId == supervisorId).SelectMany(a => a.Employees!).ToListAsync();

        return departmentSupervisors;
    }

    public async Task<bool> IsSupervisor(int employeeId)
    {
        var employeeSupervisor = await context.EmployeeDepartments.AnyAsync(a => a.Id == employeeId);
        
        return employeeSupervisor;
    }
}