# UI Facade

UI 宿主（`Unity.Desktop`，以及以后可能的其它宿主）通过以下入口访问联机业务与游戏运行时：

- `MultiplayerUnityCore` — 联机模块的根，加载与销毁它，并取得下面的依赖；未加载时三个门面属性都是 `null`；
- `IUnityClient` — 客户端联机业务；
- `IUnityServer` — 内嵌服务端联机业务；
- `IGameManager` — 游戏场景状态与游戏内对象引用，不提供联机业务。

这条约束区分了业务门面和游戏引用的来源，让 UI 不必了解 `Client` / `Server` / `Unity` 三层的内部服务分工。

## Rules

- **UI 通过上述入口访问业务和游戏引用。** 不要让 UI 直接依赖 `IPlayerService`、`IPlayerManager`、`IChatService`、`IEventBus` 等内部服务。
- **新增联机业务挂在已有客户端/服务端门面，游戏引用放在 `IGameManager`，不要新开服务根接口。** "玩家名单该由谁提供"这类问题的答案永远是 `IUnityClient`，哪怕实现要组合好几个下层服务——组合工作在 `UnityClient` 里做。
- **UI 不订阅 `IEventBus`，事件由门面转发。** 见下面 Events 一节。
- **`MultiplayerSettings` 不是新的服务根接口**（`GOILauncher.Multiplayer.UI.Config`）。它和 `PlayerView` 一样，是 UI 可以使用的设置/视图类型，不是新的业务入口；而且它整个住在宿主这一层，core 不认识它。见下面 MultiplayerSettings 一节。

单一数据源和这条约束不冲突：单一数据源约束的是**谁能写、有没有人存副本**，不是成员声明在哪个接口上。只要门面上的成员是读透（read-through）的、不缓存，把它放在 `IUnityClient` 上和放在一个独立接口上完全等价。`UnityClient.Players` 每次枚举现场构造 `PlayerView`，名单的唯一所有者仍然是 `Client/Services/IPlayerService`。

## 加载与销毁

联机模块只有两种状态：已加载、已销毁。`MultiplayerUnityCore.Initialize(Target)` 建出整张对象图，`Dispose()` 拆掉它，没有“加载着但停用”这第三种状态——关就是拆，重新开就是重建。`UnityClient` / `UnityServer` / `PlayerManager` 里因此没有 `IsMultiplayerEnabled` 之类的守卫：拿得到门面就说明这一轮是活的；闸落下去之后，三个门面属性一律返回 null。

UI 不进这张对象图。`Plugin.OnInitialized` 手工组合窗口、页面和 handler，再把自己这一轮的门面递进去：

- `MultiplayerUnityCore.Initialized` 触发时，`Plugin` 读 `UnityClient` / `UnityServer`，逐个 `Bind` 到 `ClientPage`、`ServerPage`、`RoomDialogUI`、`ChatHudUI`、`PlayerListUI`。
- `MultiplayerUnityCore.Disposing` 触发时反过来逐个 `Unbind`。这个事件发出时对象图还活着，所以退订门面事件、清掉列表行上缓存的 `PlayerView`、把聊天记录归零都还来得及。
- `Bind` / `Unbind` 必须交替：两边的订阅是成对的，漏掉一次 `-=` 就是下一轮点一次按钮弹两条消息。

不进容器的理由是两套生命周期不同步。UniverseLib 的窗口按 id 注册在一张静态表里，同一个 id 建第二个会抛，所以 UI 只能活满整个进程；联机模块随开随关。塞进同一个容器，要么 UI 跟着容器一起被拆掉再也建不回来，要么容器名不副实。

`Plugin` 是唯一读 `MultiplayerUnityCore` 的类，其余 UI 只认自己 `Bind` 到的那个门面。日志按同一个方向切：UI 写 `Plugin.Logger`（BepInEx 的 `ManualLogSource`），不依赖 core 的 `ILogger<T>`；core 的 NLog 落地端由宿主建一次、以外部所有的身份注册进容器，所以它不跟着联机模块一起销毁，下一轮 `Initialize` 复用的是活的 target。

## IGameManager

`IGameManager` 提供 `IsInGame` 及 `Player`、`PlayerPrefab`、`Cursor` 等游戏内引用，不承载连接、房间、名单等业务。UI 通过 `MultiplayerUnityCore.GameManager` 现读，不保存它——每加载一次是一个新实例，存下来的引用在模块关掉之后就再也更新不动了。场景中的对象同样会被销毁或替换，使用时从它的属性读取当前引用。

桌面 UI 占用鼠标时，`Plugin` 直接操作 `IGameManager.Cursor` 上的 `Rigidbody2D.simulated`：占用时设为 `false`，释放时设为 `true`。这是 UI 对游戏对象的交互，不必为此增加 `IUnityClient` 方法，也不让 `IGameManager` 保存 UI 状态或提供业务命令。具体运行时事实见 `game-runtime.md` 的 Cursor Object。

