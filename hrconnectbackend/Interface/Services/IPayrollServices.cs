using hrconnectbackend.Interface.Repositories;
using hrconnectbackend.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace hrconnectbackend.Interface.Services
{
    public interface IPayrollServices: IGenericRepository<Payroll>
    {
        Task<Payroll?> UpdatePayrollStatus(int id, string status);
        //Task<Payroll> CalculatePayroll(int employeeId, DateTime startDate, DateTime endDate);
        Task GeneratePayrollForAllEmployees(DateTime period1, DateTime period2);
    }
}
