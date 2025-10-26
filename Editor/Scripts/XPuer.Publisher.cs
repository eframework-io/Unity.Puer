// Copyright (c) 2025 EFramework Innovation. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using EFramework.Unity.Utility;
using EFramework.Unity.Editor;

namespace EFramework.Unity.Puer.Editor
{
    public partial class XPuer
    {
        /// <summary>
        /// XPuer.Publisher 实现了脚本包的发布工作流，用于将打包好的脚本发布至存储服务中。
        /// </summary>
        /// <remarks>
        /// <code>
        /// 功能特性
        /// - 首选项配置：提供首选项配置以自定义发布流程
        /// - 自动化流程：提供脚本包发布任务的自动化执行
        /// 
        /// 使用手册
        /// 
        /// 1. 首选项配置
        /// 
        /// | 配置项   | 配置键                      | 默认值             |
        /// | -------- | --------------------------- | ------------------ |
        /// | 存储服务地址 | `Puer/Publisher/Endpoint@Editor` | `${Environment.StorageEndpoint}`   |
        /// | 存储分区名称 | `Puer/Publisher/Bucket@Editor` | `${Environment.StorageBucket}` |
        /// | 存储服务凭证 | `Puer/Publisher/Access@Editor` | `${Environment.StorageAccess}` |
        /// | 存储服务密钥 | `Puer/Publisher/Secret@Editor` | `${Environment.StorageSecret}` |
        /// 
        /// 关联配置项：`Puer/LocalUri`、`Puer/RemoteUri`
        /// 
        /// 以上配置项均可在 `Tools/EFramework/Preferences/Puer/Publisher` 首选项编辑器中进行可视化配置。
        /// 
        /// 2. 自动化流程
        /// 
        /// 2.1 本地环境
        /// 本地开发环境可以使用 MinIO 作为对象存储服务：
        /// 
        /// 1. 安装服务
        /// 
        /// ```bash
        /// # 启动 MinIO 容器
        /// docker run -d --name minio -p 9000:9000 -p 9090:9090 --restart=always \
        ///   -e "MINIO_ACCESS_KEY=admin" -e "MINIO_SECRET_KEY=adminadmin" \
        ///   minio/minio server /data --console-address ":9090" --address ":9000"
        /// ```
        /// 
        /// 2. 服务配置：
        ///   - 控制台：http://localhost:9090
        ///   - API：http://localhost:9000
        ///   - 凭证：
        ///     - Access Key：admin
        ///     - Secret Key：adminadmin
        ///   - 存储：创建 `default` 存储桶并设置公开访问权限
        /// 
        /// 3. 首选项配置：
        ///   ```
        ///   Puer/Publisher/Endpoint@Editor = http://localhost:9000
        ///   Puer/Publisher/Bucket@Editor = default
        ///   Puer/Publisher/Access@Editor = admin
        ///   Puer/Publisher/Secret@Editor = adminadmin
        ///   ```
        /// 
        /// 2.2 发布流程
        /// 
        /// ```mermaid
        /// stateDiagram-v2
        ///     direction LR
        ///     读取发布配置 --> 获取远端清单
        ///     获取远端清单 --> 对比本地清单
        ///     对比本地清单 --> 发布差异文件
        /// ```
        /// 
        /// 发布时根据清单对比结果进行增量上传：
        /// - 新增文件：`文件名@MD5`
        /// - 修改文件：`文件名@MD5`
        /// - 清单文件：`Manifest.db` 和 `Manifest.db@MD5`（用于版本记录）
        /// </code>
        /// 
        /// 更多信息请参考模块文档。
        /// </remarks>
        [XEditor.Tasks.Worker(name: "Publish Scripts", group: "Puer", runasync: true, priority: 302)]
        public class Publisher : XEditor.MinIO, XEditor.Tasks.Panel.IOnGUI
        {
            /// <summary>
            /// Preferences 是 PuerTS 脚本发布的首选项设置。
            /// 提供了存储服务配置和发布选项的管理功能。
            /// </summary>
            internal class Preferences : Builder.Preferences
            {
                /// <summary>
                /// Endpoint 是存储服务地址的键名。
                /// </summary>
                public const string Endpoint = "Puer/Publisher/Endpoint@Editor";

                /// <summary>
                /// EndpointDefault 是存储服务地址的默认值。
                /// </summary>
                public const string EndpointDefault = "${Environment.StorageEndpoint}";

                /// <summary>
                /// Bucket 是存储分区名称的键名。
                /// </summary>
                public const string Bucket = "Puer/Publisher/Bucket@Editor";

                /// <summary>
                /// BucketDefault 是存储分区名称的默认值。
                /// </summary>
                public const string BucketDefault = "${Environment.StorageBucket}";

                /// <summary>
                /// Access 是存储服务凭证的键名。
                /// </summary>
                public const string Access = "Puer/Publisher/Access@Editor";

                /// <summary>
                /// AccessDefault 是存储服务凭证的默认值。
                /// </summary>
                public const string AccessDefault = "${Environment.StorageAccess}";

                /// <summary>
                /// Secret 是存储服务密钥的键名。
                /// </summary>
                public const string Secret = "Puer/Publisher/Secret@Editor";

                /// <summary>
                /// SecretDefault 是存储服务密钥的默认值。
                /// </summary>
                public const string SecretDefault = "${Environment.StorageSecret}";

                public override string Section => "Puer";

                public override int Priority => 302;

