namespace hrconnectbackend.Models.DTOs
{
    public class CreateSupervisorDTO
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public ReadEmployeeDTO Employee { get; set; }
    }

    public class ReadSupervisorDTO
    {
        public ReadEmployeeDTO ReadEmployee { get; set; }
    }
}
