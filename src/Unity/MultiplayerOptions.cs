using GOILauncher.Multiplayer.Core.Log;
using NLog.Targets;

namespace GOILauncher.Multiplayer.Unity
{
    /// <summary>
    /// Input to <see cref="MultiplayerCore.Initialize"/>. Today it carries only the log sink;
    /// new initialization config gets added as fields here without touching Initialize's signature.
    /// Data/config only — never services meant for the container. The container must stay
    /// self-contained so it can be torn down whole when multiplayer is unloaded.
    /// </summary>
    public sealed class MultiplayerOptions
    {
        /// <summary>
        /// The host's NLog sink. Defaults to the module's shared console target, so a host that
        /// configures nothing still logs and Initialize can register this unconditionally.
        /// </summary>
        public Target LogTarget { get; set; } = DefaultLogTarget.Create();
    }
}

