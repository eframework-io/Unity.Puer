# XPuer.Builder

[![NPM](https://img.shields.io/npm/v/io.eframework.unity.puer?label=NPM&logo=npm)](https://www.npmjs.com/package/io.eframework.unity.puer)
[![UPM](https://img.shields.io/npm/v/io.eframework.unity.puer?label=UPM&logo=unity&registry_uri=https://package.openupm.com)](https://openupm.com/packages/io.eframework.unity.puer)
[![DeepWiki](https://img.shields.io/badge/DeepWiki-Explore-blue)](https://deepwiki.com/eframework-io/Unity.Puer)
[![Discord](https://img.shields.io/discord/1422114598835851286?label=Discord&logo=discord)](https://discord.gg/XMPx2wXSz3)

XPuer.Builder 提供了脚本的构建工作流，支持 TypeScript 脚本的编译及打包功能。

## 功能特性

- 首选项配置：提供首选项配置以自定义构建流程
- 自动化流程：提供脚本包构建任务的自动化执行

## 使用手册

### 1. 首选项配置

| 配置项 | 配置键 | 默认值 | 功能说明 |
|--------|--------|--------|----------|
| 输入路径 | `Puer/Builder/Input@Editor` | `Assets/Temp/TypeScripts` | 设置 TypeScript 编译文件的临时目录 |
| 输出路径 | `Puer/Builder/Output@Editor` | `Builds/Patch/Scripts/TS` | 设置构建输出的目标目录，按照渠道和平台自动组织目录结构 |
| 构建任务 | `Puer/Builder/Tasks@Editor` | `["build-modules", "build-sources"]` | 配置需要执行的 NPM 构建任务，按照配置顺序依次执行任务 |

关联配置项：`Puer/AssetUri`、`Puer/LocalUri`

以上配置项均可在 `Tools/EFramework/Preferences/Puer/Builder` 首选项编辑器中进行可视化配置。

### 2. 自动化流程

#### 2.1 构建流程

```mermaid
stateDiagram-v2
    direction LR
    编译脚本 --> 加密脚本
    加密脚本 --> 打包脚本
    打包脚本 --> 生成清单
```

#### 2.2 构建产物

在 `Puer/Builder/Output@Editor` 目录下会生成以下文件：
- `*.jsc`：脚本包文件，格式为 `path_to_scripts.jsc`
- `Manifest.db`：脚本包清单，格式为 `名称|MD5|大小`

构建产物会在内置构建事件 `XEditor.Event.Internal.OnPreprocessBuild` 触发时内置于安装包的资源目录下：

- 移动平台 (Android/iOS/..)
  ```
  <AssetPath>/
  └── <AssetUri>  # 脚本包压缩为 ZIP
  ```

- 桌面平台 (Windows/macOS/..)
  ```
  <输出目录>_Data/
  └── Local/
      └── <LocalUri>  # 脚本包直接部署
  ```

## 常见问题

更多问题，请查阅[问题反馈](../CONTRIBUTING.md#问题反馈)。

## 项目信息

- [更新记录](../CHANGELOG.md)
- [贡献指南](../CONTRIBUTING.md)
- [许可协议](../LICENSE.md)