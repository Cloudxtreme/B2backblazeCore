using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace B2API
{
    public class B2Bucket
    {
        public string bucketId;
        public string accountId;
        public string bucketName;
        public string bucketType;
    }

    public enum B2BucketType
    {                
        allPublic,
        allPrivate
    }
}
