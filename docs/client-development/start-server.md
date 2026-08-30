# 启动服务器
服务器可以通过控制台方式启动，也可以通过插件内嵌的服务器启动。
## 插件内嵌启动
调用`IUnityServer.Start`并传入服务器端口，即可发起启动。
```csharp
using GOILauncher.Multiplayer.Unity;

MultiplayerUnityCore.UnityServer.Start(9027);
```