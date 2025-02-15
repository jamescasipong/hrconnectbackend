using hrconnectbackend.Data;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Models;
using hrconnectbackend.Repository;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace hrconnectbackend.Repositories;

public class DepartmentServices : GenericRepository<Department>, IDepartmentServices
{
    public DepartmentServices(DataContext context) : base(context)
    {

    }

    public async Task AddEmployeToDepartment(int employeeId, int departmentId)
    {
        var department = await GetByIdAsync(employeeId);

        if (department == null) throw new KeyNotFoundException($"Department with ID: {departmentId} not found.");

        var employee = await _context.Employees.FindAsync(employeeId);

        if (employee == null) throw new KeyNotFoundException($"Employee with ID: {employeeId} not found.");

        employee.DepartmentId = departmentId;

        _context.Employees.Update(employee);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteEmployeFromDepartment(int employeeId)
    {
        var employee = await _context.Employees.FindAsync(employeeId);

        if (employee == null) throw new KeyNotFoundException($"Employee with ID: {employeeId} not found");

        employee.Department = null;
        employee.DepartmentId = null;

        _context.Employees.Update(employee);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateEmployeeDepartment(int employeeId, int departmentId)
    {
        var employee = await _context.Employees.FindAsync(employeeId);

        if (employee == null) throw new KeyNotFoundException($"Employee with ID: {employeeId} not found.");

        var department = await GetByIdAsync(departmentId);

        if (department == null) throw new KeyNotFoundException($"Department with ID: {departmentId} not found.");

        employee.DepartmentId = employeeId;

        _context.Employees.Update(employee);
        await _context.SaveChangesAsync();
    }

    public async Task<Department> GetDepartmentByManagerId(int supervisorId)
    {
        var department = await _context.Departments.FirstOrDefaultAsync(x => x.ManagerId == supervisorId);

        if (department == null)
        {
            throw new KeyNotFoundException($"Department with manager {supervisorId} not found.");
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

        employee.DepartmentId= departmentId;

        _context.Employees.Update(employee);
        await _context.SaveChangesAsync();
    }
}