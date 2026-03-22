using Autofac;
using GOILauncher.Multiplayer.Client.Extensions;
using GOILauncher.Multiplayer.Client.Services;
using UnityEngine;

namespace GOILauncher.Multiplayer.Client
{
    public class UnityClient : MonoBehaviour
    {
        private IContainer _container;
        private IClientService _clientService;
        private void Awake()
        {
            var builder = new ContainerBuilder();

            builder.RegisterMultiplayerClient();

            _container = builder.Build();
            _clientService = _container.Resolve<IClientService>();
        }

        private void OnDestroy()
        {
            _container.Dispose();
        }

        public void Connect(string host, int port)
        {
            _clientService.Connect(host, port);
        }

        public void Disconnect()
        {
            _clientService.Disconnect();
        }
    }
}
