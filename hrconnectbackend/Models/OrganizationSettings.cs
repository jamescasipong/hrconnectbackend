namespace hrconnectbackend.Models;

public class OrganizationSettings
{
    public int OrganizationId { get; set; }
    public int MaxOfEmployees { get; set; } = 100;
    public int MaxOfLoginAttempts { get; set; } = 5;
}