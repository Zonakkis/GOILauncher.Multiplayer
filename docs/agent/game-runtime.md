# Game Runtime Reference

本文记录已经确认的 Getting Over It 运行时结构。只记录经过代码、游戏运行时或领域专家确认的事实，不根据对象名称推测职责。

## Scene Flow

- `Loader` 是打开游戏后所在的主界面场景。
- `Mian` 是开始游戏后所在的正式游戏场景；名称确实是 `Mian`，不是 `Main`。
- 通关后进入 `Reward Loader` 或 `Reward Loader Offline`，主要用于显示游戏时间并提供返回主界面的入口。
- 通关流程结束后可以返回 `Loader` 主界面。

```text
Loader -> Mian -> Reward Loader / Reward Loader Offline -> Loader
```

只有 `Mian` 属于正在游戏中的场景；`Loader`、`Reward Loader` 和 `Reward Loader Offline` 均视为不在游戏中。

## Player Object Hierarchy

玩家实体对应的 Unity 根对象名为 `Player`。

已确认的直接子物体，以及锤子那条链的完整层级：

```text
Player
|-- handle                      (Camera, Directional Light, Mesh)
|-- Hub
|   `-- Slider
|       |-- Handle
|       |   |-- PoleMiddle
|       |   |   |-- climbinghammer_remap -> RetopoGroup1 -> Shadow
|       |   |   `-- Tip          (Sparks, Debris, Dust)
|       |   |-- GripCenterRight -> RightTarget -> GhostRightHand (inactive)
|       |   `-- GripCenterLeft  -> LeftTarget  -> GhostLeftHand  (inactive)
|       `-- Ghostpoles (inactive)  八个方向，每个下面若干 inactive 的 ghost handle
|-- Pot                         (Camera, Directional Light, Mesh -> Reflection Probe, PotSplash)
|-- dude                        (Body, Camera, Eyelashes, Directional Light, Eyes,
|                                mixamorig:Hips 整套骨架, leftCenter, rightCenter, LookTarget)
`-- PotCollider
    |-- Sensor (inactive)
    `-- Sides
```

`Player/handle` 是锤柄，与 `Player/Hub/Slider/Handle` 是两个不同对象，必须使用完整路径和准确大小写区分。

更深层级的组件暂未完整记录。后续只在它们与多人状态、视觉表现、物理行为或生命周期有关时补充。

### Rigidbody2D 集合与顺序

`Player` 子树里恰好有 6 个 `Rigidbody2D`，`GetComponentsInChildren<Rigidbody2D>()` 返回的顺序是（已实机确认）：

| 下标 | 刚体 | 相对 `Player` 根的路径 |
| --- | --- | --- |
| 0 | `Player` | （根自己） |
| 1 | `Hub` | `Hub` |
| 2 | `Slider` | `Hub/Slider` |
| 3 | `Handle` | `Hub/Slider/Handle` |
| 4 | `PoleMiddle` | `Hub/Slider/Handle/PoleMiddle` |
| 5 | `Tip` | `Hub/Slider/Handle/PoleMiddle/Tip` |

`Player` 的其他直接子物体（`handle`、`Pot`、`dude`、`PotCollider`）都不带刚体。

## Player Prefab

远端实例的模板 `PlayerPrefab` 是 `GameManager.CreatePlayerPrefab` 对场景里的 `Player` 做 `Instantiate` 得到的克隆，创建后 `SetActive(false)`。克隆上被移除或改写的部分：

- 删组件：`Saviour`、`Screener`、`MipmapBias`、`PlayerControl`、`PotSounds`、`PlayerSounds`、`HammerCollisions`、所有 `Camera`、所有 `Collider2D`；
- 删物体：`PotCollider/Sensor`；
- 所有 `Rigidbody2D` 改为 `isKinematic = true`。

