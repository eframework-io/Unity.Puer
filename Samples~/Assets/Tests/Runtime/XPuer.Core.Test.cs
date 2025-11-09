// Copyright (c) 2025 EFramework Innovation. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System.Collections;
using EFramework.Unity.Puer;
using UnityEngine.TestTools;
using UnityEngine;
using Puerts;
using EFramework.Unity.Utility;
using NUnit.Framework;
using static EFramework.Unity.Puer.XPuer;

/// <summary>
/// TestXPuerCore 是 XPuer.Core 的单元测试。
/// </summary>
public class TestXPuerCore
{
    public class MyHandler : MonoBehaviour, IHandler
    {
        public bool IsPreInit = false;
        public bool IsVMStart = false;
        public bool IsPostInit = false;

        ILoader IHandler.Loader
        {
            get
            {
                var loader = new Puerts.TSLoader.TSLoader();
                loader.UseRuntimeLoader(new DefaultLoader());
                loader.UseRuntimeLoader(new NodeModuleLoader(XEnv.ProjectPath));
                return loader;
            }
        }

        IEnumerator IHandler.OnPreInit()
        {
            IsPreInit = true;
            yield return null;
        }

        IEnumerator IHandler.OnVMStart()
        {
            VM.UsingAction<JSObject, string, object[]>();
            VM.UsingAction<JSObject, string, object, int>();
            VM.UsingAction<bool>();
            VM.UsingAction<float>();
            VM.UsingAction<string, bool>();
            VM.UsingAction<string, bool>();
            IsVMStart = true;
            yield return null;
        }

        IEnumerator IHandler.OnPostInit()
        {
            IsPostInit = true;
            yield return null;
        }
    }

    [UnityTest]
    public IEnumerator Initialize()
    {
        XPrefs.Asset.Set(Preferences.DebugWait, false); // 测试时关闭等待调试器

        Constants.releaseMode = false;
        bool[] debugModes = { false, true };
        foreach (var debugMode in debugModes)
        {
            var isPreInit = false;
            var isVMStart = false;
            var isPostInit = false;

            var handler = new MyHandler();
            XPuer.Event.Register(XPuer.EventType.OnPreInit, () => isPreInit = true);
            XPuer.Event.Register(XPuer.EventType.OnVMStart, () => isVMStart = true);
            XPuer.Event.Register(XPuer.EventType.OnPostInit, () => isPostInit = true);

            Constants.debugMode = debugMode;
            Constants.debugWait = debugMode;
            var stime = Time.realtimeSinceStartup;
            yield return XPuer.Initialize(handler);

            // 测试属性是否正确初始化
            var xpuer = GameObject.Find("XPuer");
            Assert.That(xpuer != null, Is.True, "应创建 XPuer 游戏对象。");
            Assert.That(xpuer.GetComponent<XPuer>() != null, Is.True, "XPuer 对象应包含 XPuer 组件。");
            Assert.That(VM.debugPort == (debugMode ? Constants.DebugPort : -1), Is.True, "调试端口应正确设置。");
            Assert.That(handler.IsPreInit, Is.True, "handler 的 PreInit 事件应被调用。");
            Assert.That(handler.IsVMStart, Is.True, "handler 的 VMStart 事件应被调用。");
            Assert.That(handler.IsPostInit, Is.True, "handler 的 PostInit 事件应被调用。");
            Assert.That(isPreInit, Is.True, "OnPreInit 事件应被触发。");
            Assert.That(isVMStart, Is.True, "OnVMStart 事件应被触发。");
            Assert.That(isPostInit, Is.True, "OnPostInit 事件应被触发。");
        }
    }
}
