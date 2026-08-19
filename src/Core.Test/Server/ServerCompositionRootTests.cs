using System.Collections.Generic;
using System.Linq;
using Autofac;
using FluentAssertions;
using GOILauncher.Multiplayer.Core.Extensions;
using GOILauncher.Multiplayer.Server.Extensions;
using GOILauncher.Multiplayer.Server.Services;
using GOILauncher.Multiplayer.Server.Synchronization;
using NUnit.Framework;

namespace GOILauncher.Multiplayer.Core.Test.Server
{
    /// <summary>
    /// Guards the composition root itself: every server module registers its packet
    /// callbacks in IStartable.Start(), so a module that is registered but never
    /// activated fails silently at runtime. ChatService was exactly that case before.
    /// </summary>
    [TestFixture]
    public class ServerCompositionRootTests
    {
        private IContainer BuildServerContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterMultiplayerCore().WithServer();
            return builder.Build();
        }

        [Test]
        public void ServerContainer_Builds()
        {
            using (var container = BuildServerContainer())
            {
                container.Resolve<IServerService>().Should().NotBeNull();
            }
        }

        [Test]
        public void EveryServerModule_IsActivatedByTheContainer()
        {
            using (var container = BuildServerContainer())
            {
                var started = container.Resolve<IEnumerable<IStartable>>()
                    .Select(s => s.GetType())
                    .ToList();

                started.Should().Contain(typeof(PlayerService));
                started.Should().Contain(typeof(ServerService));
                started.Should().Contain(typeof(PlayerStateRelay));
                // Regression guard: ChatService used to be reachable only through the
                // Unity host's property injection, so ConsoleServer never relayed chat.
                started.Should().Contain(typeof(ChatService));
            }
        }
    }
}
