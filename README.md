# EFramework Puer for Unity

[![NPM](https://img.shields.io/npm/v/io.eframework.unity.puer?label=NPM&logo=npm)](https://www.npmjs.com/package/io.eframework.unity.puer)
[![UPM](https://img.shields.io/npm/v/io.eframework.unity.puer?label=UPM&logo=unity&registry_uri=https://package.openupm.com)](https://openupm.com/packages/io.eframework.unity.puer)
[![DeepWiki](https://img.shields.io/badge/DeepWiki-Explore-blue)](https://deepwiki.com/eframework-io/Unity.Puer)
[![Discord](https://img.shields.io/discord/1422114598835851286?label=Discord&logo=discord)](https://discord.gg/XMPx2wXSz3)

基于 PuerTS 的扩展模块，提供了易用的 PuerBehaviour 组件和完整的 TypeScript 开发工作流，优化了脚本调试和自动化打包。

## 功能特性

- [XPuer.Core](Documentation~/XPuer.Core.md) 提供了 JavaScript 虚拟机的运行时环境，支持事件系统管理、生命周期控制和脚本调试等功能。
- [XPuer.Behaviour](Documentation~/XPuer.Behaviour.md) 提供了 MonoBehaviour 的 TypeScript 扩展实现，用于管理组件的生命周期、序列化和事件系统。
- [XPuer.Constants](Documentation~/XPuer.Constants.md) 提供了一些常量定义和运行时环境控制，包括运行配置、资源路径和标签生成等功能。
- [XPuer.Preferences](Documentation~/XPuer.Preferences.md) 提供了运行时的首选项管理，用于控制运行模式、调试选项和资源路径等配置项。
- [XPuer.Inspector](Documentation~/XPuer.Inspector.md) 实现了 PuerBehaviour 组件的检视器界面，用于组件的可视化编辑和类型检查。
- [XPuer.Builder](Documentation~/XPuer.Builder.md) 提供了脚本的构建工作流，支持 TypeScript 脚本的编译及打包功能。
- [XPuer.Publisher](Documentation~/XPuer.Publisher.md) 实现了脚本包的发布工作流，用于将打包好的脚本发布至存储服务中。
- [XPuer.Generator](Documentation~/XPuer.Generator.md) 提供了代码生成工具，支持 PuerTS 绑定代码的生成、模块导出和自动安装等功能。
- [XPuer.Workspace](Documentation~/XPuer.Workspace.md) 用于源文件的解析和多编辑器集成，是 TypeScript 的项目管理模块。

## 常见问题

更多问题，请查阅[问题反馈](CONTRIBUTING.md#问题反馈)。

## 项目信息

- [更新记录](CHANGELOG.md)
- [贡献指南](CONTRIBUTING.md)
- [许可协议](LICENSE.md)