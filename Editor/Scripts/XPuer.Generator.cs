// Copyright (c) 2025 EFramework Innovation. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEditor;
using Puerts;
using Puerts.Editor.Generator.DTS;
using EFramework.Unity.Editor;
using EFramework.Unity.Utility;

namespace EFramework.Unity.Puer.Editor
{
    public partial class XPuer
    {
        /// <summary>
        /// XPuer.Generator 提供了代码生成工具，支持 PuerTS 绑定代码的生成、模块导出和自动安装等功能。
        /// </summary>
        /// <remarks>
        /// <code>
        /// 功能特性：
        /// - 类型绑定：支持自定义类型的动态绑定和过滤，包括 Unity 导出类型和自定义程序集类型
        /// - 模块导出：支持通过 UPM 定义 puerExports/puerAdapter 或代码定义等方式将模块导出为 NPM 包
        /// 
        /// 使用手册：
        /// 1. 首选项配置
        /// 
        /// 代码生成工具提供了以下配置选项：
        /// 
        /// 输出路径：
        /// - 配置键：Puer/Generator/Output@Editor
        /// - 默认值：Assets/Plugins/Puer/Generate/
        /// - 功能说明：设置生成代码的输出目录
        /// 
        /// 自动生成：
        /// - 配置键：Puer/Generator/Auto/Binding@Editor
        /// - 默认值：true
        /// - 功能说明：
        ///   - 控制项目加载时是否自动生成代码
        ///   - 首次加载或输出目录为空时会触发生成
        ///   - 通过 Tools/PuerTS/Generate (all in one) 执行生成
        /// 
        /// 自动安装：
        /// - 配置键：Puer/Generator/Auto/Install@Editor
        /// - 默认值：true
        /// - 功能说明：
        ///   - 控制项目加载时是否自动安装 NPM 模块
        ///   - 检测到 package-lock.json 变化时会触发安装
        ///   - 自动执行 npm install 安装依赖
        /// 
        /// 2. 类型绑定
        /// 
        /// 2.1 类型定义
        /// 配置需要生成的类型：
        /// // 配置需要生成类型定义的类型
        /// XPuer.Generator.Types = new List&lt;Type&gt;
        /// {
        ///     typeof(UnityEngine.GameObject),
        ///     typeof(UnityEngine.Transform),
        ///     typeof(Your.Game.PlayerController)
        /// };
        /// 
        /// // 配置需要生成 Blittable 拷贝的类型
        /// XPuer.Generator.Blittables = new List&lt;Type&gt;
        /// {
        ///     typeof(UnityEngine.Vector3),
        ///     typeof(UnityEngine.Quaternion)
        /// };
        /// 
        /// // 配置需要生成绑定代码的类型
        /// XPuer.Generator.Bindings = new List&lt;Type&gt;
        /// {
        ///     typeof(Your.Game.NetworkManager),
        ///     typeof(Your.Game.UIManager)
        /// };
        /// 
        /// 2.2 类型过滤
        /// 配置需要排除的类型：
        /// // 配置需要排除的程序集
        /// XPuer.Generator.ExcludeAssemblys = new List&lt;string&gt;
        /// {
        ///     "Assembly-CSharp-Editor.dll",
        ///     "Your.Game.Editor.dll"
        /// };
        /// 
        /// // 配置需要排除的类型
        /// XPuer.Generator.ExcludeTypes = new List&lt;string&gt;
        /// {
        ///     "Your.Game.Internal.DebugHelper",
        ///     "Your.Game.Editor.*"  // 使用通配符排除所有 Editor 命名空间下的类型
        /// };
        /// 
        /// // 配置需要排除的成员
        /// XPuer.Generator.ExcludeMembers = new List&lt;List&lt;string&gt;&gt;
        /// {
        ///     // 排除 GameObject 的 SetActive 方法
        ///     new List&lt;string&gt; { "UnityEngine.GameObject", "SetActive", "System.Boolean" },
        ///     
        ///     // 排除所有参数的 Transform.Translate 方法
        ///     new List&lt;string&gt; { "UnityEngine.Transform", "Translate", "*" }
        /// };
        /// 
        /// 3. 模块导出
        /// 
        /// 3.1 适配器
        /// 在 UPM 包中配置适配器：
        /// {
        ///   "name": "your.package.name",
        ///   "version": "1.0.0",
        ///   "puerAdapter": ".puer"  // 推荐使用 .puer 目录
        /// }
        /// 
        /// 适配器目录结构：
        /// your.package.name/
        ///   ├── .puer/              # 推荐使用此目录名
        ///   │   ├── package.json    # 适配器包配置
        ///   │   ├── index.js        # JavaScript 模块入口
        ///   │   └── index.d.ts      # TypeScript 声明文件
        ///   └── package.json        # UPM 包配置
        /// 
        /// 3.2 命名空间
        /// 
        /// 3.2.1 代码配置
        /// 通过 Namespaces 属性配置需要导出的命名空间：
        /// // 添加自定义命名空间
        /// XPuer.Generator.Namespaces = new List&lt;string&gt;
        /// {
        ///     "Your.Game.Core",
        ///     "Your.Game.UI",
        ///     "Your.Game.Network"
        /// };
        /// 
        /// // 获取当前配置的命名空间列表（包含 UPM 包中的配置）
        /// var namespaces = XPuer.Generator.Namespaces;
        /// foreach (var ns in namespaces)
        /// {
        ///     Debug.Log($"Namespace to export: {ns}");
        /// }
        /// 
        /// 3.2.2 UPM 包配置
        /// 在 UPM 包的 package.json 中配置 puerExports 字段：
        /// {
        ///   "name": "your.package.name",
        ///   "version": "1.0.0",
        ///   "puerExports": [
        ///     "Your.Namespace.Name",
        ///     "Your.Another.Namespace"
        ///   ]
        /// }
        /// 
        /// 3.3 代码生成
        /// 
        /// 3.3.1 生成命令
        /// - 手动生成：通过菜单项 Tools/PuerTS/Generate Module 触发
        /// - 自动生成：项目加载时根据 Puer/Generator/Auto/Binding@Editor 配置自动执行
        /// - 自动安装：项目加载时根据 Puer/Generator/Auto/Install@Editor 配置自动执行
        /// 
        /// 3.3.2 输出结构
        /// 生成的模块位于 node_modules/.puer/ 目录：
        /// node_modules/.puer/&lt;namespace&gt;/
        ///   ├── package.json      # NPM 包配置
        ///   ├── index.js         # JavaScript 模块代码
        ///   └── index.d.ts       # TypeScript 声明文件
        /// 
        /// 3.3.3 增量更新
        /// 更新检测：
        /// - 监控 package-lock.json 和 packages-lock.json 的 MD5 值变化
        /// - 根据变化决定是否需要重新生成和安装
        /// - 对已存在的文件进行内容合并，保持现有导出不丢失
        /// </code>
        /// 更多信息请参考模块文档。
        /// </remarks>
        [Configure]
        public class Generator : AssetPostprocessor, XEditor.Event.Internal.OnEditorLoad
        {
            #region Preferences
            /// <summary>
            /// Preferences 是 PuerTS 代码生成器的首选项设置。
            /// 提供了代码生成和模块安装的配置选项。
            /// </summary>
            internal class Preferences : Puer.XPuer.Preferences
            {
                /// <summary>
                /// Output 是代码生成输出路径的配置键。
                /// </summary>
                public const string Output = "XPuer/Generator/Output@Editor";

