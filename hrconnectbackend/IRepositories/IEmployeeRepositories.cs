﻿using hrconnectbackend.Models;
using hrconnectbackend.Models.DTOs;

namespace hrconnectbackend.IRepositories
{
    public interface IEmployeeRepositories
    {
        Task<Employee> GetEmployeeByIdAsync(int id);
        Task<ICollection<Employee>> GetAllEmployeesAsync();
        Task AddEmployeeAsync(Employee employee);
        Task UpdateEmployeeAsync(UpdateEmployeeDTO employee);
        Task DeleteEmployeeAsync(int id);
        Task<Employee> GetSupervisor(int id);
        Task<List<Employee>> GetSupervisee(int id);
    }
}
