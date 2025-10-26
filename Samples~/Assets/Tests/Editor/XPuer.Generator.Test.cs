// Copyright (c) 2025 EFramework Innovation. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using NUnit.Framework;
using System;
using System.Linq;
using System.Collections.Generic;
using Puerts;
using EFramework.Unity.Utility;
using EFramework.Unity.Puer.Editor;
using TestNamespace;

/// <summary>
/// TestXPuerGenerator 是 XPuer.Generator 的单元测试。
/// </summary>
public class TestXPuerGenerator
{
    [Test]
    public void Directory()
    {
        var genDir = XPuer.Generator.Output;

        // 验证结果
        Assert.IsTrue(XFile.HasDirectory(genDir), "如果目录不存在，GenDir应创建该目录");
        Assert.IsTrue(genDir.EndsWith("/"), "GenDir应以斜杠结尾");
    }

    [Test]
    public void Bindings()
    {
        // 保存原始 Bindings
        var originalBindings = XPuer.Generator.Bindings;
        try
        {
            // 添加测试类型到 Bindings
            XPuer.Generator.Bindings = new List<Type>(XPuer.Generator.Bindings) { typeof(TestClass) };
            var bindings = XPuer.Generator.DynamicBindings;

            // 验证结果
            Assert.IsTrue(bindings.Any(type => type.Namespace == "TestNamespace"), "动态绑定类型应包含TestNamespace命名空间");
            Assert.IsFalse(bindings.Any(type => type.Namespace == "TestNamespace2"), "动态绑定类型不应包含不存在的命名空间");
        }
        finally
        {
            // 恢复原始 Bindings
            XPuer.Generator.Bindings = originalBindings;
        }
    }

    [Test]
    public void DoFilter()
    {
        // 保存原始 Bindings
        var originalExcludeMembers = XPuer.Generator.ExcludeMembers;
        var originalExcludeMemberMap = XPuer.Generator.excludeMemberMap;
        try
        {
            XPuer.Generator.excludeMemberMap = null;
            XPuer.Generator.ExcludeMembers = new List<List<string>> { new List<string> { "TestNamespace.TestClass", "ExcludeMember" } };

            // 测试正常方法和过时方法
            var memberInfo1 = typeof(TestClass).GetMethod("NormalMethod");
            var memberInfo2 = typeof(TestClass).GetMethod("ObsoleteMethod");
            var memberInfo3 = typeof(TestClass).GetMember("ExcludeMember");
            var bindingMode1 = XPuer.Generator.DoFilter(memberInfo1);
            var bindingMode2 = XPuer.Generator.DoFilter(memberInfo2);
            var bindingMode3 = XPuer.Generator.DoFilter(memberInfo3[0]);

            // 验证结果
            Assert.AreEqual(BindingMode.FastBinding, bindingMode1, "DoFilter应返回正确的绑定模式");
            Assert.AreEqual(BindingMode.DontBinding, bindingMode2, "DoFilter应返回正确的绑定模式");
            Assert.AreEqual(BindingMode.DontBinding, bindingMode3, "DoFilter应返回正确的绑定模式");
        }
        finally
        {
            XPuer.Generator.ExcludeMembers = originalExcludeMembers;
            XPuer.Generator.excludeMemberMap = originalExcludeMemberMap;
        }
    }

    [Test]
    public void IsExcluded()
    {
        var originalExcluded = XPuer.Generator.ExcludeTypes;
        try
        {
            XPuer.Generator.ExcludeTypes = new List<string> { "System.Tuple" };
            // 测试排除和非排除类型
            var noExcludedType = typeof(JsEnv);
            var excludedType = typeof(System.Tuple);

            // 验证结果
            Assert.IsFalse(XPuer.Generator.IsExcluded(noExcludedType), "IsExcluded对非排除类型应返回false");
            Assert.IsTrue(XPuer.Generator.IsExcluded(excludedType), "IsExcluded对排除类型应返回true");
        }
        finally
        {
            XPuer.Generator.ExcludeTypes = originalExcluded;
        }
    }

