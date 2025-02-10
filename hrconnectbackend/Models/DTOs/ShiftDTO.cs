namespace hrconnectbackend.Models.DTOs
{
    public class ShiftDTO
    {
        public int EmployeeShiftId { get; set; }
        public string DaysOfWorked { get; set; }
        public string TimeIn { get; set; }
        public string TimeOut { get; set; }
    }
}
