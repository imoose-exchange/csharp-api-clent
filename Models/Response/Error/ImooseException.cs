using System;
using System.Collections.Generic;
using System.Text;

namespace ImooseApiClient
{
    class ImooseException : Exception
    {
        public string[] _errors { get; set; }

        public ImooseException(string message, string[] errors) : base(message)
        {
            _errors = errors;
        }
    }
}
