// Copyright (c) 2025 EFramework Innovation. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using UnityEngine;
using EFramework.Unity.Utility;

namespace EFramework.Unity.Puer
{
    public partial class XPuer
    {
        /// <summary>
        /// XPuer.Preferences 提供了运行时的首选项管理，用于控制运行模式、调试选项和资源路径等配置项。
        /// </summary>
        /// <remarks>
        /// <code>
        /// 功能特性
        /// - 运行模式配置：提供发布模式和调试模式的切换
        /// - 调试选项管理：支持调试等待和端口设置
        /// - 资源路径配置：支持配置内置、本地和远端资源路径
        /// - 可视化配置界面：在 Unity 编辑器中提供直观的设置面板
        /// 
        /// 使用手册
        /// 
        /// 1. 运行模式
        /// 
        /// | 配置项 | 配置键 | 默认值 | 功能说明 |
        /// |--------|--------|--------|----------|
        /// | 发布模式 | `Puer/ReleaseMode` | `false` | 控制是否启用发布模式，启用后将禁用所有调试相关功能，用于生产环境部署 |
        /// | 调试模式 | `Puer/DebugMode` | `false` | 控制是否启用调试模式，仅在非发布模式下可用，启用后可连接调试器进行调试 |
        /// 
        /// 2. 调试选项
        /// 
        /// | 配置项 | 配置键 | 默认值 | 功能说明 |
        /// |--------|--------|--------|----------|
        /// | 调试等待 | `Puer/DebugWait` | `true` | 控制是否等待调试器连接，仅在调试模式下可用，启用后程序将等待调试器连接后再继续执行 |
        /// | 调试端口 | `Puer/DebugPort` | `9222` | 设置调试器连接的端口号，仅在调试模式下可用，确保端口未被其他程序占用 |
        /// 
        /// 3. 资源路径
        /// 
        /// | 配置项 | 配置键 | 默认值 | 功能说明 |
        /// |--------|--------|--------|----------|
        /// | 内置资源 | `Puer/AssetUri` | `Patch@Scripts@TS.zip` | 设置脚本包的内置路径，用于打包时将资源内置于安装包内 |
        /// | 本地资源 | `Puer/LocalUri` | `Scripts/TS` | 设置脚本包的本地路径，用于运行时的加载 |
        /// | 远端资源 | `Puer/RemoteUri` | `Builds/Patch/${Environment.Author}/${Environment.Version}/${Environment.Platform}/Scripts/TS` | 设置脚本包的远端路径，用于资源的下载 |
        /// </code>
        /// 更多信息请参考模块文档。
        /// </remarks>
        public class Preferences : ScriptableObject, XPrefs.IEditor
        {
            /// <summary>
            /// ReleaseMode 是发布模式的配置键。
            /// 用于控制是否启用发布模式。
            /// </summary>
            public const string ReleaseMode = "XPuer/ReleaseMode";

            /// <summary>
            /// ReleaseModeDefault 是发布模式的默认值。
            /// 默认为 false，表示不启用发布模式。
            /// </summary>
            public const bool ReleaseModeDefault = false;

            /// <summary>
            /// DebugMode 是调试模式的配置键。
            /// 用于控制是否启用调试模式。
            /// </summary>
            public const string DebugMode = "XPuer/DebugMode";

            /// <summary>
            /// DebugModeDefault 是调试模式的默认值。
            /// 默认为 false，表示不启用调试模式。
            /// </summary>
            public const bool DebugModeDefault = false;

            /// <summary>
            /// DebugWait 是调试等待的配置键。
            /// 用于控制是否等待调试器连接。
            /// </summary>
            public const string DebugWait = "XPuer/DebugWait";

            /// <summary>
            /// DebugWaitDefault 是调试等待的默认值。
            /// 默认为 true，表示等待调试器连接。
            /// </summary>
            public const bool DebugWaitDefault = true;

            /// <summary>
            /// DebugPort 是调试端口的配置键。
            /// 用于设置调试器连接的端口号。
            /// </summary>
            public const string DebugPort = "XPuer/DebugPort";

            /// <summary>
            /// DebugPortDefault 是调试端口的默认值。
            /// 默认为 9222。
            /// </summary>
            public const int DebugPortDefault = 9222;

            /// <summary>
            /// AssetUri 是资源 URI 的配置键。
            /// 用于设置脚本资源的打包路径。
            /// </summary>
            public const string AssetUri = "XPuer/AssetUri";

            /// <summary>
            /// AssetUriDefault 是资源 URI 的默认值。
            /// 默认为 "Patch@Scripts@TS.zip"。
            /// </summary>
            public const string AssetUriDefault = "Patch@Scripts@TS.zip";

