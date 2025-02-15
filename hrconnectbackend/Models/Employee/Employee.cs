using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using hrconnectbackend.Models;
using hrconnectbackend.Models.Enums;

namespace hrconnectbackend.Models
{
    public class Employee
    {

        // Defined Columns for Employee
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public bool IsAdmin { get; set; }
        public string? Position { get; set; } = "N/A";
        public decimal BaseSalary { get; set; } = 18000m;
        public string? BankName { get; set; } = "N/A";
        public string? BankAccountNumber { get; set; } = "N/A";
        public string? TaxId { get; set; } = "N/A";
        public string? EmergencyContactName { get; set; } = "N/A";
        public string? EmergencyContactPhone { get; set; } = "N/A";
        public string Status { get; set; } = StatusType.Offline.ToString();
        public DateOnly CreatedAt { get; set; } = new DateOnly(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
        public DateOnly UpdatedAt { get; set; } = new DateOnly(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
        public int? SupervisorId { get; set; } // Foreign Key
        public int? DepartmentId { get; set; }


        // Tables that will use EmployeeID as its foreign keys
        public virtual Supervisor? Supervisor { get; set; }
        public virtual Department? Department { get; set; }
        public virtual List<Attendance>? Attendance { get; set; }
        public virtual List<OTApplication>? OTApplication { get; set; }
        public virtual List<LeaveApplication>? LeaveApplication { get; set; }
        public virtual AboutEmployee? AboutEmployee { get; set; }
        public virtual List<Shift>? Shifts { get; set; }
        public virtual UserAccount? UserAccount { get; set; }
        public virtual List<Payroll>? Payroll { get; set; }
        public virtual List<Notifications>? Notifications { get; set; }
        public virtual List<UserNotification>? UserNotification { get; set; }
        public virtual List<AttendanceCertification>? AttendanceCertifications { get; set; }
        public virtual UserSettings? UserSettings { get; set; }
        public virtual List<LeaveBalance>? LeaveBalance { get; set; }
    }
}
