using System.Text;

namespace TenentManagement.Services.Utilities
{
    public static class StringUtilities
    {
        public static string ToTitleCase(this string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return str;

            StringBuilder result = new StringBuilder(str.Length);
            bool newWord = true;

            for (int i = 0; i < str.Length; i++)
            {
                if (newWord)
                {
                    result.Append(char.ToUpper(str[i]));
                    newWord = false;
                }
                else
                {
                    result.Append(char.ToLower(str[i]));
                }

                // Define word separators
                if (char.IsWhiteSpace(str[i]) || str[i] == '-' || str[i] == '\'' || str[i] == '.')
                {
                    newWord = true;
                }
            }

            return result.ToString();
        }
    }
}
