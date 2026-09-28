# 初始化插件

在使用插件之前，需要初始化并拿到`MultiplayerCore`实例。

`MultiplayerCore`类中主要包含三个对象：
- `IMultiplayerClient`：进行客户端相关操作。
- `IMultiplayerServer`：进行服务端相关操作。
- `IGameManager`：获取游戏相关状态。

## 初始化
通过`MultiplayerCore.Initialize(Action<MultiplayerOptions> configure = null)`来进行初始化：

```csharp
using GOILauncher.Multiplayer.Unity;

MultiplayerCore core = MultiplayerCore.Initialize();
// MultiplayerCore core = MultiplayerCore.Initialize(o => {
//     // 也可以传递相关参数...
// });
IMultiplayerClient client = core.MultiplayerClient; // 获取客户端入口
```
::: tip
`MultiplayerOptions`中属性见
:::

接下来可以使用`IMultiplayerClient`中的一系列API进行开发客户端了！

为简单起见，接下来的示例将默认`core`已经初始化，`client`等已经从`core`中获取。