namespace hrconnectbackend.Models
{
    public class EmployeeInfoDTO
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public DateOnly BirthDate { get; set; }
        public string EducationalBackground { get; set; }
        public int Age { get; set; }

        
    }
}
