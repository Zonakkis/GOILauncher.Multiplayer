using GOILauncher.Multiplayer.Client.Interfaces;
using System;
using System.Globalization;
using GOILauncher.Multiplayer.Client;

namespace ConsoleGameClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            IGameClient client = new ConsoleClient();
            client.Start("localhost", 9050, "zonakkis");
            while (Console.ReadKey(true).Key != ConsoleKey.Q)
            {
                client.SendChatMessage(DateTime.Now.ToString(CultureInfo.CurrentCulture));
            }
        }
    }
}
