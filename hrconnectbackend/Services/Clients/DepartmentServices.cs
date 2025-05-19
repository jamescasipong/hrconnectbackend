using hrconnectbackend.Constants;
using hrconnectbackend.Data;
using hrconnectbackend.Exceptions;
using hrconnectbackend.Interface.Services.Clients;
using hrconnectbackend.Models;
using hrconnectbackend.Models.EmployeeModels;
using hrconnectbackend.Repository;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Transactions;

namespace hrconnectbackend.Services.Clients;

public class DepartmentServices(DataContext context, ILogger<DepartmentServices> logger, IPaginatedService<EmployeeDepartment> paginatedServiceEmployeeDepartment) : GenericRepository<Department>(context), IDepartmentServices
{
    public async Task<PagedResponse<IEnumerable<EmployeeDepartment>>> RetrieveEmployeeDepartments(int organizationId, PaginationParams paginationParams)
    {
        var employeeDepartments = await paginatedServiceEmployeeDepartment.GetPaginatedAsync(
            paginationParams,
            x => x.OrganizationId == organizationId,
            orderBy: q => q.OrderBy(x => x.Id),
            includeProperties: "Department,Employees");

        return employeeDepartments;
    }

    public async Task CreateEmployeeDepartment(int departmentId, int supervisorId, int organizationId)
    {
        using var transaction = _context.Database.BeginTransaction();

        try
        {
            var employeeDepartment = new EmployeeDepartment
            {
                DepartmentId = departmentId,
                SupervisorId = supervisorId,
                OrganizationId = organizationId
            };

            await _context.EmployeeDepartments.AddAsync(employeeDepartment);
            await _context.SaveChangesAsync();

            var employee = await _context.Employees.FindAsync(supervisorId);
            if (employee != null)
            {
                employee.EmployeeDepartmentId = employeeDepartment.Id;
                await _context.SaveChangesAsync();
            }

            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, "Error creating employee department");

            throw new Exception("An error occurred while creating the employee department.", ex);

        }
    }

    public async Task AddEmployeToDepartment(int employeeId, int departmentId)
    {
        await GetByIdAsync(employeeId);

        var employee = await _context.Employees.FindAsync(employeeId);

        if (employee == null) throw new NotFoundException(ErrorCodes.EmployeeNotFound, $"Employee with ID: {employeeId} not found.");

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

    public async Task<Department> GetDepartmentByGuid(Guid guid)
    {
        var department = await _context.Departments.FirstOrDefaultAsync(a => a.DepartmentGuid == guid);

        if (department == null)
        {
            throw new NotFoundException(ErrorCodes.DepartmentNotFound, $"Department with GUID: {guid} not found.");
        }

        return department;
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

    public async Task<EmployeeDepartment> UpdateEmployeeDepartmentSupervisor(int newSupervisorId, int departmentId)
    {
        var department = await _context.EmployeeDepartments.FindAsync(departmentId);

        if (department == null) throw new NotFoundException(ErrorCodes.EmployeeDepartmentNotFound, $"Employee department with ID: {departmentId} not found.");

        department.SupervisorId = newSupervisorId;

        _context.EmployeeDepartments.Update(department);
        await _context.SaveChangesAsync();

        return department;
    }

    public async Task<EmployeeDepartment> GetEmployeeDepartment(int employeeDeptId)
    {
        var empDepartment = await _context.EmployeeDepartments.FindAsync(employeeDeptId);

        if (empDepartment == null)
        {
            throw new NotFoundException(ErrorCodes.EmployeeDepartmentNotFound, $"Employee department with ID: {employeeDeptId} not found.");
        }
        return empDepartment;
    }

    public async Task<EmployeeDepartment> GetDepartmentByManagerId(int managerId)
    {
        var department = await _context.EmployeeDepartments.FirstOrDefaultAsync(x => x.SupervisorId == managerId);

        if (department == null)
        {
            throw new NotFoundException(ErrorCodes.DepartmentNotFound, $"Department with manager {managerId} not found.");
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

    public async Task<object> RetrieveDepartment(int organizationId)
    {
        List<object> departmentWithEmployees = new List<object>();
        var departments = await _context.Departments.ToListAsync();

        foreach (var dept in departments)
        {
            var employees = await _context.Employees
                .Where(e => e.EmployeeDepartmentId == dept.DepartmentId)
                .ToListAsync();

            logger.LogInformation($"Employees in department {dept.DeptName}: {employees.Count}");

            var department = new
            {
                dept.DepartmentId,
                dept.DeptName,
                dept.Description,
                dept.DepartmentGuid,
                dept.OrganizationId,
                dept.CreatedAt,
                dept.UpdatedAt,
                Employees = employees
            };

            departmentWithEmployees.Add(department);
        }

        return departmentWithEmployees;
    }

}