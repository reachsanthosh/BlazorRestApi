﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorApi.Contracts;
using NLog;

namespace BlazorApi.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class LoggerService : ILoggerService
    {
        private static ILogger logger = LogManager.GetCurrentClassLogger();
        public void LogInfo(string message)
        {
            logger.Info(message);
        }

        public void LogWarning(string message)
        {
            logger.Warn(message);
        }

        public void LogError(string message)
        {
            logger.Error(message);
        }
    }
}
