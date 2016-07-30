using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APITest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //Creat a new API object
            B2API.B2API t = new B2API.B2API();

            //Authorize B2 account
            bool r = t.AuthorizeAccount("", "").Result;

            //get a list of buckets
            List<B2API.B2Bucket> buckets =  t.ListBuskets().Result;

            //get a list of files in the first bucket
            List<B2API.B2File> files = t.ListFiles(buckets[0]).Result;

            //download first file in first bucket
            var dl = t.DownloadFile(files[0], "test.jpg");
            
            //wait for download to finish
            Console.WriteLine();
            Console.Write("Downloading...");  
            while (!dl.IsCompleted)
            {
                Console.Write(".");
                System.Threading.Thread.Sleep(500);
            }
            Console.WriteLine("Complete");

            //upload file to bucket
            using (var fileStream = new System.IO.FileStream("test.jpg", System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                var ul = t.UploadLargeFile(buckets[1], fileStream, "largetest.jpg", 2);
                //wait for file upload to finish
                Console.Write("Uploading...");
                while (!ul.IsCompleted)
                {
                    Console.Write(".");
                    System.Threading.Thread.Sleep(500);
                }
            }
            Console.WriteLine("Complete");
        }
    }
}
