# 玩家相关功能

## PlayerView
玩家信息统一通过`PlayerView`类表示。
`PlayerView`类包含了玩家的各种信息：

- `int Id { get; }`：玩家的唯一标识符。

- `string Name { get; }`：玩家的名称。

- `Platform Platform { get; }`：玩家的设备平台。

- `bool IsInGame { get; }`：玩家是否在游戏中。

- `bool IsLocal { get; }`：玩家是否为本地玩家。

- `float? Distance { get; }`：玩家与本地玩家的距离。当本地玩家或目标玩家有一方不在游戏中时，距离为`null`。

## 本地玩家
通过`IUnityClient.LocalPlayer`可以获取本地玩家的`PlayerView`。
```csharp
PlayerView localPlayer = MultiplayerUnityCore.UnityClient.LocalPlayer;
```

## 所有玩家
通过`IUnityClient.Players`可以获取所有玩家的`PlayerView`。
```csharp
IEnumerable<PlayerView> players = MultiplayerUnityCore.UnityClient.Players;
```

## 传送
通过`IUnityClient.Teleport`方法可以将本地玩家传送至其他玩家位置。
```csharp
MultiplayerUnityCore.UnityClient.Teleport(targetPlayer.Id);
```
