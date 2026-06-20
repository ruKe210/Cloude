# Demo0 白模战斗场景

## 场景路径

`Assets/Scenes/Demo0.unity`

## 场景结构（MCP 搭建）

```
Demo0
├── Main Camera          (+ CameraShake)
├── Directional Light
├── Environment
│   ├── Floor            (Plane 白模地面)
│   └── BackWall         (Cube 背景墙)
├── Entities
│   ├── PlayerWhiteBox   (Cube + WorldEntityView) → Prefab
│   └── EnemyWhiteBox    (Cube + WorldEntityView) → Prefab
└── BattleRoot           (+ Demo0BattleController)
```

## Prefab

| Prefab | 路径 |
|--------|------|
| 玩家白模 | `Assets/Prefabs/Demo0/PlayerWhiteBox.prefab` |
| 敌人白模 | `Assets/Prefabs/Demo0/EnemyWhiteBox.prefab` |

## 运行

1. 打开 `Demo0` 场景
2. 点击 Play
3. 点击底部手牌攻击红色方块敌人
4. 敌人受击时：**屏幕震动** + 敌人方块闪白放大

## Demo0 战斗规则（简化）

- **仅玩家攻击**，敌人不会还手
- 无「结束回合」按钮
- 初始牌组：**4× Strike + 1× Bash**
- 击败 Jaw Worm（44 HP）即胜利

## 新增脚本

| 脚本 | 作用 |
|------|------|
| `Demo0BattleController` | Demo0 战斗入口、UI、震屏触发 |
| `WorldEntityView` | 3D 白模实体 HP 显示、受击反馈 |
| `CameraShake` | 相机震动 |
