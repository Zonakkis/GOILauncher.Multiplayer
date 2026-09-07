using System.Reflection;
using Autofac;
using FluentAssertions;
using GOILauncher.Multiplayer.Core.Log;
using GOILauncher.Multiplayer.Unity;
using GOILauncher.Multiplayer.Unity.Config;
using Moq;
using NUnit.Framework;

namespace GOILauncher.Multiplayer.Tests.Unity
{
    [TestFixture, NonParallelizable]
    public class MultiplayerUnityCoreTests
    {
        [Test]
        public void Settings_ReturnsContainerInstanceAndSharesUpdates()
        {
            var store = new Mock<ISettingsStore>();
            var settings = new MultiplayerSettings(store.Object, new Mock<ILogger<MultiplayerSettings>>().Object);
            var builder = new ContainerBuilder();
            builder.RegisterInstance(settings).AsSelf().As<IMultiplayerState>();

            using (var container = builder.Build())
            {
                // Initialize needs a real Unity runtime. Install only the test container, and restore
                // the shared static field even if an assertion fails. This fixture must not run in parallel.
                var containerField = typeof(MultiplayerUnityCore).GetField("_container", BindingFlags.Static | BindingFlags.NonPublic);
                containerField.Should().NotBeNull();
                var previousContainer = containerField.GetValue(null);
                try
                {
                    containerField.SetValue(null, container);
                    var resolved = container.Resolve<MultiplayerSettings>();
                    var notifications = 0;
                    resolved.EnabledChanged += _ => notifications++;

                    MultiplayerUnityCore.Settings.Should().BeSameAs(resolved);
                    MultiplayerUnityCore.Settings.SetEnabled(false);

                    container.Resolve<IMultiplayerState>().Enabled.Should().BeFalse();
                    notifications.Should().Be(1);
                    store.Verify(s => s.Write(MultiplayerSettings.EnabledKey, "false"), Times.Once);
                    MultiplayerUnityCore.Settings.Should().BeSameAs(resolved);
                }
                finally
                {
                    containerField.SetValue(null, previousContainer);
                }
            }
        }
    }
}
