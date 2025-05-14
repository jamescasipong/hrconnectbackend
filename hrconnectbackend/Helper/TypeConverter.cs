namespace hrconnectbackend.Helper
{
    public class TypeConverter
    {
        public static int StringToInt(string value)
        {
            if (!int.TryParse(value, out var result))
            {
                throw new ArgumentException(value);
            }
            return result;
        }

        public static string IntToString(int value)
        {
            return value.ToString();
        }
    }
}