                /// <summary>
                /// OutputDefault 是代码生成输出路径的默认值。
                /// </summary>
                public const string OutputDefault = "Assets/Plugins/Puer/Generate/";

                /// <summary>
                /// AutoBinding 是自动生成绑定的配置键。
                /// </summary>
                public const string AutoBinding = "XPuer/Generator/Auto/Binding@Editor";

                /// <summary>
                /// AutoBindingDefault 是自动生成绑定的默认值。
                /// </summary>
                public const bool AutoBindingDefault = true;

                /// <summary>
                /// AutoInstall 是自动安装依赖的配置键。
                /// </summary>
                public const string AutoInstall = "XPuer/Generator/Auto/Install@Editor";

                /// <summary>
                /// AutoInstallDefault 是自动安装依赖的默认值。
                /// </summary>
                public const bool AutoInstallDefault = true;

                public override string Section => "XPuer";

                public override int Priority => 303;

                public override void OnVisualize(string searchContext, XPrefs.IBase context)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    foldout = EditorGUILayout.Foldout(foldout, new GUIContent("Generator", "XPuer Generator Options."));
                    if (foldout)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label(new GUIContent("Auto"), GUILayout.Width(60));

                        GUILayout.Label(new GUIContent("Binding", "Run PuerTS/Generate (all in one) When Project Was Loaded."), GUILayout.Width(60));
                        context.Set(AutoBinding, EditorGUILayout.Toggle(context.GetBool(AutoBinding, AutoBindingDefault)));

                        GUILayout.Label(new GUIContent("Install", "Install Module Dependencies When Project Was Loaded."), GUILayout.Width(60));
                        context.Set(AutoInstall, EditorGUILayout.Toggle(context.GetBool(AutoInstall, AutoInstallDefault)));
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label(new GUIContent("Output", "PuerTS Binding Output Directory."), GUILayout.Width(60));
                        context.Set(Output, EditorGUILayout.TextField("", context.GetString(Output, OutputDefault)));
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                    }
                    EditorGUILayout.EndVertical();
                }
            }
            #endregion

            #region Bindings
            /// <summary>
            /// Output 获取代码生成的输出目录。
            /// 如果目录不存在则自动创建。
            /// </summary>
            [CodeOutputDirectory]
            internal static string Output
            {
                get
                {
                    var path = XPrefs.GetString(Preferences.Output, Preferences.OutputDefault);
                    if (!XFile.HasDirectory(path))
                    {
                        XFile.CreateDirectory(path);
                    }
                    return path;
                }
            }

            /// <summary>
            /// Types 获取或设置需要生成类型定义的类型集合。
            /// </summary>
            [Typing]
            public static IEnumerable<Type> Types { get; set; } = new List<Type>();

            /// <summary>
            /// Blittables 获取或设置需要生成 Blittable 拷贝的类型集合。
            /// </summary>
            [BlittableCopy]
            public static IEnumerable<Type> Blittables { get; set; } = new List<Type>();

            /// <summary>
            /// namespaces 是缓存的命名空间列表。
            /// </summary>
            internal static List<string> namespaces;

            /// <summary>
            /// Namespaces 获取或设置需要导出的命名空间列表。
            /// 包含从 UPM 包中导出的命名空间。
            /// </summary>
            public static List<string> Namespaces
            {
                get
                {
                    if (namespaces == null)
                    {
                        namespaces = new List<string>();
                        foreach (var kvp in UpmExports())
                        {
                            foreach (var ele in kvp.Value) namespaces.Add(ele);
                        }
                        namespaces = namespaces.Distinct().ToList();
                    }
                    return namespaces;
                }
                set
                {
                    namespaces = new List<string>(value);
                    foreach (var kvp in UpmExports())
                    {
                        foreach (var ele in kvp.Value) namespaces.Add(ele);
                    }
                    namespaces = namespaces.Distinct().ToList();
                }
            }

