# A simple API for Backblazes B2 service that uses .net core

### Simple Example:
```c#
 //Creat a new API object
B2API.B2API t = new B2API.B2API();
//Authorize B2 account
bool r = t.AuthorizeAccount("", "").Result;
//get a list of buckets
List<B2API.B2Bucket> buckets =  t.ListBuskets().Result;
//get a list of files in the first bucket
List<B2API.B2File> files = t.ListFileNames(buckets[1]).Result;
//download first file in first bucket
var dl = t.DownloadFileByID(files[0], "test.jpg");
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
    
    byte[] bytes = File.ReadAllBytes("test.jpg");
    var ul = t.UploadFile(buckets[1], t.GetUploadURL(buckets[0]).Result, "uploadtest.jpg", bytes);
    //wait for file upload to finish
    Console.Write("Uploading...");
    while (!ul.IsCompleted)
    {
        Console.Write(".");
        System.Threading.Thread.Sleep(500);
    }
}
Console.WriteLine("Complete");
```