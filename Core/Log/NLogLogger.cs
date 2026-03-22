using NLog;
using System;

namespace GOILauncher.Multiplayer.Core.Log
{
    public class NLogLogger<T> : ILogger<T>
    {
        private readonly Logger _logger;

        public NLogLogger()
        { 
            _logger = LogManager.GetLogger(typeof(T).FullName);
        }
        public void Info(string message)
        {
            _logger.Info(message);
        }
        public void Debug(string message)
        {
            _logger.Debug(message);
        }
        public void Warn(string message)
        {
            _logger.Warn(message);
        }
        public void Error(string message, Exception ex = null)
        {
            _logger.Error(message, ex);
        }

    }
}