            /// <summary>
            /// UpmExports 获取 UPM 包中定义的导出命名空间。
            /// 从每个包的 package.json 中读取 puerExports 字段。
            /// </summary>
            /// <returns>包名到导出命名空间列表的映射</returns>
            internal static Dictionary<string, List<string>> UpmExports()
            {
                var exports = new Dictionary<string, List<string>>();
                var packages = UnityEditor.PackageManager.PackageInfo.GetAllRegisteredPackages();
                foreach (var pkg in packages)
                {
                    var meta = JSON.Parse(XFile.OpenText(XFile.PathJoin(pkg.resolvedPath, "package.json")));
                    if (meta != null)
                    {
                        if (meta.HasKey("puerExports"))
                        {
                            var pexports = meta["puerExports"].AsArray;
                            if (pexports != null && pexports.Count > 0)
                            {
                                foreach (var pexport in pexports)
                                {
                                    var export = pexport.Value.Value.Trim();
                                    if (string.IsNullOrEmpty(export)) continue;
                                    if (!exports.TryGetValue(pkg.name, out var nss))
                                    {
                                        nss = new();
                                        exports[pkg.name] = nss;
                                    }
                                    nss.Add(export);
                                }
                            }
                        }
                    }
                }
                return exports;
            }

            /// <summary>
            /// Bindings 获取或设置需要生成绑定代码的类型集合。
            /// </summary>
            public static IEnumerable<Type> Bindings { get; set; } = new List<Type>();

            /// <summary>
            /// BaseTypes 获取或设置基础类型列表。
            /// </summary>
            public static List<string> BaseTypes { get; set; } = new List<string>();

            /// <summary>
            /// ExcludeAssemblys 获取或设置需要排除的程序集列表。
            /// </summary>
            public static List<string> ExcludeAssemblys { get; set; } = new List<string>();

            /// <summary>
            /// ExcludeTypes 获取或设置需要排除的类型列表。
            /// </summary>
            public static List<string> ExcludeTypes { get; set; } = new List<string>();

            /// <summary>
            /// ExcludeMembers 获取或设置需要排除的成员列表。
            /// </summary>
            public static List<List<string>> ExcludeMembers { get; set; } = new List<List<string>>();

            /// <summary>
            /// excludeMemberMap 是缓存的排除成员映射。
            /// </summary>
            internal static Dictionary<string, Dictionary<string, List<string[]>>> excludeMemberMap;

            /// <summary>
            /// ExcludeMemberMap 获取排除成员的映射关系。
            /// 用于快速查找需要排除的类型成员。
            /// </summary>
            internal static Dictionary<string, Dictionary<string, List<string[]>>> ExcludeMemberMap
            {
                get
                {
                    if (excludeMemberMap == null)
                    {
                        excludeMemberMap = new Dictionary<string, Dictionary<string, List<string[]>>>();
                        foreach (var member in ExcludeMembers)
                        {
                            if (member.Count < 2) continue;
                            var temp = member.Select(o => o.Replace(" ", "")).ToList();
                            var fullname = temp[0];
                            var methodName = temp[1];
                            Dictionary<string, List<string[]>> methodOrProp;
                            if (!excludeMemberMap.TryGetValue(fullname, out methodOrProp))
                            {
                                methodOrProp = new Dictionary<string, List<string[]>>();
                                excludeMemberMap.Add(fullname, methodOrProp);
                            }
                            List<string[]> paramtersList;
                            if (!methodOrProp.TryGetValue(methodName, out paramtersList))
                            {
                                paramtersList = new List<string[]>();
                                methodOrProp.Add(methodName, paramtersList);
                            }
                            paramtersList.Add(temp.GetRange(2, temp.Count - 2).ToArray());
                        }
                    }
                    return excludeMemberMap;
                }
            }

            /// <summary>
            /// DynamicBindings 获取动态绑定的类型集合。
            /// 包括 Unity 导出类型和自定义程序集类型。
            /// </summary>
            /// <returns>需要动态绑定的类型集合</returns>
            [Binding]
            internal static IEnumerable<Type> DynamicBindings
            {
                get
                {
                    var unityTypes = from assembly in AppDomain.CurrentDomain.GetAssemblies()
                                     where !(assembly.ManifestModule is System.Reflection.Emit.ModuleBuilder)
                                     from type in assembly.GetExportedTypes()
                                     where type.Namespace != null && Namespaces.Contains(type.Namespace) && !IsExcluded(type)
                                     select type;
                    var customAssemblys = new string[] { };
                    var customTypes = from assembly in customAssemblys.Select(s => Assembly.Load(s))
                                      where assembly.ManifestModule is not System.Reflection.Emit.ModuleBuilder
                                      from type in assembly.GetExportedTypes()
                                      where type.Namespace == null || !type.Namespace.StartsWith("PuertsStaticWrap")
                                           && !IsExcluded(type)
                                      select type;
                    return unityTypes
                        .Concat(customTypes)
                        .Concat(Bindings)
                        .Distinct();
                }
            }

