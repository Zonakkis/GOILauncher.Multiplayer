using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GOILauncher.Multiplayer.Core.Log
{
    public interface ILogger<T>
    {
        void Info(string message);
        void Info(string message, params object[] args);
        void Debug(string message);
        void Debug(string message, params object[] args);
        void Warn(string message);
        void Warn(string message, params object[] args);
        void Error(string message, Exception ex = null);
        void Error(string message, Exception ex = null, params object[] args);
    }
}
