using System;
using hrconnectbackend.Models;
using hrconnectbackend.Models.DTOs.Shifts;

namespace hrconnectbackend.IRepositories;

public interface IShiftRepositories : IGenericRepository<Shift>
{
    Task<bool> CheckIfEmployeeHasShift(int employeeId, DateTime date);
    Task<string> AddShiftToEmployee(int employeeId, CreateShiftDTO shiftDTO);
    Task RemoveShiftFromEmployee(int employeeId, int shiftId);
    Task<ICollection<Shift>> GetEmployeeShiftsById(int employeeId);
    Task<bool> HasShiftToday(int employeeId);
}