            /// <summary>
            /// DoFilter 过滤成员的绑定模式。
            /// 检查成员是否被标记为过时或被排除。
            /// </summary>
            /// <param name="member">要检查的成员信息</param>
            /// <returns>成员的绑定模式</returns>
            [Filter]
            internal static BindingMode DoFilter(MemberInfo member) => (FilterObsolete(member) || FilterMember(member)) ? BindingMode.DontBinding : BindingMode.FastBinding;

            /// <summary>
            /// IsExcluded 检查类型是否被排除。
            /// 根据程序集名称、类型全名和基类类型进行检查。
            /// </summary>
            /// <param name="type">要检查的类型</param>
            /// <returns>如果类型被排除则返回 true，否则返回 false</returns>
            internal static bool IsExcluded(Type type)
            {
                if (type == null) return false;

                string assemblyName = Path.GetFileName(type.Assembly.Location);
                if (ExcludeAssemblys.Contains(assemblyName)) return true;

                string fullname = type.FullName != null ? type.FullName.Replace("+", ".") : "";
                if (ExcludeTypes.Contains(fullname)) return true;

                if (type.BaseType != null &&
                    !string.IsNullOrEmpty(type.BaseType.FullName) &&
                    type != typeof(object) &&
                    type != typeof(ValueType) &&
                    type != typeof(void) &&
                    !BaseTypes.Contains(type.BaseType.FullName.Replace("+", ".")))
                {
                    return IsExcluded(type.BaseType);
                }

                foreach (var excludeType in ExcludeTypes)
                {
                    if (excludeType.EndsWith("*"))
                    {
                        var prefix = excludeType.TrimEnd('*');
                        if (fullname.StartsWith(prefix))
                        {
                            return true;
                        }
                    }
                }
                return IsExcluded(type.BaseType);
            }

            /// <summary>
            /// FilterObsolete 检查成员是否被标记为过时。
            /// </summary>
            /// <param name="member">要检查的成员信息</param>
            /// <returns>如果成员被标记为过时且错误则返回 true，否则返回 false</returns>
            internal static bool FilterObsolete(MemberInfo member)
            {
                var obsolete = member.GetCustomAttributes(typeof(ObsoleteAttribute), true).FirstOrDefault() as ObsoleteAttribute;
                if (obsolete != null) return obsolete.IsError;
                return false;
            }

            /// <summary>
            /// FilterMember 检查成员是否被排除。
            /// 根据成员名称和参数列表进行匹配。
            /// </summary>
            /// <param name="member">要检查的成员信息</param>
            /// <returns>如果成员被排除则返回 true，否则返回 false</returns>
            internal static bool FilterMember(MemberInfo member)
            {
                string declaringTypeName = member.DeclaringType.FullName.Replace("+", ".");
                if (ExcludeMemberMap.TryGetValue(declaringTypeName, out var methodOrProp) && (
                    methodOrProp.TryGetValue(member.Name, out var paramtersList) ||
                    member is ConstructorInfo && methodOrProp.TryGetValue(member.DeclaringType.Name, out paramtersList)
                ))
                {
                    if (!(member is MethodInfo || member is ConstructorInfo)) return true;

                    var paramters = member is MethodInfo ? ((MethodInfo)member).GetParameters() : ((ConstructorInfo)member).GetParameters();
                    var paramterNames = (from pInfo in paramters select GetFriendlyName(pInfo.ParameterType)).ToArray();

                    if (IsMatch(paramtersList, paramterNames))
                        return true;
                    for (var i = paramterNames.Length - 1; i >= 0; i--)
                    {
                        if (!paramters[i].IsOptional)
                            break;
                        if (IsMatch(paramtersList, paramterNames.Take(i).ToArray()))
                            return true;
                    }
                }

                if (member is MethodInfo info && info.IsStatic && member.IsDefined(typeof(ExtensionAttribute)))
                {
                    var mParamters = info.GetParameters();
                    var mParamterNames = (from pInfo in mParamters select GetFriendlyName(pInfo.ParameterType)).ToArray();

                    declaringTypeName = mParamterNames[0];
                    mParamters = mParamters.Skip(1).ToArray();
                    mParamterNames = mParamterNames.Skip(1).ToArray();
                    if (ExcludeMemberMap.TryGetValue(declaringTypeName, out methodOrProp) && methodOrProp.TryGetValue(member.Name, out paramtersList))
                    {
                        if (IsMatch(paramtersList, mParamterNames)) return true;
                        for (var i = mParamterNames.Length - 1; i >= 0; i--)
                        {
                            if (!mParamters[i].IsOptional)
                                break;
                            if (IsMatch(paramtersList, mParamterNames.Take(i).ToArray()))
                                return true;
                        }
                    }
                }
                return false;
            }

            /// <summary>
            /// IsMatch 检查参数列表是否匹配。
            /// 支持通配符和精确匹配。
            /// </summary>
            /// <param name="paramtersList">要匹配的参数列表</param>
            /// <param name="mParamters">目标参数名称数组</param>
            /// <returns>如果参数列表匹配则返回 true，否则返回 false</returns>
            internal static bool IsMatch(List<string[]> paramtersList, string[] mParamters)
            {
                foreach (var paramters in paramtersList)
                {
                    if (paramters.Length == 1 && paramters[0] == "*")
                        return true;
                    if (paramters.Length == mParamters.Length)
                    {
                        var exclude = true;
                        for (int i = 0; i < paramters.Length && exclude; i++)
                        {
                            if (paramters[i] != mParamters[i]) exclude = false;
                        }
                        if (exclude) return true;
                    }
                }
                return false;
            }

