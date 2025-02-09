using System.Collections.Generic;
namespace hrconnectbackend.Models
{
    public class AboutEmployee
    {
        public int EmployeeInfoId { get; set; }
        public string ProfilePicture { get; set; }
        public string Address { get; set; }
        public DateOnly? BirthDate { get; set; }
        public List<EducationBackground>? EducationBackground { get; set; }
        public int Age { get; set; }
        public Employee Employee { get; set; }
    }
    public class EducationBackground
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string InstitutionName { get; set; }
        public string Degree { get; set; }
        public string FieldOfStudy { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double GPA { get; set; }
        public AboutEmployee? EmployeeInfo { get; set; }
    }
}