## Events

`IEventBus` 是下层的总线，UI 不订阅它。`UnityClient` 订阅需要的总线事件，再以自己的 `event` 转发出去：

| 门面事件 | 转发自 |
|---|---|
| `IUnityClient.Connected` | `Core.Event.ServerConnectedEvent` |
| `IUnityClient.Disconnected(reason)` | `Core.Event.ServerDisconnectedEvent` |
| `IUnityClient.ChatMessageReceived(message)` | `Client.Events.ChatMessageEvent` |
| `IUnityClient.PlayerListUpdated` | `Client.Events.PlayerListUpdatedEvent` |
| `IUnityClient.RoomListUpdated` | `Client.Events.RoomListUpdatedEvent` |
| `IUnityClient.CurrentRoomChanged` | `Client.Events.CurrentRoomChangedEvent` |
| `IUnityClient.RoomOperationCompleted(result)` | `Client.Events.RoomOperationCompletedEvent` |
| `IUnityClient.ChatHistoryReset` | `Client.Events.ChatHistoryResetEvent` |

这样做的原因：

- 客户端与服务端已按角色拆成独立的 `IClientEventBus` / `IServerEventBus`；Unity 游戏事件属于客户端。UI 仍不订阅任何下层总线，不需要掌握传输或房间事件的内部阶段。
- 总线上的事件类型属于 `Core` / `Client`，UI 一订阅就得 `using` 下层命名空间，UI 不依赖内部服务的约定名存实亡。
- 门面事件的参数按 UI 需不需要来定，不是照抄总线事件。`Disconnected` 只给 `reason`，`ChatMessageReceived` 给消息本体（见下），`PlayerListUpdated` 什么都不给——名单从 `Players` 读。

签名用 `Action` / `Action<T>` 而不是 `EventHandler`：订阅方一个都没用到 `sender`，`Action` 还能让参数一致的方法直接 `+=` 上去。

### ChatMessageReceived 为什么带消息本体

带上 `Message` 是为了让 UI 有两条路可走：

- **要完整聊天记录**：忽略参数，从 `ChatMessages` 读全量重绘。`ChatHudUI` 走这条——展示的就是全部消息，从唯一来源读比自己维护追加逻辑简单。
- **只要特定类型的消息**：读参数的 `Type` 过滤（系统提示、私聊、队伍消息……），自攒一份记录。这条路不用每次事件都扫一遍 `ChatMessages` 去找新增的那条。

名单没有"只要一部分"这种用法（UI 拿到的永远是当前快照），所以 `PlayerListUpdated` 不带参数。

## Rooms

房间能力都在 `IUnityClient`：`Rooms` / `CurrentRoom` / `IsRoomOperationPending`，以及 `RefreshRooms` / `CreateRoom` / `JoinRoom` / `LeaveRoom` / `UpdateRoom`。`IUnityServer` 不承担房主操作。

`RoomInfo`、`RoomOperationResult`、`RoomPasswordChange` 与共享长度常量是数据契约，不是额外的服务入口。

目录与当前房间读透客户端 `RoomService`，名单仍从 `PlayerService` 读。`RoomInfo` 只含公开元数据，密码仅有 `HasPassword`；编辑密码明确区分 Keep / Set / Remove，不回填原密码。

`CurrentRoom` 在入房快照到来前为 null；`IsConnected` 仍只代表真实连接，不混入房间就绪状态。`CurrentRoomChanged` 包含属性、房主及成员关系变化，不能把每次通知都当作换房。`ChatHistoryReset` 单独通知记录清空，即使没有新消息，聊天 UI 也必须重读 `ChatMessages`。

`PlayerListUI` 用 `CurrentRoom` 和 `TryGetPlayer` 绘制 Tab 顶部“当前房间 / 房主”两列，订阅 `CurrentRoomChanged` 更新房名与房主；名单变化和重新显示时也重读，避免房主名字或隐藏期间的数据滞后。距离刷新仍独立按帧节流，不重建房间信息行。房间 ID 保留为命令参数，但游戏 UI 不展示。

内部切房按“写入房间和完整名单 → 清理旧房间 → 创建新实例 → UI 通知”分阶段，不能依赖订阅顺序。详细规则见 `room-system.md`。旧房间 UI 接口和假数据已移除，没有额外的房间服务入口。

## PlayerView

`GOILauncher.Multiplayer.Unity.Player.PlayerView` 是"UI 看到的一名玩家"。它只记 `Id` 和两个权威来源，每次读属性都回去取，所以可以存在列表行上跨帧复用。

**派生显示值放 `PlayerView`，命令放门面。** 读的东西会随着列增长（距离，以后可能的高度差、进度、速度），每加一个就往 `IUnityClient` 上加一个方法的话，门面的方法就会不断膨胀；命令的数量是有限的（传送、踢人），放门面上不会失控。