    [Test]
    public void GetTypeName()
    {
        // 测试基本类型
        var intType = typeof(int);
        var intTypeName = XPuer.Generator.GetTypeName(intType);
        Assert.AreEqual(intTypeName, "Int32", "GetTypeName应返回int的正确类型名称");

        // 测试数组类型
        var intArrayType = typeof(int[]);
        var intArrayTypeName = XPuer.Generator.GetTypeName(intArrayType);
        Assert.AreEqual(intArrayTypeName, "Int32[]", "GetTypeName应返回int数组的正确类型名称");

        // 测试多维数组类型
        var intMultiArrayType = typeof(int[,]);
        var intMultiArrayTypeName = XPuer.Generator.GetTypeName(intMultiArrayType);
        Assert.AreEqual(intMultiArrayTypeName, "Int32[,]", "GetTypeName应返回int多维数组的正确类型名称");

        // 测试泛型类型
        var listType = typeof(List<string>);
        var listTypeName = XPuer.Generator.GetTypeName(listType);
        Assert.AreEqual(listTypeName, "List<System.String>", "GetTypeName应返回泛型List<string>的正确类型名称");

        // 测试嵌套泛型类型
        var nestedType = typeof(Dictionary<int, List<string>>);
        var nestedTypeName = XPuer.Generator.GetTypeName(nestedType);
        Assert.AreEqual(nestedTypeName, "Dictionary<System.Int32,System.Collections.Generic.List<System.String>>", "GetTypeName应返回嵌套泛型类型的正确类型名称");

        // 测试空类型
        var voidType = typeof(void);
        var voidTypeName = XPuer.Generator.GetTypeName(voidType);
        Assert.AreEqual(voidTypeName, "Void", "GetTypeName应返回void的正确类型名称");

        // 测试嵌套类型
        var nestedGenericType = typeof(Dictionary<int, List<Dictionary<string, int>>>);
        var nestedGenericTypeName = XPuer.Generator.GetTypeName(nestedGenericType);
        Assert.AreEqual(nestedGenericTypeName, "Dictionary<System.Int32,System.Collections.Generic.List<System.Collections.Generic.Dictionary<System.String,System.Int32>>>", "GetTypeName应返回深度嵌套泛型类型的正确类型名称");
    }

    [Test]
    public void FriendlyName()
    {
        // 测试基本类型
        var intType = typeof(int);
        var intFriendlyName = XPuer.Generator.GetFriendlyName(intType);
        Assert.AreEqual(intFriendlyName, "System.Int32", "GetFriendlyName应返回int的正确友好名称");

        // 测试数组类型
        var intArrayType = typeof(int[]);
        var intArrayFriendlyName = XPuer.Generator.GetFriendlyName(intArrayType);
        Assert.AreEqual(intArrayFriendlyName, "System.Int32[]", "GetFriendlyName应返回int数组的正确友好名称");

        // 测试多维数组类型
        var intMultiArrayType = typeof(int[,]);
        var intMultiArrayFriendlyName = XPuer.Generator.GetFriendlyName(intMultiArrayType);
        Assert.AreEqual(intMultiArrayFriendlyName, "System.Int32[,]", "GetFriendlyName应返回int多维数组的正确友好名称");

        // 测试泛型类型
        var listType = typeof(List<string>);
        var listFriendlyName = XPuer.Generator.GetFriendlyName(listType);
        Assert.AreEqual(listFriendlyName, "System.Collections.Generic.List<System.String>", "GetFriendlyName应返回泛型List<string>的正确友好名称");

        // 测试嵌套类型
        var nestedType = typeof(Dictionary<int, List<string>>);
        var nestedFriendlyName = XPuer.Generator.GetFriendlyName(nestedType);
        Assert.AreEqual(nestedFriendlyName, "System.Collections.Generic.Dictionary<System.Int32,System.Collections.Generic.List<System.String>>", "GetFriendlyName应返回嵌套泛型类型的正确友好名称");

        // 测试空类型
        var nullType = typeof(void);
        var nullFriendlyName = XPuer.Generator.GetFriendlyName(nullType);
        Assert.AreEqual(nullFriendlyName, "System.Void", "GetFriendlyName应返回void的正确友好名称");
    }

