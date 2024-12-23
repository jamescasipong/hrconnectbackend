using hrconnectbackend.Data;
using hrconnectbackend.Models;
using Microsoft.EntityFrameworkCore;

namespace hrconnectbackend.Repositories;

public class SupervisorRepositories
{
    private readonly DataContext _context;

    public SupervisorRepositories(DataContext context)
    {
        _context = context;
    }

    public async Task<Supervisor> CreateSupervisor(Supervisor supervisor)
    {
        await _context.Supervisors.AddAsync(supervisor);
        await _context.SaveChangesAsync();
        return supervisor;
    }

    public async Task<Supervisor> GetSupervisorById(int id)
    {
        return await _context.Supervisors.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<Supervisor>> GetAllSupervisors()
    {
        return await _context.Supervisors.ToListAsync();
    }

    public async Task<List<Employee>> GetSuperVisorSubordinates(int id)
    {
        return await _context.Supervisors.Where(s => s.Id == id).SelectMany(s => s.Subordinates).ToListAsync();
    }
}