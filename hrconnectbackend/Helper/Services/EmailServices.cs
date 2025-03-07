namespace hrconnectbackend.Helper;

public class EmailServicesz
{
    public static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            // Extract domain and check if it's in the allowed list
            var allowedDomains = new List<string> { "gmail.com", "yahoo.com", "outlook.com" };
            var domain = addr.Host.ToLower();

            return addr.Address == email && allowedDomains.Contains(domain);
        }
        catch
        {
            return false;
        }
    }
}