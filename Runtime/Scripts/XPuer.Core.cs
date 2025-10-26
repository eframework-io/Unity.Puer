// Copyright (c) 2025 EFramework Innovation. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System;
using System.Collections;
using UnityEngine;
using Puerts;
using EFramework.Unity.Utility;

namespace EFramework.Unity.Puer
{
    /// <summary>
    /// XPuer.Core 提供了 JavaScript 虚拟机的运行时环境，支持事件系统管理、生命周期控制和脚本调试等功能。
    /// </summary>
    /// <remarks>
    /// <code>
    /// 功能特性
    /// - 虚拟机管理：支持 JavaScript 运行时环境的启动和生命周期控制
    /// - 跨语言交互：实现 C# 与 JavaScript 对象及函数的相互操作
    /// - 调试器支持：提供了调试模式和调试器连接功能，可以通过配置启用
    /// 
    /// 使用手册
    /// 1. 初始化流程
    /// 
    /// 1.1 预初始化 (OnPreInit)
    /// - 调用 handler.OnPreInit()
    /// - 触发 XPuer.EventType.OnPreInit 事件
    /// - 用于准备环境和资源
    /// 
    /// 1.2 虚拟机启动 (OnVMStart)
    /// - 创建并配置 JavaScript 虚拟机
    /// - 调用 handler.OnVMStart()
    /// - 触发 XPuer.EventType.OnVMStart 事件
    /// - 用于虚拟机配置和初始化
    /// 
    /// 1.3 后初始化 (OnPostInit)
    /// - 加载核心模块
    /// - 调用 handler.OnPostInit()
    /// - 触发 XPuer.EventType.OnPostInit 事件
    /// - 用于最终配置和模块加载
    /// 
    /// 示例：
    /// internal class MyHandler : MonoBehaviour, IHandler
    /// {
    ///     ILoader IHandler.Loader
    ///     {
    ///         get
    ///         {
    ///             // 返回一个实现了ILoader接口的加载器
    ///             return new DefaultLoader();
    ///         }
    ///     }
    /// 
    ///     IEnumerator IHandler.OnPreInit()
    ///     {
    ///         // 预初始化操作
    ///         yield return null;
    ///     }
    /// 
    ///     IEnumerator IHandler.OnVMStart()
    ///     {
    ///         // 配置虚拟机
    ///         VM.UsingAction&lt;string, bool&gt;();
    ///         yield return null;
    ///     }
    /// 
    ///     IEnumerator IHandler.OnPostInit()
    ///     {
    ///         // 后初始化操作
    ///         yield return null;
    ///     }
    /// }
    /// 
    /// // 启动初始化
    /// StartCoroutine(XPuer.Initialize(new MyHandler()));
    /// 
    /// 2. 跨语言交互
    /// 
    /// 2.1 创建 JavaScript 对象
    /// // 获取JS模块并创建对象
    /// var module = VM.ExecuteModule("MyModule");
    /// var jsClass = module.Get&lt;JSObject&gt;("MyClass");
    /// var jsObject = XPuer.NewObject(jsClass, new object[] { "参数1", 123 });
    /// 
    /// 2.2 调用 JavaScript 方法
    /// // 调用JS对象的方法
    /// var result = XPuer.FuncApply(jsObject, "myMethod", new object[] { "参数1", 123 });
    /// 
    /// 2.3 初始化 JavaScript 字段
    /// // 设置JS对象的字段
    /// XPuer.InitField(jsObject, "myField", "字段值", 1 &lt;&lt; 2);
    /// 
    /// 3. 调试器支持
    /// 
    /// 3.1 首选项配置
    /// 参考 XPuer.Preferences 中的配置项说明
    /// 
    /// 3.2 VSCode 配置
    /// .vscode/launch.json 配置示例：
    /// {
    ///     "configurations": [
    ///         {
    ///             "name": "Attach to JVM",
    ///             "port": 9222,
    ///             "request": "attach",
    ///             "type": "node",
    ///             "pauseForSourceMap": true
    ///         }
    ///     ]
    /// }
    /// - 注：pauseForSourceMap 可以防止首次断点时未完整加载 sourceMap 导致无法命中的问题
    /// </code>
    /// 更多信息请参考模块文档。
    /// </remarks>
    public partial class XPuer : MonoBehaviour
    {
        /// <summary>
        /// VM 是 JavaScript 的虚拟机实例。
        /// </summary>
        public static JsEnv VM;

