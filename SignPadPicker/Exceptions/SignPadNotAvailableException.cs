using System;

namespace SignPadPicker.Exceptions
{
    public class SignPadNotAvailableException : Exception
    {
        public SignPadNotAvailableException() : base() { }

        public SignPadNotAvailableException(string message) : base(message) { }
    }
}
