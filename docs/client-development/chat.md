# 聊天相关功能

## Message
`Message`类是插件内置的聊天消息模型。
`Message`类包含了聊天消息的各种信息：
- `MessageType Type { get; }`：消息的类型。
  - `MessageType.System`：系统消息。
  - `MessageType.Server`：服务器消息。
  - `MessageType.Player`：玩家消息。
- `string Sender { get; }`：消息发送者的名称。
- `string Content { get; }`：消息的内容。
- `DateTime DateTime { get; }`：消息发送的时间。

## 发送消息
通过`IUnityClient.SendMessage`方法可以发送聊天消息，该方法会触发`ChatMessageReceived`事件。

通过该方法发送的消息都会添加到`IUnityClient.ChatMessages`中，其中`System`和`Server`消息都不会被真正发送到服务器。
```csharp
MultiplayerUnityCore.UnityClient.SendMessage(MessageType.System, "Hello world!");
MultiplayerUnityCore.UnityClient.SendMessage(MessageType.Server, "Hello world!");
MultiplayerUnityCore.UnityClient.SendMessage(MessageType.Player, "Hello world!");
```

## 获取聊天消息
通过`IUnityClient.ChatMessageReceived`事件可以获取最新的消息。
```csharp
MultiplayerUnityCore.UnityClient.ChatMessageReceived += (message) =>
{
    // Do something...
};
```
通过`IUnityClient.ChatMessages`可以获取插件的所有聊天消息。
```csharp
ReadOnlyCollection<Message> chatMessages = MultiplayerUnityCore.UnityClient.ChatMessages;
```
除此以外，你还可以构建构建自己的历史消息记录，只需要在`ChatMessageEvent`事件触发时选择性地收取即可。
