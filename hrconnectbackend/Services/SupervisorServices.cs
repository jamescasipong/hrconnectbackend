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

    public async Task<List<Employee>> GetSuperVisorSubordinates(int id)
    {
        return await _context.Supervisors.Where(s => s.Id == id).SelectMany(s => s.Subordinates).ToListAsync();
    }
}