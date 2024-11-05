namespace hrconnectbackend.Models
{
    public class Shift
    {
        public int EmployeeShiftId { get; set; }
        public string FirstDay { get; set; }
        public string LastDay { get; set; }
        public TimeOnly TimeIn { get; set; }
        public TimeOnly TimeOut { get; set; }
        public Employee Employee { get; set; }
    }
}
