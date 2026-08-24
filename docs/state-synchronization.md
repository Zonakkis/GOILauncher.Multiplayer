# State Synchronization Context

本文记录状态同步方案的来源、已验证基线和当前实验状态，避免把旧版经验误认为新版的最终设计。

## Project Generations

- `master` 分支是旧版实现。
- 当前重写分支是新一代版本，属于全面重写，不要求沿用旧版架构。
- 新版可以复用旧版经过实际测试的结论，但必须重新验证实现效果和传输性能。

## Legacy Baseline

旧版经过测试后，选择同步以下对象的世界位置和世界旋转，作为视觉效果与传输性能之间的最佳折中：

- `Player` 根对象
- `Player/Hub/Slider`
- `Player/Hub/Slider/Handle`

旧版实现以 60 Hz 采样，并通过不可靠投递发送状态。旧版 `RemotePlayer` 包含位置线性插值和旋转球面插值代码，但 `SetNextMove` 同时会立即应用收到的状态，因此插值是否实际参与了已观察到的效果仍需单独确认。

## New Version Status

- 当前 `PlayerState` 延续了旧版 `Move` 的六组字段，仅作为已验证的起始基线。
- 这些字段不表示新版只需要同步三个对象，也不表示同步对象集合已经定稿。
- 新版完成首个端到端同步实现后，还会继续测试其他对象集合、采样方式和传输方案。
- 状态采集和应用应保持便于实验，避免把旧版对象集合固化到无关模块中。

## Confirmed Field Mappings

- `PlayerState.PlayerPosition` 和 `PlayerState.PlayerRotation` 对应 `Player` 根对象。
- `PlayerState.SliderPosition` 和 `PlayerState.SliderRotation` 对应 `Player/Hub/Slider`。
- `PlayerState.HandlePosition` 和 `PlayerState.HandleRotation` 对应 `Player/Hub/Slider/Handle`。
- `Player/handle` 是另一个独立对象，不是 `PlayerState.Handle*` 所指的对象。

## Current Rewrite Implementation

当前新版首个端到端实现采用独立的状态同步 Module，旧版基线中的对象集合和 60 Hz 不可靠投递仍然只是可调整的实验起点。

```text
Mian LateUpdate
  -> LocalPlayer 采集 PlayerState
  -> ClientPlayerStateSync 添加客户端 uint Sequence
  -> C2SPlayerStatePacket (Unreliable)
  -> Server PlayerStateRelay 校验 PacketSender 与 IsInGame，并写入 sender.Id
  -> S2CPlayerStatePacket (Unreliable) 转发给其他 IsInGame 玩家
  -> 客户端按 PlayerId 丢弃过期 Sequence
  -> PlayerStateReceivedEvent
  -> RemotePlayer 首次定位、后续插值
```

- `C2SPlayerStatePacket` 不携带玩家 ID；服务端始终以 `PacketSender.Id` 为发送者身份。
- `S2CPlayerStatePacket` 携带服务端写入的 `PlayerId`、原样 `Sequence` 和 `PlayerState`。
- 序号使用应用层 `uint` 环回比较；LiteNetLib 的不可靠投递不使用跨玩家共享的 `Sequenced` 通道。
- 客户端出站序号存在于持久化的 `ClientPlayerStateSync`，跨越 `Mian` 重开和奖励/主界面切换，仅在断开连接时重置。
- 客户端按玩家保存最后序号；玩家离开或断开时清理该记录，使重新进入的玩家可以从任意序号开始。
- `PlayerService` 仍只负责玩家身份和 `IsInGame`；`PlayerManager` 仍只负责 Unity 实例生命周期。同步 Module 实现 `Autofac.IStartable`，在容器 `Build()` 时自动激活，组合根不再逐个 `Resolve` 具体类型。
- 客户端与服务端各自拥有独立的 `NetPacketProcessor`（`IClientPacketDispatcher` / `IServerPacketDispatcher`）。宿主同时跑两个角色时，服务端 socket 收到的 S2C 包不会触发本地客户端回调，反之亦然；未订阅的包在 `PacketDispatcher.Dispatch` 内记 `Warn` 日志后丢弃。
- 包回调签名使用 Core 自己的 `PacketSender`（仅含 `int Id`）而非 LiteNetLib 的 `NetPeer`，服务端始终以 `PacketSender.Id` 为发送者身份。
- `Client` 与 `Server` 的事件类型不得复用同一个类型：两个角色共用一条 `IEventBus`，隔离目前只靠事件类型不相交维持。引入 `Room` 若出现对称事件，需要先按角色拆分事件总线。
- 当前 `PlayerStateRelay` 将状态发给所有其他 `IsInGame` 玩家；未来加入 `Room` 后，接收者筛选应在该 Module 内改为房间成员与房间交互设置，不扩散到 Unity 采样或协议模型。
- 远端实例尚未创建时收到的状态可以丢弃；服务端按约 60 Hz 发送，实例创建后会继续收到后续状态。

