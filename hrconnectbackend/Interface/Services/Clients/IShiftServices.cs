using System;
using hrconnectbackend.Interface.Repositories;
using hrconnectbackend.Models;

namespace hrconnectbackend.Interface.Services;

public interface IShiftServices : IGenericRepository<Shift>
{
    Task<bool> HasShiftToday(int employeeId);
    Task<List<Shift>> GetEmployeeShifts(int employeeId, int orgId);
    Task<List<Shift>> GenerateShiftForEmployee(int employeeId, int orgId);
}