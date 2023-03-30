using System;

namespace SignPadPicker.Exceptions
{
    public class NoPluginFoundException : Exception
    {
        public NoPluginFoundException() : base() { }

        public NoPluginFoundException(string message) : base(message) { }
    }
}
