using System.ComponentModel.DataAnnotations;
using hrconnectbackend.Models.EmployeeModels;

namespace hrconnectbackend.Models
{
    public class Shift
    {
        [Key]
        public int Id { get; set; }
        public int EmployeeShiftId { get; set; }
        public string DaysOfWorked { get; set; } = string.Empty;
        public TimeSpan TimeIn { get; set; }
        public int TenantId { get; set; }
        public TimeSpan TimeOut { get; set; }
        public Employee? Employee { get; set; }
    }
}
