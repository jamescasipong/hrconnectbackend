namespace hrconnectbackend.Models.DTOs.Shifts;

public class CreateShiftDTO
{
    public string DaysOfWorked { get; set; }
    public string TimeIn { get; set; }
    public string TimeOut { get; set; }
}