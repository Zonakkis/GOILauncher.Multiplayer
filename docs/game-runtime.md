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

## Teleport

`LocalPlayer.TeleportTo(Transform target)` 把本地玩家搬到目标玩家的远端实例处。除刚体配对（见 Player Prefab）外，它还用到本地 `Player` 上的 `Saviour`：

- `Saviour.pc.fakeCursor` 与 `Saviour.hammer` 是两个 `Transform`，搬完后把前者的位置对齐到后者；
- `Saviour.slider`、`Saviour.hinge`、`Saviour.hubJoint` 是三个带 `JointMotor2D motor` 的关节，可用于把马达清零。

搬运期间物理模拟被关掉再恢复。`Physics2D.autoSimulation`（旧版 Unity）和 `Physics2D.simulationMode`（新版 Unity）是同一件事的两种 API，游戏可能是任一版本，所以由 `Unity/Helpers/Physics2DHelper` 用反射二选一；只有这两种情况。

## State Synchronization Runtime Behavior

- `LocalPlayer` 挂载在 `Player` 根对象上，从 `Player`、`Player/Hub/Slider` 和 `Player/Hub/Slider/Handle` 读取世界位置与世界旋转。
- 这三个对象只绕 Z 轴转，**世界旋转的 `X`、`Y` 恒为 0**（已实机确认）。状态包因此只传 `Z` 和 `W`，收端把 `X` / `Y` 写回 0；`RemotePlayer` 用 `(0, 0, z, w)` 重建出来的仍是单位四元数，插值也照常。以后要同步会绕 X / Y 转的对象，得先改 `UnityQuaternion` 的读写——格式只在那一处（见 `docs/state-synchronization.md` 的“线上格式只写一遍”）。
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

## Multiplayer Interaction

- 当前阶段只要求远端玩家不与本地玩家产生交互。
- 玩家之间是否允许交互将在未来房间系统中成为房间设置。
- 当前限制不是永久规则，实现时应避免把“不允许玩家交互”固化为不可配置的领域假设。

## Multiplayer Settings

