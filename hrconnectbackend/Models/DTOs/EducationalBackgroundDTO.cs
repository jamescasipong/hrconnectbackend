namespace hrconnectbackend.Models.DTOs
{
    public class EducationBackgroundDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string InstitutionName { get; set; }
        public string Degree { get; set; }
        public string FieldOfStudy { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double GPA { get; set; }
    }
}
