using hrconnectbackend.Controllers;
using hrconnectbackend.Data;
using hrconnectbackend.IRepositories;
using hrconnectbackend.Models;

namespace hrconnectbackend.Repositories
{
    public class EmployeeRepositories: IEmployeeRepositories
    {
        private readonly DataContext _context;
        public EmployeeRepositories(DataContext dataContext) {
            _context = dataContext;
        }

        public ICollection<Employee> GetAll()
        {
            return _context.Employees.ToList();
        }

        public Employee GetEmployee(int id)
        {
            var employee = _context.Employees.FirstOrDefault(e => e.Id == id);

            if (employee == null)
            {
                return null;

            }

            return employee;
        }
    }
}