- 设置的所有者是 `MultiplayerSettings`（`src/Unity`，平台无关）：键名、类型、默认值和变更事件都在它身上，UI 只读它、调它、订阅它。
- 落盘由 `ISettingsStore` 承担，当前唯一实现是 `FileSettingsStore`，写到 `Application.persistentDataPath` 下的 `GOILauncher.Multiplayer.cfg`。这个目录三个平台都可写且不需要权限：Windows 在 `%USERPROFILE%\AppData\LocalLow\<company>\<product>`，Android 在应用私有目录，iOS 在沙盒内。
- 文件是 `key=value` 纯文本，按键名 Ordinal 排序后整体重写，所以行序稳定、可 diff；`#` 开头是注释，但手写的注释在下次保存时不会保留。值里的 `\`、换行和回车做转义。
- 每次 `SetXxx` 立即写盘（写临时文件再 `File.Move` 覆盖），崩溃不会丢已经改过的设置。文件缺失、读不出来或某行格式不对都只记日志并退回默认值，不阻塞插件加载。
- 目前有四项：

  | 键 | 类型 | 默认 | 说明 |
  |---|---|---|---|
  | `Multiplayer.Enabled` | bool | `true` | 联机总开关，见下面“关闭联机保证什么” |
  | `Multiplayer.Client.DefaultHost` | string | `127.0.0.1` | 客户端页“服务器IP”输入框的初值 |
  | `Multiplayer.Client.DefaultPort` | int | `9027` | 客户端页“端口”输入框的初值 |
  | `Multiplayer.Server.DefaultPort` | int | `9027` | 服务端页“启动端口”输入框的初值 |

- 后三项存的是**默认值，不回写**：客户端页 / 服务端页拿它们填输入框初值，玩家在那儿临时改成别的地址只影响那一次连接。设置页是唯一的写入方。改了默认值时，处于空闲状态（未连接 / 未开服）的页面输入框会立刻跟着更新；连接中或已开服时不动，那时输入框显示的是这条连接的实际目标。
- 端口的合法范围是 `MultiplayerSettings.MinPort`–`MaxPort`（1–65535），判据是 `MultiplayerSettings.IsValidPort`，读侧和 UI 校验共用它，所以 UI 不会写进一个下次加载会被判为非法的值。端口的落盘和解析都走 `CultureInfo.InvariantCulture`：文件是机器写的，换个系统区域也要读回同一个值。
- 读到解析不出来或越界的端口、空白的主机名，都只 `Warn` 一行并退回默认值；`SetClientPort` / `SetServerPort` 收到越界值则抛 `ArgumentOutOfRangeException`——校验玩家输入是 UI 的事，走到这里就是 bug。`SetClientHost` 做 trim，空白按“清空即恢复默认”处理。
- BepInEx 的 `ConfigFile` 不再参与联机设置——它只存在于 PC 宿主，而这套设置要给三个平台共用。

### 关闭联机保证什么

判据只有 `IMultiplayerState` 一个来源（`Enabled` 读当前值，`EnabledChanged` 等它变），两个订阅者各管一半：

- **网络那一半**归 `MultiplayerLifecycleController`：断开当前连接、停掉内嵌服务端，并拒绝之后的连接与开服请求。
- **玩家实例那一半**归 `PlayerManager`，它把开关当成和 `GameStartedEvent` 同类的输入：
  - 关着联机进 `Mian` 不挂 `LocalPlayer`、不预热实例池，一个 `Player` 克隆都不造；
  - 关闭那一刻按和“退出游戏”“断线”完全相同的一套动作（`ReleaseGamePlayers`）撤掉已经建起来的实例；
  - 重新打开时如果已经在 `Mian` 里就地补做初始化，不需要退出关卡再进。
- **UI 那一半**归 `Plugin` 和各 `IPage`：藏掉聊天窗口和玩家列表（见下面 Multiplayer UI Lifecycle）。

`PlayerManager.EnsureRemoteInstance` 除了上面的初始化入口，自己也查一次开关：关闭触发的断开要到下一次 `Poll` 才真正生效，这中间到达的包仍会走到创建实例那条路上。

两件**故意不做**的事，改之前先看这里：

- `GameManager.CreatePlayerPrefab` 不看开关。`GameManager` 是场景权威，只回答“什么场景、`Player` 在哪”，掺进联机开关会让它多背一个概念；代价是关着联机进游戏仍有 1 个 inactive、无碰撞的克隆（之前是 5 个），换来的是重新打开联机时 `PlayerPrefab` 已经就绪，不用重新准备。
- 网络层的 socket 不关。`NetworkClient` 是 `IStartable`，容器 `Build()` 时就 `NetManager.Start()`，关闭联机后仍每帧 `PollEvents`。延迟开 socket 会把“现在能不能连”变成一个状态机，代价大于一次空轮询。

`UnityClient.IsConnected` 和 `UnityServer.IsRunning` 都只读真实状态，不再 AND 一次开关：既然关闭会真的断开，再掺一层只会在断开失败时报出一个假的“没连接”，而 `PlayerStateSynchronizer` 那边看的是真 socket，两边就会各说一套。

## Multiplayer UI Lifecycle

- 联机开关是 `MultiplayerSettings.Enabled`，`SettingsPage` 的勾选框读写它，`Plugin`、`ClientPage`、`ServerPage` 订阅 `EnabledChanged` 刷新自己。
- “设置”页除了开关还管三项默认地址，按“客户端设置”（默认主机 + 默认端口同一行）和“服务端设置”（默认端口）两段排：`ClientPage` 和 `ServerPage` 从它们取输入框初值，并订阅对应的变更事件，在空闲时跟着更新（见上面 Multiplayer Settings）。这三个输入框不随联机开关变灰——关着联机也要能先把默认值配好。落盘发生在编辑结束（`InputField.onEndEdit`）和离开设置页时，不是每敲一个字符就写一次：每次写都要整份重写配置文件。端口填了非法值会弹提示并把输入框回填成当前设置值；主机清空则归一化回默认值。
- F2 始终切换 `MultiplayerUI`（“连接配置”）窗口，不受联机开关影响，因此关闭联机后仍可进入“设置”页重新启用。
- 关闭联机开关时，`MultiplayerLifecycleController`（`src/Unity`）立即请求 `IUnityClient.Disconnect()` 和 `IUnityServer.Stop()`；Unity 客户端和服务端适配器也会拒绝后续的连接或启动请求。它在容器初始化末尾被显式解析一次，构造时就按持久化的值补做一遍，所以上次退出时是关闭状态的话，这次启动不会先起服务再关掉。玩家实例那一半不在它身上，见上面“关闭联机保证什么”。
- 因为关开关也算一次主动断开，`ClientPage` 的提示按当前开关状态分支（“联机已关闭，连接已断开”），不靠标志位——设置在通知任何监听者之前就已写好，谁先收到通知都不影响读到的值。
- 关闭联机时 `ChatHudUI` 被隐藏并退出输入激活状态，`PlayerListUI` 被隐藏，Plugin 不再响应 Tab 来显示玩家列表。
- Tab 玩家列表的列为：玩家 / 信息 / 状态 / 距离 / 操作。距离读的是远端实例到本地玩家的直线距离，玩家没有场景实例时（在大厅、实例池已满、首个状态包未到）显示 `-`。
- 操作列是每行的“传送”按钮，只在该玩家有距离（即有远端实例）时可点；本地玩家自己那行不显示按钮，但格子留着以保持列对齐。
- 重新启用联机时聊天窗口恢复为可用面板；客户端连接按钮、服务端启动按钮和对应输入控件会在页面激活或状态变更时刷新，Tab 玩家列表在下一次按键时恢复。

## Documentation Rules

- 对象名和大小写按 Unity 运行时中的真实名称记录。
- 不根据名称推断对象或组件职责。
- 同步相关对象需要记录其 Transform 使用世界坐标还是局部坐标。
- 会驱动 Transform、动画、物理或对象创建销毁的组件需要记录；无关组件可以省略。
