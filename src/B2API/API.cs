using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using System.IO;
using System.Security.Cryptography;

namespace B2API
{
    /// <summary>
    /// A simple API for Backblazes B2 service that uses .net core
    /// </summary>
    public class B2API
    {

        private string _accountId;
        private string _applicationKey;
        private string _authToken;
        private string _apiURL;
        private string _downloadUrl;

        private string _credentials { get { return Convert.ToBase64String(Encoding.UTF8.GetBytes(_accountId + ":" + _applicationKey)); } }

        /// <summary>
        /// 
        /// </summary>
        public B2API()
        {
            _accountId = "";
            _applicationKey = "";
            _authToken = "";
            _apiURL = "";
            _downloadUrl = "";
        }

        /// <summary>
        /// Used to log in to the B2 API.
        /// Must be the first function called
        /// </summary>
        /// <param name="accountId">B2 Account ID</param>
        /// <param name="applicationKey">B2 Application Key</param>
        /// <returns>true if successfull</returns>
        public async Task<bool> AuthorizeAccount(string accountId, string applicationKey)
        {
            _accountId = accountId;
            _applicationKey = applicationKey;
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", "Basic " + _credentials);
            var response = await client.GetAsync("https://api.backblaze.com/b2api/v1/b2_authorize_account");
            if (!response.IsSuccessStatusCode)
            {
                B2Error error = JsonConvert.DeserializeObject<B2Error>(await response.Content.ReadAsStringAsync());
                throw new B2Exception(error.message)
                {
                    HttpStatusCode = error.status,
                    ErrorCode = error.code
                };
            }
            else
            {
                var json = await response.Content.ReadAsStringAsync();
                Dictionary<string, object> values = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                _authToken = values["authorizationToken"].ToString();
                _apiURL = values["apiUrl"].ToString();
                _downloadUrl = values["downloadUrl"].ToString();
                return true;
            }
        }

        /// <summary>
        /// Lists buckets associated with an account, in alphabetical order by bucket ID. 
        /// </summary>
        /// <returns>A list of Bucket objects</returns>
        public async Task<List<B2Bucket>> ListBuskets()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", _authToken);
            var response = await client.PostAsync(_apiURL + "/b2api/v1/b2_list_buckets", new StringContent("{\"accountId\":\"" + _accountId + "\"}"));
            if (!response.IsSuccessStatusCode)
            {
                B2Error error = JsonConvert.DeserializeObject<B2Error>(await response.Content.ReadAsStringAsync());
                throw new B2Exception(error.message)
                {
                    HttpStatusCode = error.status,
                    ErrorCode = error.code
                };
            }
            else
            {
                var json = await response.Content.ReadAsStringAsync();
                Dictionary<string, List<B2Bucket>> values = JsonConvert.DeserializeObject<Dictionary<string, List<B2Bucket>>>(json);
                List<B2Bucket> buckets = values["buckets"];
                return buckets;
            }
        }

        /// <summary>
        /// Creates a new bucket. A bucket belongs to the account used to create it.
        /// </summary>
        /// <param name="bucketName">The name to give the new bucket.
        ///                          Bucket names must be a minimum of 6 and a maximum of 50 characters long, and must be globally unique; 
        ///                          two different B2 accounts cannot have buckets with the name name. Bucket names can consist of: letters, 
        ///                          digits, and "-". Bucket names cannot start with "b2-"; these are reserved for internal Backblaze use. </param>
        /// <param name="bucketType">Either "allPublic", meaning that files in this bucket can be downloaded by anybody, or "allPrivate", 
        ///                          meaning that you need a bucket authorization token to download the files. </param>
        /// <returns>New bucket object</returns>
        public async Task<B2Bucket> CreateBucket(string bucketName, B2BucketType bucketType)
        {            
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", _authToken);
            string content =
                "{\"accountId\":\"" + _accountId + "\",\n" +
                "\"bucketName\":\"" + bucketName + "\",\n" +
                "\"bucketType\":\"" + bucketType.ToString() + "\"}";
            var response = await client.PostAsync(_apiURL + "/b2api/v1/b2_create_bucket", new StringContent(content));
            if (!response.IsSuccessStatusCode)
            {
                B2Error error = JsonConvert.DeserializeObject<B2Error>(await response.Content.ReadAsStringAsync());
                throw new B2Exception(error.message)
                {
                    HttpStatusCode = error.status,
                    ErrorCode = error.code
                };
            }
            else
            {
                var json = await response.Content.ReadAsStringAsync();                
                B2Bucket bucket = JsonConvert.DeserializeObject<B2Bucket>(json);
                return bucket;
            }
        }

