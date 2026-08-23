using System;

namespace GOILauncher.Multiplayer.Unity.Config
{
    /// <summary>
    /// Unity 运行时服务看联机开关的唯一端口：既能读当前值，也能等它变。
    /// </summary>
    /// <remarks>
    /// 变更事件和 <see cref="Enabled"/> 必须成对出现，因为"关闭联机"有对称的两半：拒绝之后的
    /// 请求靠读 <see cref="Enabled"/>，撤掉此刻已经建立的东西靠订阅 <see cref="EnabledChanged"/>。
    /// 只有读没有事件，关闭就只能拦住后来的操作，管不了已经在跑的。
    /// </remarks>
    public interface IMultiplayerState
    {
        bool Enabled { get; }

        event Action<bool> EnabledChanged;
    }
}
