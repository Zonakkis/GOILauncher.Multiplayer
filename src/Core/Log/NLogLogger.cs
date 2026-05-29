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
        public void Info(string message, params object[] args)
        {
            _logger.Info(message, args);
        }
        public void Debug(string message)
        {
            _logger.Debug(message);
        }
        public void Debug(string message, params object[] args)
        {
            _logger.Debug(message, args);
        }
        public void Warn(string message)
        {
            _logger.Warn(message);
        }
        public void Warn(string message, params object[] args)
        {
            _logger.Warn(message, args);
        }
        public void Error(string message)
        {
            _logger.Error(message);
        }

        public void Error(string message, params object[] args)
        {
            _logger.Error(message, args);
        }

        public void Error(Exception ex, string message)
        {
            _logger.Error(ex, message);
        }

        public void Error(Exception ex, string message, params object[] args)
        {
            _logger.Error(ex, message, args);
        }
    }
}
