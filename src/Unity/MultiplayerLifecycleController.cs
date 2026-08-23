using System;
using GOILauncher.Multiplayer.Core.Log;
using GOILauncher.Multiplayer.Unity.Config;

namespace GOILauncher.Multiplayer.Unity
{
    /// <summary>
    /// Applies what the multiplayer switch means for the running services: turning it off tears down
    /// the client connection and the embedded server. Turning it on only lifts the block — connecting
    /// and hosting stay explicit player actions.
    /// </summary>
    /// <remarks>
    /// This lives apart from <see cref="MultiplayerSettings"/> so that the settings object stays a leaf
    /// with no dependency on the client or the server. That is what lets the container build the whole
    /// graph in one pass, with no post-construction wiring step.
    /// </remarks>
    public sealed class MultiplayerLifecycleController
    {
        private readonly IUnityClient _client;
        private readonly IUnityServer _server;
        private readonly ILogger<MultiplayerLifecycleController> _logger;

        public MultiplayerLifecycleController(
            MultiplayerSettings settings,
            IUnityClient client,
            IUnityServer server,
            ILogger<MultiplayerLifecycleController> logger)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            _client = client;
            _server = server;
            _logger = logger;
            settings.EnabledChanged += OnEnabledChanged;
            // The persisted value predates every listener, so enforce it once on startup.
            Apply(settings.Enabled);
        }

        private void OnEnabledChanged(bool enabled)
        {
            Apply(enabled);
        }

        private void Apply(bool enabled)
        {
            if (enabled)
                return;

            Disconnect();
            StopServer();
        }

        private void Disconnect()
        {
            try
            {
                _client.Disconnect();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to disconnect the multiplayer client while disabling the feature.");
            }
        }

        private void StopServer()
        {
            try
            {
                _server.Stop();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to stop the embedded multiplayer server while disabling the feature.");
            }
        }
    }
}
