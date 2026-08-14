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

## Validation Workflow

- 仅凭对象层级和代码无法确认最终视觉效果是否正确。
- 状态同步实现完成后，由项目维护者在游戏内测试对象姿态、抖动、错位和传输效果。
- 在没有游戏内问题证据时，不因推测其他对象可能需要同步而阻塞基线方案。
- 收到具体游戏内现象后，再结合对象层级、状态采样和应用逻辑定位并迭代方案。
