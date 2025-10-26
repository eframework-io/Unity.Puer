# XPuer.Publisher

[![NPM](https://img.shields.io/npm/v/io.eframework.unity.puer?label=NPM&logo=npm)](https://www.npmjs.com/package/io.eframework.unity.puer)
[![UPM](https://img.shields.io/npm/v/io.eframework.unity.puer?label=UPM&logo=unity&registry_uri=https://package.openupm.com)](https://openupm.com/packages/io.eframework.unity.puer)
[![DeepWiki](https://img.shields.io/badge/DeepWiki-Explore-blue)](https://deepwiki.com/eframework-io/Unity.Puer)
[![Discord](https://img.shields.io/discord/1422114598835851286?label=Discord&logo=discord)](https://discord.gg/XMPx2wXSz3)

XPuer.Publisher 实现了脚本包的发布工作流，用于将打包好的脚本发布至存储服务中。

## 功能特性

- 首选项配置：提供首选项配置以自定义发布流程
- 自动化流程：提供脚本包发布任务的自动化执行

## 使用手册

### 1. 首选项配置

| 配置项 | 配置键 | 默认值 |
|--------|--------|--------|
| 存储服务地址 | `Puer/Publisher/Endpoint@Editor` | `${Environment.StorageEndpoint}` |
| 存储分区名称 | `Puer/Publisher/Bucket@Editor` | `${Environment.StorageBucket}` |
| 存储服务凭证 | `Puer/Publisher/Access@Editor` | `${Environment.StorageAccess}` |
| 存储服务密钥 | `Puer/Publisher/Secret@Editor` | `${Environment.StorageSecret}` |

关联配置项：`Puer/LocalUri`、`Puer/RemoteUri`

以上配置项均可在 `Tools/EFramework/Preferences/Puer/Publisher` 首选项编辑器中进行可视化配置。

### 2. 自动化流程

#### 2.1 本地环境

本地开发环境可以使用 MinIO 作为对象存储服务：

1. 安装服务：

```bash
# 启动 MinIO 容器
docker run -d --name minio -p 9000:9000 -p 9090:9090 --restart=always \
  -e "MINIO_ACCESS_KEY=admin" -e "MINIO_SECRET_KEY=adminadmin" \
  minio/minio server /data --console-address ":9090" --address ":9000"
```

2. 服务配置：
   - 控制台：http://localhost:9090
   - API：http://localhost:9000
   - 凭证：
     - Access Key：admin
     - Secret Key：adminadmin
   - 存储：创建 `default` 存储桶并设置公开访问权限

3. 首选项配置：
   ```
   Puer/Publisher/Endpoint@Editor = http://localhost:9000
   Puer/Publisher/Bucket@Editor = default
   Puer/Publisher/Access@Editor = admin
   Puer/Publisher/Secret@Editor = adminadmin
   ```

#### 2.2 发布流程

```mermaid
stateDiagram-v2
    direction LR
    读取发布配置 --> 获取远端清单
    获取远端清单 --> 对比本地清单
    对比本地清单 --> 发布差异文件
```

发布时根据清单对比结果进行增量上传：
- 新增文件：`文件名@MD5`
- 修改文件：`文件名@MD5`
- 清单文件：`Manifest.db` 和 `Manifest.db@MD5`（用于版本记录）

## 常见问题

更多问题，请查阅[问题反馈](../CONTRIBUTING.md#问题反馈)。

## 项目信息

- [更新记录](../CHANGELOG.md)
- [贡献指南](../CONTRIBUTING.md)
- [许可协议](../LICENSE.md)