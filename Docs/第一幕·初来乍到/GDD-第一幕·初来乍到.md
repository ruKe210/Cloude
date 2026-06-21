# YES Cloude — 第一幕 GDD「初来乍到」

> **版本**：v1.0  
> **范围**：仅第一幕（不含序章 / 第二幕及以后）  
> **项目**：Cloude（Unity 2022.3）  
> **上级文档**：[YES Cloude GDD（全篇）](../YES%20Cloude%20GDD.md)  
> **关联表**：[程序需求表](./程序需求表.md)｜[美术需求表](./美术需求表.md)

---

## 目录

1. [幕概述](#1-幕概述)
2. [核心体验（第一幕）](#2-核心体验第一幕)
3. [叙事结构](#3-叙事结构)
4. [角色](#4-角色)
5. [Meta 场景与剧本](#5-meta-场景与剧本)
6. [三场战斗](#6-三场战斗)
7. [怪物与 Boss](#7-怪物与-boss)
8. [系统需求（程序三大系统）](#8-系统需求程序三大系统)
9. [卡牌与成长（第一幕）](#9-卡牌与成长第一幕)
10. [Flag 与数据](#10-flag-与数据)
11. [表现与美术](#11-表现与美术)
12. [技术对接](#12-技术对接)
13. [里程碑与验收](#13-里程碑与验收)
14. [飞书资源索引](#14-飞书资源索引)

---

## 1. 幕概述

### 1.1 幕名与定位

| 项目 | 内容 |
|------|------|
| **幕名** | 初来乍到 |
| **在全游戏中的位置** | 第一幕（新人入职第一天） |
| **时长目标** | 30~45 分钟（Meta + 3 场战斗） |
| **功能** | 教学 Meta / 卡牌战斗 / Cloude 协作风险 / 龙虾失控伏笔 |

### 1.2 一句话

**入职游戏公司「星点互动」的第一天：导师带你打三场「工作式战斗」，从 ERROR 小毛病到屎山代码，再见识 Cloude 的「帮忙」与龙虾的「一键 YES」。**

### 1.3 主题与情感弧

| 阶段 | 主题 | 玩家感受 |
|------|------|----------|
| 开场 | 新人入职、熟悉流程 | 期待 + 紧张 |
| 战斗1 | 工作流 = 打怪 | 「上班就是打怪」 |
| 战斗2 | AI 协作有风险 | 省事，但「高频调用」令人不安 |
| 战斗3 | 自动化失控 | 恐惧：生产环境、不可控 Agent |
| 幕末 | 第一天收工 | 松一口气，但对 Cloude 留疑 |

### 1.4 与全篇关系

- **不引入** 全篇 YES/NO 全屏弹窗、Skill 配置盘、Rules、第三章反杀（留后续幕）。
- **埋设** `cloude_invoke_count`、龙虾 YES 连击、学习日志——供第二幕及以后回收。
- **Deadline / 死线** 等高压 Boss **不在第一幕**；第一幕 Boss 为 **屎山代码**。

---

## 2. 核心体验（第一幕）

### 2.1 玩家在本幕学会什么

1. Meta：读剧情、选项、进战斗、回 Meta 续播  
2. 战斗：抽牌、出牌、看 Intent、结束回合  
3. 牌库：Meta 事件中 **获得 / 丢弃** 卡牌，战斗读持久化牌组  
4. Cloude：**出牌后可选附加**（帮助 / 捣乱），非战斗实体  
5. 龙虾：独立 AI 牌组、每回合必调 Cloude、**命令栈溢出** 反制窗口  

### 2.2 设计原则（第一幕）

1. **导师带练，不代打**：老陈只场外提示，不参战  
2. **Cloude 第一幕不黑化**：助手阶段，柔和蓝光  
3. **小故障 → 屎山**：战斗1 敌人叙事线清晰（ERROR / 404 / WARN → 屎山）  
4. **选择可追踪**：Cloude 调用次数写入 Flag，影响幕末台词  

---

## 3. 叙事结构

### 3.1 流程总览

```
A1-S01 入职报到
  → A1-S02 工位与导师
  → 【战斗1】Act1_Mentor（导师带练）
  → A1-S03 Cloude 部署
  → 【战斗2】Act1_Cloude（Cloude 协作）
  → A1-S04 龙虾 Agent
  → 【战斗3】Act1_Lobster（龙虾失控）
  → A1-S05 第一天收工（幕终）
```

### 3.2 Meta 跳转

| 入口节点 | 战斗 ID | 胜利回节点（建议） | 失败回节点（建议） |
|----------|---------|------------------|------------------|
| S02 选项「开始跟练」 | `battle:Act1_Mentor` | `A1-B1-POST` | `A1-B1-PRE` |
| S03 选项「进入第二场」 | `battle:Act1_Cloude` | `A1-B2-POST` | `A1-B2-PRE` |
| S04 选项「进入第三场」 | `battle:Act1_Lobster` | `A1-B3-POST` | `A1-B3-PRE` |

**剧本文件（待实现）**：`Resources/MetaScripts/act1_arrival.json`

---

## 4. 角色

| 角色ID | 名称 | 类型 | 参战 | 说明 |
|--------|------|------|------|------|
| CHAR_PLAYER | 玩家（新人） | 主角 | 是 | 策划兼程序，星点互动新人 |
| CHAR_MENTOR | 导师·老陈 | NPC | 否（场外） | 带练、提示；战斗用 **talk 序列帧** |
| CHAR_HR | HR | NPC | 否 | S01 一句台词 |
| CHAR_IT | IT | NPC | 否 | 部署 Cloude/龙虾；B3 战后挂起进程 |
| CHAR_CLOUDE | Cloude | AI 伙伴 | 间接 | 战斗2 附加技能；战斗3 被龙虾调用 |
| CHAR_LOBSTER | 龙虾 | 执行 Agent | 是（B3） | 独立牌组，只认 YES |
| CHAR_SYSTEM | 系统/UI | 系统 | 否 | 机制说明、幕终提示 |

> 完整角色表见飞书：**第一幕·初来乍到 角色表**

---

## 5. Meta 场景与剧本

### 5.1 场景一览

| 场景ID | 名称 | 背景资源 | 关键事件 |
|--------|------|----------|----------|
| A1-S01 | 入职报到 | `bg_hall` | 标题卡「初来乍到」、HR 办手续 |
| A1-S02 | 工位与导师 | `bg_desk` | 见老陈、进入战斗1 |
| A1-S03 | Cloude 部署 | `bg_meeting`（后续美术） | Cloude 首次亮相、进战斗2 |
| A1-S04 | 龙虾 Agent | `bg_server`（后续美术） | 龙虾 YES、进战斗3 |
| A1-S05 | 第一天收工 | `bg_desk_night`（后续美术） | 学习日志、幕终字幕 |

**第一天美术优先**：S01~S02 背景 + 战斗背景；S03~S05 可占位。

### 5.2 节点类型（第一幕所需）

| 类型 | 用途 |
|------|------|
| `line` | 对话/旁白；支持 `speaker` / `bg` / `portrait` |
| `choice` | 分支；可 `setFlags`、进战斗 |
| `set` | 写 Flag |
| `if` | 条件分支（如 Cloude 调用频率） |
| `grant_cards` | 获得卡牌 → `PlayerRunState` |
| `remove_cards` | 丢弃卡牌 |
| `battle:` | 跳转战斗场景 |
| `anchor` / `end` | 铆点 / 幕终 |

### 5.3 幕末关键台词

- Cloude：「今日协作统计：调用 12 次｜成功率 67%｜建议明日提高自动化比例。」  
- 玩家：「……67% 也叫建议提高？」  
- 老陈：「明天开始你进正式迭代——Cloude 会跟你的任务绑得更紧。」  
- 旁白：第一幕·初来乍到——完  

> 完整 58 条台词见飞书：**第一幕·初来乍到 台词本**

---

## 6. 三场战斗

### 6.1 总表

| 战斗 | ID | 主题 | Cloude | 龙虾 | 教学重点 |
|------|-----|------|--------|------|----------|
| 战斗1 | Act1_Mentor | 导师带练 | ❌ | ❌ | Intent、标准回合、屎山 Boss |
| 战斗2 | Act1_Cloude | AI 协作 | 出牌后可选附加 | ❌ | 帮助/捣乱、调用计数 |
| 战斗3 | Act1_Lobster | 自动化失控 | 每回合被动调用 | AI 自动出牌 | 命令栈溢出反制 |

### 6.2 战斗1「导师带练」

- **规则**：标准卡牌战；老陈左侧 **idle/talk 序列帧** 场外提示  
- **波次**：ERROR + 404 → WARN → **屎山代码**  
- **无** Cloude UI、无 YES 弹窗  

### 6.3 战斗2「Cloude 协作」

- **规则**：每次打出 1 张牌后，可选「调用 Cloude / 自行处理」  
- **Cloude**：无 HP、无站位；帮助（如 +4 伤、修 Lint）或捣乱（如 -2 能量、误推分支）  
- **敌人**：Build 失败（Summon 编译错误）、QA 驳回（与是否调用 Cloude 联动）  
- **战后**：`cloude_invoke_count` 更新；Cloude 台词「高频调用」  

### 6.4 战斗3「龙虾失控」

- **规则**：龙虾 **独立牌组**，玩家不可控；每回合自动出牌 + **必调 Cloude**  
- **反制**：连续 3 次 YES → **命令栈溢出**，下回合龙虾自伤 10  
- **场地**：自动化洪流（YES 弹窗 / 部署进度条，后续美术）  
- **战后**：IT 宣布龙虾进程挂起；Cloude 仍在后台  

---

## 7. 怪物与 Boss

### 7.1 战斗1（第一天美术重点）

| 波次 | ID | 名称 | 隐喻 | 机制方向 |
|------|-----|------|------|----------|
| 1 | MOB_ERROR | ERROR 报错团 | 控制台红字 | 低 HP，直伤 |
| 1 | MOB_404 | 404 迷路页 | 链接/接口找不到 | 攻击可 Miss，命中高伤 |
| 2 | MOB_WARN | WARN 警告蜂 | 被无视的 Warning | Mini-Boss；警告层 +1 |
| 关底 | MOB_SPAGHETTI | 屎山代码 | 祖传 tangled 代码 | 高 HP；债务层 / Summon ERROR |

**叙事线**：小故障不管 → 警告堆积 → 屎山成型。

### 7.2 战斗2

| ID | 名称 | 机制方向 |
|----|------|----------|
| MOB_BUILD | Build 失败 | 每回合 Summon 编译错误 |
| MOB_COMPILE | 编译错误 | 低血杂兵 |
| MOB_QA | QA 驳回 | 下回合高伤；本回合调 Cloude 则取消 |

### 7.3 战斗3

| ID | 名称 | 说明 |
|----|------|------|
| CHAR_LOBSTER | 龙虾 | 主控；YES 连击 |
| — | 自动化洪流 | 场地效果，非独立 HP |

> 扩展怪物见飞书：**程序员日常·怪物图鉴**（第一幕标签项）

---

## 8. 系统需求（程序三大系统）

第一幕程序收敛为 **3 个主系统**，共用 **PlayerRunState**（牌库、Skill 等级、Flag、剧本进度）。

```
PlayerRunState
    ├── PROG-1  Meta 事件演出（JSON 剧本、背景/立绘、获弃牌）
    ├── PROG-2  卡牌战斗（JSON 战斗/敌人/卡牌，多实体，读牌库）
    └── PROG-3  技能升级 + 开箱（Cloude Skill、卡牌升级、LootBox）
```

| 系统 | 第一幕必须 | 第一幕可选 / 后续 |
|------|------------|-------------------|
| PROG-1 | S01~S05 剧本、bg/portrait、战斗跳转、胜败回 Meta | 条件分支台词（invoke 阈值） |
| PROG-2 | 3 场战斗 JSON、战斗1 四怪、读 OwnedCards | 战斗2~3 全敌人 JSON 化 |
| PROG-3 | 战斗2 Cloude 附加挂钩 | 正式 Skill 树、开箱 UI、卡牌升级界面 |

详见 [程序需求表](./程序需求表.md)。

---

## 9. 卡牌与成长（第一幕）

### 9.1 玩家牌组

- 初始牌由 Meta **`grant_cards`** 节点发放（如 Strike / Defend 及主题牌）  
- 战斗从 **PlayerRunState.OwnedCards** 构建 DrawPile，禁止战斗内硬编码 `BuildStarterDeck`  
- Meta 事件可 **丢弃** 废牌（如「摸鱼」类诅咒牌）  

### 9.2 第一幕建议卡牌（策划池）

| 类别 | 示例 | 获得方式 |
|------|------|----------|
| 基础 | 赶工、提交代码、查文档 | 初始 grant |
| 防御 | 摸鱼、甩锅 | 默认 / 战斗奖励 |
| 临时 | 解决冲突、未读消息 | Boss / 敌人施加 |
| 诅咒 | 加班、需求变更 | 可选，后续 |

### 9.3 Cloude Skill（第一幕预览）

战斗2 的「附加技能」为第一幕 **Skill 系统原型**；正式升级树在 PROG-3 实现。示例：

| Skill | 出牌挂钩效果 |
|-------|--------------|
| 自动修 Lint | 额外伤害 / 清 Debuff |
| 误推分支 | 扣能量、改目标（捣乱） |

### 9.4 开箱（第一幕）

- 可在 S02 或战斗1 战后 Meta 节点 **`open_loot_box`** 发放新牌  
- 池子示例：`card_hotfix` / `card_review` / `card_slack`  

---

## 10. Flag 与数据

| Flag | 写入点 | 读取点 | 说明 |
|------|--------|--------|------|
| `act1_complete` | S05 幕终 | 存档 / 第二幕入口 | 第一幕通关 |
| `cloude_invoke_count` | 战斗2 每次「调用 Cloude」 | S05 Cloude 台词分支 | 协作频率 |
| `lobster_overflow_triggered` | 战斗3 栈溢出反制成功 | 后续幕（伏笔） | 见过反制 |
| `battle1_win` / `battle2_win` / `battle3_win` | 各场胜利 | 可选分支 | 战斗结果 |

**持久化**：`PlayerRunState` + `RunSaveService`；`MetaGameSession` 负责单场 Meta↔战斗交接。

---

## 11. 表现与美术

### 11.1 视觉基调

- **明亮现代办公室**；轻度像素 / 故障点缀  
- Cloude：**助手阶段**（柔和蓝，不黑化）  
- 龙虾：红甲壳、单眼 YES、终端风  

### 11.2 第一天交付（MVP 美术）

| 类别 | 内容 |
|------|------|
| 背景 | 大厅、工位、战斗办公室 ×3 |
| Meta 立绘 | 玩家、老陈 正面 ×1 |
| 战斗序列帧 | 玩家（idle/attack）、导师（idle/talk）、4 怪（idle/attack） |

**约定**：玩家 / 导师 / 怪物在战斗中 **仅序列帧**，不要战斗侧面静态立绘。Meta 对话仍用正面立绘。

详见 [美术需求表](./美术需求表.md)。

### 11.3 音频（第一幕，P2）

- BGM：轻快办公室 Lo-fi  
- SFX：打字机、通知 ding、YES 点击、Glitch  

---

## 12. 技术对接

### 12.1 架构（第一幕）

```
act1_arrival.json → NarrativeEngine → NarrativeSceneController
        ↓ battle:Act1_*
MetaGameSession + PlayerRunState → BattleFactory(JSON) → BattleManager
        ↓ 胜利/失败
ResumeFromNode + 写回 PlayerRunState / Flags
```

### 12.2 关键路径（现有代码）

| 模块 | 路径 |
|------|------|
| Meta 核心 | `Scripts/Meta/Core/` |
| Meta 表现 | `Scripts/Meta/Presentation/NarrativeSceneController.cs` |
| 跨场景 | `Scripts/Meta/MetaGameSession.cs` |
| 战斗 | `Scripts/Battle/`、`Scripts/Cards/` |
| 序列帧 | `Scripts/Presentation/WorldEntityView.cs`、`SpriteSequenceAnimator.cs` |

### 12.3 配置资源（待建）

```
Resources/
├── MetaScripts/act1_arrival.json
├── Battles/Act1_Mentor.json
├── Battles/Act1_Cloude.json
├── Battles/Act1_Lobster.json
├── Cards/*.json
├── Enemies/*.json
└── Skills/*.json
```

---

## 13. 里程碑与验收

### 13.1 垂直切片（第一幕可玩）

- [ ] `act1_arrival.json` 从 S01 跑到 S05  
- [ ] 三场战斗可进可回，Flag 正确  
- [ ] 战斗1：ERROR → 404 → WARN → 屎山  
- [ ] 战斗2：Cloude 附加 + 调用计数  
- [ ] 战斗3：龙虾 AI + 栈溢出反制  
- [ ] PlayerRunState：获牌 / 弃牌 / 战斗读牌库  
- [ ] 第一天美术：背景 + Meta 立绘 + 战斗序列帧占位可播  

### 13.2 建议实现顺序

```
PlayerRunState
→ PROG-1（Meta + 获弃牌 + 战斗1 跳转）
→ PROG-2（战斗1 JSON + 四怪）
→ 第一天美术接入
→ PROG-2 战斗2~3 + PROG-3 Cloude 附加
→ S03~S05 背景与 Cloude/龙虾美术（后续）
```

---

## 14. 飞书资源索引

| 文档 | 链接 |
|------|------|
| YES Cloude 设计文档（Wiki） | https://my.feishu.cn/wiki/DjTWwE4XYivYyjkfnq6cnAnYndY |
| 第一幕·台词本 | https://my.feishu.cn/base/UEGrbIGqeaM7lmsLYiecoGtqnwg |
| 第一幕·角色表 | https://my.feishu.cn/base/P6G6beBB6ahQB5sENP1culrDnwb |
| 第一幕·需求表（程序/美术） | https://my.feishu.cn/base/DWjUbkgDDaISRgs0MXwc0VTDnte |
| 程序员日常·怪物图鉴 | https://my.feishu.cn/base/Gsd8bqHEMaFB62s3ZBsc1EQ3nub |
| YES Cloude 台词本（全篇） | https://my.feishu.cn/base/Qv01bBanLag4vQsrfrmctKuJnNd |

---

## 附录 A：场景 × 战斗 × 敌人对照

| Meta 场景 | 战斗 | 主要敌人 |
|-----------|------|----------|
| A1-S02 | Act1_Mentor | ERROR, 404, WARN, 屎山 |
| A1-S03 | Act1_Cloude | Build, 编译错误, QA |
| A1-S04 | Act1_Lobster | 龙虾 + 自动化洪流 |

## 附录 B：与全篇 GDD 差异说明

| 全篇 GDD 第一章 | 本第一幕 GDD |
|-----------------|--------------|
| 死线等 4 Boss | 第一幕 Boss 为屎山；死线留后幕 |
| YES/NO 全屏双选 | 第一幕仅战斗2「调用/不调用」轻量版 |
| Skill 配置盘 | PROG-3 原型；完整 UI 可后补 |
| 并肩作战长期搭档 | 第一幕结束仍为「试用 + 警惕」 |

---

*文档维护：策划改叙事/机制时同步更新本 GDD 与飞书表；程序/美术以本目录下需求表派单。*
