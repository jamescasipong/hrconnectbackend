namespace hrconnectbackend.Models.DTOs;

public class OrganizationsDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}