### 线上格式只写一遍

- **每个模型的线上格式只存在于它自己的 `Serialize` / `Deserialize`。** 读一律 `reader.Get<T>()`，不再给模型另配 `NetDataReaderExtensions.GetXxx`。
- `NetDataWriter.Put<T>` 和 `NetDataReader.Get<T>` 都对 `INetSerializable` 泛型约束，`UnityVector3` / `UnityQuaternion` 这些 struct 走它们不装箱，60 Hz 这条路上没有额外分配。
- 整个 `PlayerState` 因此是 3×3 + 2×2 + 4 = 17 个 float、68 字节。`PlayerStatePacketTests` 有一条直接断言这个字节数——读写两边再不对称，会先炸在那里，而不是变成 reader 深处一个 `ArgumentOutOfRangeException`。

## Skin Synchronization

皮肤和 `PlayerState` 是两条完全不同的链：状态是每帧一份的小包、丢了就丢，皮肤是一局一份的大字节、必须到。所以它走独立的 `NetworkChannels.S
kin` 通道（`ChannelsCount = 2`，两端必须一致）和 `ReliableOrdered`，不和 60 Hz 那条路抢序。

同步的是**机制**而不是某个皮肤 Mod：所有皮肤 Mod 最终都把 `Pot/Mesh` 那个 MeshRenderer 的 `mainTexture` 换成一张自己加载的 `Texture2D`，本
模块读的就是那张贴图本身，不读任何 Mod 的配置、目录或 `PlayerPrefs`（见 `docs/game-runtime.md` 的 Pot Skin）。

```text
进 Mian 后 0.1 s
  -> LocalSkinReader 读 Pot/Mesh 的 sharedMaterial（贴图 + _Goldness）
  -> ClientSkinSync.Announce(SkinState, PNG 字节)
  -> C2SSkinManifestPacket + C2SSkinDataPacket
  -> Server SkinRelay 校验发送者、槽位、哈希、载荷
  -> S2CSkinManifestPacket 广播给其他所有玩家（含大厅里的）
  -> 收端没有这个哈希的字节 -> C2SSkinRequestPacket
  -> S2CSkinDataPacket（有）/ S2CSkinUnavailablePacket（没有）
  -> PlayerSkinReceivedEvent
  -> SkinSynchronizer 解码贴图，写远端实例的 material
```

- **一局只读一次，读完不再看。** 皮肤 Mod 是在场景加载后自己去换贴图的，谁先跑没有保证，所以等 0.1 s（`WaitForSecondsRealtime`，与 `timeSc
ale` 无关）。没有轮询，也没有手动刷新：**游戏中途换皮肤不会同步，玩家要重开关卡**。`GameStartedEvent` 和 `GameRestartedEvent` 各触发一次读
取，重开关卡因此是那个"生效"入口。
- **清单和字节是分开的两个包，顺序是协议规定的。** 服务端只接受当前已宣告哈希的字节，否则它存下来的字节没有任何东西能证明属于哪张皮肤。上
传不等别人来要：一张贴图迟早每个人都要。
- **原版贴图不是"没有皮肤"，是一个正常的状态。** 没装皮肤时插件用内嵌的 `src/Unity/Resources/VanillaPot.png`，清单里哈希为空但 `Goldness`
照常传。金罐和黑罐是同一张贴图、`_Goldness` 0↔1 的差别，所以只需要一张内嵌图。从自定义皮肤换回原版也必须发清单，收端才知道该换回去。
- **哈希是内容标识，不是跨玩家去重。** 服务端缓存的粒度是每人一份（`Dictionary<int, SkinBlob>`），换皮肤时旧那份直接扔掉，玩家断开时连清单
一起忘掉。它存在的理由是让上传只发生一次：一份贴图要发给房间里每个人、还要发给之后进来的人，按人头重传等于把上传方的上行带宽乘以人数。不落
盘、不做 LRU。
- **握手时补发全部清单。** 新来的人只收到玩家名单，名单里没有皮肤；不补这一遍，他看到的所有人都是原版罐子，直到那些人各自重开一次关卡。
- **`S2CSkinUnavailablePacket` 是必须有的回音。** 客户端收到"没有"就退回原版且**不重试**——请求要是可以没有回音，那名玩家会永远停在原版上，
而不是"暂时"停在那里。金度在退回时仍然生效，它从来不依赖贴图。
- **两端各自校验载荷**（`SkinPayloadValidator`）：服务端转发的是别的客户端上传的字节，服务端本身也可能是别人的。`MaxPayloadBytes`（4 MB）
只是给服务端缓存定天花板；真正拦压缩炸弹的是 `MaxTextureSize`（4096），而且必须在 `LoadImage` 之前从 PNG 的 IHDR 里读出尺寸判掉——解码完再
查已经晚了，一张 4096² 的 RGBA32 是 64 MB。
- **两级缓存、两条 prune 规则，都按"现在还有人穿吗"判。** `ClientSkinSync` 按哈希存字节，`SkinSynchronizer` 按哈希存解码后的 `Texture2D`；
后者贵得多，所以它每次贴完贴图就问一次 `IsHashReferenced` 再扫。代价是那张要是又被换回来得重新下一次——上传侧的 A→B→A 会重传，正是为了让这
条路走得通。
- **断线清空远端一切，本地状态留着。** 重连可能是另一台服务器，或者同一台重启过，它那份缓存不算数了；本地皮肤没变，所以只需要在 `LocalPlay
erReadyEvent` 时按原样再宣告一次。本地那次读取也可能发生在连上之前（先进游戏再连服务器），走的是同一条补发路径。
- **`byte Slot` 现在只有 `PotSlot`。** 留着是为了以后加部件，服务端收到别的槽位会丢掉并警告——转发出去也没人渲染得了。

