namespace hrconnectbackend.Helper
{
    public class BodyRequestCorrection
    {
        public static string CapitalLowerCaseName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("Name cannot be null or empty");
            }

            string copyName = name.Trim();

            var firstLetter = copyName.Substring(0, 1).ToUpper();
            var restOfName = copyName.Substring(1).ToLower();

            return firstLetter + restOfName;
        }

        public static bool IsValidEmail(string email)
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

        public static bool IsValidPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
            throw new ArgumentNullException("Password cannot be null or empty");
            }

            if (password.Length < 8)
            {
            throw new ArgumentException("Password must be at least 8 characters long");
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
}