        /// <summary>
        /// NewObject 是创建新对象的函数。
        /// </summary>
        public static Func<JSObject, object[], JSObject> NewObject;

        /// <summary>
        /// FuncApply 是应用函数的函数。
        /// </summary>
        public static Func<JSObject, string, object[], object> FuncApply;

        /// <summary>
        /// InitField 是初始化字段的函数。
        /// </summary>
        public static Action<JSObject, string, object, int> InitField;

        /// <summary>
        /// OnDestroy 在组件被销毁时释放虚拟机资源。
        /// </summary>
        internal void OnDestroy() { VM?.Dispose(); }

        /// <summary>
        /// Update 处理虚拟机的 Tick 和组件的 Update 事件。
        /// </summary>
        internal void Update() { VM?.Tick(); PuerBehaviour.OnUpdate(); }

        /// <summary>
        /// LateUpdate 处理组件的 LateUpdate 事件。
        /// </summary>
        internal void LateUpdate() { PuerBehaviour.OnLateUpdate(); }

        /// <summary>
        /// FixedUpdate 处理组件的 FixedUpdate 事件。
        /// </summary>
        internal void FixedUpdate() { PuerBehaviour.OnFixedUpdate(); }

#if UNITY_EDITOR
        /// <summary>
        /// OnApplicationQuit 在应用退出时清理进度条。
        /// </summary>
        internal void OnApplicationQuit() { UnityEditor.EditorUtility.ClearProgressBar(); }
#endif

        /// <summary>
        /// EventType 是事件类型的枚举，定义了 PuerTS 初始化过程中的关键事件。
        /// </summary>
        public enum EventType
        {
            /// <summary>
            /// OnPreInit 是预初始化的事件。
            /// 在初始化流程开始时触发，用于准备环境和资源。
            /// </summary>
            OnPreInit,

            /// <summary>
            /// OnVMStart 是虚拟机启动的事件。
            /// 在 JavaScript 虚拟机启动时触发，用于执行虚拟机相关的初始化操作。
            /// </summary>
            OnVMStart,

            /// <summary>
            /// OnPostInit 是后初始化的事件。
            /// 在所有初始化完成后触发，用于执行最终的设置和清理工作。
            /// </summary>
            OnPostInit
        }

        /// <summary>
        /// Event 是事件管理器实例。
        /// 用于管理和分发 PuerTS 的系统事件。
        /// </summary>
        public static readonly XEvent.Manager Event = new();

        /// <summary>
        /// IHandler 是处理程序接口，用于管理初始化过程。
        /// </summary>
        public interface IHandler
        {
            /// <summary>
            /// Loader 获取加载器。
            /// </summary>
            ILoader Loader { get; }

            /// <summary>
            /// OnPreInit 是预初始化处理。
            /// </summary>
            /// <returns>一个 IEnumerator，用于协程支持</returns>
            IEnumerator OnPreInit();

            /// <summary>
            /// OnVMStart 是虚拟机启动处理。
            /// </summary>
            /// <returns>一个 IEnumerator，用于协程支持</returns>
            IEnumerator OnVMStart();

            /// <summary>
            /// OnPostInit 是后初始化处理。
            /// </summary>
            /// <returns>一个 IEnumerator，用于协程支持</returns>
            IEnumerator OnPostInit();
        }

