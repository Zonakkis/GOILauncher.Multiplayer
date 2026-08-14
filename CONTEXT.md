# GOI Multiplayer

本上下文描述 Getting Over It 联机功能中的核心游戏概念，供所有实现和讨论使用统一语言。

## Language

**Player**:
游戏中代表一名参与者的完整玩家实体。
_Avoid_: Avatar、Instance（这两个词只用于描述具体表现或运行时实例）

**Hammer Handle（锤柄）**:
玩家锤子的可见柄部。
_Avoid_: Handle（会与运行时控制节点混淆）

**Room**:
一组共同游玩的玩家及其可配置多人规则的归属范围。
_Avoid_: Server、Scene

## Relationships

- 一名处于游戏场景中的参与者由一个 **Player** 表现
- 一个 **Player** 包含一个 **Hammer Handle**
- 一个 **Room** 的设置决定其中玩家之间是否允许交互

## Example dialogue

> **Dev:** “同步 **Player** 时，是否需要同步它的全部子对象？”
> **Domain expert:** “不需要，只同步会影响其他玩家所见状态的部分。”

## Flagged ambiguities

- `Player` 既可能指玩家实体，也可能指 Unity 中名为 `Player` 的根对象；讨论领域规则时指玩家实体，讨论对象层级时明确称为 `Player` 根对象。
- “handle” 可能指锤柄或名为 `Handle` 的运行时节点；领域讨论使用 **Hammer Handle**，对象层级讨论使用完整路径。
