using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Redis;
using StackExchange.Redis;
using StressTest.App.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StressTest.App
{
    class Program
    {
        static void Main(string[] args)
        {

            string data;
            using (var sr = new StreamReader(@"Sample.json"))
            {
                data = sr.ReadToEnd();
            }
            //data = "test";
            var logger = new AppLogger();
           

            var executor = new RedisExecutor(logger);

            int.TryParse(ConfigurationManager.AppSettings["threadsNumber"], out int threadsNumber);
            int.TryParse(ConfigurationManager.AppSettings["operationsNumber"], out int operationsNumber);
            int.TryParse(ConfigurationManager.AppSettings["secondsNumber"], out int secondsToRun);

            int[] tasks = Enumerable.Range(1, threadsNumber).ToArray();

            logger.Info($"Starting with {threadsNumber} threads and {operationsNumber} operations.");
            var timeUp = TimeSpan.FromSeconds(secondsToRun);

            RedisCacheW.Instance.RedisCache.Get("asasjakjsa");

            Parallel.ForEach(tasks, t => {
                var watcher = new Stopwatch();
                watcher.Start();
                do {
                    Task.Factory
                    .StartNew(() =>
                    {
                        executor.ExecuteInsertsAndReads(data, operationsNumber);
                    })
                    .Wait();
                    Console.Clear();
                    Console.WriteLine($"Elapsed: {watcher.Elapsed} of {timeUp}");
                }
                while (watcher.Elapsed < timeUp);
                
            });


            Console.WriteLine("Press any key...");
            Console.ReadLine();
        }



    }
}