            /// <summary>
            /// LocalUri 是本地 URI 的配置键。
            /// 用于设置脚本资源的本地路径。
            /// </summary>
            public const string LocalUri = "XPuer/LocalUri";

            /// <summary>
            /// LocalUriDefault 是本地 URI 的默认值。
            /// 默认为 "Scripts/TS"。
            /// </summary>
            public const string LocalUriDefault = "Scripts/TS";

            /// <summary>
            /// RemoteUri 是远端 URI 的配置键。
            /// 用于设置脚本资源的远端路径。
            /// </summary>
            public const string RemoteUri = "XPuer/RemoteUri";

            /// <summary>
            /// RemoteUriDefault 是远端 URI 的默认值。
            /// </summary>
            public const string RemoteUriDefault = "Builds/Patch/${Environment.Author}/${Environment.Version}/${Environment.Platform}/Scripts/TS";

            public virtual string Section => "XPuer";

            public virtual string Tooltip => "Preferences of XPuer.";

            public virtual int Priority => 300;

            [SerializeField] protected bool foldout;

            public virtual bool Foldable => true;

            public virtual void OnActivate(string searchContext, UnityEngine.UIElements.VisualElement rootElement, XPrefs.IBase context) { }

            public virtual void OnVisualize(string searchContext, XPrefs.IBase context)
            {
#if UNITY_EDITOR
                var releaseMode = context.GetBool(ReleaseMode, ReleaseModeDefault);
                var debugMode = context.GetBool(DebugMode, DebugModeDefault);

                UnityEditor.EditorGUILayout.BeginVertical(UnityEditor.EditorStyles.helpBox);

                UnityEditor.EditorGUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent("Release", "Switch to Release Mode."), GUILayout.Width(60));
                releaseMode = UnityEditor.EditorGUILayout.Toggle(releaseMode);
                context.Set(ReleaseMode, releaseMode);

                var ocolor = GUI.color;
                if (releaseMode) GUI.color = Color.gray;
                GUILayout.Label(new GUIContent("Debug", "Switch to Debug Mode."), GUILayout.Width(60));
                debugMode = UnityEditor.EditorGUILayout.Toggle(debugMode);
                if (!releaseMode) context.Set(DebugMode, debugMode);
                GUI.color = ocolor;
                UnityEditor.EditorGUILayout.EndHorizontal();

                if (!releaseMode && debugMode)
                {
                    UnityEditor.EditorGUILayout.BeginVertical(UnityEditor.EditorStyles.helpBox);
                    UnityEditor.EditorGUILayout.BeginHorizontal();

                    GUILayout.Label(new GUIContent("Wait", "Wait for the debugger to attach before running."), GUILayout.Width(60));
                    var debugWait = UnityEditor.EditorGUILayout.Toggle(context.GetBool(DebugWait, DebugWaitDefault));
                    context.Set(DebugWait, debugWait);

                    GUILayout.Label(new GUIContent("Port", "Debugger Listen Port."), GUILayout.Width(60));
                    var debugPort = UnityEditor.EditorGUILayout.IntField(context.GetInt(DebugPort, DebugPortDefault));
                    context.Set(DebugPort, debugPort);

                    UnityEditor.EditorGUILayout.EndHorizontal();
                    UnityEditor.EditorGUILayout.EndVertical();
                }

                UnityEditor.EditorGUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent("Asset", "Asset Uri of Scripts."), GUILayout.Width(60));
                var assetFile = UnityEditor.EditorGUILayout.TextField("", context.GetString(AssetUri, AssetUriDefault));
                context.Set(AssetUri, assetFile);
                UnityEditor.EditorGUILayout.EndHorizontal();

                UnityEditor.EditorGUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent("Local", "Local Uri of Scripts."), GUILayout.Width(60));
                var localPath = UnityEditor.EditorGUILayout.TextField("", context.GetString(LocalUri, LocalUriDefault));
                context.Set(LocalUri, localPath);
                UnityEditor.EditorGUILayout.EndHorizontal();

                UnityEditor.EditorGUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent("Remote", "Remote Uri of Scripts."), GUILayout.Width(60));
                var remoteUri = UnityEditor.EditorGUILayout.TextField("", context.GetString(RemoteUri, RemoteUriDefault));
                context.Set(RemoteUri, remoteUri);
                UnityEditor.EditorGUILayout.EndHorizontal();

                UnityEditor.EditorGUILayout.EndVertical();
#endif
            }

            public virtual void OnDeactivate(XPrefs.IBase context) { }

            public virtual bool OnSave(XPrefs.IBase context) { return true; }

            public virtual bool OnApply(XPrefs.IBase context) { return true; }

            public virtual bool OnBuild(XPrefs.IBase context) { return true; }
        }
    }
}