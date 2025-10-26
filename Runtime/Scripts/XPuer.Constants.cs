// Copyright (c) 2025 EFramework Innovation. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System.IO;
using System.Collections.Generic;
using EFramework.Unity.Utility;

namespace EFramework.Unity.Puer
{
    public partial class XPuer
    {
        /// <summary>
        /// XPuer.Constants 提供了一些常量定义和运行时环境控制，包括运行配置、资源路径和标签生成等功能。
        /// </summary>
        /// <remarks>
        /// <code>
        /// 功能特性
        /// - 运行配置：提供发布模式、调试模式、资源路径等配置
        /// - 标签生成：提供资源路径的标准化处理和标签生成
        /// 
        /// 使用手册
        /// 1. 运行配置
        /// 
        /// 1.1 发布模式
        /// - 配置说明：控制是否启用发布模式
        /// - 访问方式：
        /// var isRelease = XPuer.Constants.ReleaseMode;
        /// 
        /// 1.2 调试模式
        /// - 配置说明：控制是否启用调试模式
        /// - 依赖条件：仅在非发布模式下可用
        /// - 访问方式：
        /// var isDebug = XPuer.Constants.DebugMode;
        /// 
        /// 1.3 调试等待
        /// - 配置说明：控制是否等待调试器连接
        /// - 依赖条件：仅在调试模式下可用
        /// - 访问方式：
        /// var shouldWait = XPuer.Constants.DebugWait;
        /// 
        /// 1.4 调试端口
        /// - 配置说明：设置调试器连接端口
        /// - 依赖条件：仅在调试模式下有效，否则返回 -1
        /// - 访问方式：
        /// var port = XPuer.Constants.DebugPort;
        /// 
        /// 1.5 本地路径
        /// - 配置说明：获取脚本资源的本地路径
        /// - 访问方式：
        /// var localPath = XPuer.Constants.LocalPath;
        /// 
        /// 2. 标签生成
        /// 
        /// 2.1 默认扩展名
        /// public const string Extension = ".jsc";
        /// 
        /// 2.2 生成规则
        /// 生成资源的标签名称，规则如下：
        /// 1. 如果路径不包含斜杠，返回 "default.jsc"
        /// 2. 否则使用目录名生成标签：
        ///    - 将路径分隔符替换为下划线
        ///    - 移除特殊字符
        ///    - 转换为小写
        ///    - 添加 .jsc 扩展名
        /// 
        /// 使用示例：
        /// var assetPath = "Scripts/Example/Test.ts";
        /// var tag = XPuer.Constants.GenTag(assetPath);
        /// // 输出: scripts_example.jsc
        /// 
        /// 特殊字符处理规则：
        /// {
        ///     "_" -&gt; "_"  // 保留
        ///     " " -&gt; ""   // 移除
        ///     "#" -&gt; ""   // 移除
        ///     "[" -&gt; ""   // 移除
        ///     "]" -&gt; ""   // 移除
        /// }
        /// </code>
        /// 更多信息请参考模块文档。
        /// </remarks>
        public static class Constants
        {
            #region 运行配置
            internal static bool bReleaseMode;

            internal static bool releaseMode;

            /// <summary>
            /// ReleaseMode 获取当前是否启用发布模式。
            /// </summary>
            public static bool ReleaseMode
            {
                get
                {
                    if (bReleaseMode == false)
                    {
                        bReleaseMode = true;
                        releaseMode = XPrefs.GetBool(Preferences.ReleaseMode, Preferences.ReleaseModeDefault);
                    }
                    return releaseMode;
                }
            }

            internal static bool bDebugMode;

            internal static bool debugMode;

            /// <summary>
            /// DebugMode 获取当前是否启用调试模式。
            /// </summary>
            public static bool DebugMode
            {
                get
                {
                    if (bDebugMode == false)
                    {
                        bDebugMode = true;
                        debugMode = !ReleaseMode && XPrefs.GetBool(Preferences.DebugMode, Preferences.DebugModeDefault);
                    }
                    return debugMode;
                }
            }

            internal static bool bDebugWait;

            internal static bool debugWait;

            /// <summary>
            /// DebugWait 获取当前是否启用调试等待。
            /// </summary>
            public static bool DebugWait
            {
                get
                {
                    if (bDebugWait == false)
                    {
                        bDebugWait = true;
                        debugWait = DebugMode && XPrefs.GetBool(Preferences.DebugWait, Preferences.DebugWaitDefault);
                    }
                    return debugWait;
                }
            }

            internal static bool bDebugPort;

            internal static int debugPort;

            /// <summary>
            /// DebugPort 获取当前调试端口。
            /// </summary>
            public static int DebugPort
            {
                get
                {
                    if (bDebugPort == false)
                    {
                        bDebugPort = true;
                        debugPort = DebugMode ? XPrefs.GetInt(Preferences.DebugPort, Preferences.DebugPortDefault) : -1;
                    }
                    return debugPort;
                }
            }

            internal static bool bLocalPath;

            internal static string localPath;

            /// <summary>
            /// LocalPath 获取当前本地路径。
            /// </summary>
            public static string LocalPath
            {
                get
                {
                    if (bLocalPath == false)
                    {
                        bLocalPath = true;
                        localPath = XFile.PathJoin(XEnv.LocalPath, XPrefs.GetString(Preferences.LocalUri, Preferences.LocalUriDefault));
                    }
                    return localPath;
                }
            }
            #endregion

            #region 标签生成
            /// <summary>
            /// Extension 是文件扩展名，用于 Puer 生成的脚本文件。
            /// </summary>
            public const string Extension = ".jsc";

            /// <summary>
            /// escapeChars 是字符串转义字符的字典，用于在生成标签名称时替换特定字符。
            /// </summary>
            internal static readonly Dictionary<string, string> escapeChars = new() {
                { "_", "_"},
                { " ", ""},
                { "#", ""},
                { "[", ""},
                { "]", ""}
            };

            /// <summary>
            /// GenTag 生成资源的标签名称。
            /// </summary>
            /// <param name="assetPath">资源的路径</param>
            /// <returns>生成的标签名称，格式为小写字母，使用下划线连接路径段</returns>
            public static string GenTag(string assetPath)
            {
                assetPath = XFile.NormalizePath(assetPath);
                if (!assetPath.Contains("/")) return "default" + Extension;
                else
                {
                    var bundleName = Path.GetDirectoryName(assetPath);
                    bundleName = bundleName.Replace("/", "_").Replace("\\", "_");
                    foreach (var item in escapeChars) bundleName = bundleName.Replace(item.Key, item.Value);
                    bundleName += Extension;
                    bundleName = bundleName.ToLower();
                    return bundleName;
                }
            }
            #endregion
        }
    }
}