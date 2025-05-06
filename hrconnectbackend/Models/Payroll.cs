using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using hrconnectbackend.Models.EmployeeModels;

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
        [Column(TypeName = "decimal(18, 2)")]
        public decimal BasicSalary { get; set; } = decimal.Zero;
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Allowances { get; set; } = decimal.Zero;
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Deductions { get; set; } = decimal.Zero;
        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal NetSalary { get; set; } // Computed as BasicSalary + Allowances - Deductions

        [Column(TypeName = "decimal(18, 2)")]
        public decimal OvertimePay { get; set; } = 0;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalWorkingHours { get; set; } = 0; // Total hours worked

        [Column(TypeName = "decimal(18, 2)")]
        public decimal AttendanceDeduction { get; set; } = 0; // Deductions for late clock-ins/early leave

        [Column(TypeName = "decimal(18, 2)")]
        public decimal ThirteenthMonthPay { get; set; } = 0; // For 13th Month Pay

        public DateTime PayDate { get; set; } = DateTime.UtcNow;

        [Required]
        public string PaymentStatus { get; set; } = "Pending"; // Pending, Paid

        [Required] public string PayPeriod { get; set; } = string.Empty; // New field for Pay Period (e.g., "6th-20th" or "21st-5th")
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int OrganizationId { get; set; } // Foreign key to Organization
        public Organization? Organization { get; set; } // Navigation property to Organization
    }
}