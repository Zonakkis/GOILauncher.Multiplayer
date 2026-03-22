using Autofac;
using GOILauncher.Multiplayer.Client;
using GOILauncher.Multiplayer.Client.Extensions;
using GOILauncher.Multiplayer.Client.Services;
using GOILauncher.Multiplayer.Core.Extensions;
using GOILauncher.Multiplayer.Server.Extensions;
using UnityEngine;
namespace GOILauncher.Multiplayer.Unity
{
    public class UnityCore
    {

        private static IContainer _container;
        private static readonly object _clientLock = new object();
        private static UnityClient _client;

        public static void Init()
        {
            var builder = new ContainerBuilder();

            builder.RegisterMultiplayerCore().WithServer().WithClient();

            _container = builder.Build();
        }

        public static UnityClient UnityClient
        {
            get
            {
                if (_client == null)
                {
                    lock (_clientLock)
                    {
                        if (_client == null)
                        {
                            var unityClient = new GameObject(nameof(UnityClient))
                                .AddComponent<UnityClient>();
                            unityClient.ClientService = _container.Resolve<IClientService>();
                            _client = unityClient;
                        }
                    }
                }
                return _client;
            }
        }

    }
}
