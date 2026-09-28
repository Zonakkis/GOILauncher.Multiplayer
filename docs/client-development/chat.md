# 聊天操作

## Message
`Message`类是插件内置的聊天消息模型。
`Message`类包含了聊天消息的各种信息：
- `MessageType Type { get; }`：消息的类型。
  - `MessageType.System`：系统消息。
  - `MessageType.Server`：服务器消息。
  - `MessageType.Player`：玩家消息。
- `string Sender { get; }`：发送消息玩家的名称。
- `string Content { get; }`：消息的内容。
- `DateTime DateTime { get; }`：消息发送的时间。

## 发送消息
通过`IMultiplayerClient.SendMessage(MessageType type, string message)`方法可以发送聊天消息。

通过该方法发送的消息都会添加到`IMultiplayerClient.ChatMessages`中，其中只有`Player`类型的消息会被真正发送到服务器。

```csharp
client.SendMessage(MessageType.System, "这是一条系统消息。");
client.SendMessage(MessageType.Server, "这是一条服务器消息。");
client.SendMessage(MessageType.Player, "你说的对，但是《Getting Over It》是由Bennet Foddy自主研发的一款全新开放世界冒险游戏。");
```

## 获取聊天消息
### 获取最新消息
通过订阅`IMultiplayerClient.ChatMessageReceived`事件可以获取最新的消息。

```csharp
client.ChatMessageReceived += (message) =>
{
    // Do something...
};
```

### 获取所有消息
通过`IMultiplayerClient.ChatMessages`可以获取插件内置的所有聊天消息。

```csharp
ReadOnlyCollection<Message> chatMessages = client.ChatMessages;
```

除此以外，你还可以构建自己的历史消息记录，只需要在`ChatMessageEvent`事件触发时选择性地收取即可。
