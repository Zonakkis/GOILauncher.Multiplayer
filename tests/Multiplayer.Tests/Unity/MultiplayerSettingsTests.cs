using System;
using System.Collections.Generic;
using FluentAssertions;
using GOILauncher.Multiplayer.Core.Log;
using GOILauncher.Multiplayer.Unity.Config;
using Moq;
using NUnit.Framework;

namespace GOILauncher.Multiplayer.Tests.Unity
{
    /// <summary>
    /// MultiplayerSettings owns the names, types and defaults of the persisted settings; the store only
    /// holds strings. The store is a dictionary here, so these tests are about that ownership: what a
    /// missing or corrupt value reads back as, what reaches the store on a write, and who gets told.
    /// </summary>
    [TestFixture]
    public class MultiplayerSettingsTests
    {
        private FakeSettingsStore _store;

        [SetUp]
        public void Setup()
        {
            _store = new FakeSettingsStore();
        }

        [Test]
        public void EmptyStore_UsesDefaults()
        {
            MultiplayerSettings settings = CreateSettings();

            settings.Enabled.Should().Be(MultiplayerSettings.DefaultEnabled);
            settings.ClientHost.Should().Be(MultiplayerSettings.DefaultClientHost);
            settings.ClientPort.Should().Be(MultiplayerSettings.DefaultClientPort);
            settings.ServerPort.Should().Be(MultiplayerSettings.DefaultServerPort);
        }

        [Test]
        public void PersistedValues_AreReadBack()
        {
            _store.Set(MultiplayerSettings.EnabledKey, "false");
            _store.Set(MultiplayerSettings.ClientHostKey, "192.168.1.20");
            _store.Set(MultiplayerSettings.ClientPortKey, "1234");
            _store.Set(MultiplayerSettings.ServerPortKey, "65535");

            MultiplayerSettings settings = CreateSettings();

            settings.Enabled.Should().BeFalse();
            settings.ClientHost.Should().Be("192.168.1.20");
            settings.ClientPort.Should().Be(1234);
            settings.ServerPort.Should().Be(65535);
        }

        // A config file edited by hand must not be able to stop the plugin from loading.
        [TestCase("abc")]
        [TestCase("")]
        [TestCase("0")]
        [TestCase("70000")]
        [TestCase("-1")]
        [TestCase("9027.5")]
        public void UnusablePort_FallsBackToDefaultWithoutThrowing(string raw)
        {
            _store.Set(MultiplayerSettings.ClientPortKey, raw);
            _store.Set(MultiplayerSettings.ServerPortKey, raw);

            MultiplayerSettings settings = null;
            Action construct = () => settings = CreateSettings();

            construct.Should().NotThrow();
            settings.ClientPort.Should().Be(MultiplayerSettings.DefaultClientPort);
            settings.ServerPort.Should().Be(MultiplayerSettings.DefaultServerPort);
        }

        [Test]
        public void BlankPersistedHost_FallsBackToDefault()
        {
            _store.Set(MultiplayerSettings.ClientHostKey, "   ");

            CreateSettings().ClientHost.Should().Be(MultiplayerSettings.DefaultClientHost);
        }

        [Test]
        public void PersistedHost_IsTrimmed()
        {
            _store.Set(MultiplayerSettings.ClientHostKey, "  10.0.0.5  ");

            CreateSettings().ClientHost.Should().Be("10.0.0.5");
        }

        [Test]
        public void SetClientHost_TrimsWritesAndRaises()
        {
            MultiplayerSettings settings = CreateSettings();
            List<string> raised = new List<string>();
            settings.ClientHostChanged += raised.Add;

            settings.SetClientHost("  10.0.0.5  ");

            settings.ClientHost.Should().Be("10.0.0.5");
            _store.Read(MultiplayerSettings.ClientHostKey).Should().Be("10.0.0.5");
            raised.Should().Equal("10.0.0.5");
        }

        // Clearing the field in the UI reads as "back to the default" rather than storing a host that
        // nothing can connect to.
        [Test]
        public void SetClientHost_Blank_StoresTheDefault()
        {
            _store.Set(MultiplayerSettings.ClientHostKey, "10.0.0.5");
            MultiplayerSettings settings = CreateSettings();

            settings.SetClientHost("   ");

            settings.ClientHost.Should().Be(MultiplayerSettings.DefaultClientHost);
            _store.Read(MultiplayerSettings.ClientHostKey).Should().Be(MultiplayerSettings.DefaultClientHost);
        }

