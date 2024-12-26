using AutoMapper;

using hrconnectbackend.Data;
using hrconnectbackend.IRepositories;
using hrconnectbackend.Models;
using hrconnectbackend.Models.DTOs;
using Microsoft.EntityFrameworkCore;


namespace hrconnectbackend.Repositories
{
    public class EmployeeRepositories : IEmployeeRepositories
    {
        private readonly DataContext _context;


        public EmployeeRepositories(DataContext dataContext)
        {
            _context = dataContext;
        }


        public async Task<Supervisor> GetSupervisor(int id)
        {
            return await _context.Employees.Where(e => e.Id == id).Select(e => e.Supervisor).FirstOrDefaultAsync();
        }

        public async Task<List<Employee>> GetSubordinates(int id)
        {
            return await _context.Supervisors.Where(s => s.Id == id).SelectMany(s => s.Subordinates).ToListAsync();
        }

        public async Task<Employee> GetEmployeeByIdAsync(int id)
        {
            var employee = await _context.Employees
                .Include(e => e.EmployeeInfo)
                .Include(A => A.Attendance)
                .FirstOrDefaultAsync(e => e.Id == id);

            return employee;

        }

        public async Task<ICollection<Employee>> GetAllEmployeesAsync()
        {
            var employees = await _context.Employees.Include(e => e.EmployeeInfo).Include(a => a.Attendance).ToListAsync();


            return employees;
        }

        public async Task AddEmployeeAsync(Employee employee)
        {
            await _context.Employees.AddAsync(employee);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateEmployeeAsync(Employee employee)
        {

            _context.Employees.Update(employee);
            await _context.SaveChangesAsync();
        }


        public async Task DeleteEmployeeAsync(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee != null)
            {
                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();
            }
        }


    }
}
