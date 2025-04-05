namespace hrconnectbackend.Models.DTOs
{
    public class CreateSupervisorDto
    {
        public int EmployeeId { get; set; }
        public ReadEmployeeDto? Employee { get; set; }
    }

    public class ReadSupervisorDto
    {
        public ReadEmployeeDto? ReadEmployee { get; set; }
    }
}
