using hrconnectbackend.Data;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Interface.Services.Clients;
using hrconnectbackend.Models;
using hrconnectbackend.Repository;
using Microsoft.EntityFrameworkCore;

namespace hrconnectbackend.Services.Clients;

public class AboutEmployeeServices(DataContext context)
    : GenericRepository<AboutEmployee>(context), IAboutEmployeeServices
{
    public async Task<EducationBackground> AddEducationBackgroundAsync(EducationBackground educationBackground)
    {
        await _context.EducationBackgrounds.AddAsync(educationBackground);
        _context.SaveChanges();

        return educationBackground;
    }

    public async Task<List<EducationBackground>> GetEmployeeEducationBackgroundAsync(int id)
    {
        var educationBackground = await _context.EducationBackgrounds.Where(e => e.EmployeeId == id).ToListAsync();

        return educationBackground;
    }



    public async Task AddEmployeeInfoAsync(AboutEmployee employeeInfo)
    {
        await _context.AboutEmployees.AddAsync(employeeInfo);
        await _context.SaveChangesAsync();
    }

    

}