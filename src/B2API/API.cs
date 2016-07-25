using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using System.IO;

namespace B2API
{
    public class B2API
    {

        private string _accountId;
        private string _applicationKey;
        private string _authToken;
        private string _apiURL;
        private string _downloadUrl;

        private string _credentials { get { return Convert.ToBase64String(Encoding.UTF8.GetBytes(_accountId + ":" + _applicationKey)); } }

        public B2API()
        {
            _accountId = "";
            _applicationKey = "";
            _authToken = "";
            _apiURL = "";
            _downloadUrl = "";
        }

        /// <summary>
        /// Authorizes the user accoung and key and gets the api url
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
                throw new B2Exception(response.Content.ToString())
                {
                    HttpStatusCode = response.StatusCode.ToString(),
                    ErrorCode = response.ReasonPhrase
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
        /// Gets a list of the users current buckets
        /// </summary>
        /// <returns>A list of Buckets</returns>
        public async Task<List<B2Bucket>> ListBuskets()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", _authToken);
            var response = await client.PostAsync(_apiURL + "/b2api/v1/b2_list_buckets", new StringContent("{\"accountId\":\"" + _accountId + "\"}"));
            if (!response.IsSuccessStatusCode)
            {
                throw new B2Exception(response.Content.ToString())
                {
                    HttpStatusCode = response.StatusCode.ToString(),
                    ErrorCode = response.ReasonPhrase
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
        /// Creats a new bucket
        /// </summary>
        /// <param name="bucketName">Name of the new bucket between 6 and 5 chars</param>
        /// <param name="bucketType">Bucket type, private or public</param>
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
                throw new B2Exception(response.Content.ToString())
                {
                    HttpStatusCode = response.StatusCode.ToString(),
                    ErrorCode = response.ReasonPhrase
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
        /// Updates the bucket type (Private/Public)
        /// </summary>
        /// <param name="bucket">Bucket you would like to change</param>
        /// <param name="newBucketType">New bucket type</param>
        /// <returns>Modified bucket</returns>
        public async Task<B2Bucket> ChangeBucketType(B2Bucket bucket, B2BucketType newBucketType)
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
                throw new B2Exception(response.Content.ToString())
                {
                    HttpStatusCode = response.StatusCode.ToString(),
                    ErrorCode = response.ReasonPhrase
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
        /// Returns a list of files
        /// </summary>
        /// <param name="bucket">Bucket to get list of files from</param>
        /// <param name="startFromFileName">Optional: start the list from this file name (i.e. the last file returned)</param>
        /// <param name="returncount">Optional: Number of file names to return max: 1000 default: 1000</param>
        /// <returns>A List of file objects</returns>
        public async Task<List<B2File>> ListFiles(B2Bucket bucket, string startFromFileName = "", int returncount = 1000)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", _authToken);
            string content ="{\"bucketId\":\"" + bucket.bucketId + "\",\n" +
                            "\"startFileName\":\"" + startFromFileName + "\",\n" +
                            "\"maxFileCount\":" + returncount.ToString() + "}";
            var response = await client.PostAsync(_apiURL + "/b2api/v1/b2_list_file_names", new StringContent(content));
            if (!response.IsSuccessStatusCode)
            {
                throw new B2Exception(response.Content.ToString())
                {
                    HttpStatusCode = response.StatusCode.ToString(),
                    ErrorCode = response.ReasonPhrase
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
        /// Downloads the provided file. this function allowes for downloading large files without storing in memory
        /// </summary>
        /// <param name="file">File object to download</param>
        /// <param name="filename">filename to save file as</param>
        /// <returns>Task object</returns>
        public async Task DownloadFile(B2File file, string filename)
        {
            var client = new HttpClient();
            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Post, _downloadUrl + "/b2api/v1/b2_download_file_by_id");
            msg.Headers.Add("Authorization", _authToken);
            msg.Content = new StringContent("{\"fileId\":\"" + file.fileId + "\"}");
            HttpResponseMessage response = await client.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead);
            if (!response.IsSuccessStatusCode)
            {
                throw new B2Exception(response.Content.ToString())
                {
                    HttpStatusCode = response.StatusCode.ToString(),
                    ErrorCode = response.ReasonPhrase
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
        /// Deletes the provided file
        /// </summary>
        /// <param name="file">file object to delete</param>
        /// <returns>file object deleted</returns>
        public async Task<B2File> DeleteFile(B2File file)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", _authToken);
            string content = "{\"fileName\":\"" + file.fileName + "\",\n" +
                             "\"fileId\":\"" + file.fileId + "\"}";
            var response = await client.PostAsync(_apiURL + "/b2api/v1/b2_delete_file_version", new StringContent(content));
            if (!response.IsSuccessStatusCode)
            {
                throw new B2Exception(response.Content.ToString())
                {
                    HttpStatusCode = response.StatusCode.ToString(),
                    ErrorCode = response.ReasonPhrase
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
        /// <param name="bucket">bucket uploading to</param>
        /// <returns>upload url object</returns>
        private async Task<B2UploadUrl> GetUploadURL(B2Bucket bucket)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", _authToken);
            string content = "{\"bucketId\":\"" + bucket.bucketId + "\"}";
            var response = await client.PostAsync(_apiURL + "/b2api/v1/b2_get_upload_url", new StringContent(content));
            if (!response.IsSuccessStatusCode)
            {
                throw new B2Exception(response.Content.ToString())
                {
                    HttpStatusCode = response.StatusCode.ToString(),
                    ErrorCode = response.ReasonPhrase
                };
            }
            else
            {
                var json = await response.Content.ReadAsStringAsync();
                B2UploadUrl uploadurl = JsonConvert.DeserializeObject<B2UploadUrl>(json);
                return uploadurl;
            }
        }
    }
}

