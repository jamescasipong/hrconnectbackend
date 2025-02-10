using System.ComponentModel.DataAnnotations;

namespace hrconnectbackend.Models
{
    public class Shift
    {
        [Key]
        public int Id { get; set; }
        public int EmployeeShiftId { get; set; }
        public string DaysOfWorked { get; set; }
        public TimeSpan TimeIn { get; set; }
        public TimeSpan TimeOut { get; set; }
        public Employee Employee { get; set; }
    }
}