    [Test]
    public void UpmExports()
    {
        var exports = XPuer.Generator.UpmExports();

        // 验证结果
        Assert.IsTrue(exports.ContainsKey("io.eframework.unity.puer"), "UpmExports应包含io.eframework.unity.puer包");
        Assert.IsFalse(exports.ContainsKey("io.eframework.unity.puer2"), "UpmExports不应包含不存在的包");
    }

    [Test]
    public void GenModule()
    {
        try
        {
            // 待删除模块
            var deleteModule = "node_modules/.puer/EFramework.Unity.Puer";

            // 测试ToModule
            var modules = XPuer.Generator.ToModule();
            Assert.IsTrue(modules.Contains(deleteModule), "模块列表应包含EFramework.Unity.Puer模块");
            modules.Remove(deleteModule);

            // 测试EFramework.Unity.Puer是否被删除
            XPuer.Generator.LinkModule(modules);
            var fixedJson = XFile.OpenText(XFile.PathJoin(XEnv.ProjectPath, "package.json"));
            Assert.IsFalse(fixedJson.Contains(deleteModule), "处理后的package.json不应包含已删除的模块");
        }
        finally
        {
            XPuer.Generator.GenModule(); // 恢复模块导出
        }
    }

    [Test]
    public void IsMatch()
    {
        // 通配符模式应匹配任意参数列表
        var wildcardList = new List<string[]> { new string[] { "*" } };
        Assert.IsTrue(XPuer.Generator.IsMatch(wildcardList, new string[] { "UnityEngine.Vector3", "System.Single" }));

        // 完全相同的参数列表应匹配
        var exactList = new List<string[]> { new string[] { "UnityEngine.Vector3", "System.Single" } };
        Assert.IsTrue(XPuer.Generator.IsMatch(exactList, new string[] { "UnityEngine.Vector3", "System.Single" }));

        // 不同长度的参数列表不应匹配
        var differentLengthList = new List<string[]> { new string[] { "UnityEngine.Vector3" } };
        Assert.IsFalse(XPuer.Generator.IsMatch(differentLengthList, new string[] { "UnityEngine.Vector3", "System.Single" }));

        // 内容不同的参数列表不应匹配
        var differentContentList = new List<string[]> { new string[] { "UnityEngine.Vector3", "System.Int32" } };
        Assert.IsFalse(XPuer.Generator.IsMatch(differentContentList, new string[] { "UnityEngine.Vector3", "System.Single" }));

        // 多个模式中有一个匹配应返回true
        var multiplePatternList = new List<string[]> {
                new string[] { "UnityEngine.Vector3", "System.Int32" },
                new string[] { "UnityEngine.Vector3", "System.Single" }
            };
        Assert.IsTrue(XPuer.Generator.IsMatch(multiplePatternList, new string[] { "UnityEngine.Vector3", "System.Single" }));

        // 空参数列表应匹配空模式
        var emptyList = new List<string[]> { new string[0] };
        Assert.IsTrue(XPuer.Generator.IsMatch(emptyList, new string[0]));

        // 空的参数列表集合应返回false
        var emptyParamtersList = new List<string[]>();
        Assert.IsFalse(XPuer.Generator.IsMatch(emptyParamtersList, new string[] { "UnityEngine.Vector3" }));
    }
}

namespace TestNamespace
{
    // 测试DynamicBindings
    public class TestClass
    {
        public string ExcludeMember = "ExcludeMember";

        public void NormalMethod()
        {
            // 测试一般方法
        }

        [Obsolete("this is a obsolete method", true)]
        public void ObsoleteMethod()
        {
            // 测试过时的方法
        }
    }
}
