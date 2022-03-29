using System;

namespace DdnsService.SDKs
{
    public class DomainNotExistsException : System.Exception
    {
        public DomainNotExistsException(string message, Exception ex) : base(message, ex)
        {

        }
    }
}
