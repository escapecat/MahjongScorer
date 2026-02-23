# 架构笔记：为什么会有副露相关的 Bug？

## 当前架构

```
用户输入 (手牌 + 吃碰杠)
        ↓
BuildEvaluationCounts() → int[34]  (所有牌混合计数)
BuildMeldsList()        → List<IReadOnlyList<string>>  (副露面子列表)
        ↓
FanEvaluator.Evaluate(counts, melds, ...)
        ↓
各检测器直接操作 counts 数组
```

### 问题根因

`int[34] counts` 是一个**扁平的牌数统计**，手牌和副露牌混在一起。
大部分检测器（约 40 个番种）直接在这个混合数组上做模式匹配或拆解。

这导致：

1. **拆解型检测器**（如 `IsAllTriplets`、`CanFormSets`）会自由重新组合所有牌，
   无法区分哪些牌已被副露锁定。

2. **补丁越来越多**：为修复 (1)，需要引入各种中间数组：
   - `sequenceCounts`：从 counts 减去碰/杠牌（防止刻子被拆成顺子）
   - `fullSuitCounts`：用原始 counts（刻子节高检测需要看到碰的刻子）
   - `IsWinningHandWithMelds`：单独的手牌减副露逻辑

3. **不同检测器用不同的 counts 版本**，容易遗漏。

## 更好的架构（面子分解法）

```
用户输入 (手牌 + 吃碰杠)
        ↓
HandDecomposer.Decompose(handCounts, melds)
        ↓
List<HandDecomposition>  (所有合法的面子拆解方案)
  每个方案 = {
    Pair:     (int tileIndex)
    Melds:    List<MeldInfo>  // 包含副露(locked) + 手牌拆出的面子
    每个 MeldInfo = { Type: 顺子/刻子/杠, Tiles: int[], IsOpen: bool }
  }
        ↓
FanEvaluator.Evaluate(decomposition, context)
        ↓
所有检测器基于面子列表工作，无需拆解 counts
```

### 优势

- **副露面子天然固定**，不会被重新拆解
- **手牌面子也已确定**，每个检测器只需遍历面子列表
- **消除 sequenceCounts / fullSuitCounts / IsWinningHandWithMelds 等补丁**
- **多种拆解方案取最高番**（国标规则要求）

### 为什么暂不重构

1. 当前 201 个测试全部通过，经审计确认绝大多数检测器不受副露混合影响
2. 面子分解法需要重写几乎所有检测器，风险很大
3. 已发现的具体问题已用定向补丁修复

### 已修复的 Bug

| Bug | 修复方式 |
|---|---|
| 胡牌判定自由拆解副露牌 | `IsWinningHandWithMelds`: 先减去副露再拆解 |
| 一色X节高/三色三节高检测不到碰的刻子 | `suitTriplets` 改用原始 `counts` |
| 杠牌 counts=3 但 meld=4 导致负数 | `Math.Max(0, handCounts[index] - 1)` |

### 不受影响的检测器（审计结论）

- **纯属性检测**（检查哪些 index 有值）：绿一色、清一色、全大/中/小等
- **刻子计数**（检查 `count >= 3`）：风刻、箭刻、同刻等
- **碰碰和**（`IsAllTriplets`）：吃的牌是序列 (+1 到 3 个连续 index)，无法形成刻子
- **平和**（`IsAllSequencesHand`）：使用 `sequenceCounts`，碰已被排除；吃本身是序列不影响
