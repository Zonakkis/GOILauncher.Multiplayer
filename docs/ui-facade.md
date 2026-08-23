# UI Facade

UI 宿主（`Unity.Desktop`，以及以后可能的其它宿主）只通过三个门面访问联机功能：

- `MultiplayerUnityCore` — 初始化容器，取到下面两个门面；
- `IUnityClient` — 客户端的一切；
- `IUnityServer` — 内嵌服务端的一切。

这条约束是为了让写 UI 的人只需要学三个入口，不用先读懂 `Client` / `Server` / `Unity` 三层的分工。

## Rules

- **UI 层向下只能依赖这三个类型。** 不要让 UI 直接依赖 `IPlayerService`、`IPlayerManager`、`IChatService`、`IEventBus` 等下层类型。
- **新增能力挂在已有门面下面，不要新开一个根接口。** "玩家名单该由谁提供"这类问题的答案永远是 `IUnityClient`，哪怕实现要组合好几个下层服务——组合工作在 `UnityClient` 里做。
- **UI 不订阅 `IEventBus`，事件由门面转发。** 见下面 Events 一节。
- **`MultiplayerSettings` 不算第四个根接口**（`GOILauncher.Multiplayer.Unity.Config`）。它和 `PlayerView` 一样，是从容器里取到的类型，不是新开的入口。见下面 MultiplayerSettings 一节。

单一数据源和这条约束不冲突：单一数据源约束的是**谁能写、有没有人存副本**，不是成员声明在哪个接口上。只要门面上的成员是读透（read-through）的、不缓存，把它放在 `IUnityClient` 上和放在一个独立接口上完全等价。`UnityClient.Players` 每次枚举现场构造 `PlayerView`，名单的唯一所有者仍然是 `Client/Services/IPlayerService`。

## Events

`IEventBus` 是下层的总线，UI 不订阅它。`UnityClient` 订阅需要的总线事件，再以自己的 `event` 转发出去：

| 门面事件 | 转发自 |
|---|---|
| `IUnityClient.Connected` | `Core.Event.ServerConnectedEvent` |
| `IUnityClient.Disconnected(reason)` | `Core.Event.ServerDisconnectedEvent` |
| `IUnityClient.ChatMessageReceived(message)` | `Client.Events.ChatMessageEvent` |
| `IUnityClient.PlayerListUpdated` | `Client.Events.PlayerListUpdatedEvent` |

这样做的原因：

- 客户端和服务端**共用一条 `IEventBus`**，隔离只靠事件类型名不相交维持（见 `docs/state-synchronization.md`）。UI 直接订阅总线，就得自己知道 `ServerConnectedEvent` 是"我连上了服务器"而不是"有客户端连上我了"——这正是不该让 UI 层背的知识。
- 总线上的事件类型属于 `Core` / `Client`，UI 一订阅就得 `using` 下层命名空间，三门面的约定名存实亡。
- 门面事件的参数按 UI 需不需要来定，不是照抄总线事件。`Disconnected` 只给 `reason`，`ChatMessageReceived` 给消息本体（见下），`PlayerListUpdated` 什么都不给——名单从 `Players` 读。

签名用 `Action` / `Action<T>` 而不是 `EventHandler`：订阅方一个都没用到 `sender`，`Action` 还能让参数一致的方法直接 `+=` 上去。

### ChatMessageReceived 为什么带消息本体

带上 `Message` 是为了让 UI 有两条路可走：

- **要完整聊天记录**：忽略参数，从 `ChatMessages` 读全量重绘。`ChatHudUI` 走这条——展示的就是全部消息，从唯一来源读比自己维护追加逻辑简单。
- **只要特定类型的消息**：读参数的 `Type` 过滤（系统提示、私聊、队伍消息……），自攒一份记录。这条路不用每次事件都扫一遍 `ChatMessages` 去找新增的那条。

名单没有"只要一部分"这种用法（UI 拿到的永远是当前快照），所以 `PlayerListUpdated` 不带参数。

## PlayerView

