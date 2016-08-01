using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace B2API
{
    /// <summary>
    /// B2 File object
    /// </summary>
    public class B2File
    {
        /// <summary>
        /// File action, upload etc..
        /// </summary>
        public string action;
        /// <summary>
        /// file content length
        /// </summary>
        public int contentLength;
        /// <summary>
        /// Unique file ID
        /// </summary>
        public string fileId;
        /// <summary>
        /// File name
        /// </summary>
        public string fileName;
        /// <summary>
        /// File size
        /// </summary>
        public int size;
        /// <summary>
        /// Upload time stamp
        /// </summary>
        public long uploadTimestamp;    
    }
}
