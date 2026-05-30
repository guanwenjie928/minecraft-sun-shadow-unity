# Minecraft Sun & Shadow - Unity 3D 科学实验

**知雀** 🐦 | Minecraft 风格的太阳与影子科学可视化实验

---

## 项目简介

通过 Minecraft 风格的 3D 场景，直观展示一天中不同时刻（0-24时）太阳位置、影子长度和方向的变化规律。支持冬至/春分/夏至三季切换，帮助理解地球自转、太阳高度角和影子形成原理。

## 快速开始

### 环境要求
- **Unity 2022.3 LTS** 或更高版本
- 操作系统：Windows / macOS / Linux

### 步骤

1. **克隆/下载项目**
   ```bash
   git clone <repo-url>
   ```

2. **用 Unity Hub 打开项目**
   - 打开 Unity Hub → Add → 选择 `minecraft-sun-shadow-unity/` 文件夹
   - Unity 会自动导入资源和脚本

3. **打开场景**
   - 在 Project 窗口中找到 `Assets/Scenes/MainScene.unity`
   - 双击打开

4. **一键搭建场景**（推荐）
   - 顶部菜单 → `Tools` → `Minecraft Sun Shadow` → `Setup Scene`
   - 自动创建所有 GameObjects、光照、UI

5. **运行**
   - 点击 Play 按钮 ▶
   - 如果场景为空，`SceneBootstrap` 会自动构建所有组件

## 操作说明

| 操作 | 说明 |
|------|------|
| **右键拖拽** | 旋转摄像机视角 |
| **滚轮** | 缩放 |
| **中键拖拽** | 平移 |
| **R 键** | 重置视角 |

### UI 控制
- **24 小时按钮栏** — 点击切换不同时刻
- **时间滑动条** — 连续调整时间
- **季节按钮** — 切换冬至/春分/夏至
- **自动播放** — 开启后太阳自动运行

## 项目结构

```
Assets/
├── Scenes/
│   └── MainScene.unity          # 主场景
├── Scripts/
│   ├── SunShadowController.cs    # 核心：太阳位置、影子计算
│   ├── CameraOrbitController.cs  # 鼠标轨道摄像机
│   ├── GroundBuilder.cs          # Minecraft 方块地面生成
│   ├── TimeUI.cs                 # UI 管理与数据更新
│   ├── UIBuilder.cs              # 运行时 UI 自动构建
│   ├── SceneBootstrap.cs         # 场景自启动引导
│   └── EditorSetup.cs            # 编辑器一键搭建
├── Textures/
│   ├── grass-top.jpg             # 草方块顶部 (1024×1024)
│   ├── grass-side.jpg            # 草方块侧面
│   ├── dirt.jpg                  # 泥土
│   ├── pole.jpg                  # 标杆条纹纹理
│   └── ...
└── Materials/                    # (Unity 自动生成)
```

## 科学原理

| 概念 | 说明 |
|------|------|
| **太阳高度角** | 太阳光线与地平面的夹角，决定影子长度 |
| **太阳方位角** | 太阳在水平面上的方向角（南=180°） |
| **影子长度公式** | L = H / tan(α)，其中 H=杆高，α=太阳高度角 |
| **太阳赤纬** | 冬至-23.5° / 春分0° / 夏至+23.5° |
| **时角** | 每小时间隔 15°，正午=0° |

## WebGL 构建

导出为 Web 版部署到 GitHub Pages：

1. File → Build Settings → WebGL → Switch Platform
2. Player Settings → Resolution: 1280×720
3. Build → 输出到 `Build/` 目录
4. 部署 `Build/` 到 Web 服务器

## License

教育用途，自由使用。