**没有任何 `Rigidbody2D` 组件被删除。** 因此克隆和本地 `Player` 的刚体集合一致，`GetComponentsInChildren<Rigidbody2D>()` 在两边返回相同长度、相同顺序——`LocalPlayer.TeleportTo` 靠下标逐个配对两边的刚体，依赖的就是这一点（实机已验证长度一致，且打印顺序确为 `Player → Hub → Slider → Handle → PoleMiddle → Tip`）。以后要在 `CreatePlayerPrefab` 里再删物体、删刚体，或往本地 `Player` 上加带刚体的子物体，必须同时改 `TeleportTo` 的配对方式，否则那里的长度检查会让传送静默失效。

这些组件只在克隆上被删掉，场景里真正的 `Player` 一直保留着它们（`Saviour` 等在本地玩家上始终存在）。

## Local Player Component Lifecycle

`LocalPlayer` 组件每次进入游戏都是新的，它上面的缓存字段不会指向上一个场景的对象：

- `GameStartedEvent` 和 `GameRestartedEvent` 只从 `GameManager.OnSceneLoaded`（以及插件在 `Mian` 中途初始化时 `GameManager.Start` 的那次补发）发布，所以"游戏内重开"必然是一次真正的场景重载，不存在原地重置的路径。
- 两处发布之前都先跑 `RefreshGameResources()`，`GameManager.Player` 因此一定已经指向新场景的 `Player`。
- `PlayerManager.OnGameRestartedEvent` 先 `ReleaseLocalPlayer()` 把引用清掉，再 `EnsureLocalPlayer()` 在新的 `Player` 上 `AddComponent<LocalPlayer>()`。

结论：`LocalPlayer` 上按需缓存 Unity 对象（`Saviour`、`Rigidbody2D[]`）不需要失效逻辑，也不需要防"缓存指向已销毁对象"的空检查——那种检查在这里是恒不成立的死代码。唯一复用已有组件的路径是同一场景内断线重连（`EnsureLocalPlayer` 的 `GetComponent<LocalPlayer>()` 分支），那时缓存指向的还是同一个场景里的对象，同样有效。

`PlayerManager.Dispose()` 里不能出现 Unity ECall（`Object.Destroy`），**包括被 JIT 内联进来的**：无 Unity 运行时的进程里整个方法编译不过（`SecurityException: ECall 方法必须打包到系统模块中`），空判挡不住它，因为失败发生在编译方法时，而不是执行到那一行时。销毁场景 `Player` 上的 `LocalPlayer` 组件因此走单独的 `DestroyLocalPlayer()`，并且**必须**标 `NoInlining`——那个方法只有一行，不标就会被内联回 `Dispose`，等于没拆。`MultiplayerUnityCore.TearDown` 是为同一个理由拆出去的（它靠方法体够大躲过内联）。往 `Dispose` 里加回内联的 Unity 调用，会让 `PlayerManagerTests` 的两条 Dispose 测试连调用都进不去。

已知不对称：远端玩家的 Unity 对象生命周期在 `IPlayerInstancePool` 后面（测试整个 Mock 掉），本地玩家没有对应接缝。原因是 `PlayerBase` 是 `MonoBehaviour`，要在测试里造出一个非 null 的本地玩家，得先把 `PlayerBase` / `LocalPlayer` 抽成接口，而这会波及 `PlayerManager._players` 表、皮肤同步的 `GetPlayer(id)` 和 UI 的 `PlayerView`。等真要给本地玩家组件生命周期写覆盖时再做，不要为单条测试启动这个重构。

## Teleport

`LocalPlayer.TeleportTo(Transform target)` 把本地玩家搬到目标玩家的远端实例处。除刚体配对（见 Player Prefab）外，它还用到本地 `Player` 上的 `Saviour`：

- `Saviour.pc.fakeCursor` 与 `Saviour.hammer` 是两个 `Transform`，搬完后把前者的位置对齐到后者；
- `Saviour.slider`、`Saviour.hinge`、`Saviour.hubJoint` 是三个带 `JointMotor2D motor` 的关节，可用于把马达清零。

搬运期间物理模拟被关掉再恢复。`Physics2D.autoSimulation`（旧版 Unity）和 `Physics2D.simulationMode`（新版 Unity）是同一件事的两种 API，游戏可能是任一版本，所以由 `Unity/Helpers/Physics2DHelper` 用反射二选一；只有这两种情况。

