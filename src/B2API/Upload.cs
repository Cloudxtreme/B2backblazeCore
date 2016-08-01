using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace B2API
{
    /// <summary>
    /// The JSON response for uploading a file. 
    /// </summary>
    public class B2UploadUrl
    {
        /// <summary>
        /// The unique ID of the bucket. 
        /// </summary>
        public string bucketId;
        /// <summary>
        /// The URL that can be used to upload files to this bucket
        /// </summary>
        public string uploadUrl;
        /// <summary>
        /// The authorizationToken that must be used when uploading files to this bucket. 
        /// This token is valid for 24 hours or until the uploadUrl endpoint rejects an upload.
        /// </summary>
        public string authorizationToken;
    }

    /// <summary>
    /// The JSON response for uploading a file part. 
    /// </summary>
    public class B2UploadPartUrl
    {
        /// <summary>
        /// The authorizationToken that must be used when uploading files to this bucket. 
        /// This token is valid for 24 hours or until the uploadUrl endpoint rejects an upload.
        /// </summary>
        public string authorizationToken;
        /// <summary>
        /// The unique ID of file being uploaded. 
        /// </summary>
        public string fileId;
        /// <summary>
        /// The URL that can be used to upload parts of this file
        /// </summary>
        public string uploadUrl;
    }
}
