﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APITest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            B2API.B2API t = new B2API.B2API();
            bool r = t.AuthorizeAccount("5685b21a6d74", "001e1849ad73e3d19af02700318ccbf6adf1dea51b").Result;
            List<B2API.B2Bucket> buckets =  t.ListBuskets().Result;
            List<B2API.B2File> files = t.ListFiles(buckets[1]).Result;
            var dl = t.DownloadFile(files[0], "test.jpg");
            Console.WriteLine();
            Console.Write("Downloading...");
            while (!dl.IsCompleted)
            {
                Console.Write(".");
                System.Threading.Thread.Sleep(500);
            }
            Console.WriteLine("Complete");            
        }
    }
}
