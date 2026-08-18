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

已确认的直接子物体：

```text
Player
|-- handle
|-- Hub
|   `-- Slider
|       `-- Handle
|-- Pot
|-- dude
`-- PotCollider
```

`Hub/Slider` 对应当前 `PlayerState` 中的 `SliderPosition` 和 `SliderRotation` 所描述的对象。
`Hub/Slider/Handle` 对应当前 `PlayerState` 中的 `HandlePosition` 和 `HandleRotation` 所描述的对象。
`Player/handle` 是锤柄，与 `Player/Hub/Slider/Handle` 是两个不同对象，必须使用完整路径和准确大小写区分。

更深层级和对象上的组件暂未完整记录。后续只在它们与多人状态、视觉表现、物理行为或生命周期有关时补充。

## State Synchronization Runtime Behavior

- `LocalPlayer` 挂载在 `Player` 根对象上，从 `Player`、`Player/Hub/Slider` 和 `Player/Hub/Slider/Handle` 读取世界位置与世界旋转。
- `PlayerStateSynchronizer` 挂载在持久化的 `MultiplayerUnityCore` 子对象上，在 `LateUpdate` 以 60 Hz 采样本地 `Player`，仅在 `Mian` 且连接有效时发送。
- `RemotePlayer` 使用相同的完整路径写入世界位置与世界旋转。首次收到状态时直接定位，后续状态在约一个 60 Hz 间隔内插值。
- 远端实例由 `PlayerInstancePool` 创建的 `PlayerPrefab` 派生，并继续使用现有的无碰撞远端对象处理，因此当前不会与本地 `Player` 产生交互。
- 如果多人插件在当前场景已经是 `Mian` 时才完成初始化，`GameManager.Start` 会补做当前场景资源准备并发布初始 `GameStartedEvent`；游戏中连接服务器时，握手也会触发玩家生命周期初始化并重新绑定本地玩家 ID。

## Multiplayer Interaction

- 当前阶段只要求远端玩家不与本地玩家产生交互。
- 玩家之间是否允许交互将在未来房间系统中成为房间设置。
- 当前限制不是永久规则，实现时应避免把“不允许玩家交互”固化为不可配置的领域假设。

## Documentation Rules

- 对象名和大小写按 Unity 运行时中的真实名称记录。
- 不根据名称推断对象或组件职责。
- 同步相关对象需要记录其 Transform 使用世界坐标还是局部坐标。
- 会驱动 Transform、动画、物理或对象创建销毁的组件需要记录；无关组件可以省略。
