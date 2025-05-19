using hrconnectbackend.Constants;
using hrconnectbackend.Data;
using hrconnectbackend.Exceptions;
using hrconnectbackend.Interface.Services.Clients;
using hrconnectbackend.Models.EmployeeModels;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace hrconnectbackend.Services.Clients;

public class SupervisorServices(DataContext context, IPaginatedService<Employee> paginatedServiceEmployee) : ISupervisorServices
{
    public async Task<PagedResponse<IEnumerable<Employee>>> GetAllSupervisors(int organizationId, PaginationParams paginationParams)
    {

        var paginatedSupervisors = await paginatedServiceEmployee.GetPaginatedAsync(paginationParams,
            filter: a => a.EmployeeDepartment!.SupervisorId != null && a.OrganizationId == organizationId,
            orderBy: a => a.OrderBy(a => a.CreatedAt),
            includeProperties: "EmployeeDepartment");

        return paginatedSupervisors;

    }

    public async Task<Employee> GetEmployeeSupervisor(int employeeId)
    {
        var employeeDepartment = context.Employees.Where(a => a.Id == employeeId)
            .Include(a => a.EmployeeDepartment).Select(a => a.EmployeeDepartment!);

        if (!employeeDepartment.Any())
        {
            throw new NotFoundException(ErrorCodes.EmployeeNotFound, $"Employee (Supervisor) with id {employeeId} not found");
        }

        var supervisor = await context.Employees.FindAsync(employeeDepartment.Select(a => a.SupervisorId));

        if (supervisor == null)
        {
            throw new NotFoundException(ErrorCodes.EmployeeNotFound, $"Employee (Supervisor) with id {employeeId} not found");
        }

        return supervisor;
    }

    public async Task<Employee> GetSupervisor(int employeeId)
    {
        var supervisorDepartment = await context.EmployeeDepartments.FirstOrDefaultAsync(a => a.SupervisorId == employeeId);

        if (supervisorDepartment == null)
        {
            throw new NotFoundException(ErrorCodes.EmployeeNotFound, $"Supervisor with id {employeeId} not found");
        }

        var supervisor = await context.Employees.FirstOrDefaultAsync(a => a.Id == supervisorDepartment.SupervisorId);

        if (supervisor == null)
        {
            throw new NotFoundException(ErrorCodes.EmployeeNotFound, $"Supervisor with id {employeeId} not found");
        }

        return supervisor;
    }

    public async Task<PagedResponse<IEnumerable<Employee>>> GetAllEmployeesByASupervisor(int supervisorId, PaginationParams paginationParams)
    {
        var departmentSupervisors = await paginatedServiceEmployee.GetPaginatedAsync(paginationParams,
            filter: a => a.EmployeeDepartment!.SupervisorId == supervisorId,
            orderBy: a => a.OrderBy(a => a.CreatedAt),
            includeProperties: "EmployeeDepartment");

        return departmentSupervisors;
    }

    public async Task<bool> IsSupervisor(int employeeId)
    {
        var employeeSupervisor = await context.EmployeeDepartments.AnyAsync(a => a.Id == employeeId);

        return employeeSupervisor;
    }

    public async Task DeleteSupervisor(int employeeId)
    {
        var supervisor = await context.Employees.FirstOrDefaultAsync(a => a.Id == employeeId);

        if (supervisor == null)
        {
            throw new NotFoundException(ErrorCodes.EmployeeNotFound, $"Supervisor with id {employeeId} not found");
        }

        context.Employees.Remove(supervisor);
        await context.SaveChangesAsync();
    }
}