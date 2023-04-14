namespace SignPadPicker.Extensions
{
    public static class StringExtension
    {
        public static string IfEmptyReplace(this string value, string replace)
            => !string.IsNullOrEmpty(value) ? value : replace;
    }
}
