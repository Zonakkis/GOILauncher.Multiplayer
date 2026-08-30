# 连接服务器

完成[初始化插件](/client-development/initialize)后，即可连接服务器。

## 发起连接
调用`IUnityClient.Connect`并传入服务器地址、端口和玩家名称，即可发起连接.这会触发`Connected`事件。
```csharp
using GOILauncher.Multiplayer.Unity;
MultiplayerUnityCore.UnityClient.Connect(host, port, playerName);
```
## 断开和重连

调用`Disconnect`断开连接，这会触发`Disconnected`事件。

```csharp
MultiplayerUnityCore.UnityClient.Disconnect();
```