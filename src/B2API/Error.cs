using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace B2API
{
    /// <summary>
    /// Error returned from B2 API
    /// </summary>
    public class B2Error
    {
        /// <summary>
        /// Status code
        /// </summary>
        public string status;
        /// <summary>
        /// Error code
        /// </summary>
        public string code;
        /// <summary>
        /// Error message
        /// </summary>
        public string message;
    }
}
