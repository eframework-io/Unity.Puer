// Copyright (c) 2025 EFramework Innovation. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System.Linq;
using NUnit.Framework;
using static EFramework.Unity.Puer.Editor.XPuer;

/// <summary>
/// TestXPuerWorkspace 是 XPuer.Workspace 的单元测试。
/// </summary>
public class TestXPuerWorkspace
{
    [Test]
    public void Parse()
    {
        // 执行解析
        Workspace.Parse();

        // 验证不同目录的类是否被解析
        Assert.That(Workspace.Classes, Contains.Item("TestComponent@MyComponent"), "Workspace.Classes 应包含 MyComponent 类。");
        Assert.That(Workspace.Fields.Keys, Contains.Item("TestComponent@MyComponent"), "Workspace.Fields 应包含 MyComponent 类的字段。");
        Assert.That(Workspace.Classes, Contains.Item("Nested/TestNested@MyModule"), "Workspace.Classes 应包含 MyModule 类。");
        Assert.That(Workspace.Fields.Keys, Contains.Item("Nested/TestNested@MyModule"), "Workspace.Fields 应包含 MyModule 类的字段。");

        Assert.That(Workspace.Classes, Does.Not.Contain("NoExistClass"), "Workspace.Classes 不应包含不存在类。");
        Assert.That(Workspace.Fields.Keys, Does.Not.Contain("NoExistClass"), "Workspace.Fields 不应包含不存在类。");
    }

    [Test]
    public void Find()
    {
        Workspace.Parse();
        // 测试查找功能
        var index = Workspace.Find("Nested/TestNested@MyModule");
        Assert.That(index, Is.Not.EqualTo(-1), "Find 应该为已存在的类返回有效索引。");

        // 测试查找不存在的类
        var nonExistentIndex = Workspace.Find("NonExistentClass");
        Assert.That(nonExistentIndex, Is.EqualTo(-1), "Find 应该为不存在的类返回 -1。");
    }
}
