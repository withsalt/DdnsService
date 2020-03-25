using NLog;
using System;

namespace Logger
{
    public class Log
    {
        private static readonly NLog.Logger logger = null;

        private static readonly object _locker = new object();

        static Log()
        {
            if (logger == null)
            {
                lock (_locker)
                {
                    if (logger == null)
                    {
                        logger = LogManager.GetCurrentClassLogger();
                    }
                }
            }
        }

        /// <summary>
        /// 普通日志
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public static string Info(string message, Exception exception = null)
        {
            if (exception == null)
                logger.Info(message);
            else
                logger.Info(exception, message);
            return message;
        }

        /// <summary>
        /// 告警日志
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public static string Warn(string message, Exception exception = null)
        {
            if (exception == null)
                logger.Warn(message);
            else
                logger.Warn(exception, message);
            return message;
        }

        /// <summary>
        /// 错误日志
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public static string Error(string message, Exception exception = null)
        {
            if (exception == null)
                logger.Error(message);
            else
                logger.Error(exception, message);
            return message;
        }
    }
}