            /// <summary>
            /// GetTypeName 获取类型的友好名称。
            /// 处理泛型类型的显示格式。
            /// </summary>
            /// <param name="type">要获取名称的类型</param>
            /// <returns>类型的友好名称</returns>
            internal static string GetTypeName(Type type)
            {
                if (type.IsGenericType)
                {
                    var genericArgumentNames = type.GetGenericArguments()
                        .Select(x => GetFriendlyName(x)).ToArray();
                    return type.Name.Split('`')[0] + "<" + string.Join(",", genericArgumentNames) + ">";
                }
                else
                {
                    return type.Name;
                }
            }

            /// <summary>
            /// GetFriendlyName 获取类型的完整友好名称。
            /// 处理数组、嵌套类型和泛型类型的显示格式。
            /// </summary>
            /// <param name="type">要获取名称的类型</param>
            /// <returns>类型的完整友好名称</returns>
            internal static string GetFriendlyName(Type type)
            {
                if (string.IsNullOrEmpty(type.FullName)) return string.Empty;
                if (type.IsArray)
                {
                    if (type.GetArrayRank() > 1)
                    {
                        return GetFriendlyName(type.GetElementType()) + "[" + new String(',', type.GetArrayRank() - 1) + "]";
                    }
                    else
                    {
                        return GetFriendlyName(type.GetElementType()) + "[]";
                    }
                }
                else if (type.IsNested)
                {
                    if (type.DeclaringType.IsGenericTypeDefinition)
                    {
                        var genericArgumentNames = type.GetGenericArguments()
                            .Select(x => GetFriendlyName(x)).ToArray();
                        return type.DeclaringType.FullName.Split('`')[0] + "<" + string.Join(",", genericArgumentNames) + ">" + '.' + type.Name;
                    }
                    else
                    {
                        return GetFriendlyName(type.DeclaringType) + '.' + GetTypeName(type);
                    }
                }
                else if (type.IsGenericType)
                {
                    var genericArgumentNames = type.GetGenericArguments()
                        .Select(x => GetFriendlyName(x)).ToArray();
                    return type.FullName.Split('`')[0] + "<" + string.Join(",", genericArgumentNames) + ">";
                }
                return type.FullName.Replace("+", ".");
            }
            #endregion

            #region Functions
            /// <summary>
            /// NamespaceGenInfo 是命名空间的生成信息。
            /// 用于管理命名空间的成员和子命名空间。
            /// </summary>
            internal class NamespaceGenInfo
            {
                /// <summary>
                /// FirstName 获取命名空间的第一部分名称。
                /// </summary>
                public string FirstName { get; private set; }

                /// <summary>
                /// Name 获取命名空间的最后一部分名称。
                /// </summary>
                public string Name { get; private set; }

                /// <summary>
                /// Members 获取命名空间的成员类型信息数组。
                /// </summary>
                public TypeGenInfo[] Members { get; private set; }

                /// <summary>
                /// Namespaces 获取子命名空间列表。
                /// </summary>
                public List<string> Namespaces { get; private set; }

                /// <summary>
                /// AddChildNamespace 添加子命名空间。
                /// </summary>
                /// <param name="namespace">要添加的命名空间</param>
                public void AddChildNamespace(string @namespace) { Namespaces.Add(@namespace); }

                /// <summary>
                /// From 从完整名称和成员数组创建命名空间生成信息。
                /// </summary>
                /// <param name="fullName">命名空间的完整名称</param>
                /// <param name="members">命名空间的成员类型信息数组</param>
                /// <returns>命名空间生成信息实例</returns>
                public static NamespaceGenInfo From(string fullName, TypeGenInfo[] members)
                {
                    int lastIndex = fullName != null ? fullName.LastIndexOf(".") : -1;
                    return new NamespaceGenInfo()
                    {
                        FirstName = lastIndex >= 0 ? fullName.Substring(0, lastIndex) : string.Empty,
                        Name = lastIndex >= 0 ? fullName.Substring(lastIndex + 1) : fullName,
                        Members = members,
                        Namespaces = new List<string>()
                    };
                }
            }

            /// <summary>
            /// TypeGenInfo 是类型生成的信息。
            /// 用于管理类型的名称信息。
            /// </summary>
            internal class TypeGenInfo
            {
                /// <summary>
                /// FirstName 获取类型的命名空间名称。
                /// </summary>
                public string FirstName { get; private set; }

                /// <summary>
                /// Name 获取类型的简单名称。
                /// </summary>
                public string Name { get; private set; }

                /// <summary>
                /// From 从命名空间和类型名称创建类型生成信息。
                /// </summary>
                /// <param name="firstName">类型的命名空间名称</param>
                /// <param name="name">类型的简单名称</param>
                /// <returns>类型生成信息实例</returns>
                public static TypeGenInfo From(string firstName, string name)
                {
                    return new TypeGenInfo()
                    {
                        FirstName = firstName == null ? string.Empty : firstName,
                        Name = name,
                    };
                }

                /// <summary>
                /// From 从完整类型名称创建类型生成信息。
                /// </summary>
                /// <param name="fullName">类型的完整名称</param>
                /// <returns>类型生成信息实例</returns>
                public static TypeGenInfo From(string fullName)
                {
                    int lastIndex = fullName.LastIndexOf(".");
                    string firstName = lastIndex >= 0 ? fullName.Substring(0, lastIndex) : string.Empty;
                    string name = lastIndex >= 0 ? fullName.Substring(lastIndex + 1) : fullName;
                    return From(firstName, name);
                }

