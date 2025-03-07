using hrconnectbackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Attendance
{
    [Key]
    public int AttendanceId { get; set; }
    public int EmployeeId { get; set; }
    public DateTime DateToday { get; set; }
    public TimeSpan ClockIn { get; set; }
    public TimeSpan? ClockOut { get; set; }
    //public string Status { get; set; }

    // This will store the total working hours, including overtime if applicable
    [Column(TypeName = "decimal(18, 2)")]
    public decimal WorkingHours { get; set; }
    public TimeSpan? LateClockIn { get; set; }
    public TimeSpan? EarlyLeave { get; set; }

    public Employee? Employee { get; set; }

    // Method to calculate working hours (you can adjust this logic as per your needs)
    public void CalculateWorkingHours()
    {
        if (ClockOut.HasValue)
        {
            var totalHours = (ClockOut.Value - ClockIn).TotalHours;
            WorkingHours = (decimal)totalHours;
        }
        else
        {
            WorkingHours = 0;
        }
    }
}
