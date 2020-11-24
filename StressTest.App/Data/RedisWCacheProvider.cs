using System;
using System.Configuration;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Redis;
using StackExchange.Redis;

namespace StressTest.App.Data
{
    public sealed class RedisCacheW
    {
        public RedisCache RedisCache { get; set; }
        private static readonly Lazy<RedisCacheW>
            lazy =
            new Lazy<RedisCacheW>
                (() =>
                {
                    RedisCacheW rw = new RedisCacheW();
                    rw.RedisCache = new RedisCache(new RedisCacheOptions()
                    {
                        Configuration = ConfigurationManager.AppSettings["connectionString"] as String
                        //,ConfigurationOptions = new ConfigurationOptions() { 
                        //    ConnectRetry =3,
                        //    SyncTimeout = 5000,
                        //    ResponseTimeout = 5000,
                        //    ConnectTimeout = 5000,
                        //    KeepAlive = 1000
                        //}
                    });
                    Console.WriteLine("Connecting...");
                    while (true)
                    {
                        try
                        {
                            rw.RedisCache.GetString("0");
                            break;
                        }
                        catch (Exception e)
                        {
                            if (!(e is RedisTimeoutException || e is RedisServerException || e is RedisConnectionException))
                                break;
                        }
                    }
                    Console.WriteLine("Connected.");
                    return rw;
                });

        public static RedisCacheW Instance { get { return lazy.Value; } }

        private RedisCacheW()
        {
        }
    }

    class RedisWCacheProvider : IRedisCacheProvider
    {
        private const int MAX_RETRIES = 3;
        public AppLogger logger;
        public RedisWCacheProvider(AppLogger logger)
        {
            this.logger = logger;
        }

        public void GetItem(string key)
        {
            int retries = MAX_RETRIES;
            while (retries > 0)
            {
                try
                {
                    var redisValue = RedisCacheW.Instance.RedisCache.GetString(key);
                    logger.Info($"Geting item : ok ({MAX_RETRIES - retries + 1})");
                    break;
                }
                catch (Exception e)
                {
                    retries--;
                    if (retries == 0)
                        logger.Error(e, $"{e.Message}, {e.StackTrace}");
                    //Task.Delay(1000).Wait();
                }
            }
        }

        public void SetItem(string key, string item)
        {
            int retries = MAX_RETRIES;
            while (retries > 0)
            {
                try
                {
                    RedisCacheW.Instance.RedisCache.SetString(key, item);
                    logger.Info($"Seting item  : ok ({MAX_RETRIES - retries + 1})");
                    break;
                }
                catch (Exception e)
                {
                    retries--;
                    if (retries == 0)
                        logger.Error(e, $"{e.Message}, {e.StackTrace}");
                    //Task.Delay(1000).Wait();
                }
            }
        }
    }
}
