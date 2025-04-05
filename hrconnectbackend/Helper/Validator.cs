namespace hrconnectbackend.Helper;

public static class Validator
{
    
    public static bool IsValidEmail(this string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            return false;
        }

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        
        catch
        {
            return false;
        }
    }

    public static bool IsValidPassword(this string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            throw new ArgumentNullException(nameof(password));
        }

        if (password.Length < 5)
        {
            throw new ArgumentException("Password must be at least 5 characters long");
        }

        bool hasUpperCase = password.Any(char.IsUpper);
        bool hasLowerCase = password.Any(char.IsLower);
        bool hasDigit = password.Any(char.IsDigit);
        bool hasSpecialChar = password.Any(ch => !char.IsLetterOrDigit(ch));

        if (!hasUpperCase || !hasLowerCase || !hasDigit || !hasSpecialChar)
        {
            throw new ArgumentException("Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character");
        }

        return true;
    }
}