## Cursor Object

- `IGameManager.Cursor` 是场景中名为 `Cursor` 的游戏物体，`GameManager.RefreshGameResources` 用 `GameObject.Find("Cursor")` 获取它；它不是控制系统鼠标显隐的 `UnityEngine.Cursor`。
- `Cursor` 物体上带有 `Rigidbody2D`。用户已实机验证：UI 占用鼠标时将其 `simulated` 设为 `false`，释放时恢复为 `true`，可以正确屏蔽并恢复本地游戏输入。
- 之前直接禁用 `PlayerControl` 组件会出现 bug，使用 Harmony 跳过 `PlayerControl.FixedUpdate` 也未达到预期，均已由用户实测否定；当前不再使用这两种方案，具体失败原因不作推断。

## State Synchronization Runtime Behavior

- `LocalPlayer` 挂载在 `Player` 根对象上，从 `Player`、`Player/Hub/Slider` 和 `Player/Hub/Slider/Handle` 读取世界位置与世界旋转。
- 这三个对象只绕 Z 轴转，**世界旋转的 `X`、`Y` 恒为 0**（已实机确认）。状态包因此只传 `Z` 和 `W`，收端把 `X` / `Y` 写回 0；`RemotePlayer` 用 `(0, 0, z, w)` 重建出来的仍是单位四元数，插值也照常。以后要同步会绕 X / Y 转的对象，得先改 `UnityQuaternion` 的读写——格式只在那一处（见 `docs/agent/state-synchronization.md` 的“线上格式只写一遍”）。
- `PlayerStateSynchronizer` 挂载在持久化的 `MultiplayerUnityCore` 子对象上，在 `LateUpdate` 以 60 Hz 采样本地 `Player`，仅在 `Mian` 且连接有效时发送。
- `RemotePlayer` 使用相同的完整路径写入世界位置与世界旋转。首次收到状态时直接定位，后续状态在约一个 60 Hz 间隔内插值。
- 远端实例由 `PlayerInstancePool` 创建的 `PlayerPrefab` 派生，并继续使用现有的无碰撞远端对象处理，因此当前不会与本地 `Player` 产生交互。
- 如果多人插件在当前场景已经是 `Mian` 时才完成初始化，`GameManager.Start` 会补做当前场景资源准备并发布初始 `GameStartedEvent`；游戏中连接服务器时，握手也会触发玩家生命周期初始化并重新绑定本地玩家 ID。

## Pot Skin

罐子的外观全在 `Player/Pot/Mesh` 那个 MeshRenderer 的材质上，贴图和金度是同一个材质上的两件事（已实机确认）：

| 材质成员 | 含义 |
| --- | --- |
| `mainTexture` | 罐子的贴图。皮肤 Mod 换的就是这个 |
| `_Goldness` | 黑罐 `0`、金罐 `1`。通关后的金罐和普通黑罐用的是同一张贴图，差别只有这个值 |

- **皮肤 Mod 只能依赖机制，不能依赖实现。** 现有皮肤 Mod（`SkinCustomizer` 只是其中一个）的实现各不相同，唯一稳定的是它们最终都把上面那个
`mainTexture` 换成一张从本地图片加载的 `Texture2D`。所以同步皮肤读的是那张贴图本身，不读任何 Mod 的配置、目录或 `PlayerPrefs`。
- **读本地用 `sharedMaterial`，写远端用 `material`。** `renderer.material` 会让 Unity 给这个 Renderer 拷一份独立材质，在本地玩家身上读它等
于替皮肤 Mod 改了它的对象；反过来，写远端实例必须用 `material`，否则会改到所有实例共享的那一份。
- **`Object.Instantiate` 不深拷贝 Material。** 所以 `PlayerPrefab` 和从池子里借来的远端实例，材质上挂的是本地玩家的贴图或前一个使用者的贴
图——两种都不能直接给玩家看。远端实例出现时必须重画一次，入口是 `RemotePlayerInstanceCreatedEvent`（`PlayerManager.EnsureRemoteInstance` 里
先入表再发，订阅者第一件事就是 `GetPlayer`）。
- **这个 Unity 版本没有 `Texture.isReadable`**（2018.2+ 才有，游戏是 2017.x；`UnityEngine.CoreModule.dll` 里那个 `isReadable` 字符串属于 `
Mesh`）。所以"这张贴图能不能编码成 PNG"只能试一次：`ImageConversion.EncodeToPNG` 用 try/catch 包住，结果按贴图对象记住。游戏自带的贴图在 C
PU 侧没有像素副本，编码必然失败——**"没装皮肤"走的正是这条失败路径，它不是错误**，只是 Unity 会往控制台打一行错，所以记住结果是为了每张贴图
最多撞一次。
- 因此没装皮肤时的基线不是"读到的贴图"，而是插件内嵌的一张原版贴图（`src/Unity/Resources/VanillaPot.png`），再叠上传过来的 `_Goldness`。