        [Test]
        public void SetClientHost_SameValue_DoesNotWriteOrRaise()
        {
            MultiplayerSettings settings = CreateSettings();
            int raised = 0;
            settings.ClientHostChanged += host => raised++;

            settings.SetClientHost(MultiplayerSettings.DefaultClientHost);

            _store.Writes.Should().Be(0);
            raised.Should().Be(0);
        }

        [Test]
        public void SetClientPort_WritesAndRaisesOnce()
        {
            MultiplayerSettings settings = CreateSettings();
            List<int> raised = new List<int>();
            settings.ClientPortChanged += raised.Add;

            settings.SetClientPort(4000);
            settings.SetClientPort(4000);

            settings.ClientPort.Should().Be(4000);
            _store.Read(MultiplayerSettings.ClientPortKey).Should().Be("4000");
            raised.Should().Equal(4000);
        }

        [Test]
        public void SetServerPort_WritesAndRaisesOnce()
        {
            MultiplayerSettings settings = CreateSettings();
            List<int> raised = new List<int>();
            settings.ServerPortChanged += raised.Add;

            settings.SetServerPort(4001);
            settings.SetServerPort(4001);

            settings.ServerPort.Should().Be(4001);
            _store.Read(MultiplayerSettings.ServerPortKey).Should().Be("4001");
            raised.Should().Equal(4001);
        }

        // The UI validates what the player typed, so a port out of range here is a bug rather than input.
        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(70000)]
        public void SetPort_OutOfRange_Throws(int port)
        {
            MultiplayerSettings settings = CreateSettings();

            settings.Invoking(s => s.SetClientPort(port)).Should().Throw<ArgumentOutOfRangeException>();
            settings.Invoking(s => s.SetServerPort(port)).Should().Throw<ArgumentOutOfRangeException>();
            _store.Writes.Should().Be(0);
        }

        [Test]
        public void SetEnabled_StillWritesAndRaises()
        {
            MultiplayerSettings settings = CreateSettings();
            List<bool> raised = new List<bool>();
            settings.EnabledChanged += raised.Add;

            settings.SetEnabled(!MultiplayerSettings.DefaultEnabled);

            settings.Enabled.Should().Be(!MultiplayerSettings.DefaultEnabled);
            raised.Should().Equal(!MultiplayerSettings.DefaultEnabled);
        }

        // One faulty listener must not keep the others from seeing the change.
        [Test]
        public void FailingListener_DoesNotStopTheOthers()
        {
            MultiplayerSettings settings = CreateSettings();
            List<int> raised = new List<int>();
            settings.ClientPortChanged += port => { throw new InvalidOperationException("listener"); };
            settings.ClientPortChanged += raised.Add;

            settings.Invoking(s => s.SetClientPort(4002)).Should().NotThrow();

            raised.Should().Equal(4002);
        }

        [Test]
        public void IsValidPort_MatchesTheDocumentedRange()
        {
            MultiplayerSettings.IsValidPort(MultiplayerSettings.MinPort).Should().BeTrue();
            MultiplayerSettings.IsValidPort(MultiplayerSettings.MaxPort).Should().BeTrue();
            MultiplayerSettings.IsValidPort(MultiplayerSettings.MinPort - 1).Should().BeFalse();
            MultiplayerSettings.IsValidPort(MultiplayerSettings.MaxPort + 1).Should().BeFalse();
        }

        private MultiplayerSettings CreateSettings()
        {
            MultiplayerSettings settings = new MultiplayerSettings(_store, new Mock<ILogger<MultiplayerSettings>>().Object);
            _store.Writes = 0;
            return settings;
        }

        private sealed class FakeSettingsStore : ISettingsStore
        {
            private readonly Dictionary<string, string> _values = new Dictionary<string, string>(StringComparer.Ordinal);

            public int Writes { get; set; }

            public void Set(string key, string raw)
            {
                _values[key] = raw;
            }

            public string Read(string key)
            {
                string raw;
                return _values.TryGetValue(key, out raw) ? raw : null;
            }

            public bool TryRead(string key, out string raw)
            {
                return _values.TryGetValue(key, out raw);
            }

            public void Write(string key, string raw)
            {
                _values[key] = raw;
                Writes++;
            }

            public void Flush()
            {
            }
        }
    }
}
