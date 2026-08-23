using System;
using GOILauncher.Multiplayer.Core.Log;
using GOILauncher.Multiplayer.Unity.Config;

namespace GOILauncher.Multiplayer.Unity
{
    /// <summary>
    /// Applies what the multiplayer switch means for the network services: turning it off tears down the
    /// client connection and the embedded server. Turning it on only lifts the block — connecting and
    /// hosting stay explicit player actions.
    /// </summary>
    /// <remarks>
    /// 这里只管网络那一半。玩家实例那一半在 <see cref="Player.PlayerManager"/> 自己身上，
    /// 因为它才是实例生命周期的所有者；两边订阅的是同一个 <see cref="IMultiplayerState"/>，
    /// 所以"关闭"的含义仍然只有一个来源，不存在两套判据。
    /// <para>
    /// 本类和 <see cref="MultiplayerSettings"/> 分开，是为了让设置对象保持叶子节点、不依赖
    /// 客户端和服务端。容器因此能一次建完整张图，不需要构造后再补一步接线。
    /// </para>
    /// </remarks>
    public sealed class MultiplayerLifecycleController
    {
        private readonly IUnityClient _client;
        private readonly IUnityServer _server;
        private readonly ILogger<MultiplayerLifecycleController> _logger;

        public MultiplayerLifecycleController(
            IMultiplayerState state,
            IUnityClient client,
            IUnityServer server,
            ILogger<MultiplayerLifecycleController> logger)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            _client = client;
            _server = server;
            _logger = logger;
            state.EnabledChanged += OnEnabledChanged;
            // The persisted value predates every listener, so enforce it once on startup.
            Apply(state.Enabled);
        }

        private void OnEnabledChanged(bool enabled)
        {
            Apply(enabled);
        }

        private void Apply(bool enabled)
        {
            // 打开只是解除拦截：连谁、开不开服都还是玩家自己按按钮的事。
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