## Room Membership and Scene Independence

- “大厅”是默认 Room，不是 `Loader`；大厅成员也可同时位于 `Mian` 并正常同步。玩家列表的非游戏状态显示为“未在游戏中”。
- 房间成员资格绑定连接，不绑定场景；退出关卡、通关和重开不会退房或转移房主。
- 成功换房先替换客户端名单，发布 `RoomMembershipChangedEvent` 清理远端对象与皮肤，再用 `PlayerRosterReceivedEvent` 创建新房实例；`PlayerManager` 保留本地 Player 及其索引，不调用完整的 `ReleaseGamePlayers`。
- 不移动本地刚体、不重载场景、不重新采集本地皮肤；服务端缓存负责新房间双方的皮肤补发。失败或仅编辑属性不会执行换房清理。
- 目录、密码、继任及入房代次的规则见 `room-system.md`；以上是当前重写实现，不改变旧版姿态采样基线。

## Multiplayer Interaction

- 当前阶段只要求远端玩家不与本地玩家产生交互。
- 玩家之间是否允许交互将在未来房间系统中成为房间设置。
- 当前限制不是永久规则，实现时应避免把“不允许玩家交互”固化为不可配置的领域假设。

## Multiplayer Settings

- 设置完全归宿主：所有者是 `MultiplayerSettings`（`src/Unity.Desktop/Config`，`GOILauncher.Multiplayer.UI.Config`）。core 一层不认识它，`src/Unity` 里没有任何设置类型。
- 落盘用 BepInEx 的 `ConfigFile`（`Plugin.Config`，节名 `Multiplayer`）。键名、类型、默认值、解析容错和写文件都归它，`MultiplayerSettings` 只补两条它不管的规矩：端口范围，和“清空主机名等于回到默认值”。
- 换回 `ConfigFile` 的代价是这套设置只存在于 PC 宿主。以前放在 `src/Unity` 是为了三个平台共用一份键名和默认值，现在认定的边界是“怎么存是宿主自己的事”：以后的 Android / iOS 宿主各自实现自己的存储，core 不掺和。
- 目前有六项：

  | 键 | 类型 | 默认 | 说明 |
  |---|---|---|---|
  | `Enabled` | bool | `true` | 联机开关。唯一真值是 `MultiplayerUnityCore.IsLoaded`，宿主每次加载与销毁后回写这里，所以它同时决定下次启动要不要自动加载 |
  | `HideServerPage` | bool | `true` | 隐藏主面板上的“服务端”页签。纯 UI 可见性：`ServerPage` 照旧构造与 `Bind`，内嵌服务端照常能加载 |
  | `PlayerName` | string | （空） | 客户端页“名字”输入框的初值；空表示还没填，连接会被客户端页拦下 |
  | `ClientHost` | string | `127.0.0.1` | 客户端页“服务器地址”输入框的初值 |
  | `ClientPort` | int | `9027` | 客户端页“端口”输入框的初值 |
  | `ServerPort` | int | `9027` | 服务端页“启动端口”输入框的初值 |

