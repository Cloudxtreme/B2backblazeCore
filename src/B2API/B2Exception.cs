using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace B2API
{
    /// <summary>
    /// B2API exception
    /// </summary>
    public class B2Exception : Exception
    {
        /// <summary>
        /// Status code, see B2 docs for details
        /// </summary>
        public string HttpStatusCode { get; set; }

        /// <summary>
        /// Error code, see B2 docs for details
        /// </summary>
        public string ErrorCode { get; set; }

        /// <summary>
        /// Creat new B2Exceptions
        /// </summary>
        /// <param name="message">Error Message,  see B2 docs for details</param>
        public B2Exception(string message) : base(message) { }
    }
}
