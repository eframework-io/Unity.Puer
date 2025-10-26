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
        Assert.Contains("TestComponent@MyComponent", Workspace.Classes, "Workspace.Classes 应包含 MyComponent 类。");
        Assert.Contains("TestComponent@MyComponent", Workspace.Fields.Keys, "Workspace.Fields 应包含 MyComponent 类的字段。");
        Assert.Contains("Nested/TestNested@MyModule", Workspace.Classes, "Workspace.Classes 应包含 MyModule 类。");
        Assert.Contains("Nested/TestNested@MyModule", Workspace.Fields.Keys, "Workspace.Fields 应包含 MyModule 类的字段。");

        Assert.IsFalse(Workspace.Classes.Contains("NoExistClass"), "Workspace.Classes 不应包含不存在类。");
        Assert.IsFalse(Workspace.Fields.Keys.Contains("NoExistClass"), "Workspace.Fields 不应包含不存在类。");
    }

    [Test]
    public void Find()
    {
        Workspace.Parse();
        // 测试查找功能
        var index = Workspace.Find("Nested/TestNested@MyModule");
        Assert.AreNotEqual(-1, index, "Find 应该为已存在的类返回有效索引。");

        // 测试查找不存在的类
        var nonExistentIndex = Workspace.Find("NonExistentClass");
        Assert.AreEqual(-1, nonExistentIndex, "Find 应该为不存在的类返回 -1。");
    }
}