- 后四项存的是**默认值，不回写**：客户端页 / 服务端页拿它们填输入框初值，玩家在那儿临时改成别的值只影响那一次连接。设置页是唯一的写入方。改了默认值时，处于空闲状态（未连接 / 未开服）的页面输入框会立刻跟着更新；连接中或已开服时不动，那时输入框显示的是这条连接的实际目标。
- 端口的合法范围是 `MultiplayerSettings.MinPort`–`MaxPort`（1–65535），判据是 `MultiplayerSettings.IsValidPort`，读侧和 UI 的输入校验（`InputFieldExtensions.TryReadPort`）共用它，所以 UI 不会写进一个下次加载要被判为非法的值。手改配置文件填进来越界的端口按“没配过”处理，读的时候退回默认值。
- `SetClientPort` / `SetServerPort` 收到越界值抛 `ArgumentOutOfRangeException`——校验玩家输入是 UI 的事，走到这里就是 bug。`SetClientHost` 做 trim，空白按“清空即恢复默认”处理。名字只做 trim，空白保留：空是合法状态（还没填），客户端页在连接时会弹“名字不能为空”并拒绝连接，不在这里归一化。
- 每个 `SetXxx` 立刻 `Config.Save()` 落盘：和以前一样，每次写都重写整个配置文件，所以崩溃不会丢掉玩家已经改过的设置。

### 加载与销毁保证什么

联机模块只有两种状态：`MultiplayerUnityCore.IsLoaded` 为真（整张对象图活着），或者为假（没加载，或者已经被 `Dispose` 拆掉）。**没有“已加载但停用”的中间态**，所以也不再有 `IMultiplayerState`、`MultiplayerLifecycleController`，以及 Unity 客户端 / 服务端门面里那圈 `IsMultiplayerEnabled` 守卫。

- 加载：`MultiplayerUnityCore.Initialize(logTarget)` 建 `_core` GameObject、build 容器、激活 `IStartable`，成功后发 `Initialized`。失败会回滚（销毁 `_core`、丢掉半张容器）并返回 false，不会留下一个半死不活的加载状态。
- 销毁：`Dispose()` 的顺序是有条件的，改之前先对一遍：
  1. 先把 `_container` 置 null——从这一刻起三个门面属性全返回 null，没人能再拿到这一轮的实例。判据必须是 `_container`，不能是对象本身：`UnityClient` 是 MonoBehaviour，`Destroy` 之后托管引用仍在，`IsConnected` 这类纯托管属性不碰 Unity API，照样返回上一轮的旧值；而按 `IUnityClient` 接口做的 `== null` 用的是 `object` 的引用相等，Unity 那套假 null 根本不参与。
  2. 发 `Disposing`，趁对象图还活着让 UI 退订、清掉缓存。
  3. `_core.SetActive(false)` 停掉轮询。`Destroy` 要到帧末才生效，不先关的话本帧的 `Update` 还会 `Poll` 一次，到的包能把刚清掉的远端实例重新建出来。
  4. `Disconnect()` → `Stop()` → `PlayerManager.Dispose()`（退订 + `ReleaseGamePlayers` + 销毁挂在场景 `Player` 上的 `LocalPlayer` 组件）。
  5. `Object.Destroy(_core)` 一次带走 `GameManager`、`UnityClient`、`UnityServer`、`PlayerStateSynchronizer`、`SkinSynchronizer`（都在 `_core` 底下）。`GameManager.OnDestroy` 负责退订静态的 `SceneManager.sceneLoaded` 并销毁自己 `Instantiate` 的 `PlayerPrefab` 克隆。
  6. `container.Dispose()` 收掉容器创建的服务：`ClientService` / `ServerService` 连同底下的 `NetworkClient` / `NetworkServer`（`Dispose` 里 `NetManager.Stop()`，收下网络线程）。
- NLog 的 target 注册成 `ExternallyOwned`。它挂在 NLog 的全局配置上，跟着容器一起销毁的话，下一轮 `Initialize` 会按名字复用那个已销毁的 target，日志就全哑了。`CoreManager` 本身按 target 名字去重，所以反复加载不会重复挂规则。
- 随开随关把“关掉时把场景还原干净”从可选变成必做：`LocalPlayer` 组件、`PlayerPrefab` 克隆、`sceneLoaded` 订阅这三样都会活到下一轮，而 `PlayerManager.EnsureLocalPlayer` 的 `GetComponent<LocalPlayer>()` 分支正好会捞到上一轮那个揣着已销毁服务的组件。
- 第二次 `Initialize` 落在 `Mian` 里是能自愈的：新建的 `GameManager.Start` 走 `if (IsInGame && Player == null)` 那一次补做，重新找 `Player`、重造 `PlayerPrefab`、补发 `GameStartedEvent`。这条路径本来就有（插件在 Loader 初始化、进游戏才补做），随开随关只是复用它。