也不要把派生值加进 `PlayerInfo`：那是协议模型（`S2CPlayerListPacket` 逐字段序列化），服务端也在用。

三类事实的归属：

| 事实 | 来源 | 例子 |
|---|---|---|
| 名单事实 | `IPlayerService`（网络权威） | `Id` / `Name` / `Platform` / `IsInGame` |
| 实例事实 | `IPlayerManager` → `RemotePlayer`（只在场景里） | 有没有远端实例、世界坐标 |
| 派生显示值 | 读时现算，任何地方都不存 | `Distance` |

`PlayerView.Distance` 为 `null` 表示这名玩家当前没有远端实例——未在游戏中、实例池已满、首个状态包还没到都属于正常情况，本地玩家自己也是 `null`。这个判据同时就是"能不能传送过去"：没有实例就没有目标位置，所以 `IUnityClient.TeleportTo(playerId)` 不用再配一个 `CanTeleportTo`，UI 直接按 `Distance.HasValue` 决定按钮的可用性。传送本身是命令，所以放门面上而不是 `PlayerView` 上。

传送这条链是：`IUnityClient.TeleportTo(playerId)` → `UnityClient` 从 `IPlayerManager` 取出本地玩家和目标实例 → `LocalPlayer.TeleportTo(Transform target)`。搬运逻辑落在最后一环，那里本地玩家（`this`）和目标（`target`）两边的层级都在手上——它靠逐个配对两边的 `Rigidbody2D` 来搬，所以依赖"远端实例和本地玩家出自同一个 `PlayerPrefab`、刚体结构一致"这个前提（见 `docs/agent/game-runtime.md`）。

UI 一侧多一跳：玩家列表每行的传送按钮点击后只发 `PlayerListHandler.TeleportRequested(playerId)`，由 `PlayerListUI`（它本来就持有 `IUnityClient`）接到 `TeleportTo` 上。行渲染类因此仍然只认识 `PlayerView`，不引用任何门面类型——**纯视图类不必自己去拿门面，让持有门面的那一层把事件接过去**，这条对以后的踢人、私聊按钮同样适用。

## MultiplayerSettings

`GOILauncher.Multiplayer.UI.Config.MultiplayerSettings` 是持久化设置的唯一所有者，整个住在宿主这一层：core 不认识这个类型，也没有为设置留任何端口。怎么存是宿主的事，PC 宿主直接用 BepInEx 的 `ConfigFile`（`Multiplayer` 节下的 `Enabled` / `PlayerName` / `ClientHost` / `ClientPort` / `ServerPort` / `HideServerPage`），键名、类型、解析容错都交给它，这里只补两条它不管的规矩——端口的合法范围，和“清空主机名等于回到默认值”。

以前反过来：设置归 `src/Unity`，只把“字节落到哪”抽成 `ISettingsStore`，理由是 BepInEx 只存在于 PC 宿主、平台无关层要能读同一套设置。现在还没有第二个宿主，这套抽象换来了一个 Unity 层的 `MultiplayerUnityCore.Settings` 入口，代价比收益大，所以 `ISettingsStore` / `FileSettingsStore` 一起删了。Android / iOS 宿主真要出现，各写各的设置类型，共享的是语义而不是实现。

四项初值的方向没变：`SettingsPage` 是唯一写入方，`ClientPage` / `ServerPage` 只在填输入框时读它，并订阅对应的 `XxxChanged` 在空闲时更新输入框——页面上临时改名字或地址只影响那一次连接，不回写。名字允许为空（“还没填”），空名字连接时由 `ClientPage` 弹“名字不能为空”拦下，没有兜底默认名（见 `docs/agent/game-runtime.md` 的 Multiplayer Settings）。

`Enabled` 是那份开关状态落了盘。真值只有一处——`MultiplayerUnityCore.IsLoaded`，宿主在每次加载与销毁之后回写它，所以配置文件里写的、勾选框显示的、模块实际的，是同一件事；启动那次自动加载失败也一样回写成 false，不会留下一个没成真的 true。它顺带回答“下次启动要不要自动加载”，因为落盘的正是当时的状态。

`HideServerPage`（默认 true）和 `Enabled` 正好相反，它不是状态的回写目标，而是 UI 自己的一个选择：设置页写它，`Plugin` 读到变化后调 `MultiplayerUI.SetServerPageVisible` 收放“服务端”页签。**它只影响页签可见性**：`ServerPage` 照旧构造、照旧被 `Bind` / `Unbind`，内嵌服务端不因为它而少加载一分；设置页里的“服务端设置”块（默认端口）也不受它影响。这也是设置页不直接去改另一个页面的按钮、而是往上抛 `HideServerPageToggled` 的理由——跨页面的应用动作归宿主。

