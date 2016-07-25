using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace B2API
{
    public class B2Exception : Exception
    {
        public string HttpStatusCode { get; set; }

        public string ErrorCode { get; set; }

        public B2Exception(string message) : base(message) { }
    }
}