                public override void OnVisualize(string searchContext, XPrefs.IBase context)
                {
                    var taskPanel = searchContext == "Task Runner";

                    if (!taskPanel)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        foldout = EditorGUILayout.Foldout(foldout, new GUIContent("Publisher", "Puer Publish Options."));
                    }
                    else foldout = true;
                    if (foldout)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label(new GUIContent("Endpoint", "Storage Service Endpoint."), GUILayout.Width(60));
                        context.Set(Endpoint, EditorGUILayout.TextField("", context.GetString(Endpoint, EndpointDefault)));
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label(new GUIContent("Bucket", "Storage Bucket Name."), GUILayout.Width(60));
                        context.Set(Bucket, EditorGUILayout.TextField("", context.GetString(Bucket, BucketDefault)));
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label(new GUIContent("Access", "Storage Access Key."), GUILayout.Width(60));
                        context.Set(Access, EditorGUILayout.TextField("", context.GetString(Access, AccessDefault)));
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label(new GUIContent("Secret", "Storage Secret Key."), GUILayout.Width(60));
                        context.Set(Secret, EditorGUILayout.TextField("", context.GetString(Secret, SecretDefault)));
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                    }
                    if (!taskPanel) EditorGUILayout.EndVertical();
                }
            }

            internal Preferences preferencesPanel;

            void XEditor.Tasks.Panel.IOnGUI.OnGUI()
            {
                preferencesPanel = preferencesPanel != null ? preferencesPanel : ScriptableObject.CreateInstance<Preferences>();
                preferencesPanel.OnVisualize("Task Runner", XPrefs.Asset);
            }

            public override void Preprocess(XEditor.Tasks.Report report)
            {
                Endpoint = XPrefs.GetString(Preferences.Endpoint, Preferences.EndpointDefault).Eval(XEnv.Instance);
                Bucket = XPrefs.GetString(Preferences.Bucket, Preferences.BucketDefault).Eval(XEnv.Instance);
                Access = XPrefs.GetString(Preferences.Access, Preferences.AccessDefault).Eval(XEnv.Instance);
                Secret = XPrefs.GetString(Preferences.Secret, Preferences.SecretDefault).Eval(XEnv.Instance);
                base.Preprocess(report);
                Local = XFile.PathJoin(Temp, XPrefs.GetString(Puer.XPuer.Preferences.LocalUri, Puer.XPuer.Preferences.LocalUriDefault));
                Remote = XPrefs.GetString(Puer.XPuer.Preferences.RemoteUri, Puer.XPuer.Preferences.RemoteUriDefault).Eval(XEnv.Instance);
            }

            public override void Process(XEditor.Tasks.Report report)
            {
                var root = XFile.PathJoin(XPrefs.GetString(Builder.Preferences.Output, Builder.Preferences.OutputDefault), XEnv.Channel, XEnv.Platform.ToString());

                var remoteMani = new XMani.Manifest();
                var tempFile = Path.GetTempFileName();
                var task = XEditor.Command.Run(bin: Bin, args: new string[] { "get", $"\"{Alias}/{Bucket}/{Remote}/{XMani.Default}\"", tempFile });
                task.Wait();
                if (task.Result.Code != 0)
                {
                    XLog.Warn("XPuer.Publisher.Process: get remote mainifest failed: {0}", task.Result.Error);
                }
                else
                {
                    remoteMani.Read(tempFile);
                    if (!string.IsNullOrEmpty(remoteMani.Error)) XLog.Warn("XPuer.Publisher.Process: parse remote mainifest failed: {0}", remoteMani.Error);
                }

                var localMani = new XMani.Manifest();
                localMani.Read(XFile.PathJoin(root, XMani.Default));
                if (!string.IsNullOrEmpty(localMani.Error)) XLog.Warn("XPuer.Publisher.Process: parse local mainifest failed: {0}", remoteMani.Error);
                else
                {
                    var diff = remoteMani.Compare(localMani);
                    var files = new List<string[]>();
                    for (var i = 0; i < diff.Added.Count; i++) { files.Add(new string[] { XFile.PathJoin(root, diff.Added[i].Name), diff.Added[i].MD5 }); }
                    for (var i = 0; i < diff.Modified.Count; i++) { files.Add(new string[] { XFile.PathJoin(root, diff.Modified[i].Name), diff.Modified[i].MD5 }); }
                    if (diff.Added.Count > 0 || diff.Modified.Count > 0)
                    {
                        var maniFile = XFile.PathJoin(root, XMani.Default);
                        files.Add(new string[] { maniFile, "" });
                        files.Add(new string[] { maniFile, XFile.FileMD5(maniFile) });
                    }
                    if (files.Count == 0)
                    {
                        XLog.Debug("XPuer.Publisher.Process: diff files is zero, no need to publish.");
                        return;
                    }
                    else
                    {
                        foreach (var kvp in files)
                        {
                            var file = kvp[0];
                            var md5 = kvp[1];
                            var src = file;
                            var dst = XFile.PathJoin(Local, Path.GetRelativePath(root, file));
                            if (!string.IsNullOrEmpty(md5)) dst += "@" + md5; // file@md5
                            var dir = Path.GetDirectoryName(dst);
                            if (!XFile.HasDirectory(dir)) XFile.CreateDirectory(dir);
                            XFile.CopyFile(src, dst);
                        }
                    }

                    base.Process(report);
                }
            }
        }
    }
}