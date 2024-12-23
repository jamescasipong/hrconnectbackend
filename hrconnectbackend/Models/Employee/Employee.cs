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
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool IsAdmin { get; set; }
        public string Status { get; set; } = StatusType.Offline.ToString();
        public DateOnly CreatedAt { get; set; } = new DateOnly(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);

        public DateOnly UpdatedAt { get; set; } = new DateOnly(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
        public int? SupervisorId { get; set; } // Foreign Key
        public int? DepartmentId { get; set; }
        public Supervisor Supervisor { get; set; }
        public Department Department { get; set; }


        // Tables that will use EmployeeID as its foreign keys
        public List<Attendance> Attendance { get; set; }
        public List<OTApplication> OTApplication { get; set; }
        public List<LeaveApplication> LeaveApplication { get; set; }
        public EmployeeInfo EmployeeInfo { get; set; }
        public Shift Shift { get; set; }
        public Auth Auth { get; set; }
        public List<Payroll> Payroll { get; set; }
    }
}
