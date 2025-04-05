using hrconnectbackend.Data;
using hrconnectbackend.Interface.Services.Clients;
using hrconnectbackend.Models;
using hrconnectbackend.Repository;
using Microsoft.EntityFrameworkCore;

namespace hrconnectbackend.Services.Clients;

public class DepartmentServices(DataContext context) : GenericRepository<Department>(context), IDepartmentServices
{
    public async Task<List<EmployeeDepartment>> RetrieveDepartment(int? pageIndex, int? pageSize)
    {
        var departments = await _context.EmployeeDepartments.Include(e => e.Employees).ToListAsync();
        
        if (pageIndex.HasValue && pageSize.HasValue)
        {
            departments = departments.Skip(pageIndex.Value * pageSize.Value).Take(pageSize.Value).ToList();
        }

        return departments;
    }

    public async Task AddEmployeToDepartment(int employeeId, int departmentId)
    {
        var department = await GetByIdAsync(employeeId);

        if (department == null) throw new KeyNotFoundException($"Department with ID: {departmentId} not found.");

        var employee = await _context.Employees.FindAsync(employeeId);

        if (employee == null) throw new KeyNotFoundException($"Employee with ID: {employeeId} not found.");

        employee.EmployeeDepartmentId = departmentId;

        _context.Employees.Update(employee);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteEmployeFromDepartment(int employeeId)
    {
        var employee = await _context.Employees.FindAsync(employeeId);

        if (employee == null) throw new KeyNotFoundException($"Employee with ID: {employeeId} not found");

        employee.EmployeeDepartment = null;
        employee.EmployeeDepartmentId = null;

        _context.Employees.Update(employee);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateEmployeeDepartment(int employeeId, int departmentId)
    {
        var employee = await _context.Employees.FindAsync(employeeId);

        if (employee == null) throw new KeyNotFoundException($"Employee with ID: {employeeId} not found.");

        var department = await GetByIdAsync(departmentId);

        if (department == null) throw new KeyNotFoundException($"Department with ID: {departmentId} not found.");

        employee.EmployeeDepartmentId = employeeId;

        _context.Employees.Update(employee);
        await _context.SaveChangesAsync();
    }

    public async Task<Department?> GetDepartmentByEmployee(int employeeId)
    {
        return await _context.Employees
            .Include(a => a.EmployeeDepartment)
            .Where(a => a.Id == employeeId)
            .Select(a => a.EmployeeDepartment!)
            .Include(a => a.Department)
            .Select(a => a.Department)
            .SingleOrDefaultAsync();
    }

    public async Task<EmployeeDepartment?> UpdateEmployeeDepartmentSupervisor(int employeeId, int departmentId)
    {
        var department = await _context.EmployeeDepartments.FindAsync(departmentId);
        
        if (department == null) return null;
        
        department.SupervisorId = employeeId;
        
        _context.EmployeeDepartments.Update(department);
        await _context.SaveChangesAsync();

        if (department.SupervisorId != departmentId)
        {
            return null;
        }
        
        return department;
    }

    public async Task<EmployeeDepartment?> GetEmployeeDepartment(int departmentId)
    {
        return await _context.EmployeeDepartments.FindAsync(departmentId);
    }

    public async Task<EmployeeDepartment?> GetDepartmentByManagerId(int managerId)
    {
        var department = await _context.EmployeeDepartments.FirstOrDefaultAsync(x => x.SupervisorId == managerId);

        if (department == null)
        {
            throw new KeyNotFoundException($"Department with manager {managerId} not found.");
        }

        return department;
    }

    public Task UpdateDepartmentByAsync(Department department)
    {
        _context.Departments.Update(department);
        return _context.SaveChangesAsync();
    }

    public async Task AddSupervisor(int employeeId, int departmentId)
    {
        var employee = await _context.Employees.FindAsync(employeeId);

        if (employee == null) throw new KeyNotFoundException($"Employee with ID: {employeeId} not found.");

        var department = await GetByIdAsync(departmentId);

        if (department == null) throw new KeyNotFoundException($"Department with ID: {departmentId} not found.");

        employee.EmployeeDepartmentId = departmentId;

        _context.Employees.Update(employee);
        await _context.SaveChangesAsync();
    }
}