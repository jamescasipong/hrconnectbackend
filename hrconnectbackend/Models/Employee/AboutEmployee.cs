using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace hrconnectbackend.Models
{
    public class AboutEmployee
    {
        public int EmployeeInfoId { get; set; }
        [Required]
        public string ProfilePicture { get; set; } = "default.jpg";
        [Required]
        public string Address { get; set; } = string.Empty;
        public DateOnly? BirthDate { get; set; } = null;
        public List<EducationBackground>? EducationBackground { get; set; }
        public int Age { get; set; }
        [Required]
        public Employee Employee { get; set; } = null!;
    }
    public class EducationBackground
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        [Required]
        public string InstitutionName { get; set; } = string.Empty;
        [Required]
        public string Degree { get; set; } = string.Empty;
        [Required]
        public string FieldOfStudy { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double? GPA { get; set; } = null;
        public AboutEmployee? EmployeeInfo { get; set; }
    }
}
