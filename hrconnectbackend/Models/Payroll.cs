namespace hrconnectbackend.Models
{
    public class Payroll
    {
        public int PayrollId { get; set; }
        public int EmployeeId { get; set; }
        public double Salary { get; set; }
        public double Bonus { get; set; }
        public DateOnly PayDate {  get; set; }
        public Employee Employee { get; set; }
    }
}
