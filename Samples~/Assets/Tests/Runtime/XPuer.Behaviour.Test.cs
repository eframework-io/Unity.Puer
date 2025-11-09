// Copyright (c) 2025 EFramework Innovation. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System;
using System.Collections;
using System.Text.RegularExpressions;
using EFramework.Unity.Puer;
using EFramework.Unity.Utility;
using NUnit.Framework;
using Puerts;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// TestXPuerBehaviour 是 XPuer.Behaviour 的单元测试。
/// </summary>
public class TestXPuerBehaviour
{
    private JSObject myComponent;

    [OneTimeSetUp]
    public void Init()
    {
        var loader = new Puerts.TSLoader.TSLoader();
        loader.UseRuntimeLoader(new DefaultLoader());
        loader.UseRuntimeLoader(new NodeModuleLoader(XEnv.ProjectPath));
        XPuer.VM = new JsEnv(loader, -1);
        XPuer.VM.UsingAction<JSObject, string, object[]>();
        XPuer.VM.UsingAction<JSObject, string, object, int>();

        var puerModule = XPuer.VM.ExecuteModule("EFramework.Unity.Puer");
        XPuer.NewObject = puerModule.Get<Func<JSObject, object[], JSObject>>("NewObject");
        XPuer.FuncApply = puerModule.Get<Func<JSObject, string, object[], object>>("FuncApply");
        XPuer.InitField = puerModule.Get<Action<JSObject, string, object, int>>("InitField");
        myComponent = XPuer.VM.ExecuteModule("TestComponent").Get<JSObject>("MyComponent");
    }

    [Test]
    public void Property()
    {
        // 验证属性是否正确序列化
        LogAssert.Expect(LogType.Log, new Regex(@"Property Number:1"));
        LogAssert.Expect(LogType.Log, new Regex(@"Property String:test string"));
        LogAssert.Expect(LogType.Log, new Regex(@"Property Boolean:True"));
        LogAssert.Expect(LogType.Log, new Regex(@"Property Vector3:\(3\.00, 3\.00, 3\.00\)"));
        LogAssert.Expect(LogType.Log, new Regex(@"Property Vector4:\(4\.40, 4\.40, 4\.40, 4\.40\)"));
        LogAssert.Expect(LogType.Log, new Regex(@"Property Color:RGBA\((1\.000, 0\.000, 0\.000, 0\.000)\)"));
        LogAssert.Expect(LogType.Log, new Regex(@"Property Object:Child2"));
        LogAssert.Expect(LogType.Log, new Regex(@"Property NumberArray:2,2"));
        LogAssert.Expect(LogType.Log, new Regex(@"Property ObjectArray:1,Child2"));

        var obj = GameObject.Instantiate(Resources.Load<GameObject>("Bundle/Prefab/MyComponent"));

        // 清理对象
        if (obj) GameObject.Destroy(obj);
    }

    [UnityTest]
    public IEnumerator Life()
    {
        LogAssert.Expect(LogType.Log, new Regex(@"TestComponent Awake: .*"));
        LogAssert.Expect(LogType.Log, new Regex(@"TestComponent OnEnable: .*"));
        LogAssert.Expect(LogType.Log, new Regex(@"TestComponent Start: .*"));
        LogAssert.Expect(LogType.Log, new Regex(@"TestComponent Update: .*"));
        LogAssert.Expect(LogType.Log, new Regex(@"TestComponent LateUpdate: .*"));
        LogAssert.Expect(LogType.Log, new Regex(@"TestComponent FixedUpdate: .*"));
        LogAssert.Expect(LogType.Log, new Regex(@"TestComponent OnDisable: .*"));
        LogAssert.Expect(LogType.Log, new Regex(@"TestComponent OnDestroy: .*"));

        var obj = new GameObject();
        PuerBehaviour.Add(obj, "", myComponent);
        yield return null;
        PuerBehaviour.OnUpdate();
        PuerBehaviour.OnUpdate();
        PuerBehaviour.OnLateUpdate();
        PuerBehaviour.OnFixedUpdate();
        obj.SetActive(false);
        GameObject.DestroyImmediate(obj);
    }

