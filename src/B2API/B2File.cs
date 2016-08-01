using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace B2API
{
    /// <summary>
    /// The JSON response for B2 File object
    /// </summary>
    public class B2File
    {
        /// <summary>
        /// File action, upload etc..
        /// </summary>
        public string action;
        /// <summary>
        /// Your account ID.
        /// </summary>
        public string accountId;
        /// <summary>
        /// The bucket that the file is in.
        /// </summary>
        public string bucketId;
        /// <summary>
        /// The number of bytes stored in the file. 
        /// </summary>
        public string contentLength;
        /// <summary>
        /// The Sha1 hash of the contents of the file stored in B2. 
        /// </summary>
        public string contentSha1;
        /// <summary>
        /// The MIME type of the file.
        /// </summary>
        public string contentType;
        /// <summary>
        /// The unique identifier for this version of this file.
        /// </summary>
        public string fileId;
        /// <summary>
        /// The custom information that was uploaded with the file. This is a JSON object, holding the name/value pairs that were uploaded with the file. 
        /// </summary>
        public Dictionary<string, object> fileInfo;
        /// <summary>
        /// The name of this file, which can be used with 
        /// </summary>
        public string fileName;
        /// <summary>
        /// This is a UTC time when this file was uploaded. It is a base 10 number of milliseconds since midnight, January 1, 1970 UTC. 
        /// </summary>
        public long uploadTimestamp;
    }

    /// <summary>
    /// The JSON response for file part successfully uploaded.
    /// </summary>
    public class B2FilePart
    {
        /// <summary>
        /// The number of bytes stored in the part. 
        /// </summary>
        public long contentLength;
        /// <summary>
        /// The SHA1 of the bytes stored in the part. 
        /// </summary>
        public string contentSha1;
        /// <summary>
        /// The unique ID for uploading this file. 
        /// </summary>
        public string fileId;
        /// <summary>
        /// Which part this is. 
        /// </summary>
        public int partNumber;
    }
}
