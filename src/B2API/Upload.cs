using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace B2API
{
    public class B2UploadUrl
    {
        public string bucketId;
        public string uploadUrl;
        public string authorizationToken;
    }

    public class B2UploadPartUrl
    {
        public string authorizationToken;
        public string fileId;
        public string uploadUrl;
    }
}
