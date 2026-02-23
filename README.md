# 🀄 国标麻将算番器

> Chinese National Standard Mahjong Fan Calculator

在线使用：**[escapecat.github.io/MahjongScorer](https://escapecat.github.io/MahjongScorer/)**

![Blazor WASM](https://img.shields.io/badge/Blazor-WebAssembly-512bd4?logo=blazor)
![.NET 10](https://img.shields.io/badge/.NET-10.0-512bd4?logo=dotnet)
![License](https://img.shields.io/badge/license-MIT-green)

## 功能

- 🎯 **自动算番** — 输入手牌、副露、和牌，自动计算所有番种
- 👂 **听牌提示** — 13 张时自动显示可听的牌及对应番数
- 🏷️ **番种分解** — 展示每个番种的来源和原因
- 🀄 **胡牌标记** — 在牌型分解中高亮标记胡的那张牌
- ⚙️ **完整设置** — 支持自摸/点炮、门风/圈风、花牌、绝张/海底/杠花/抢杠
- 📱 **PWA 离线** — 安装到手机桌面，离线可用
- 🚀 **纯客户端** — 无服务器，所有计算在浏览器中完成

## 支持的番种

全部 **81 个番种**，涵盖 88 番到 1 番：

| 番值 | 示例 |
|---|---|
| 88 番 | 大四喜、大三元、绿一色、九莲宝灯、四杠、连七对、十三幺 |
| 64 番 | 清幺九、小四喜、小三元、字一色、四暗刻、一色双龙会 |
| 48 番 | 一色四同顺、一色四节高 |
| 32 番 | 一色四步高、三杠、混幺九 |
| 24 番 | 七对、七星不靠、全双刻、清一色、一色三同顺、全大/全中/全小 |
| 16 番 | 清龙、三色双龙会、一色三步高、全带五、三同刻、三暗刻 |
| 12 番 | 全不靠、组合龙、大于五、小于五、三风刻 |
| 8 番 | 花龙、推不倒、三色三同顺、无番和、妙手回春、海底捞月、杠上开花、抢杠和 |
| 6 番 | 碰碰和、混一色、三色三步高、五门齐、全求人、双箭刻 |
| 4 番 | 全带幺、不求人、双明杠、和绝张 |
| 2 番 | 箭刻、圈风刻、门风刻、门前清、平和、四归一、双同刻、双暗刻、暗杠、断幺 |
| 1 番 | 一般高、喜相逢、连六、老少副、幺九刻、明杠、缺一门、无字、边张、坎张、单钓将、自摸、花牌 |

## 技术栈

- **Blazor WebAssembly** (.NET 10) — 纯客户端 SPA
- **PWA** — Service Worker 离线缓存
- **GitHub Pages** — 静态托管，自动 CI/CD

## 本地运行

```bash
# 克隆
git clone https://github.com/escapecat/MahjongScorer.git
cd MahjongScorer

# 运行
dotnet run

# 或发布
dotnet publish -c Release -o publish
```

## 项目结构

```
MahjongScorer/
├── Pages/
│   └── Home.razor              # 主界面（计算器 UI）
├── Utilities/
│   ├── FanEvaluator.cs         # 番种计算核心引擎
│   ├── FanEvaluationResult.cs  # 计算结果模型
│   ├── HandDecomposer.cs       # 手牌分解（面子+将）
│   ├── HandPatternDetector.cs  # 特殊牌型检测
│   ├── MahjongHandEvaluator.cs # 和牌判定
│   ├── WaitAnalyzer.cs         # 听牌分析（边张/坎张/单钓）
│   ├── TileConstants.cs        # 牌编码常量
│   └── TileIconHelper.cs       # 牌图标映射
├── Services/
│   ├── FanService.cs           # 番种数据（名称/描述/示例）
│   └── CalculatorStateService.cs # UI 状态持久化
├── wwwroot/
│   ├── tiles/                  # 麻将牌图片
│   └── css/app.css             # 样式
└── MahjongScorer.Tests/        # 单元测试
```

## 许可证

MIT
