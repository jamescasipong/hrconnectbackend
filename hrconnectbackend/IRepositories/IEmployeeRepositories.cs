using hrconnectbackend.Models;

namespace hrconnectbackend.IRepositories
{
    public interface IEmployeeRepositories
    {
        public ICollection<Employee> GetAll();
        public Employee GetEmployee(int id);
    }
}
