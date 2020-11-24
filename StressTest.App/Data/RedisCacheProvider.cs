using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Redis;
using StackExchange.Redis;
using System;
using System.Configuration;

namespace StressTest.App.Data
{
    public sealed class Database
    {
        private static readonly Lazy<IDatabase>
            lazy =
            new Lazy<IDatabase>
                (() =>
                {
                    ConnectionMultiplexer muxer = ConnectionMultiplexer.Connect(ConfigurationManager.AppSettings["connectionString"] as String);
                    Console.WriteLine("Connecting...");
                    var db = muxer.GetDatabase();
                    while (true)
                    {
                        try
                        {
                            db.StringGet("0");
                            break;
                        }
                        catch (Exception e)
                        {
                            if (!(e is RedisTimeoutException || e is RedisServerException || e is RedisConnectionException))
                                break;
                        }
                    }
                    Console.WriteLine("Connected.");
                    return muxer.GetDatabase();
                });

        public static IDatabase Instance { get { return lazy.Value; } }

        private Database()
        {
        }
    }

    public class RedisCacheProvider : IRedisCacheProvider
    {

        public static string NoValueFound = "No value found for specific key";
        public static string Success = "Success!";
        public static string ErrorSet = "Error! The object could not be set";

        private const int MAX_RETRIES = 3;
        public AppLogger logger;
        public RedisCacheProvider(AppLogger logger)
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
                    if (Database.Instance.Multiplexer.IsConnected)
                    {
                        var redisValue = Database.Instance.StringGet(key);
                        var message = redisValue.IsNullOrEmpty ? NoValueFound : Success;
                        logger.Info($"Geting item {key} : {Success} (from {MAX_RETRIES - retries + 1})");
                        break;
                    }
                    else
                    {
                        retries--;
                        if (retries == 0)
                            logger.Info($"Get failed...{MAX_RETRIES} retries");
                    }
                }
                catch (Exception e)
                {
                    retries--;
                    if (retries == 0)
                        logger.Error(e, $"{e.Message}, {e.StackTrace}");

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
                    if (Database.Instance.Multiplexer.IsConnected)
                    {
                        var wasSet = Database.Instance.StringSet(key, item, expiry: TimeSpan.FromSeconds(600));
                        var message = wasSet ? Success : ErrorSet;
                        logger.Info($"Setting item {key} : {message} (from { MAX_RETRIES - retries + 1})");
                        break;
                    }
                    else
                    {
                        retries--;
                        if (retries == 0)
                            logger.Info($"Set failed...{MAX_RETRIES} retries");
                    }
                }
                catch (Exception e)
                {
                    retries--;
                    if (retries == 0)
                        logger.Error(e, $"{e.Message}, {e.StackTrace}");
                }
            }
        }

        //public void DeleteItem(string key)
        //{
        //    try
        //    {
        //        var wasDeleted = Database.Instance.KeyDelete(key);
        //        var message = wasDeleted ? Success : ErrorDelete;
        //        logger.Info($"Deleting item {key} : {message}");
        //    }
        //    catch (Exception e)
        //    {
        //        logger.Error(e,e.Message);
        //    }            
        //}


    }
}
