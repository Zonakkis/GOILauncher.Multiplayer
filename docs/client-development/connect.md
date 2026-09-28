# 连接操作

完成[初始化插件](/client-development/initialize)后，即可连接服务器。

## 发起连接
通过`IMultiplayerClient.Connect(string host, int port, string playerName)`方法可以发起连接。

```csharp
using GOILauncher.Multiplayer.Unity;

client.Connect("127.0.0.1", 9027, "Blastbolt is not real");
```
连接到服务器后，插件会自动提供玩家状态同步、玩家皮肤同步等功能。

## 断开连接

通过`IMultiplayerClient.Disconnect()`方法可以主动断开已经建立的连接。

```csharp
using GOILauncher.Multiplayer.Unity;

client.Connect("127.0.0.1", 9027, "Blastbolt is not real");
client.Disconnect();
```