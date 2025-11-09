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
        Assert.That(XFile.HasDirectory(genDir), Is.True, "如果目录不存在，GenDir 应创建该目录。");
        Assert.That(genDir.EndsWith("/"), Is.True, "GenDir 应以斜杠结尾。");
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
            Assert.That(bindings.Any(type => type.Namespace == "TestNamespace"), Is.True, "动态绑定类型应包含 TestNamespace 命名空间。");
            Assert.That(bindings.Any(type => type.Namespace == "TestNamespace2"), Is.False, "动态绑定类型不应包含 TestNamespace2 命名空间。");
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
            Assert.That(bindingMode1, Is.EqualTo(BindingMode.FastBinding), "DoFilter应返回正确的绑定模式");
            Assert.That(bindingMode2, Is.EqualTo(BindingMode.DontBinding), "DoFilter应返回正确的绑定模式");
            Assert.That(bindingMode3, Is.EqualTo(BindingMode.DontBinding), "DoFilter应返回正确的绑定模式");
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
            Assert.That(XPuer.Generator.IsExcluded(noExcludedType), Is.False, "IsExcluded对非排除类型应返回false");
            Assert.That(XPuer.Generator.IsExcluded(excludedType), Is.True, "IsExcluded对排除类型应返回true");
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
        Assert.That(intTypeName, Is.EqualTo("Int32"), "GetTypeName应返回int的正确类型名称");

        // 测试数组类型
        var intArrayType = typeof(int[]);
        var intArrayTypeName = XPuer.Generator.GetTypeName(intArrayType);
        Assert.That(intArrayTypeName, Is.EqualTo("Int32[]"), "GetTypeName应返回int数组的正确类型名称");

        // 测试多维数组类型
        var intMultiArrayType = typeof(int[,]);
        var intMultiArrayTypeName = XPuer.Generator.GetTypeName(intMultiArrayType);
        Assert.That(intMultiArrayTypeName, Is.EqualTo("Int32[,]"), "GetTypeName应返回int多维数组的正确类型名称");

        // 测试泛型类型
        var listType = typeof(List<string>);
        var listTypeName = XPuer.Generator.GetTypeName(listType);
        Assert.That(listTypeName, Is.EqualTo("List<System.String>"), "GetTypeName应返回泛型List<string>的正确类型名称");

        // 测试嵌套泛型类型
        var nestedType = typeof(Dictionary<int, List<string>>);
        var nestedTypeName = XPuer.Generator.GetTypeName(nestedType);
        Assert.That(nestedTypeName, Is.EqualTo("Dictionary<System.Int32,System.Collections.Generic.List<System.String>>"), "GetTypeName应返回嵌套泛型类型的正确类型名称");

        // 测试空类型
        var voidType = typeof(void);
        var voidTypeName = XPuer.Generator.GetTypeName(voidType);
        Assert.That(voidTypeName, Is.EqualTo("Void"), "GetTypeName应返回void的正确类型名称");

        // 测试嵌套类型
        var nestedGenericType = typeof(Dictionary<int, List<Dictionary<string, int>>>);
        var nestedGenericTypeName = XPuer.Generator.GetTypeName(nestedGenericType);
        Assert.That(nestedGenericTypeName, Is.EqualTo("Dictionary<System.Int32,System.Collections.Generic.List<System.Collections.Generic.Dictionary<System.String,System.Int32>>>"), "GetTypeName应返回深度嵌套泛型类型的正确类型名称");
    }

    [Test]
    public void FriendlyName()
    {
        // 测试基本类型
        var intType = typeof(int);
        var intFriendlyName = XPuer.Generator.GetFriendlyName(intType);
        Assert.That(intFriendlyName, Is.EqualTo("System.Int32"), "GetFriendlyName 应返回 int 的正确友好名称。");

        // 测试数组类型
        var intArrayType = typeof(int[]);
        var intArrayFriendlyName = XPuer.Generator.GetFriendlyName(intArrayType);
        Assert.That(intArrayFriendlyName, Is.EqualTo("System.Int32[]"), "GetFriendlyName 应返回 int 数组的正确友好名称。");

        // 测试多维数组类型
        var intMultiArrayType = typeof(int[,]);
        var intMultiArrayFriendlyName = XPuer.Generator.GetFriendlyName(intMultiArrayType);
        Assert.That(intMultiArrayFriendlyName, Is.EqualTo("System.Int32[,]"), "GetFriendlyName 应返回 int 多维数组的正确友好名称。");

        // 测试泛型类型
        var listType = typeof(List<string>);
        var listFriendlyName = XPuer.Generator.GetFriendlyName(listType);
        Assert.That(listFriendlyName, Is.EqualTo("System.Collections.Generic.List<System.String>"), "GetFriendlyName 应返回泛型 List<string> 的正确友好名称。");

        // 测试嵌套类型
        var nestedType = typeof(Dictionary<int, List<string>>);
        var nestedFriendlyName = XPuer.Generator.GetFriendlyName(nestedType);
        Assert.That(nestedFriendlyName, Is.EqualTo("System.Collections.Generic.Dictionary<System.Int32,System.Collections.Generic.List<System.String>>"), "GetFriendlyName 应返回嵌套泛型类型的正确友好名称。");

        // 测试空类型
        var nullType = typeof(void);
        var nullFriendlyName = XPuer.Generator.GetFriendlyName(nullType);
        Assert.That(nullFriendlyName, Is.EqualTo("System.Void"), "GetFriendlyName 应返回 void 的正确友好名称。");
    }

    [Test]
    public void UpmExports()
    {
        var exports = XPuer.Generator.UpmExports();

        // 验证结果
        Assert.That(exports.ContainsKey("io.eframework.unity.puer"), Is.True, "UpmExports 应包含 io.eframework.unity.puer 包。");
        Assert.That(exports.ContainsKey("io.eframework.unity.puer2"), Is.False, "UpmExports 不应包含 io.eframework.unity.puer2 包。");
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
            Assert.That(modules.Contains(deleteModule), Is.True, "模块列表应包含 EFramework.Unity.Puer 模块。");
            modules.Remove(deleteModule);

            // 测试EFramework.Unity.Puer是否被删除
            XPuer.Generator.LinkModule(modules);
            var fixedJson = XFile.OpenText(XFile.PathJoin(XEnv.ProjectPath, "package.json"));
            Assert.That(fixedJson.Contains(deleteModule), Is.False, "处理后的 package.json 不应包含已删除的模块。");
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
        Assert.That(XPuer.Generator.IsMatch(wildcardList, new string[] { "UnityEngine.Vector3", "System.Single" }), Is.True, "通配符模式应匹配任意参数列表。");

        // 完全相同的参数列表应匹配
        var exactList = new List<string[]> { new string[] { "UnityEngine.Vector3", "System.Single" } };
        Assert.That(XPuer.Generator.IsMatch(exactList, new string[] { "UnityEngine.Vector3", "System.Single" }), Is.True, "完全相同的参数列表应匹配。");

        // 不同长度的参数列表不应匹配
        var differentLengthList = new List<string[]> { new string[] { "UnityEngine.Vector3" } };
        Assert.That(XPuer.Generator.IsMatch(differentLengthList, new string[] { "UnityEngine.Vector3", "System.Single" }), Is.False, "不同长度的参数列表不应匹配。");

        // 内容不同的参数列表不应匹配
        var differentContentList = new List<string[]> { new string[] { "UnityEngine.Vector3", "System.Int32" } };
        Assert.That(XPuer.Generator.IsMatch(differentContentList, new string[] { "UnityEngine.Vector3", "System.Single" }), Is.False, "内容不同的参数列表不应匹配。");

        // 多个模式中有一个匹配应返回true
        var multiplePatternList = new List<string[]> {
                new string[] { "UnityEngine.Vector3", "System.Int32" },
                new string[] { "UnityEngine.Vector3", "System.Single" }
            };
        Assert.That(XPuer.Generator.IsMatch(multiplePatternList, new string[] { "UnityEngine.Vector3", "System.Single" }), Is.True, "多个模式中有一个匹配应返回 true。");

        // 空参数列表应匹配空模式
        var emptyList = new List<string[]> { new string[0] };
        Assert.That(XPuer.Generator.IsMatch(emptyList, new string[0]), Is.True, "空的参数列表集合应返回 true。");

        // 空的参数列表集合应返回false
        var emptyParamtersList = new List<string[]>();
        Assert.That(XPuer.Generator.IsMatch(emptyParamtersList, new string[] { "UnityEngine.Vector3" }), Is.False, "空的参数列表集合应返回 false。");
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
