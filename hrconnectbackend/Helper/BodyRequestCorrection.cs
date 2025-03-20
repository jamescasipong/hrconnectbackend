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

      
    }
}