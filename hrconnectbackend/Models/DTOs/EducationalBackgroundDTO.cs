namespace hrconnectbackend.Models.DTOs
{
    public class EducationBackgroundDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string InstitutionName { get; set; } = string.Empty;
        public string Degree { get; set; } = string.Empty;
        public string FieldOfStudy { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double Gpa { get; set; } = 0.0;
    }
}
