namespace hrconnectbackend.Models.DTOs
{

    public class CreateAboutEmployeeDto
    {
        public int EmployeeInfoId { get; set; }
        public string ProfilePicture { get; set; } = String.Empty;
        public string FirstName { get; set; } = String.Empty;
        public string LastName { get; set; } = String.Empty;
        public string Address { get; set; } = String.Empty;
        public DateOnly BirthDate { get; set; }
        public string EducationalBackground { get; set; } = String.Empty;
        public int Age { get; set; }

    }

    public class ReadAboutEmployeeDto
    {
        public int EmployeeInfoId { get; set; }
        public string ProfilePicture { get; set; } = String.Empty;
        public string FirstName { get; set; } = String.Empty;
        public string LastName { get; set; } = String.Empty;
        public string Address { get; set; } = String.Empty;
        public DateOnly BirthDate { get; set; }
        public int Age { get; set; }
        public List<EducationBackgroundDto>? EducationBackground { get; set; }
    }

}
