# 客户端信息
连接到服务器后，可以在`IUnityClient`中获取相关信息。

## 属性
### IsConnected
`bool`：是否已连接到服务器。

### LocalPlayer
`PlayerView`：本地玩家的视图。

### Players
`IEnumerable<PlayerView>`：服务器中所有玩家的视图。

### ChatMessages
`ReadOnlyCollection<Message>`：聊天消息的集合。

## 事件
### Connected
`Action`：当连接到服务器时触发。

### Disconnected
`Action<string>`：当断开连接时触发。

参数：断开连接的原因。

### PlayerListUpdated
`Action`：当玩家列表更新时触发。UI异步更新时使用。

### ChatMessageReceived
`Action<Message>`：当收到聊天消息时触发。

参数：收到的聊天消息。
