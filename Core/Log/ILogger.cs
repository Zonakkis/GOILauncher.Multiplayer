using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GOILauncher.Multiplayer.Core.Log
{
    public interface ILogger<T>
    {
        void Info(string message);
        void Debug(string message);
        void Warn(string message);
        void Error(string message, Exception ex = null);
    }
}
