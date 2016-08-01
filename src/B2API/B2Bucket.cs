using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace B2API
{
    /// <summary>
    /// B2Bucket object
    /// </summary>
    public class B2Bucket
    {
        /// <summary>
        /// ID of the bucket
        /// </summary>
        public string bucketId;
        /// <summary>
        /// Buckets owner account ID
        /// </summary>
        public string accountId;
        /// <summary>
        /// Bucket name
        /// </summary>
        public string bucketName;
        /// <summary>
        /// Bucket type
        /// </summary>
        public string bucketType;
    }

    /// <summary>
    /// bucket type
    /// </summary>
    public enum B2BucketType
    {
        ///Public              
        allPublic,
        /// <summary>
        /// Private
        /// </summary>
        allPrivate
    }
}
