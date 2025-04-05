namespace hrconnectbackend.Models.Sessions;

public class UserPermission
{
    public int Id { get; set; }
    public int UserId { get; set; }  // Reference to ASP.NET Identity user
    public int ResourceId { get; set; } // ID of the resource (could be a document, post, etc.)
    public bool Read { get; set; } = true;
    public bool Write { get; set; } = true;
    public bool Delete { get; set; } = false;
    public bool Update { get; set; } = false;

    public UserAccount? User { get; set; }  // Navigation property for the user
}