                /// <summary>
                /// From 从类型创建类型生成信息。
                /// 处理泛型类型的特殊情况。
                /// </summary>
                /// <param name="type">要转换的类型</param>
                /// <returns>类型生成信息实例，如果类型无效则返回 null</returns>
                public static TypeGenInfo From(Type type)
                {
                    if (string.IsNullOrEmpty(type.FullName)) return null;
                    string fullName;
                    if (type.IsGenericType)
                    {
                        var parts = type.FullName.Replace('+', '.').Split('`');
                        fullName = $"{parts[0]}${parts[1].Split('[')[0]}";
                    }
                    else
                    {
                        fullName = type.FullName.Replace('+', '.');
                    }
                    return From(fullName);
                }
            }

            /// <summary>
            /// GetNamespaces 获取需要生成绑定代码的命名空间信息。
            /// </summary>
            /// <returns>命名空间生成信息的集合</returns>
            internal static IEnumerable<TsNamespaceGenInfo> GetNamespaces()
            {
                var configure = Configure.GetConfigureByTags(new List<string>() { "Puerts.BindingAttribute", "Puerts.TypingAttribute" });

                var bindings = configure["Puerts.BindingAttribute"].Select(kv => kv.Key)
                    .Where(o => o is Type)
                    .Cast<Type>()
                    .Where(t => !t.IsGenericTypeDefinition);

                var typings = configure["Puerts.TypingAttribute"].Select(kv => kv.Key)
                    .Where(o => o is Type)
                    .Cast<Type>()
                    .Where(t => !t.IsGenericTypeDefinition);

                var types = bindings.Concat(typings).Concat(new Type[] { typeof(Type) }).Distinct();

                var namespaces = TypingGenInfo.FromTypes(types).NamespaceInfos
                    .Distinct()
                    .Where(ele => !string.IsNullOrEmpty(ele.Name) && Namespaces.Contains(ele.Name));

                return namespaces;
            }

            /// <summary>
            /// ToModule 将命名空间转换为 NPM 模块。
            /// 生成模块的 JavaScript、TypeScript 定义和包配置文件。
            /// </summary>
            /// <returns>生成的模块路径列表</returns>
            internal static List<string> ToModule()
            {
                var droot = "node_modules/.puer/";
                if (XFile.HasDirectory(droot)) XFile.DeleteDirectory(droot);

                var modules = new List<string>();
                var namespaces = GetNamespaces();
                var packages = UnityEditor.PackageManager.PackageInfo.GetAllRegisteredPackages();

                foreach (var pkg in packages)
                {
                    var meta = JSON.Parse(XFile.OpenText(XFile.PathJoin(pkg.resolvedPath, "package.json")));
                    if (meta != null)
                    {
                        if (meta.HasKey("puerAdapter"))
                        {
                            var pmeta = meta["puerAdapter"];
                            if (pmeta == null || string.IsNullOrEmpty(pmeta.Value))
                            {
                                XLog.Error("XPuer.Generator.ToModule: null puer adapter defined in: {0}.", pkg.name);
                                continue;
                            }
                            var nroot = XFile.PathJoin(pkg.resolvedPath, pmeta.Value);
                            if (!XFile.HasDirectory(nroot))
                            {
                                XLog.Error("XPuer.Generator.ToModule: puer adapter {0} was defined but {1} not found.", pkg.name, nroot);
                                continue;
                            }
                            var nfile = XFile.PathJoin(nroot, "package.json");
                            if (!XFile.HasFile(nfile))
                            {
                                XLog.Error("XPuer.Generator.ToModule: puer adapter {0} was defined but package.json not found.", pkg.name);
                                continue;
                            }
                            var nmeta = JSON.Parse(XFile.OpenText(nfile));
                            if (nmeta == null)
                            {
                                XLog.Error("XPuer.Generator.ToModule: puer adapter {0} was defined but package.json parse error.", pkg.name);
                                continue;
                            }

                            var nname = nmeta.HasKey("name") ? nmeta["name"].Value : pkg.name;
                            var module = $"{droot}{nname}";
                            if (XFile.HasDirectory(module))
                            {
                                XLog.Error("XPuer.Generator.ToModule: dumplicated module: {0}.", nname);
                                continue;
                            }

                            XFile.CopyDirectory(nroot, module);
                            modules.Add(module);
                            XLog.Debug("XPuer.Generator.ToModule: export upm package: <a href=\"file:///{0}\">{1}</a> as npm module: <a href=\"file:///{2}\">{3}</a>.", Path.GetFullPath(nroot), pkg.name, Path.GetFullPath(module), module);
                        }
                    }
                }

                foreach (var ns in namespaces)
                {
                    var module = modules.FirstOrDefault(m => Path.GetFileName(m).Equals(ns.Name, StringComparison.OrdinalIgnoreCase));
                    if (string.IsNullOrEmpty(module))
                    {
                        module = $"{droot}{ns.Name}";
                        XFile.CreateDirectory(module);
                        modules.Add(module);
                    }
                    {
                        var file = XFile.PathJoin(module, "index.js");
                        if (!XFile.HasFile(file))
                        {
                            using var sw = new StreamWriter(file, false);
                            sw.Write(DJSTemplate(ns));
                            sw.Flush();
                        }
                        else XFile.SaveText(file, $"{XFile.OpenText(file)}\n{DJSTemplate(ns)}");
                    }
                    {
                        var file = XFile.PathJoin(module, "index.d.ts");
                        if (!XFile.HasFile(file))
                        {
                            using var sw = new StreamWriter(file, false);
                            sw.Write(DTSTemplate(ns));
                            sw.Flush();
                        }
                        else XFile.SaveText(file, $"{XFile.OpenText(file)}\n{DTSTemplate(ns)}");
                    }
                    {
                        var file = XFile.PathJoin(module, "package.json");
                        if (!XFile.HasFile(file))
                        {
                            using var sw = new StreamWriter(file, false);
                            sw.Write(PKGTemplate(ns));
                            sw.Flush();
                            XLog.Debug("XPuer.Generator.ToModule: export namespace: {0} as npm module: <a href=\"file:///{1}\">{2}</a>.", ns.Name, Path.GetFullPath(module), module);
                        }
                        else XLog.Debug("XPuer.Generator.ToModule: merge namespace: {0} into npm module: <a href=\"file:///{1}\">{2}</a>.", ns.Name, Path.GetFullPath(module), module);
                    }
                }

                return modules;
            }

