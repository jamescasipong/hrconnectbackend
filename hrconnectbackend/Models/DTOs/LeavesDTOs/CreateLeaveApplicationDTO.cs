namespace hrconnectbackend.Models.DTOs;

public class CreateLeaveApplicationDTO
{
    public string Type { get; set; } = "Vacation";
    public string StartDate { get; set; }
    public string EndDate { get; set; }
    public string Reason { get; set; }
}