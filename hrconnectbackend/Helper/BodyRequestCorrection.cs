namespace hrconnectbackend.Helper
{
    public static class BodyRequestCorrection
    {
        public static string CapitalLowerCaseName(this string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            string copyName = name.Trim();

            var firstLetter = copyName.Substring(0, 1).ToUpper();
            var restOfName = copyName.Substring(1).ToLower();

            return firstLetter + restOfName;
        }
    }
}