## Multiplayer UI Lifecycle

- 设置页那个勾选框是**动作**，不是状态：`SettingsPage` 发 `LoadToggled`，`Plugin` 去 `Initialize()` / `Dispose()`，再由 `SyncLoadedState()` 按 `IsLoaded` 回写 `Enabled` 并推 `SetLoaded`。所以 `Enabled` 总是等于现状——它是那份开关状态落了盘，不是另一个平行概念；页面也不自己去读它，等宿主推。
- 页面感知加载与销毁靠 `Plugin` 转发的 `Bind(门面)` / `Unbind()`：`Initialized` 时绑，`Disposing` 时解。两者必须成对——漏一次 `Unbind` 不报错，只会让下一次点按钮响应两遍，所以每页的 `Bind` 在已绑定时直接抛。
- “隐藏服务端页面”勾选框在“常规”块里、加载开关下面，与那个开关不同：它**就是状态**，`onValueChanged` 里直接 `_settings.SetHideServerPage(value)` 落盘，再发 `HideServerPageToggled`；`Plugin` 收到后调 `MultiplayerUI.SetServerPageVisible(!hide)` 把页签按钮 `SetActive(false)`。页签按钮行是 `HorizontalLayoutGroup`，inactive 对象不参与布局，剩下的“客户端”“设置”两枚自动撑满。启动时 `Plugin` 按落盘值先应用一次，所以默认安装看不到“服务端”。设置页里的“服务端设置”块不受它影响。
- “设置”页除了那个加载开关还管四个默认值，按“客户端设置”（从上到下是“名字”一行、默认主机和默认端口同一行）和“服务端设置”（默认端口）两段排：`ClientPage` 和 `ServerPage` 从它们取输入框初值，并订阅对应的变更事件，在空闲时跟着更新（见上面 Multiplayer Settings）。这四个输入框不随加载状态变灰——联机没加载也要能先把默认值配好。落盘发生在编辑结束（`InputField.onEndEdit`）和离开设置页时，不是每敲一个字符就写一次：每次写都要整份重写配置文件。端口填了非法值会弹提示并把输入框回填成当前设置值；主机清空则归一化回默认值；名字清空就保留为空，连接时为空会弹“名字不能为空”并拦下连接。名字输入框不设占位提示，空就是空框；已知限制：uGUI 的 `InputField` 文本为空时聚焦不画光标，空名字框点进去看不到光标（未处理）。
- F2 始终切换 `MultiplayerUI`（“连接配置”）窗口，不看联机加没加载，因此关掉之后仍能进入“设置”页重新开启。
- `Plugin.ApplyCursorState` 使用同一判据解锁系统鼠标和把游戏 `Cursor` 刚体的 `simulated` 切为 `false`：配置窗口、房间弹窗、Tab 玩家列表 + 空格、聊天输入激活，或其他 UniverseLib UI 显示；被动聊天 HUD 不屏蔽输入。单独按住 Tab 只显示玩家列表，游戏输入照常，鼠标要 Tab 和空格一起按住才交出。UI 不再占用鼠标时把 `simulated` 切回 `true`，不修改 `PlayerControl` 的启用状态或拦截其方法。
- `Plugin` 每次要用光标时现读 `MultiplayerUnityCore.GameManager`，不缓存实例（缓存了就在关掉联机之后剩下一个已销毁的对象），输入屏蔽直接读它的 `Cursor`，不依赖连接或 `LocalPlayer` 组件。刚体处理在光标状态缓存的提前返回之前执行：UI 开着进入或重载场景时也会处理新 Cursor，并释放此前记录的旧刚体（如果仍存在）。Plugin 被禁用或销毁时把已屏蔽的刚体的 `simulated` 切回 `true`。
- 插件启动时读一次 `Enabled`，为真才 `Initialize()`；这是它唯一一次被当作输入读，之后只被写。启动那次尝试失败同样回写成 false，配置里不会留一个没成真的 true；游戏里关掉再开是同一句 `Initialize()`。不再有“上次退出时是关闭状态，这次别先起服务”的补做逻辑——关着就是整张图不存在。玩家实例那一半在 `Dispose` 的顺序里，见上面“加载与销毁保证什么”。
- 关闭联机时 `ClientPage` 收不到 `Disconnected`（`Unbind` 发生在断线之前），所以它自己在 `Unbind` 里把 `isConnecting`、房间目录和按钮状态归零，不留“正在连接”的残影。
- 关闭联机时 `ChatHudUI` 被隐藏并退出输入激活状态，`PlayerListUI` 被隐藏，Plugin 不再响应 Tab 来显示玩家列表。窗口藏着不等于账清了：`RoomDialogUI` 在 `Unbind` 里关窗，`ChatHudUI` 和 `PlayerListUI` 各自把消息记录和名单行清成空——它们的刷新方法在门面为 null 时照常走，把画面落成“没有门面就没有内容”（房间两行显示未加入 / -），不留一份还能读但底下已经拆掉的 `PlayerView`。
- Tab 玩家列表顶部是一行等宽两列的房间信息：“当前房间：名称 / 房主：名字”，大厅房主显示“无”，未入房显示“未加入 / -”；下方玩家表格的列仍为：玩家 / 信息 / 状态 / 距离 / 操作。距离读的是远端实例到本地玩家的直线距离，玩家没有场景实例时（未在游戏中、实例池已满、首个状态包未到）显示 `-`。
- 操作列是每行的“传送”按钮，只在该玩家有距离（即有远端实例）时可点；本地玩家自己那行不显示按钮，但格子留着以保持列对齐。点它得先按住 Tab + 空格 交出鼠标，因为单独按住 Tab 不会解锁光标。
- 重新加载联机时聊天窗口恢复为可用面板；`Bind` 当场刷一次目录、按钮和端口，Tab 玩家列表在下一次按键时恢复。

