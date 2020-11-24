using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StressTest.App
{
    public class AppLogger
    {
        private readonly Logger logger;

        public AppLogger()
        {
            logger = LogManager.GetLogger("appLoggerRules");
        }
        public void Info(string message)
        {
            logger.Info(message);
        }
        public void Error(Exception exception, string message = null)
        {
            logger.Error(exception, message);
        }
    }
}
