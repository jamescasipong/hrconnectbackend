﻿namespace hrconnectbackend.Models
{
    public class Leaves
    {
       
        public int EmployeeId { get; set; }
        public string leaveType {  get; set; }
        public int UsedLeaves { get; set; }
        public int TotalLeaves { get; set; }
        public int RemaningLeaves { get; set; }
        public Employee Employee { get; set; }
    }
}