## Room UI Lifecycle

- 客户端页目录区块头的按钮顺序为“创建房间 / 编辑房间 / 刷新”，编辑房间仅向房主显示；移除第二行当前房间信息与返回大厅按钮，通过目录加入大厅返回。目录只展示名称、人数与操作，隐藏滚动条并保留滚动能力；目录和弹窗等游戏 UI 都不展示房间 ID，密码列也不显示，但入房密码验证保留。服务端页与客户端页结构对齐：状态块吃掉剩余高度，端口与“启动/停止”固定在底部，按钮落在两页相同的位置。
- 创建、编辑与密码输入复用 `RoomDialogUI`，使用已有 `PanelBase` / `ResponsivePanelDragger`。主配置窗口关闭、离开客户端页、断线、关闭联机时关闭弹窗并清理密码。表单统一为“字段名：输入框”，字段名占固定左列宽、输入框左边缘对齐；副标题只在“加入房间”出现。
- 房间弹窗包含光标解锁与快捷键隔离；Enter 不同时触发聊天，Tab/Shift+Tab 用于表单，不同时弹玩家列表。刚关闭弹窗的同一帧仍拦截快捷键。
- 成功入房后先通过 `ChatHistoryReset` 清掉旧显示与未发送输入，再通过已有聊天消息事件显示一条服务器类型的“已进入{房间名}”；初次进入大厅、创建房间和返回大厅也提示。属性修改、房主变化、操作失败或重复加入当前房间不清理聊天，也不重复提示。

## Documentation Rules

- 对象名和大小写按 Unity 运行时中的真实名称记录。
- 不根据名称推断对象或组件职责。
- 同步相关对象需要记录其 Transform 使用世界坐标还是局部坐标。
- 会驱动 Transform、动画、物理或对象创建销毁的组件需要记录；无关组件可以省略。