        /// <summary>
        /// Initialize 初始化 XPuer 模块。
        /// </summary>
        /// <param name="handler">处理程序接口，用于管理初始化过程</param>
        /// <returns>一个 IEnumerator，用于协程支持</returns>
        /// <exception cref="ArgumentNullException">当 handler 或 handler.Loader 为 null 时抛出</exception>
        public static IEnumerator Initialize(IHandler handler)
        {
            if (handler == null) throw new ArgumentNullException("handler");
            var loader = handler.Loader ?? throw new ArgumentNullException("handler.Loader");
            DontDestroyOnLoad(new GameObject("XPuer").AddComponent<XPuer>());

            var ltime = XTime.GetMillisecond();
            yield return handler.OnPreInit();
            var ntime = XTime.GetMillisecond();
            XLog.Notice("XPuer.Initialize: invoke handler.OnPreInit elapsed {0}ms.", ntime - ltime);
            ltime = ntime;

            Event.Notify(EventType.OnPreInit);
            ntime = XTime.GetMillisecond();
            XLog.Notice("XPuer.Initialize: notify event.OnPreInit elapsed {0}ms.", ntime - ltime);
            ltime = ntime;

            var port = -1;
            if (XEnv.Mode == XEnv.ModeType.Dev && !Constants.ReleaseMode && Constants.DebugMode)
            {
                port = Constants.DebugPort;
            }

            VM = new JsEnv(loader, port);
            ntime = XTime.GetMillisecond();
            XLog.Notice("XPuer.Initialize: create jvm with debug port: {0} elapsed {1}ms.", port, ntime - ltime);
            ltime = ntime;

            yield return handler.OnVMStart();
            ntime = XTime.GetMillisecond();
            XLog.Notice("XPuer.Initialize: invoke handler.OnVMStart elapsed {0}ms.", ntime - ltime);
            ltime = ntime;

            Event.Notify(EventType.OnVMStart);
            ntime = XTime.GetMillisecond();
            XLog.Notice("XPuer.Initialize: notify event.OnVMStart elapsed {0}ms.", ntime - ltime);
            ltime = ntime;

            var coreModule = VM.ExecuteModule("EFramework.Unity.Puer");
            NewObject = coreModule.Get<Func<JSObject, object[], JSObject>>("NewObject");
            FuncApply = coreModule.Get<Func<JSObject, string, object[], object>>("FuncApply");
            InitField = coreModule.Get<Action<JSObject, string, object, int>>("InitField");

            yield return handler.OnPostInit();
            ntime = XTime.GetMillisecond();
            XLog.Notice("XPuer.Initialize: invoke handler.OnPostInit elapsed {0}ms.", ntime - ltime);
            ltime = ntime;

            Event.Notify(EventType.OnPostInit);
            ntime = XTime.GetMillisecond();
            XLog.Notice("XPuer.Initialize: notify event.OnPostInit elapsed {0}ms.", ntime - ltime);

#if UNITY_EDITOR
            if (port > 0 && Constants.DebugWait)
            {
                var task = VM.WaitDebuggerAsync();
                var timeout = 20;
                var stime = Time.realtimeSinceStartup;
                while (true)
                {
                    var dtime = Time.realtimeSinceStartup - stime;
                    if (dtime > timeout)
                    {
                        XLog.Debug("XPuer.Initialize: wait for jvm debugger timeout.");
                        break;
                    }
                    if (task.IsCompleted)
                    {
                        XLog.Debug("XPuer.Initialize: jvm debugger connected.");
                        break;
                    }
                    if (UnityEditor.EditorUtility.DisplayCancelableProgressBar("Debug JVM", $"Wait for JVM debugger to attach port {port}... ({Math.Floor(timeout - dtime)} seconds remaining)", dtime / timeout))
                    {
                        XLog.Debug("XPuer.Initialize: wait for jvm debugger has been canceled.");
                        break;
                    }
                    yield return 0;
                }
                UnityEditor.EditorUtility.ClearProgressBar();
            }
#endif

            yield return null;
        }
    }
}