`GOILauncher.Multiplayer.Unity.Player.PlayerView` 是"UI 看到的一名玩家"。它只记 `Id` 和两个权威来源，每次读属性都回去取，所以可以存在列表行上跨帧复用。

**派生显示值放 `PlayerView`，命令放门面。** 读的东西会随着列增长（距离，以后可能的高度差、进度、速度），每加一个就往 `IUnityClient` 上加一个方法的话，门面很快就不是"三个入口"了；命令的数量是有限的（传送、踢人），放门面上不会失控。

也不要把派生值加进 `PlayerInfo`：那是协议模型（`S2CPlayerListPacket` 逐字段序列化），服务端也在用。

三类事实的归属：

| 事实 | 来源 | 例子 |
|---|---|---|
| 名单事实 | `IPlayerService`（网络权威） | `Id` / `Name` / `Platform` / `IsInGame` |
| 实例事实 | `IPlayerManager` → `RemotePlayer`（只在场景里） | 有没有远端实例、世界坐标 |
| 派生显示值 | 读时现算，任何地方都不存 | `Distance` |

`PlayerView.Distance` 为 `null` 表示这名玩家当前没有远端实例——在大厅、实例池已满、首个状态包还没到都属于正常情况，本地玩家自己也是 `null`。这个判据同时就是"能不能传送过去"：没有实例就没有目标位置，所以 `IUnityClient.TeleportTo(playerId)` 不用再配一个 `CanTeleportTo`，UI 直接按 `Distance.HasValue` 决定按钮的可用性。传送本身是命令，所以放门面上而不是 `PlayerView` 上。

传送这条链是：`IUnityClient.TeleportTo(playerId)` → `UnityClient` 从 `IPlayerManager` 取出本地玩家和目标实例 → `LocalPlayer.TeleportTo(Transform target)`。搬运逻辑落在最后一环，那里本地玩家（`this`）和目标（`target`）两边的层级都在手上——它靠逐个配对两边的 `Rigidbody2D` 来搬，所以依赖"远端实例和本地玩家出自同一个 `PlayerPrefab`、刚体结构一致"这个前提（见 `docs/game-runtime.md`）。

UI 一侧多一跳：玩家列表每行的传送按钮点击后只发 `PlayerListHandler.TeleportRequested(playerId)`，由 `PlayerListUI`（它本来就持有 `IUnityClient`）接到 `TeleportTo` 上。行渲染类因此仍然只认识 `PlayerView`，不引用任何门面类型——**纯视图类不必自己去拿门面，让持有门面的那一层把事件接过去**，这条对以后的踢人、私聊按钮同样适用。

## MultiplayerSettings

`GOILauncher.Multiplayer.Unity.Config.MultiplayerSettings` 是持久化设置的唯一所有者。UI 用构造注入拿到它，读属性、调 `SetXxx`、订阅变更事件，不自己存副本——和 `PlayerView` 是同一类东西：**约束限制的是"UI 要认识几个入口"，不是"UI 能出现几个类名"。**

这条以前是反方向的：`IMultiplayerState` 由 UI 宿主实现（`MultiplayerStateCoordinator` 把 BepInEx 的 `ConfigEntry` 包一层暴露出来），Unity 层调用。搬到 `src/Unity` 之后方向反了过来，存储归 Unity 层、UI 只是消费者，所以"反向接口不受约束"这条理由不再成立，改按 `PlayerView` 的先例走。

搬下来的原因是 **BepInEx 只存在于 PC 宿主**。Android / iOS 宿主没有 `ConfigFile`，要读的设置却是同一套；留在 UI 层就得每个宿主各写一遍键名、类型、默认值和变更通知。现在这些都在平台无关的 `src/Unity` 里，只把"字节落到哪"抽成 `ISettingsStore`（见 `docs/game-runtime.md` 的 Multiplayer Settings）。

`IMultiplayerState` 还在，但已经退成 Unity 层内部的只读端口：`UnityClient` / `UnityServer` 属性注入它，用来拒绝关闭联机后的连接和启动请求。UI 不再碰它。
