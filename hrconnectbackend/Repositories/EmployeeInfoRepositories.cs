using hrconnectbackend.Data;
using hrconnectbackend.IRepositories;
using hrconnectbackend.Models;
using Microsoft.EntityFrameworkCore;

namespace hrconnectbackend.Repositories;

public class EmployeeInfoRepositories : GenericRepository<EmployeeInfo>, IEmployeeInfoRepositories
{

    public EmployeeInfoRepositories(DataContext context) : base(context)
    {

    }

    public async Task<EducationBackground> AddEducationBackgroundAsync(EducationBackground educationBackground)
    {
        await _context.EducationBackgrounds.AddAsync(educationBackground);
        _context.SaveChanges();

        return educationBackground;
    }

    public async Task<List<EducationBackground>> GetEmployeeEducationBackgroundAsync(int id)
    {
        var educationBackground = await _context.EducationBackgrounds.Where(e => e.UserId == id).ToListAsync();

        return educationBackground;
    }

    public async Task<EmployeeInfo> GetEmployeeInfoByIdAsync(int id)
    {
        var employeeInfo = await _context.EmployeesInfo.Include(e => e.EducationBackground).Where(e => e.EmployeeInfoId == id).FirstOrDefaultAsync();

        return employeeInfo;
    }


    public async Task AddEmployeeInfoAsync(EmployeeInfo employeeInfo)
    {
        await _context.EmployeesInfo.AddAsync(employeeInfo);
        await _context.SaveChangesAsync();
    }


}