            /// <summary>
            /// LinkModule 链接 NPM 模块。
            /// 安装模块并清理包配置文件。
            /// </summary>
            /// <param name="modules">要链接的模块路径列表</param>
            internal static void LinkModule(List<string> modules)
            {
                var droot = "node_modules/.puer/";
                modules.Sort();

                var imodules = string.Join(" ", modules.Select(m => $"file:{m}"));
                XEditor.Command.Run(bin: XEditor.Command.Find("npm"), args: new string[] { "install", imodules }).Wait();

                #region cleanup package.json
                var pkg = XFile.PathJoin(XEnv.ProjectPath, "package.json");
                if (XFile.HasFile(pkg))
                {
                    var lines = File.ReadAllLines(pkg);
                    var nlines = new List<string>();
                    var depSection = false;
                    foreach (var line in lines)
                    {
                        var removed = false;
                        if (line.Contains("\"dependencies\":")) depSection = true;
                        if (line.Contains(droot))
                        {
                            var mname = line.Split("file:")[1].Replace("\"", "").Replace(",", "").Replace(" ", "").Trim();
                            if (!modules.Contains(mname)) removed = true;
                        }
                        else if (depSection && line.Contains("}"))
                        {
                            depSection = false;
                            if (nlines.Count > 0)
                            {
                                var lline = nlines[nlines.Count - 1];
                                if (lline.EndsWith(",")) // remove last char
                                {
                                    lline = lline.Substring(0, lline.Length - 1);
                                    nlines[nlines.Count - 1] = lline;
                                }
                            }
                        }
                        if (removed == false) nlines.Add(line);
                        else XLog.Debug("XPuer.Generator.LinkModule: remove {0} from file: {1}", line, pkg);
                    }
                    if (lines.Length != nlines.Count) File.WriteAllLines(pkg, nlines.ToArray());
                }
                #endregion

                #region cleanup package-lock.json
                var plock = XFile.PathJoin(XEnv.ProjectPath, "package-lock.json");
                if (XFile.HasFile(plock))
                {
                    var lines = File.ReadAllLines(plock);
                    var nlines = new List<string>();
                    var depSection = false;
                    var modSection = false;
                    var resSection = false;
                    foreach (var line in lines)
                    {
                        var removed = false;
                        if (line.Contains("\"dependencies\":")) depSection = true;
                        else if (line.Contains(droot))
                        {
                            if (line.Contains("file:"))
                            {
                                var mname = line.Split("file:")[1].Replace("\"", "").Replace(",", "").Replace(" ", "").Trim();
                                if (!modules.Any(ele => ele.Equals(mname, StringComparison.OrdinalIgnoreCase))) removed = true;
                            }
                            else if (line.Contains("resolved"))
                            {
                                var mname = line.Split("\"resolved\":")[1].Replace("\"", "").Replace(",", "").Replace(" ", "").Trim();
                                if (!modules.Any(ele => ele.Equals(mname, StringComparison.OrdinalIgnoreCase)))
                                {
                                    removed = true;
                                    resSection = true;
                                    nlines.RemoveAt(nlines.Count - 1);
                                }
                            }
                            else
                            {
                                var mname = line.Split(":")[0].Replace("\"", "").Trim();
                                if (!modules.Any(ele => ele.Equals(mname, StringComparison.OrdinalIgnoreCase)))
                                {
                                    removed = true;
                                    if (line.EndsWith("{")) modSection = true;
                                }
                            }
                        }
                        else if (depSection && line.Contains("}"))
                        {
                            depSection = false;
                            if (nlines.Count > 0)
                            {
                                var lline = nlines[nlines.Count - 1];
                                if (lline.EndsWith(",")) // remove last char
                                {
                                    lline = lline.Substring(0, lline.Length - 1);
                                    nlines[nlines.Count - 1] = lline;
                                }
                            }
                        }
                        else if (modSection || resSection)
                        {
                            removed = true;
                            modSection = line.Contains("}");
                            resSection = modSection;
                        }
                        if (removed == false) nlines.Add(line);
                        else XLog.Debug("XPuer.Generator.LinkModule: remove {0} from file: {1}", line, plock);
                    }
                    if (lines.Length != nlines.Count) File.WriteAllLines(plock, nlines.ToArray());
                }
                #endregion
            }

