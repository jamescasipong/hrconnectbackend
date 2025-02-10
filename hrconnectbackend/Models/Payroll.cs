using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hrconnectbackend.Models
{
    public class Payroll
    {
        [Key]
        public int PayrollId { get; set; }

        [ForeignKey("Employee")]
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;

        [Required]
        public decimal BasicSalary { get; set; } = decimal.Zero;

        public decimal Allowances { get; set; } = decimal.Zero;
        public decimal Deductions { get; set; } = decimal.Zero;

        [Required]
        public decimal NetSalary { get; set; } // Computed as BasicSalary + Allowances - Deductions

        public decimal OvertimePay { get; set; } = 0;
        public decimal TotalWorkingHours { get; set; } = 0; // Total hours worked
        public decimal AttendanceDeduction { get; set; } = 0; // Deductions for late clock-ins/early leave
        public decimal ThirteenthMonthPay { get; set; } = 0; // For 13th Month Pay

        public DateTime PayDate { get; set; } = DateTime.UtcNow;

        [Required]
        public string PaymentStatus { get; set; } = "Pending"; // Pending, Paid

        [Required]
        public string PayPeriod { get; set; } // New field for Pay Period (e.g., "6th-20th" or "21st-5th")
    }
}
