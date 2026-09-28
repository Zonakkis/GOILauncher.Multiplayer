# 房间操作
房间是划分玩家的基本单位，玩家、聊天等操作都以当前房间为上下文。

::: tip
连接到服务器后会默认进入房间**大厅**，若不需要房间功能，可以跳过本章，相关操作都会在**大厅**中进行。
:::

## RoomInfo
`RoomInfo`是房间信息的表示类，
- `int Id { get; }`：房间ID。
- `string Name { get; }`：房间名字。
- `bool HasPassword { get; }`：
- `int MaxPlayers { get; }`：
- `int PlayerCount { get; }`：
- `int? OwnerPlayerId { get; }`：
- `bool IsLobby { get; }`：
- `bool IsFull { get; }`：

## 创建房间

## 加入房间