            /// <summary>
            /// DJSTemplate 生成命名空间的 JavaScript 模块代码。
            /// </summary>
            /// <param name="ns">命名空间生成信息</param>
            /// <returns>生成的 JavaScript 模块代码</returns>
            internal static string DJSTemplate(TsNamespaceGenInfo ns)
            {
                var genInfo = NamespaceGenInfo.From(ns.Name, ns.Types.Select(t => TypeGenInfo.From(t.Namespace, t.Name)).ToArray());
                var typeNames = genInfo.Members
                    .Select(typeInfo => typeInfo.Name)
                    .Distinct();
                var namespaceNames = genInfo.Namespaces
                    .Where(name => !typeNames.Contains(name))
                    .Distinct();
                return
$@"{string.Join("", typeNames.Select(name => $@"
const ${name} = CS.{ns.Name}.{name}"))}
{string.Join("", namespaceNames.Select(name => $@"
const ${name} = CS.{ns.Name}.{name}s"))}
export {{{string.Join("", typeNames.Select(name => $@"
    ${name} as {name},"))}
{string.Join("", namespaceNames.Select(name => $@"
    ${name} as {name},"))}}}";
            }

            /// <summary>
            /// DTSTemplate 生成命名空间的 TypeScript 声明文件内容。
            /// </summary>
            /// <param name="ns">命名空间生成信息</param>
            /// <returns>生成的 TypeScript 声明文件内容</returns>
            internal static string DTSTemplate(TsNamespaceGenInfo ns)
            {
                return
$@"declare module ""{ns.Name}"" {{
    export = CS.{ns.Name};
}}";
            }

            /// <summary>
            /// PKGTemplate 生成命名空间的 NPM 包配置文件内容。
            /// </summary>
            /// <param name="ns">命名空间生成信息</param>
            /// <returns>生成的包配置文件内容</returns>
            internal static string PKGTemplate(TsNamespaceGenInfo ns)
            {
                return
$@"{{
  ""name"": ""{ns.Name}"",
  ""module"": ""index.js"",
  ""types"": ""index.d.ts"",
  ""type"": ""module""
}}";
            }
            #endregion

            #region Triggers
            int XEditor.Event.Callback.Priority => 0;

            bool XEditor.Event.Callback.Singleton => true;

            void XEditor.Event.Internal.OnEditorLoad.Process(params object[] args)
            {
                // Delay call to fix error while open project with no library: [Puer002]import puerts/init.mjs failed: module not found
                // EditorApplication.delayCall += () =>
                {
                    var dirty = false;
                    if (XPrefs.GetBool(Preferences.AutoBinding, Preferences.AutoBindingDefault))
                    {
                        var output = Configure.GetCodeOutputDirectory();
                        if (!XFile.HasDirectory(output) || Directory.GetFiles(output).Length == 0)
                        {
                            dirty = true;
                            // if (PlayerSettings.GetScriptingBackend(NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup)) == ScriptingImplementation.IL2CPP)
                            // {
                            //     Puerts.Editor.Generator.UnityMenu.GenerateDTS();
                            //     PuertsIl2cpp.Editor.Generator.UnityMenu.GenV2();
                            //     XLog.Debug("XPuer.Generator: invoke Tools/PuerTS/Generate For xIl2cpp mode (all in one with full wrapper) automatically.");
                            // }
                            // else
                            {
                                Puerts.Editor.Generator.UnityMenu.GenV1();
                                XLog.Debug("XPuer.Generator: invoke Tools/PuerTS/Generate (all in one) automatically.");
                            }
                        }
                    }

                    if (XPrefs.GetBool(Preferences.AutoInstall, Preferences.AutoInstallDefault))
                    {
                        var pkgLock = Path.Combine(XEnv.ProjectPath, "package-lock.json");
                        var pkgMd5 = Path.Combine(XEnv.ProjectPath, "package-lock.md5");
                        var modulesDir = XFile.PathJoin(XEnv.ProjectPath, "node_modules");

                        var install = false;
                        if (!XFile.HasFile(pkgLock) || !XFile.HasFile(pkgMd5) || !XFile.HasDirectory(modulesDir)) install = true;
                        else
                        {
                            var localMd5 = XFile.OpenText(pkgMd5);
                            var realMd5 = XFile.FileMD5(pkgLock);
                            if (localMd5 != realMd5) install = true;
                        }

                        if (install)
                        {
                            dirty = true;
                            var result = XEditor.Command.Run(bin: XEditor.Command.Find("npm"), args: new string[] { "install" });
                            result.Wait();
                            if (result.Result.Code == 0 && XFile.HasFile(pkgLock) && XFile.HasDirectory(modulesDir))
                            {
                                XFile.SaveText(pkgMd5, XFile.FileMD5(pkgLock));
                                XLog.Debug("XPuer.Generator: node_modules has been installed.");
                            }
                            else XLog.Error("XPuer.Generator: node_modules install failed.");
                        }
                    }

                    {
                        var pkgLock = Path.Combine(XEnv.ProjectPath, "Packages", "packages-lock.json");
                        var pkgMd5 = Path.Combine(XEnv.ProjectPath, "Packages", "packages-lock.md5");
                        var install = false;
                        if (!XFile.HasFile(pkgLock) || !XFile.HasFile(pkgMd5)) install = true;
                        else
                        {
                            var localMd5 = XFile.OpenText(pkgMd5);
                            var realMd5 = XFile.FileMD5(pkgLock);
                            if (localMd5 != realMd5) install = true;
                        }
                        if (dirty || install) GenModule();
                        if (install) XFile.SaveText(pkgMd5, XFile.FileMD5(pkgLock));
                    }
                }
            }

            /// <summary>
            /// GenModule 生成 NPM 模块。
            /// 将命名空间导出为模块并安装。
            /// </summary>
            [MenuItem("Tools/PuerTS/Generate Module")]
            internal static void GenModule() { LinkModule(ToModule()); }
            #endregion
        }
    }
}