        /// <summary>
        /// Modifies the bucketType of an existing bucket. Can be used to allow everyone to download the contents of the bucket without 
        /// providing any authorization, or to prevent anyone from downloading the contents of the bucket without providing a bucket auth token. 
        /// </summary>
        /// <param name="bucket">Bucket object to update</param>
        /// <param name="newBucketType">Either "allPublic", meaning that files in this bucket can be downloaded by anybody, or "allPrivate", 
        ///                             meaning that you need a bucket authorization token to download the files. </param>
        /// <returns>Modified bucket object</returns>
        public async Task<B2Bucket> UpdateBucket(B2Bucket bucket, B2BucketType newBucketType)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", _authToken);
            string content =
                "{\"accountId\":\"" + _accountId + "\",\n" +
                "\"bucketId\":\"" + bucket.bucketId + "\",\n" +
                "\"bucketType\":\"" + newBucketType.ToString() + "\"}";
            var response = await client.PostAsync(_apiURL + "/b2api/v1/b2_update_bucket", new StringContent(content));
            if (!response.IsSuccessStatusCode)
            {
                B2Error error = JsonConvert.DeserializeObject<B2Error>(await response.Content.ReadAsStringAsync());
                throw new B2Exception(error.message)
                {
                    HttpStatusCode = error.status,
                    ErrorCode = error.code
                };
            }
            else
            {
                var json = await response.Content.ReadAsStringAsync();
                bucket = JsonConvert.DeserializeObject<B2Bucket>(json);
                return bucket;
            }
        }

        /// <summary>
        /// This call returns at most 1000 file names, but it can be called repeatedly to scan through all of the file names in a bucket. Each time 
        /// you call, the last file retunred can be used as the starting point for the next call. 
        /// </summary>
        /// <param name="bucket">Bucket object to look for file names in.</param>
        /// <param name="startFromFileName">The first file name to return. If there is a file with this name, it will be returned in the list. If not,
        ///                                 the first file name after thi. </param>
        /// <param name="returncount">Optional: Number of file names to return max: 1000 default: 1000</param>
        /// <returns>A List of file objects</returns>
        public async Task<List<B2File>> ListFileNames(B2Bucket bucket, string startFromFileName = "", int returncount = 1000)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", _authToken);
            string content ="{\"bucketId\":\"" + bucket.bucketId + "\",\n" +
                            "\"startFileName\":\"" + startFromFileName + "\",\n" +
                            "\"maxFileCount\":" + returncount.ToString() + "}";
            var response = await client.PostAsync(_apiURL + "/b2api/v1/b2_list_file_names", new StringContent(content));
            if (!response.IsSuccessStatusCode)
            {
                B2Error error = JsonConvert.DeserializeObject<B2Error>(await response.Content.ReadAsStringAsync());
                throw new B2Exception(error.message)
                {
                    HttpStatusCode = error.status,
                    ErrorCode = error.code
                };
            }
            else
            {
                var json = await response.Content.ReadAsStringAsync();
                Dictionary<string, List<B2File>> values = JsonConvert.DeserializeObject<Dictionary<string, List<B2File>>>(json);
                List<B2File> files = values["files"];
                return files;
            }
        }

        /// <summary>
        /// Lists all of the versions of all of the files contained in one bucket, in alphabetical order by file name, and by reverse of date/time uploaded 
        /// for versions of files with the same name. 
        /// </summary>
        /// <param name="bucket">Bucket object to look for file names in.</param>
        /// <param name="startFromFileName">The first file name to return. If there is a file with this name, it will be returned in the list. If not,
        ///                                 the first file name after thi. </param>
        /// <param name="returncount">Optional: Number of file names to return max: 1000 default: 1000</param>
        /// <returns>A List of file objects</returns>
        public async Task<List<B2File>> ListFileVersions(B2Bucket bucket, string startFromFileName = "", int returncount = 1000)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", _authToken);
            string content = "{\"bucketId\":\"" + bucket.bucketId + "\",\n" +
                            "\"startFileName\":\"" + startFromFileName + "\",\n" +
                            "\"maxFileCount\":" + returncount.ToString() + "}";
            var response = await client.PostAsync(_apiURL + "/b2api/v1/b2_list_file_versions", new StringContent(content));
            if (!response.IsSuccessStatusCode)
            {
                B2Error error = JsonConvert.DeserializeObject<B2Error>(await response.Content.ReadAsStringAsync());
                throw new B2Exception(error.message)
                {
                    HttpStatusCode = error.status,
                    ErrorCode = error.code
                };
            }
            else
            {
                var json = await response.Content.ReadAsStringAsync();
                Dictionary<string, List<B2File>> values = JsonConvert.DeserializeObject<Dictionary<string, List<B2File>>>(json);
                List<B2File> files = values["files"];
                return files;
            }
        }

        /// <summary>
        /// Gets information about one file stored in B2. 
        /// </summary>
        /// <param name="file">b2File object</param>
        /// <returns>B2FileInfo object</returns>
        public async Task<B2File> GetFileInfo(B2File file)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", _authToken);
            string content = "{\"fileId\":\"" + file.fileId + "\"}";
            var response = await client.PostAsync(_apiURL + "/b2api/v1/b2_get_file_info", new StringContent(content));
            if (!response.IsSuccessStatusCode)
            {
                B2Error error = JsonConvert.DeserializeObject<B2Error>(await response.Content.ReadAsStringAsync());
                throw new B2Exception(error.message)
                {
                    HttpStatusCode = error.status,
                    ErrorCode = error.code
                };
            }
            else
            {
                var json = await response.Content.ReadAsStringAsync();
                B2File canceledFile = JsonConvert.DeserializeObject<B2File>(json);
                return canceledFile;
            }
        }

        /// <summary>
        /// Downloads the provided file. this function allowes for downloading large files without storing in memory
        /// </summary>
        /// <param name="file">File object to download</param>
        /// <param name="filename">filename to save file as</param>
        /// <returns>Task object</returns>
        public async Task DownloadFileByID(B2File file, string filename)
        {
            var client = new HttpClient();
            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Post, _downloadUrl + "/b2api/v1/b2_download_file_by_id");
            msg.Headers.Add("Authorization", _authToken);
            msg.Content = new StringContent("{\"fileId\":\"" + file.fileId + "\"}");
            HttpResponseMessage response = await client.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead);
            if (!response.IsSuccessStatusCode)
            {
                B2Error error = JsonConvert.DeserializeObject<B2Error>(await response.Content.ReadAsStringAsync());
                throw new B2Exception(error.message)
                {
                    HttpStatusCode = error.status,
                    ErrorCode = error.code
                };
            }
            else
            {
                using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync())
                {
                    using (Stream streamToWriteTo = File.Open(filename, FileMode.Create))
                    {
                        await streamToReadFrom.CopyToAsync(streamToWriteTo);
                    }
                }
            }
        }

        /// <summary>
        /// Downloads one file from B2. 
        /// </summary>
        /// <param name="file">File object to download</param>
        /// <param name="streamToWriteTo">Stream to write downloaded file to</param>
        /// <returns>Task object</returns>
        public async Task DownloadFileByID(B2File file, Stream streamToWriteTo)
        {
            var client = new HttpClient();
            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Post, _downloadUrl + "/b2api/v1/b2_download_file_by_id");
            msg.Headers.Add("Authorization", _authToken);
            msg.Content = new StringContent("{\"fileId\":\"" + file.fileId + "\"}");
            HttpResponseMessage response = await client.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead);
            if (!response.IsSuccessStatusCode)
            {
                B2Error error = JsonConvert.DeserializeObject<B2Error>(await response.Content.ReadAsStringAsync());
                throw new B2Exception(error.message)
                {
                    HttpStatusCode = error.status,
                    ErrorCode = error.code
                };
            }
            else
            {
                using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync())
                {
                    await streamToReadFrom.CopyToAsync(streamToWriteTo);
                }
            }
        }

        /// <summary>
        /// Downloads one file by providing the name of the bucket and the name of the file. 
        /// </summary>
        /// <param name="bucket">Bucket object to download from</param>
        /// <param name="file">File object to download</param>
        /// <param name="streamToWriteTo">filestream to write data to</param>
        /// <returns>Task object</returns>
        public async Task DownloadFileByName(B2Bucket bucket, B2File file, Stream streamToWriteTo)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", _authToken);            
            HttpResponseMessage response = await client.GetAsync(_downloadUrl + "/file/" + bucket.bucketName + "/" + file.fileName, HttpCompletionOption.ResponseHeadersRead);            
            
            if (!response.IsSuccessStatusCode)
            {
                B2Error error = JsonConvert.DeserializeObject<B2Error>(await response.Content.ReadAsStringAsync());
                throw new B2Exception(error.message)
                {
                    HttpStatusCode = error.status,
                    ErrorCode = error.code
                };
            }
            else
            {
                using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync())
                {
                    await streamToReadFrom.CopyToAsync(streamToWriteTo);
                }
            }
        }

        /// <summary>
        /// Deletes the provided file
        /// </summary>
        /// <param name="file">File object to delete</param>
        /// <returns>the file object just deleted</returns>
        public async Task<B2File> DeleteFileVersion(B2File file)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", _authToken);
            string content = "{\"fileName\":\"" + file.fileName + "\",\n" +
                             "\"fileId\":\"" + file.fileId + "\"}";
            var response = await client.PostAsync(_apiURL + "/b2api/v1/b2_delete_file_version", new StringContent(content));
            if (!response.IsSuccessStatusCode)
            {
                B2Error error = JsonConvert.DeserializeObject<B2Error>(await response.Content.ReadAsStringAsync());
                throw new B2Exception(error.message)
                {
                    HttpStatusCode = error.status,
                    ErrorCode = error.code
                };
            }
            else
            {
                var json = await response.Content.ReadAsStringAsync();
                B2File deletedFile = JsonConvert.DeserializeObject<B2File>(json);
                return deletedFile;
            }
        }

        /// <summary>
        /// Gets a upload url and auth token for a upload
        /// </summary>
        /// <param name="bucket">Bucket object you wish to upload to</param>
        /// <returns>Upload url object</returns>
        public async Task<B2UploadUrl> GetUploadURL(B2Bucket bucket)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", _authToken);
            string content = "{\"bucketId\":\"" + bucket.bucketId + "\"}";
            var response = await client.PostAsync(_apiURL + "/b2api/v1/b2_get_upload_url", new StringContent(content));
            if (!response.IsSuccessStatusCode)
            {
                B2Error error = JsonConvert.DeserializeObject<B2Error>(await response.Content.ReadAsStringAsync());
                throw new B2Exception(error.message)
                {
                    HttpStatusCode = error.status,
                    ErrorCode = error.code
                };
            }
            else
            {
                var json = await response.Content.ReadAsStringAsync();
                B2UploadUrl uploadurl = JsonConvert.DeserializeObject<B2UploadUrl>(json);
                return uploadurl;
            }
        }

        /// <summary>
        /// Uploads one file to B2, returning its unique file ID.
        /// </summary>
        /// <param name="bucket">Bucket object to upload the file to</param>
        /// <param name="uploadURL">Upload URL object from GetUploadURL</param>
        /// <param name="fileName">The name of the file</param>
        /// <param name="fileBytes">The file data in a byte form</param>
        /// <param name="fileContentType">The MIME type of the content of the file, which will be returned in the Content-Type header when downloading 
        ///                               the file. Use the Content-Type b2/x-auto to automatically set the stored Content-Type post upload. In the case 
        ///                               where a file extension is absent or the lookup fails, the Content-Type is set to application/octet-stream.</param>
        /// <returns>File object for the uploaded file</returns>
        public async Task<B2File> UploadFile(B2Bucket bucket, B2UploadUrl uploadURL, string fileName, byte[] fileBytes, string fileContentType = "b2/x-auto")
        {
            bool success = false;
            int maxfailures = 2;        //AS per the API recomendation
            while (!success)
            {
                //Generate the sha1 hash required for the upload
                SHA1 sha1 = SHA1.Create();
                byte[] hashData = sha1.ComputeHash(fileBytes, 0, fileBytes.Length);

                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashData)
                {
                    sb.Append(b.ToString("x2"));
                }
                string sha1Str = sb.ToString();

                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", uploadURL.authorizationToken);
                client.DefaultRequestHeaders.Add("X-Bz-File-Name", fileName);
                client.DefaultRequestHeaders.Add("X-Bz-Content-Sha1", sha1Str);

                HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Post, uploadURL.uploadUrl);
                msg.Content = new ByteArrayContent(fileBytes);
                msg.Content.Headers.ContentLength = fileBytes.Length;
                msg.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(fileContentType);
                var response = await client.SendAsync(msg);

                if (!response.IsSuccessStatusCode)
                {
                    if (maxfailures < 0)
                    {
                        B2Error error = JsonConvert.DeserializeObject<B2Error>(await response.Content.ReadAsStringAsync());
                        throw new B2Exception(error.message)
                        {
                            HttpStatusCode = error.status,
                            ErrorCode = error.code
                        };
                    }
                    maxfailures--;
                }
                else
                {
                    var json = await response.Content.ReadAsStringAsync();
                    B2File uploadedFile = JsonConvert.DeserializeObject<B2File>(json);
                    return uploadedFile;
                }
            }

            return null; //should never get here
        }

        /// <summary>
        /// Gets a file upload url for a multi part file upload
        /// </summary>
        /// <param name="fileid">ID profided by start file part</param>
        /// <returns>B2UploadPartUrl object</returns>
        public async Task<B2UploadPartUrl> GetUploadPartURL(string fileid)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", _authToken);
            string content = "{\"fileId\":\"" + fileid + "\"}";
            var response = await client.PostAsync(_apiURL + "/b2api/v1/b2_get_upload_part_url", new StringContent(content));
            if (!response.IsSuccessStatusCode)
            {
                B2Error error = JsonConvert.DeserializeObject<B2Error>(await response.Content.ReadAsStringAsync());
                throw new B2Exception(error.message)
                {
                    HttpStatusCode = error.status,
                    ErrorCode = error.code
                };
            }
            else
            {
                var json = await response.Content.ReadAsStringAsync();
                B2UploadPartUrl uploadurl = JsonConvert.DeserializeObject<B2UploadPartUrl>(json);
                return uploadurl;
            }
        }

        /// <summary>
        /// Starts the large file upload process
        /// </summary>
        /// <param name="bucket">Bucket object to upload to</param>
        /// <param name="fileName">The name of the file</param>
        /// <param name="fileContentType">The MIME type of the content of the file, which will be returned in the Content-Type header when downloading 
        ///                               the file. Use the Content-Type b2/x-auto to automatically set the stored Content-Type post upload. In the case 
        ///                               where a file extension is absent or the lookup fails, the Content-Type is set to application/octet-stream.</param>
        /// <returns>B2File object</returns>
        public async Task<B2File> StartLargeFile(B2Bucket bucket, string fileName, string fileContentType)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", _authToken);
            string content =    "{\"bucketId\":\"" + bucket.bucketId + "\",\n" +
                                "\"fileName\":\"" + fileName + "\",\n" +
                                "\"contentType\":\"" + fileContentType + "\"}";
            var response = await client.PostAsync(_apiURL + "/b2api/v1/b2_start_large_file", new StringContent(content));
            if (!response.IsSuccessStatusCode)
            {
                B2Error error = JsonConvert.DeserializeObject<B2Error>(await response.Content.ReadAsStringAsync());
                throw new B2Exception(error.message)
                {
                    HttpStatusCode = error.status,
                    ErrorCode = error.code
                };
            }
            else
            {
                var json = await response.Content.ReadAsStringAsync();
                B2File deletedFile = JsonConvert.DeserializeObject<B2File>(json);
                return deletedFile;
            }
        }

        /// <summary>
        /// Uploads one part of a large file to B2, using an file ID obtained from StartLargeFile
        /// </summary>
        /// <param name="largeFile">B2File object from StartLargeFile</param>
        /// <param name="uploadURL">UploadPartURL object from GetUploadPartURL</param>
        /// <param name="fileBytes">File data to upload</param>
        /// <param name="sha1Str">SHA1 checksum for the upload</param>
        /// <param name="fileContentType">The MIME type of the content of the file, which will be returned in the Content-Type header when downloading 
        ///                               the file. Use the Content-Type b2/x-auto to automatically set the stored Content-Type post upload. In the case 
        ///                               where a file extension is absent or the lookup fails, the Content-Type is set to application/octet-stream.</param>
        /// <param name="partNumber">A number from 1 to 10000. The parts uploaded for one file must have contiguous numbers, starting with 1. </param>
        /// <returns>True on success</returns>
        private async Task<B2FilePart> UploadPart(B2File largeFile, B2UploadPartUrl uploadURL, byte[] fileBytes, string fileContentType, int partNumber)
        {
            bool success = false;
            int maxfailures = 2;        //AS per the API recomendation
            while (!success)
            {
                //Generate the sha1 hash required for the uploa
                string sha1Str = GetSha1Checksum(fileBytes);
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", uploadURL.authorizationToken);
                client.DefaultRequestHeaders.Add("X-Bz-Content-Sha1", sha1Str);
                client.DefaultRequestHeaders.Add("X-Bz-Part-Number", partNumber.ToString());

                HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Post, uploadURL.uploadUrl);
                msg.Content = new ByteArrayContent(fileBytes);
                msg.Content.Headers.ContentLength = fileBytes.Length;
                msg.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(fileContentType);
                var response = await client.SendAsync(msg);


                if (!response.IsSuccessStatusCode)
                {
                    if (maxfailures < 0)
                    {
                        B2Error error = JsonConvert.DeserializeObject<B2Error>(await response.Content.ReadAsStringAsync());
                        throw new B2Exception(error.message)
                        {
                            HttpStatusCode = error.status,
                            ErrorCode = error.code
                        };
                    }
                    maxfailures--;
                }
                else
                {
                    var json = await response.Content.ReadAsStringAsync();
                    B2FilePart uploadedFile = JsonConvert.DeserializeObject<B2FilePart>(json);
                    return uploadedFile;
                }
            }

            return null; //should never get here
        }

        /// <summary>
        /// Converts the parts that have been uploaded into a single B2 file.
        /// </summary>
        /// <param name="sha1array">An array of hex SHA1 checksums of the parts of the large file. This is a double-check that the right parts were 
        ///                         uploaded in the right order, and that none were missed. Note that the part numbers start at 1, and the SHA1 of the
        ///                         part 1 is the first string in the array, at index 0. </param>
        /// <param name="fileUpload">B2File object from StartLargeFile</param>
        /// <returns>B2File object of the new file</returns>
        public async Task<B2File> FinishLargeFile(List<string> sha1array, B2File fileUpload)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", _authToken);
            string content = "{\"fileId\":\"" + fileUpload.fileId + "\",\n" +
                                "\"partSha1Array\":\n" +
                                "[\n";
            foreach (string sha1 in sha1array)
            {
                content = content + "\"" + sha1 + "\",\n";
            }
            content = content.Substring(0, content.Length - 2);
            content = content + "]\n}\n";
            var response = await client.PostAsync(_apiURL + "/b2api/v1/b2_finish_large_file", new StringContent(content));
            if (!response.IsSuccessStatusCode)
            {
                B2Error error = JsonConvert.DeserializeObject<B2Error>(await response.Content.ReadAsStringAsync());
                throw new B2Exception(error.message)
                {
                    HttpStatusCode = error.status,
                    ErrorCode = error.code
                };
            }
            else
            {
                var json = await response.Content.ReadAsStringAsync();
                B2File deletedFile = JsonConvert.DeserializeObject<B2File>(json);
                return deletedFile;
            }
        }

        /// <summary>
        /// Cancels the upload of a large file, and deletes all of the parts that have been uploaded.
        /// </summary>
        /// <param name="fileUpload">B2File object to cancel</param>
        /// <returns>B2File object of the canceled upload</returns>
        public async Task<B2File> CancelLargeFile(B2File fileUpload)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", _authToken);
            string content = "{\"fileId\":\"" + fileUpload.fileId + "\"}";
            var response = await client.PostAsync(_apiURL + "/b2api/v1/b2_cancel_large_file", new StringContent(content));
            if (!response.IsSuccessStatusCode)
            {
                B2Error error = JsonConvert.DeserializeObject<B2Error>(await response.Content.ReadAsStringAsync());
                throw new B2Exception(error.message)
                {
                    HttpStatusCode = error.status,
                    ErrorCode = error.code
                };
            }
            else
            {
                var json = await response.Content.ReadAsStringAsync();
                B2File canceledFile = JsonConvert.DeserializeObject<B2File>(json);
                return canceledFile;
            }
        }

        /// <summary>
        /// Lists information about large file uploads that have been started, but have not been finished or canceled.
        /// </summary>
        /// <param name="bucket">The bucket to look for file names in.</param>
        /// <param name="startFromFileName">The first upload to return. If there is an upload with this ID, it will be 
        ///                                 returned in the list. If not, the first upload after this the first one after this ID. 
        ///                                 </param>
        /// <param name="returncount">The maximum number of files to return from this call. The default value is 100, and the maximum allowed is 100.</param>
        /// <returns>A List of file uploads that have been started, but have not been finished or canceled.</returns>
        public async Task<List<B2File>> ListUnfinishedLargeFiles(B2Bucket bucket, string startFromFileName = "", int returncount = 100)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", _authToken);
            string content = "{\"bucketId\":\"" + bucket.bucketId + "\",\n" +
                            "\"startFileName\":\"" + startFromFileName + "\",\n" +
                            "\"maxFileCount\":" + returncount.ToString() + "}";
            var response = await client.PostAsync(_apiURL + "/b2api/v1/b2_list_unfinished_large_files", new StringContent(content));
            if (!response.IsSuccessStatusCode)
            {
                B2Error error = JsonConvert.DeserializeObject<B2Error>(await response.Content.ReadAsStringAsync());
                throw new B2Exception(error.message)
                {
                    HttpStatusCode = error.status,
                    ErrorCode = error.code
                };
            }
            else
            {
                var json = await response.Content.ReadAsStringAsync();
                Dictionary<string, List<B2File>> values = JsonConvert.DeserializeObject<Dictionary<string, List<B2File>>>(json);
                List<B2File> files = values["files"];
                return files;
            }
        }

        #region HelperFunctions
        /// <summary>
        /// Returns the SHA1 checksume of the provided file bytes
        /// </summary>
        /// <param name="fileBytes">Data to calulate checksum</param>
        /// <returns>hex String of the checksum</returns>
        public string GetSha1Checksum(byte[] fileBytes)
        {
            SHA1 sha1 = SHA1.Create();
            byte[] hashData = sha1.ComputeHash(fileBytes, 0, fileBytes.Length);

            StringBuilder sb = new StringBuilder();
            foreach (byte b in hashData)
            {
                sb.Append(b.ToString("x2"));
            }
            string sha1Str = sb.ToString();
            return sha1Str;
        }

        /// <summary>
        /// Upload a large file in chunks
        /// </summary>
        /// <param name="bucket">Bucket object to upload to</param>
        /// <param name="fileStream">Filetream to upload</param>
        /// <param name="fileName">Name of file to save in the bucket</param>
        /// <param name="threads">Number of simultaneous upload threads</param>
        /// <param name="blockSize">Block size for each file part, minimum block size is 100000000 bytes (100mb) </param>
        /// <param name="fileContentType">The MIME type of the content of the file, which will be returned in the Content-Type header when downloading 
        ///                               the file. Use the Content-Type b2/x-auto to automatically set the stored Content-Type post upload. In the case 
        ///                               where a file extension is absent or the lookup fails, the Content-Type is set to application/octet-stream.</param>
        /// <returns>B2File object of the new file</returns>
        public async Task<B2File> UploadLargeFile(B2Bucket bucket, Stream fileStream, string fileName, int threads = 2, int blockSize=100000000, string fileContentType = "b2/x-auto")
        {
            B2File largefile = (await StartLargeFile(bucket, fileName, fileContentType));
            int filepart = 1;
            int lastSizeRead = blockSize;
            List<string> sha1values = new List<string>();
            Task<B2FilePart>[] tasks = new Task<B2FilePart>[threads];
            B2UploadPartUrl[] uploadURL = new B2UploadPartUrl[threads];

            for (int i = 0; i < threads; i++)
            {
                if (lastSizeRead == blockSize)
                {
                    byte[] bytes = new byte[blockSize];
                    lastSizeRead = fileStream.ReadAsync(bytes, 0, blockSize).Result;
                    string sha1 = GetSha1Checksum(bytes);                   
                    uploadURL[i] = GetUploadPartURL(largefile.fileId).Result;                  
                    tasks[i] = UploadPart(largefile, uploadURL[i], bytes, fileContentType, filepart);
                    filepart++;
                }
            }

            while (lastSizeRead == blockSize)
            {
                for (int i = 0; i < threads; i++)
                {
                    if (tasks[i].IsCompleted)
                    {
                        if (tasks[i].Result.partNumber > sha1values.Count)
                            sha1values.Add(tasks[i].Result.contentSha1);
                        else
                            sha1values.Insert(tasks[i].Result.partNumber, tasks[i].Result.contentSha1);
                        if (lastSizeRead == blockSize)
                        {
                            byte[] bytes = new byte[blockSize];
                            lastSizeRead = fileStream.ReadAsync(bytes, 0, blockSize).Result;
                            string sha1 = GetSha1Checksum(bytes);
                            sha1values.Add(sha1);
                            tasks[i] = UploadPart(largefile, uploadURL[i], bytes, fileContentType, filepart);
                            filepart++;
                        }
                        else
                            tasks[i] = null;
                    }
                }
            }

            for (int i = 0; i < threads; i++)
            {
                if (tasks[i] != null)
                {
                    tasks[i].Wait();
                    if (tasks[i].Result.partNumber > sha1values.Count)
                        sha1values.Add(tasks[i].Result.contentSha1);
                    else
                        sha1values.Insert(tasks[i].Result.partNumber, tasks[i].Result.contentSha1);
                }
            }

            return FinishLargeFile(sha1values, largefile).Result;
        }
        #endregion
    }
}

