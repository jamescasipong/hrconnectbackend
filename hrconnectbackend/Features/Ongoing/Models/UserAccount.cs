namespace hrconnectbackend.Features.Ongoing.Models;

public class UserAccount
{
    public int Id { get; set; }
    public int Locked { get; set; }
    public string ReasonForLock { get; set; } = string.Empty;
}