## Player Roster Ownership

远端实例的创建时机取决于名单，所以名单的归属规则和同步方案绑定在一起。

- 客户端名单的唯一权威来源是 `Client/Services/IPlayerService`：只暴露 `IEnumerable<PlayerInfo> Players` 和 `TryGetPlayer`，其它模块只读取，不保存副本。
- `PlayerManager` 只保存"哪个玩家当前有 Unity 实例"，身份、名字和 `IsInGame` 一律现读；`UnityClient` 不再持有名单。
- `PlayerInfo` 是协议模型（`S2CPlayerListPacket` 逐字段序列化，服务端也在用），不要往里加距离这类只有客户端算得出来的派生值。
- `PlayerService` 一律先写自己的状态、再发布事件，订阅者在处理器里读 `IPlayerService` 一定读到新值。不要依赖 `EventBus` 的订阅顺序来保证这一点。
  - `LocalPlayerReadyEvent`：本地身份确立（`ServerHandshakeEvent` 是传输层事件，发布时 `PlayerService` 还没更新）。
  - `PlayerRosterReceivedEvent`：收到完整名单快照。服务端只在握手时发一次 `S2CPlayerListPacket`，所以这是加入一个已有玩家的服务器时，唯一能得知这些玩家存在的时机——他们不会再产生 `PlayerJoinedEvent`。
  - `PlayerListUpdatedEvent`：只是给 UI 的"有变化"信号，不携带名单，也不驱动实例增删。用它驱动生命周期会让同一次变化走两条路径。
- 远端实例出现的唯一入口是 `PlayerManager.EnsureRemoteInstance`；名单快照、中途加入、中途进入游戏都走这一条。
- UI 通过 `IUnityClient.Players` 读名单，拿到的是 `PlayerView`：只记 `Id` 和两个权威来源（`IPlayerService` / `IPlayerManager`），每次读属性都回去取，不保存副本。距离由 `RemotePlayer.DistanceToLocalPlayer` 单点定义，头顶标签和玩家列表共用。UI 只依赖三个门面，约定见 `docs/ui-facade.md`。
- 玩家列表 UI 分两种刷新节奏：名单变化时增删行（事件驱动），距离按帧节流刷新已有行的文本（可见时拉取）。距离每帧都在变，沿用"清空重建全部行"会在按住 Tab 期间每帧 `Destroy` + `Instantiate` + 重建布局。

## Validation Workflow

- 仅凭对象层级和代码无法确认最终视觉效果是否正确。
- 状态同步实现完成后，由项目维护者在游戏内测试对象姿态、抖动、错位和传输效果。
- 在没有游戏内问题证据时，不因推测其他对象可能需要同步而阻塞基线方案。
- 收到具体游戏内现象后，再结合对象层级、状态采样和应用逻辑定位并迭代方案。
