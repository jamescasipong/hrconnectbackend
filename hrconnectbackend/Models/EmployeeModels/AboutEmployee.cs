using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using hrconnectbackend.Models.EmployeeModels;

namespace hrconnectbackend.Models
{
    public class AboutEmployee
    {
        [Key]
        public int EmployeeInfoId { get; set; }
        [Required]
        public string FirstName { get; set; } = string.Empty;
        [Required]
        public string LastName { get; set; } = string.Empty;
        [Required]
        public string ProfilePicture { get; set; } = "default.jpg";
        [Required]
        public string Address { get; set; } = string.Empty;
        public DateOnly? BirthDate { get; set; } = null;
        public List<EducationBackground>? EducationBackground { get; set; }
        public int? Age { get; set; }
        [Required]
        public Employee Employee { get; set; } = null!;

        public EducationBackground CreateEducationBackground()
        {
            var newEduc = new EducationBackground
            {
                EmployeeId = this.EmployeeInfoId,
                InstitutionName = "No institution name",
                Degree = "No degree",
                FieldOfStudy = "No field of study",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow,
                GPA = 0.0
            };
            
            return newEduc;
        }
    }
    public class EducationBackground
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
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
