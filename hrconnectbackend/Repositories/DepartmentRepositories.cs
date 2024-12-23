using hrconnectbackend.Data;
using hrconnectbackend.IRepositories;
using hrconnectbackend.Models;
using Microsoft.EntityFrameworkCore;

namespace hrconnectbackend.Repositories;

public class DepartmentRepositories : GenericRepository<Department>, IDepartmentRepositories
{
    private readonly DataContext _context;
    public DepartmentRepositories(DataContext context) : base(context)
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