    [UnityTest]
    public IEnumerator Physics()
    {
        LogAssert.Expect(LogType.Log, new Regex(@"TestComponent OnCollisionEnter: .*"));
        LogAssert.Expect(LogType.Log, new Regex(@"TestComponent OnCollisionStay: .*"));
        LogAssert.Expect(LogType.Log, new Regex(@"TestComponent OnCollisionExit: .*"));
        LogAssert.Expect(LogType.Log, new Regex(@"TestComponent OnTriggerEnter: .*"));
        LogAssert.Expect(LogType.Log, new Regex(@"TestComponent OnTriggerStay: .*"));
        LogAssert.Expect(LogType.Log, new Regex(@"TestComponent OnTriggerExit: .*"));

        // 准备两个正方体
        var cube1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        var rig = cube1.AddComponent<Rigidbody>();
        rig.isKinematic = false;
        rig.useGravity = false;
        PuerBehaviour.Add(cube1, "", myComponent);
        cube1.transform.position = new Vector3(0, 2, 0);
        var cube2 = GameObject.CreatePrimitive(PrimitiveType.Cube);

        // 测试碰撞事件
        cube1.transform.position = new Vector3(0, 1, 0);
        yield return new WaitForSeconds(0.5f);

        cube1.transform.position = new Vector3(0, 2, 0);
        yield return new WaitForSeconds(0.5f);

        // 测试触发事件
        cube1.GetComponent<Collider>().isTrigger = true;
        cube1.transform.position = new Vector3(0, 1, 0);
        yield return new WaitForSeconds(0.5f);

        cube1.transform.position = new Vector3(0, 2, 0);
        yield return new WaitForSeconds(0.5f);

        // 清理对象
        GameObject.DestroyImmediate(cube1);
        GameObject.DestroyImmediate(cube2);
    }

    [TestCase(0, 1, 1, 1, 1, "当前节点")]
    [TestCase(-1, 1, 2, 3, 2, "父节点")]
    [TestCase(1, 4, 2, 1, 1, "子节点")]
    public void Get(int depth, int rootResult, int child1Result, int grandChildResult, int child2Result, string _)
    {
        // 创建测试层级结构
        var root = new GameObject("Root");
        var child1 = new GameObject("Child1");
        var child2 = new GameObject("Child2");
        var grandChild = new GameObject("GrandChild");

        try
        {
            // 建立层级关系
            child1.transform.SetParent(root.transform);
            child2.transform.SetParent(root.transform);
            grandChild.transform.SetParent(child1.transform);

            // 添加测试组件
            PuerBehaviour.Add(root, "", myComponent);
            PuerBehaviour.Add(child1, "", myComponent);
            PuerBehaviour.Add(child2, "", myComponent);
            PuerBehaviour.Add(grandChild, "", myComponent);

            // 在不同节点上测试指定深度
            PuerBehaviour[] result;

            // 测试Root
            result = PuerBehaviour.InternalGets(root, myComponent, depth, true);
            Assert.That(result.Length, Is.EqualTo(rootResult));

            // 测试Child1
            result = PuerBehaviour.InternalGets(child1, myComponent, depth, true);
            Assert.That(result.Length, Is.EqualTo(child1Result));

            // 测试GrandChild
            result = PuerBehaviour.InternalGets(grandChild, myComponent, depth, true);
            Assert.That(result.Length, Is.EqualTo(grandChildResult));

            // 测试Child2
            result = PuerBehaviour.InternalGets(child2, myComponent, depth, true);
            Assert.That(result.Length, Is.EqualTo(child2Result));

            // 测试includeInactive参数
            child1.SetActive(false);

            // 使用includeInactive=false
            result = PuerBehaviour.InternalGets(root, myComponent, depth, false);
            if (depth == 0) Assert.That(result.Length, Is.EqualTo(1), "includeInactive=false时，当前节点查询结果不受影响");
            else if (depth == -1) Assert.That(result.Length, Is.EqualTo(1), "includeInactive=false时，父节点查询不应包含非激活节点");
            else if (depth == 1) Assert.That(result.Length, Is.LessThan(4), "includeInactive=false时，子节点查询不应包含非激活节点");

            // 测试空参数
            LogAssert.Expect(LogType.Error, new Regex(@".*error caused by nil root."));
            result = PuerBehaviour.InternalGets(null, myComponent, depth, true);
            Assert.That(result, Is.Null, "传入空对象时应返回null");

            LogAssert.Expect(LogType.Error, new Regex(@".*error caused by nil type."));
            result = PuerBehaviour.InternalGets(root, null, depth, true);
            Assert.That(result, Is.Null, "传入空类型时应返回null");
        }
        finally
        {
            // 清理对象
            GameObject.DestroyImmediate(root);
        }
    }
}
