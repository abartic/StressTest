using StressTest.App.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StressTest.App
{
    public class RedisExecutor
    {
        public IRedisCacheProvider CacheProvider { get; set; }

        
        private AppLogger logger;
        public RedisExecutor(AppLogger logger)
        {
            this.logger = logger;
            //CacheProvider = new RedisCacheProvider(this.logger);
            //CacheProvider = new RedisWCacheProvider(this.logger);
            CacheProvider = (ConfigurationManager.AppSettings["provider"] as String) == "W" ?
                new RedisWCacheProvider(this.logger) as IRedisCacheProvider :
                new RedisCacheProvider(this.logger) as IRedisCacheProvider;
        }

        public void ExecuteInsertsAndReads(string value, int numberOfOperations)
        {
            try
            {
                for (int index = 0; index < numberOfOperations; index += 1)
                {
                    CacheProvider.SetItem($"Key-{index}", value);
                    CacheProvider.GetItem($"Key-{index}");
                }
            }
            catch (Exception e)
            {
                logger.Error(e);
                throw;
            }
        }

        public void ExecuteInserts(string value, int numberOfOperations)
        {
            try
            {
                for (int index = 0; index < numberOfOperations; index += 1)
                {
                    CacheProvider.SetItem($"Key-{index}", value);
                }
            }
            catch (Exception e)
            {
                logger.Error(e);
                throw;
            }
        }

        public void ExecuteReads(int numberOfOperations)
        {
            try
            {
                for (int index = 0; index < numberOfOperations; index += 1)
                {
                    
                    CacheProvider.GetItem($"Key-{index}");
                }
            }
            catch (Exception e)
            {
                logger.Error(e);
                throw;
            }
        }

        //public void ExecuteDeletes(ConcurrentStack<string> keys, int numberOfTimes)
        //{
        //    try
        //    {
        //        for (int index = 0; index < numberOfTimes; index += 1)
        //        {
        //            keys.TryPop(out string key);
        //            CacheProvider.DeleteItem(key);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        logger.Error(e);
        //        throw;
        //    }
        //}

    }
}
