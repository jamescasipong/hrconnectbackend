using hrconnectbackend.Data;
using hrconnectbackend.IRepositories;
using hrconnectbackend.Models;

namespace hrconnectbackend.Repositories;

public class EmployeeInfoRepositories : IEmployeeInfoRepositories
{
    private readonly DataContext _context;
    public EmployeeInfoRepositories(DataContext context)
    {
        _context = context;
    }
    public async Task AddEmployeeInfoAsync(EmployeeInfo employeeInfo)
    {
        var result = await _context.EmployeesInfo.AddAsync(employeeInfo);
        await _context.SaveChangesAsync();
    }

    public Task<ICollection<EmployeeInfo>> GetAllEmployeeInfosAsync()
    {
        throw new NotImplementedException();
    }

    public Task<EmployeeInfo> GetEmployeeInfoByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

}