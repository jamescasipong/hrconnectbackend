namespace hrconnectbackend.Models
{
    public class Leaves
    {
        public enum LeaveType
        {
            Vacation = 0,
            Sick = 1,
            Emergency = 2,
        }
        public int EmployeeId { get; set; }
        public LeaveType leaveType {  get; set; } = LeaveType.Vacation;
        public int UsedLeaves { get; set; }
        public int TotalLeaves { get; set; }
        public int RemaningLeaves { get; set; }
        public Employee Employee { get; set; }
    }
}
