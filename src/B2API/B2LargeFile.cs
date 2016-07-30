using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace B2API
{
    public class B2LargeFileUpload
    {
        public string accountId;
        public string bucketId;
        public string contentType;
        public string fileId;
        public Dictionary<string, string> fileInfo;
        public string fileName;
        public string uploadTimestamp;
    }

    public class B2LargeFile
    {
        public string accountId;
        public string action;
        public string bucketId;
        public string contentLength;
        public string contentSha1;
        public string contentType;
        public string fileId;
        public Dictionary<string, string> fileInfo;
        public string fileName;
        public long uploadTimestamp;
    }
}
