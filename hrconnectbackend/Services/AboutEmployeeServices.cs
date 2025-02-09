using hrconnectbackend.Data;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Models;
using hrconnectbackend.Repository;
using Microsoft.EntityFrameworkCore;

namespace hrconnectbackend.Repositories;

public class AboutEmployeeServices : GenericRepository<AboutEmployee>, IAboutEmployeeServices
{

    public AboutEmployeeServices(DataContext context) : base(context)
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



    public async Task AddEmployeeInfoAsync(AboutEmployee employeeInfo)
    {
        await _context.AboutEmployees.AddAsync(employeeInfo);
        await _context.SaveChangesAsync();
    }

    

}