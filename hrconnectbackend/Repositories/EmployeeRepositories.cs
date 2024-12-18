using AutoMapper;
using hrconnectbackend.Controllers;
using hrconnectbackend.Data;
using hrconnectbackend.IRepositories;
using hrconnectbackend.Models;
using hrconnectbackend.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using hrconnectbackend.Helper;

namespace hrconnectbackend.Repositories
{
    public class EmployeeRepositories: IEmployeeRepositories
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        
        
        public EmployeeRepositories(DataContext dataContext, IMapper mapper)
        {
            _context = dataContext;
            _mapper = mapper;
        }


        public async Task<Employee> GetSupervisor(int id)
        {
            return await _context.Employees.Where(e => e.Id == id).Select(e => e.Supervisor).FirstOrDefaultAsync();
        }

        public async Task<List<Employee>> GetSupervisee(int id)
        {
            return await _context.Employees.Where(e => e.Supervisor.Id == id).ToListAsync();
        }

        public async Task<Employee> GetEmployeeByIdAsync(int id)
        {
            return await _context.Employees
                .Include(e => e.Supervisor)
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<ICollection<Employee>> GetAllEmployeesAsync()
        {
            return await _context.Employees.ToListAsync();
        }

        public async Task AddEmployeeAsync(Employee employee)
        {
            await _context.Employees.AddAsync(employee);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateEmployeeAsync(UpdateEmployeeDTO employeeDTO)
        {
            var employee = _mapper.Map<Employee>(employeeDTO); // Map DTO to entity

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
