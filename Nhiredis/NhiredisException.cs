using System;

namespace Nhiredis
{
    public class NhiredisException : Exception
    {
        public NhiredisException()
        {}

        public NhiredisException(string message) 
            : base(message)
        {}

        public NhiredisException(string message, Exception innerException)
            : base(message, innerException)
        {}
    }
}
