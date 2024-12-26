namespace hrconnectbackend.Models
{
    public class EmployeeInfoDTO
    {

        public int EmployeeInfoId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public DateOnly BirthDate { get; set; }
        public List<EducationBackground> EducationalBackground { get; set; }
        public int Age { get; set; }


    }
}
