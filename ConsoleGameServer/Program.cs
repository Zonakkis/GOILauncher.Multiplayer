using GOILauncher.Multiplayer.Server;
using GOILauncher.Multiplayer.Server.Interfaces;

namespace ConsoleGameServer
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            IGameServer gameServer = new ConsoleServer();
            gameServer.Start(9050);
            while (gameServer.IsRunning)
            {
                Console.ReadLine();
            }
        }
    }
}
