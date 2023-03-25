using System;

namespace SignPadPicker.Exceptions
{
    public class SignPadNotInstalledException : Exception
    {
        public SignPadNotInstalledException() : base() { }

        public SignPadNotInstalledException(string message) : base(message) { }
    }
}
