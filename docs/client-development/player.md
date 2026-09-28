# 玩家操作

## PlayerView
`PlayerView`是玩家信息的表示类：

- `int Id { get; }`：玩家的唯一标识符。
- `string Name { get; }`：玩家的名称。
- `Platform Platform { get; }`：玩家的设备平台。
- `bool IsInGame { get; }`：玩家是否在游戏中。
- `bool IsLocal { get; }`：玩家是否为本地玩家。
- `float? Distance { get; }`：玩家与本地玩家的距离。当本地玩家或目标玩家有一方不在游戏中时，距离为`null`。

## 获取本地玩家
通过`IMultiplayerClient.LocalPlayer`可以获取本地玩家的`PlayerView`。
```csharp
PlayerView localplayer = client.LocalPlayer;
```

## 获取所有玩家
通过`IMultiplayerClient.LocalPlayer`可以获取当前房间所有玩家的`PlayerView`。
```csharp
IEnumerable<PlayerView> players = client.Players;
```

## 传送
通过`IMultiplayerClient.Teleport`方法可以将本地玩家传送至其他玩家位置。
```csharp
client.Teleport(targetPlayer.Id);
```
