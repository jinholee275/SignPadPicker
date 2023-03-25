using System;

namespace SignPadPicker.Exceptions
{
    public class SignFailException : Exception
    {
        public SignFailException() : base() { }

        public SignFailException(string message) : base(message) { }
    }
}
