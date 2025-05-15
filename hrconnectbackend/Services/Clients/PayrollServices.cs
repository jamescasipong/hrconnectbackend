using hrconnectbackend.Constants;
using hrconnectbackend.Data;
using hrconnectbackend.Exceptions;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Models;
using hrconnectbackend.Repository;
using Microsoft.EntityFrameworkCore;

namespace hrconnectbackend.Services.Clients
{
    public class PayrollServices(DataContext context) : GenericRepository<Payroll>(context), IPayrollServices
    {
        public async Task<Payroll?> UpdatePayrollStatus(int id, string status)
        {
            var payroll = await _context.Payrolls.FindAsync(id);
            if (payroll == null)
            {
                throw new NotFoundException(ErrorCodes.PayrollNotFound, $"Payroll with id: {id} not found.");
            }

            payroll.PaymentStatus = status;
            await _context.SaveChangesAsync();
            return payroll;
        }

        public async Task GeneratePayrollForAllEmployees(DateTime period1, DateTime period2)
        {

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var employees = await _context.Employees.ToListAsync();
                    foreach (var employee in employees)
                    {
                        //DateTime payPeriodStart1 = new DateTime(monthYear.Year, monthYear.Month, 6);
                        //DateTime payPeriodEnd1 = new DateTime(monthYear.Year, monthYear.Month, 20);
                        //DateTime payPeriodStart2 = new DateTime(monthYear.Year, monthYear.Month, 21);
                        //DateTime payPeriodEnd2 = payPeriodStart2.AddMonths(1).AddDays(-1);

                        var payroll1 = await CalculatePayroll(employee.Id, period1, period2);

                        if (payroll1 == null)
                        {
                            throw new InternalServerErrorException(ErrorCodes.PayrollCalculationFailed, $"Failed to calculate payroll for employee with id: {employee.Id}");
                        }

                        payroll1.PayPeriod = $"{period1:dd MMM} - {period2:dd MMM}";
                        _context.Payrolls.Add(payroll1);

                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task<Payroll?> CalculatePayroll(int employeeId, DateTime startDate, DateTime endDate)
        {
            var payroll = new Payroll();
            var attendanceRecords = await _context.Attendances
                .Where(a => a.EmployeeId == employeeId && a.DateToday >= startDate && a.DateToday <= endDate)
                .ToListAsync();


            var employee = await _context.Employees.FindAsync(employeeId);

            if (employee == null)
            {
                throw new NotFoundException(ErrorCodes.EmployeeNotFound, $"Employee with id: {employeeId} not found.");
            }

            decimal totalHoursWorked = 0;
            decimal overtimePay = 0;
            decimal attendanceDeduction = 0;

            foreach (var attendance in attendanceRecords)
            {
                attendance.CalculateWorkingHours();
                totalHoursWorked += attendance.WorkingHours;

                var overtime = await _context.OtApplications.FirstOrDefaultAsync(a => DateOnly.FromDateTime(a.Date) == DateOnly.FromDateTime(attendance.DateToday));

                if (overtime != null)
                {
                    totalHoursWorked += (decimal)(overtime.StartTime - overtime.EndTime).TotalHours;
                }

                if (attendance.ClockIn > new TimeSpan(9, 0, 0))
                {
                    attendanceDeduction += 10;
                }
                if (attendance.ClockOut.HasValue && attendance.ClockOut.Value < new TimeSpan(18, 0, 0))
                {
                    attendanceDeduction += 10;
                }
            }

            decimal standardWorkingHours = 8;
            if (totalHoursWorked > (attendanceRecords.Count * standardWorkingHours))
            {
                overtimePay = (totalHoursWorked - (attendanceRecords.Count * standardWorkingHours)) * 15;
            }

            decimal yearlyBasicSalary = payroll.BasicSalary * 12;
            payroll.ThirteenthMonthPay = yearlyBasicSalary / 12;

            payroll.EmployeeId = employeeId;
            payroll.BasicSalary = employee.BaseSalary;
            payroll.Allowances = 500;
            payroll.Deductions = attendanceDeduction;
            payroll.NetSalary = payroll.BasicSalary + payroll.Allowances - payroll.Deductions + overtimePay + payroll.ThirteenthMonthPay;
            payroll.TotalWorkingHours = totalHoursWorked;
            payroll.OvertimePay = overtimePay;
            payroll.AttendanceDeduction = attendanceDeduction;
            payroll.PayDate = DateTime.UtcNow;
            payroll.PayPeriod = $"{startDate:dd MMM} - {endDate:dd MMM}";

            return payroll;
        }
    }
}
