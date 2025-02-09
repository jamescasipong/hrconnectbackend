using hrconnectbackend.Data;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Models;
using hrconnectbackend.Repository;
using Microsoft.EntityFrameworkCore;

namespace hrconnectbackend.Repositories;

public class DepartmentServices : GenericRepository<Department>, IDepartmentServices
{
    public DepartmentServices(DataContext context) : base(context)
    {

    }

    public async Task<Department> GetDepartmentByManagerId(int id)
    {
        return await _context.Departments.FirstOrDefaultAsync(x => x.Supervisor.Id == id);
    }

    public Task UpdateDepartmentByAsync(Department department)
    {
        _context.Departments.Update(department);
        return _context.SaveChangesAsync();
    }



}