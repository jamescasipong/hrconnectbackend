namespace hrconnectbackend.Models.DTOs
{

    public class CreateAboutEmployeeDTO
    {
        public int EmployeeInfoId { get; set; }
        public string ProfilePicture { get; set; } 
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public DateOnly BirthDate { get; set; }
        public string EducationalBackground { get; set; }
        public int Age { get; set; }

    }

    public class ReadAboutEmployeeDTO
    {
        public int EmployeeInfoId { get; set; }
        public string ProfilePicture { get; set; } 
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public DateOnly BirthDate { get; set; }
        public int Age { get; set; }
        public List<EducationBackgroundDTO> EducationBackground { get; set; }
    }

}
