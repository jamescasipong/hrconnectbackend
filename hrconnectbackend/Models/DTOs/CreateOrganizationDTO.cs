namespace hrconnectbackend.Models.DTOs
{
    public class CreateOrganizationDTO
    {
        public string OrgName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Zipcode { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}
