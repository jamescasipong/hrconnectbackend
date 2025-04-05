using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using hrconnectbackend.Models.DTOs;
using hrconnectbackend.Models.Enums;
using hrconnectbackend.Models.Requests;

namespace hrconnectbackend.Models.EmployeeModels
{
    public class Employee
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public int UserId { get; set; }
        public int? PositionId { get; set; }
        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal BaseSalary { get; set; } = 18000m;
        public string BankName { get; set; } = "N/A";
        public int TenantId { get; set; } 
        public string BankAccountNumber { get; set; } = "N/A";
        public string TaxId { get; set; } = "N/A";
        public string EmergencyContactName { get; set; } = "N/A";
        public string EmergencyContactPhone { get; set; } = "N/A";
        public string Status { get; set; } = StatusType.Offline.ToString();
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; } = null;
        public int? EmployeeDepartmentId { get; set; } = null;

        public AboutEmployee CreateAboutEmployee(string? firstName, string? lastName)
        {
            return new AboutEmployee
            {
                EmployeeInfoId = this.Id,
                FirstName = firstName ?? "N/A",
                LastName = lastName ?? "N/A",
                BirthDate = null,
                Address = "N/A",
                Age = null,
                ProfilePicture = "N/A",
            };
        }
        // Tables that will use EmployeeID as its foreign keys
        public virtual EmployeePosition? Position { get; set; }
        public virtual EmployeeDepartment? EmployeeDepartment { get; set; }
        public virtual List<Attendance>? Attendance { get; set; }
        public virtual List<OtApplication>? OtApplication { get; set; }
        public virtual List<LeaveApplication>? LeaveApplication { get; set; }
        public virtual AboutEmployee? AboutEmployee { get; set; }
        public virtual List<Shift>? Shifts { get; set; }
        public virtual UserAccount? UserAccount { get; set; }
        public virtual List<Payroll>? Payroll { get; set; }
        public virtual List<UserNotification>? UserNotification { get; set; }
        public virtual List<AttendanceCertification>? AttendanceCertifications { get; set; }
        public virtual List<LeaveBalance>? LeaveBalance { get; set; }
    }

}
