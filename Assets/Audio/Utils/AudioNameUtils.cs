using System.Text.RegularExpressions;

namespace LightHouse.Audio
{
    public static class AudioUtils
    {
        public static string SanitizeName(string rawName)
        {
            if (string.IsNullOrEmpty(rawName)) return string.Empty;

            string clean = rawName.Replace(" ", "_")
                                  .Replace("-", "_")
                                  .Replace(".", "_");
            return Regex.Replace(clean, @"[^a-zA-Z0-9_]", "");
        }
    }
}
