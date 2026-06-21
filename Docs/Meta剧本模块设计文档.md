# Meta 剧本模块 — 设计文档

> **版本**：v1.0  
> **范围**：JSON 配置驱动的文字冒险 / 视觉小说式 Meta 层  
> **与战斗关系**：剧本节点可触发战斗场景，战斗结束可跳回剧本锚点  
> **Unity 项目**：`D:\unity_project\Cloude\Cloude`  
> **命名空间建议**：`STS.Meta`

---

## 目录

1. [模块目标](#1-模块目标)
2. [架构总览](#2-架构总览)
3. [剧本 JSON 格式规范](#3-剧本-json-格式规范)
4. [节点类型与跳转规则](#4-节点类型与跳转规则)
5. [铆点（Anchor）机制](#5-铆点anchor机制)
6. [运行时状态机](#6-运行时状态机)
7. [表现层交互规范](#7-表现层交互规范)
8. [代码目录与类职责](#8-代码目录与类职责)
9. [核心流程伪代码](#9-核心流程伪代码)
10. [与战斗模块衔接](#10-与战斗模块衔接)
11. [MVP 范围与排期](#11-mvp-范围与排期)
12. [示例剧本](#12-示例剧本)
13. [扩展预留](#13-扩展预留)

---

## 1. 模块目标

实现一个 **Meta 游戏层**：玩家在主界面或独立场景中阅读剧情，通过选择改变分支，但最终通过 **铆点事件** 收敛到关键剧情。

| 功能 | 说明 |
|------|------|
| JSON 配剧本 | 策划/程序无需改 C# 即可增删节点 |
| 打字机文本 | 底部对话框逐字显示 |
| 左键跳过 | 单击左键立即显示本句全文；全文已显示时再点进入下一句 |
| 分支选择 | 2~4 个选项，点击后进入不同节点 |
| 铆点收敛 | 多条分支最终汇入同一关键节点，保证主线可控 |
| 条件分支 | 基于 Flag 显示/隐藏选项或走不同 next |

---

## 2. 架构总览

沿用战斗模块已有原则：**逻辑在纯 C#，MonoBehaviour 只做表现**。

```
┌─────────────────────────────────────────────────────────┐
│  Presentation（MonoBehaviour）                           │
│  NarrativeSceneController                               │
│  ├── NarrativeDialogueView   打字机 + 左键跳过            │
│  └── NarrativeChoiceView     选项按钮                     │
└───────────────────────┬─────────────────────────────────┘
                        │ 订阅事件 / 转发输入
┌───────────────────────▼─────────────────────────────────┐
│  Meta Runtime（纯 C#）                                   │
│  NarrativeEngine        推进节点、解析跳转、管理状态机     │
│  NarrativeContext       当前节点、Flag、访问记录           │
└───────────────────────┬─────────────────────────────────┘
                        │ 反序列化
┌───────────────────────▼─────────────────────────────────┐
│  Data（DTO + 枚举）                                      │
│  NarrativeScriptDefinition / NarrativeNode*              │
└───────────────────────┬─────────────────────────────────┘
                        │ 读取
┌───────────────────────▼─────────────────────────────────┐
│  IO                                                      │
│  NarrativeScriptLoader   Resources 或 StreamingAssets    │
└─────────────────────────────────────────────────────────┘
```

与战斗模块对照：

| 战斗 | Meta 剧本 |
|------|-----------|
| `BattleContext` | `NarrativeContext` |
| `BattleManager` | `NativeEngine` |
| `CombatActionManager` | 无队列（MVP 同步推进即可） |
| `HandController` | `NarrativeChoiceView` |
| 卡牌 JSON（未来） | 剧本 JSON |

---

## 3. 剧本 JSON 格式规范

### 3.1 文件位置

```
Assets/
  Resources/
    MetaScripts/
      prologue.json          ← MVP 第一个剧本
      chapter01.json
  StreamingAssets/           ← 可选，便于热更新
    MetaScripts/
```

MVP 推荐 **Resources**，与现有 `ChineseFontHelper` 加载方式一致。

### 3.2 顶层结构

```json
{
  "scriptId": "prologue",
  "version": 1,
  "title": "序章：周一早晨",
  "startNodeId": "n001",
  "anchors": {
    "meet_deadline": "n050",
    "after_battle": "n080"
  },
  "nodes": {
    "n001": { ... },
    "n002": { ... }
  }
}
```

| 字段 | 类型 | 必填 | 说明 |
|------|------|------|------|
| `scriptId` | string | 是 | 剧本唯一 ID，存档用 |
| `version` | int | 是 | 剧本版本，改 JSON 时可递增 |
| `title` | string | 否 | 显示用标题 |
| `startNodeId` | string | 是 | 入口节点 ID |
| `anchors` | object | 否 | 铆点 ID → 节点 ID 映射表 |
| `nodes` | object | 是 | 节点字典，key 为节点 ID |

### 3.3 跳转目标写法（统一约定）

所有 `next`、`options[].next` 均使用 **JumpTarget 字符串**：

| 写法 | 含义 | 示例 |
|------|------|------|
| `"n050"` | 直接跳转到节点 | 普通线性剧情 |
| `"anchor:meet_deadline"` | 跳转到铆点 | 分支收敛 |
| `"end"` | 剧本结束 | 回主菜单或下一章 |
| `"battle:demo0"` | 进入战斗 | 见第 10 节 |

---

## 4. 节点类型与跳转规则

MVP 实现 **4 种节点类型** 即可覆盖 90% 需求。

### 4.1 `line` — 对话/旁白（打字机）

```json
"n001": {
  "type": "line",
  "speaker": "旁白",
  "text": "周一早上，你像往常一样走进开放式办公室。",
  "next": "n002",
  "typewriter": {
    "charsPerSecond": 30,
    "skippable": true
  }
}
```

| 字段 | 说明 |
|------|------|
| `speaker` | 说话人名字，空则只显示正文 |
| `text` | 正文，支持 `\n` 换行 |
| `next` | 本句播完并确认后的跳转 |
| `typewriter.charsPerSecond` | 打字速度，0 表示瞬间全文 |
| `typewriter.skippable` | 是否允许左键跳过 |

### 4.2 `choice` — 玩家选择

```json
"n010": {
  "type": "choice",
  "prompt": "产品经理走过来，你要怎么回应？",
  "options": [
    {
      "text": "「好的，今天一定交付。」",
      "next": "n011",
      "setFlags": ["agree_overtime"]
    },
    {
      "text": "「需求还没定稿，交付不了。」",
      "next": "n012",
      "setFlags": ["refuse_polite"]
    },
    {
      "text": "（沉默，假装在写代码）",
      "next": "n013",
      "setFlags": ["ignore_pm"],
      "requireFlagsNone": ["met_boss"]
    }
  ]
}
```

| 字段 | 说明 |
|------|------|
| `prompt` | 选择前的提示语（可选，可单独作为上一句 line 省略） |
| `options[].text` | 按钮文字 |
| `options[].next` | 选中的跳转 |
| `options[].setFlags` | 选中后写入的 Flag |
| `options[].requireFlags` | 必须全部拥有才显示 |
| `options[].requireFlagsNone` | 必须全部没有才显示 |

引擎在构建选项列表时 **过滤不可见选项**；若过滤后 0 个选项，打 Error 并停在该节点。

### 4.3 `set` — 静默设置（无 UI）

用于分支汇合前改 Flag、记变量，不显示对话框。

```json
"n040": {
  "type": "set",
  "setFlags": ["met_boss"],
  "clearFlags": ["temp_choice"],
  "next": "anchor:meet_deadline"
}
```

### 4.4 `if` — 条件分支（无 UI）

```json
"n030": {
  "type": "if",
  "conditions": [
    { "requireFlags": ["agree_overtime"], "next": "n031" },
    { "requireFlags": ["refuse_polite"], "next": "n032" }
  ],
  "fallback": "n033"
}
```

按数组顺序匹配，第一个满足的 `next` 生效；都不满足走 `fallback`。

---

## 5. 铆点（Anchor）机制

### 5.1 设计意图

- **分支**：玩家选择导致中间过程不同（不同 line / choice）
- **铆点**：无论走哪条路，都 **必须** 经过的关键剧情（见老板、触发战斗等）

### 5.2 实现方式

在顶层 `anchors` 表声明：

```json
"anchors": {
  "meet_deadline": "n050",
  "after_first_battle": "n080"
}
```

任意节点的 `next` 写 `"anchor:meet_deadline"` 时，引擎解析为 `nodes["n050"]`。

```
        n011 ──► 分支 A 台词 ──┐
                              ├──► anchor:meet_deadline ──► n050（老板登场）
        n012 ──► 分支 B 台词 ──┘
        n013 ──► 分支 C 台词 ──┘
```

### 5.3 铆点执行时的副作用（可选）

节点 `n050` 本身仍是普通 `line`，可在其前插入 `set` 节点统一写 Flag：

```json
"n049": {
  "type": "set",
  "setFlags": ["met_boss"],
  "next": "n050"
}
```

### 5.4 禁止事项

- 锚点 ID 不得成环（`n050` 不应再跳回 `anchor:meet_deadline` 除非刻意设计循环章）
- 锚点目标节点必须存在于 `nodes` 中，加载时校验

---

## 6. 运行时状态机

```
                    ┌──────────┐
                    │  Idle    │
                    └────┬─────┘
                         │ LoadScript + Start
                         ▼
              ┌──────────────────────┐
         ┌───►│  PlayingLine         │◄───┐
         │    │  （打字机播放中）      │    │
         │    └──────────┬───────────┘    │
         │               │ 左键跳过/播完  │
         │               ▼                │
         │    ┌──────────────────────┐    │
         │    │  WaitingLineAdvance  │    │
         │    │  （全文已显示，等点击）│    │
         │    └──────────┬───────────┘    │
         │               │ 左键/自动      │
         │               ▼                │
         │    ┌──────────────────────┐    │
         │    │  ResolveNext         │    │
         │    │  line→next           │    │
         │    │  set/if→自动跳       │    │
         │    │  choice→等待选择     │    │
         │    └──────────┬───────────┘    │
         │               │                │
         │      ┌────────┼────────┐       │
         │      ▼        ▼        ▼       │
         │  Playing  Waiting  Ended/     │
         │  Line     Choice    Battle    │
         │               │                │
         └───────────────┴────────────────┘
```

| 状态 | 说明 |
|------|------|
| `PlayingLine` | 正在逐字显示 |
| `WaitingLineAdvance` | 全文显示完毕，等待玩家点击进入下一句 |
| `WaitingChoice` | 显示选项，等待点击 |
| `Ended` | 剧本结束 |
| `BattleHandoff` | 跳转战斗（挂起剧本，战斗结束恢复） |

---

## 7. 表现层交互规范

### 7.1 对话框布局（UGUI）

```
┌─────────────────────────────────────────────┐
│                  游戏画面                    │
│                                             │
├─────────────────────────────────────────────┤
│ 【说话人名字】                               │
│ 打字机正文区域...........................▌   │
│                                             │
│              [ 选项1 ]  [ 选项2 ]           │  ← choice 时显示
└─────────────────────────────────────────────┘
```

- 使用现有 `ChineseFontHelper` + `UiSpriteHelper`
- `Canvas` `sortingOrder` 高于战斗 HUD

### 7.2 左键逻辑（统一入口 `OnAdvanceInput()`）

| 当前状态 | 左键行为 |
|----------|----------|
| `PlayingLine` | 跳过打字机，立即显示全文 → 进入 `WaitingLineAdvance` |
| `WaitingLineAdvance` | 调用 `Engine.Advance()` 进入下一节点 |
| `WaitingChoice` | 忽略（必须点选项） |

### 7.3 打字机实现要点

```csharp
// NarrativeDialogueView — 仅负责显示，不持有剧本逻辑
public void PlayLine(string speaker, string fullText, float charsPerSecond);
public void ShowFullText();           // 跳过
public bool IsTypingComplete { get; }
public event Action OnTypingComplete;
```

- 用 **Unicode 字符** 计数（中文按 1 字），不要用 byte
- `\n` 保留换行
- 打字中可显示简单光标 `▌`

---

## 8. 代码目录与类职责

```
Assets/Scripts/
  Meta/
    Data/
      NarrativeNodeType.cs          enum: Line, Choice, Set, If
      NarrativeScriptDefinition.cs  根 DTO
      NarrativeLineNode.cs
      NarrativeChoiceNode.cs
      NarrativeSetNode.cs
      NarrativeIfNode.cs
      NarrativeOption.cs
      NarrativeJumpTarget.cs        解析 "n001" / "anchor:xxx" / "end" / "battle:xxx"
    Core/
      NarrativeContext.cs           Flags, CurrentNodeId, ScriptRef, VisitLog
      NarrativeEngine.cs            状态机 + Advance + SelectOption
      NarrativeState.cs             enum
      NarrativeFlagSet.cs           HashSet<string> 封装
    IO/
      NarrativeScriptLoader.cs      LoadFromResources(scriptId)
      NarrativeScriptValidator.cs   加载后校验锚点、节点引用
    Presentation/
      NarrativeSceneController.cs   场景入口，连接 Engine 与 View
      NarrativeDialogueView.cs      打字机 + 说话人
      NarrativeChoiceView.cs        动态生成选项 Button
      NarrativeInputHandler.cs      统一鼠标左键 → OnAdvanceInput
```

### 8.1 各类职责（一句话）

| 类 | 职责 |
|----|------|
| `NarrativeScriptDefinition` | JSON 反序列化结果，不含逻辑 |
| `NarrativeScriptLoader` | `Resources.Load<TextAsset>` + `JsonUtility` 或 **Newtonsoft**（推荐 Dictionary 用 Newtonsoft） |
| `NarrativeScriptValidator` | 检查 startNode、anchors、next 目标是否存在 |
| `NarrativeContext` | 一场剧本运行的所有 mutable 状态 |
| `NarrativeEngine` | **唯一** 修改 Context 并发出事件的地方 |
| `NarrativeSceneController` | 听 Engine 事件 → 更新 UI；听 UI 输入 → 调 Engine |
| `NarrativeDialogueView` | 纯 View，不知道 JSON 结构 |

### 8.2 JSON 反序列化选型

Unity 自带 `JsonUtility` **不支持** `Dictionary<string, Node>`。

MVP 二选一：

| 方案 | 优点 | 缺点 |
|------|------|------|
| **A. Newtonsoft.Json**（Package Manager） | 直接反序列化 nodes 字典 | 多一个依赖 |
| **B. 节点数组 + 加载时建索引** | 零依赖 | JSON 略丑 |

**推荐 A**，节点表用 `Dictionary<string, JObject>` 再按 `type` 分发到具体类。

若坚持零依赖，JSON 改为：

```json
"nodes": [
  { "id": "n001", "type": "line", ... },
  { "id": "n002", "type": "choice", ... }
]
```

---

## 9. 核心流程伪代码

### 9.1 启动剧本

```csharp
// NarrativeSceneController.Start
var script = NarrativeScriptLoader.Load("prologue");
NarrativeScriptValidator.Validate(script);
_engine = new NarrativeEngine(script);
_engine.StateChanged += OnEngineStateChanged;
_engine.Start();
```

### 9.2 Engine 推进

```csharp
// NarrativeEngine
public void Start() {
    _context = new NarrativeContext(_script);
    EnterNode(_script.StartNodeId);
}

void EnterNode(string nodeId) {
    var node = ResolveNode(nodeId);
    switch (node.Type) {
        case Line:
            State = PlayingLine;
            Emit(new LineEvent(node.Speaker, node.Text, node.Typewriter));
            break;
        case Choice:
            State = WaitingChoice;
            Emit(new ChoiceEvent(FilterOptions(node.Options)));
            break;
        case Set:
            ApplySet(node);
            EnterNode(ResolveJump(node.Next));  // 自动节点，不等待输入
            break;
        case If:
            EnterNode(ResolveIf(node));
            break;
    }
}

public void Advance() {
    if (State != WaitingLineAdvance) return;
    var node = _context.CurrentNode as LineNode;
    EnterNode(ResolveJump(node.Next));
}

public void SelectOption(int index) {
    if (State != WaitingChoice) return;
    var opt = _visibleOptions[index];
    _context.Flags.AddRange(opt.SetFlags);
    EnterNode(ResolveJump(opt.Next));
}

string ResolveJump(string target) {
    if (target == "end") { State = Ended; return null; }
    if (target.StartsWith("anchor:"))
        return _script.Anchors[target.Substring(7)];
    if (target.StartsWith("battle:"))
        { State = BattleHandoff; return target; }
    return target; // 普通 nodeId
}
```

### 9.3 View 响应

```csharp
void OnEngineStateChanged(NarrativeEvent e) {
    switch (e) {
        case LineEvent line:
            _choiceView.Hide();
            _dialogueView.PlayLine(line.Speaker, line.Text, line.CharsPerSecond);
            break;
        case ChoiceEvent choice:
            _dialogueView.ShowPrompt(choice.Prompt);
            _choiceView.Show(choice.Options, idx => _engine.SelectOption(idx));
            break;
        case EndedEvent:
            // 加载下一章或回主菜单
            break;
        case BattleEvent battle:
            SceneManager.LoadScene(battle.SceneName);
            break;
    }
}

void Update() {
    if (Input.GetMouseButtonDown(0))
        HandleAdvanceInput();
}
```

---

## 10. 与战斗模块衔接

### 10.1 剧本跳转战斗

```json
"n060": {
  "type": "line",
  "speaker": "死线",
  "text": "那就用代码说话吧！",
  "next": "battle:demo0"
}
```

`NarrativeSceneController` 进入 `BattleHandoff` 时：

1. 将 `NarrativeContext` 序列化进 **静态会话** 或 `DontDestroyOnLoad` 的 `GameSession`：
   - `scriptId`, `returnNodeId`（战斗后继续的锚点或节点）
2. 加载 `Demo0` 战斗场景
3. 战斗胜利/失败后，`Demo0BattleController` 读 `GameSession`，跳回 `NarrativeScene` 并 `EnterNode(returnNodeId)`

### 10.2 战斗后回到铆点

```json
"anchors": {
  "after_first_battle": "n080"
}
```

战斗结束前设置：

```csharp
GameSession.ReturnJump = "anchor:after_first_battle";
```

---

## 11. MVP 范围与排期

### 11.1 MVP 包含

- [ ] JSON 加载 + 校验
- [ ] 4 种节点：line / choice / set / if
- [ ] 锚点跳转
- [ ] 打字机 + 左键跳过/前进
- [ ] 选项按钮（2~4 个）
- [ ] Flag 条件选项
- [ ] 1 个示例剧本 `prologue.json`（含 2 条分支 + 1 个铆点）
- [ ] 独立场景 `MetaStory.unity`

### 11.2 MVP 不包含

- 存档/读档（仅预留 `NarrativeContext.ToSaveData()`）
- 立绘 / 背景切换 / 语音
- 可视化节点编辑器（先用 VS Code 改 JSON）
- 复杂表达式（只用 Flag 与 / 或）

### 11.3 建议排期（约 4~5 天）

| 天 | 内容 |
|----|------|
| D1 | Data DTO + Loader + Validator + 示例 JSON |
| D2 | NarrativeEngine + Context + 单元测试式 Console 日志 |
| D3 | DialogueView 打字机 + ChoiceView + Input |
| D4 | MetaStory 场景串联 + 示例剧本跑通 |
| D5 | 战斗衔接 GameSession + Demo0 回跳 |

---

## 12. 示例剧本

完整示例见规划路径：`Assets/Resources/MetaScripts/prologue.json`

结构示意：

```
n001 旁白开场
  → n002 选择（加班 / 拒绝 / 装死）
    → n011 / n012 / n013 三条短分支
      → anchor:meet_deadline
        → n050 老板统一登场（铆点）
          → n051 选择（战斗 / 逃跑）
            → battle:demo0 或 n060 逃跑结局
```

---

## 13. 扩展预留

| 扩展 | 做法 |
|------|------|
| 存档 | `NarrativeContext` 导出 `{ scriptId, nodeId, flags[] }` |
| 背景/立绘 | line 节点加 `background`, `portrait` 字段，View 监听 |
| 变量数值 | Flag 之外加 `variables: { trust: 3 }`，if 节点支持比较 |
| 随机分支 | if 节点加 `randomWeight` 选项 |
| 编辑器 | 后续用 NodeCanvas 或自研 Editor 导出同一 JSON Schema |
| 本地化 | text 改为 `{ "key": "prologue.n001" }` 查表 |

---

## 总结

| 决策 | 选择 |
|------|------|
| 存储格式 | JSON，节点字典 + 锚点表 |
| 分支 | choice 节点 + Flag 条件 |
| 收敛 | `anchor:xxx` 跳转 |
| 架构 | 与战斗一致：Engine（纯 C#）+ View（MonoBehaviour） |
| 输入 | 左键统一：跳过打字 → 下一句 |
| 序列化 | 推荐 Newtonsoft.Json 支持 Dictionary |

下一步建议：先实现 **D1 Data + Loader + prologue.json**，用 Console 跑通 Engine 无 UI 版本，再接打字机 UI。
