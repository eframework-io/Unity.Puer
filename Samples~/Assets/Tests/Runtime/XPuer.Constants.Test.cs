// Copyright (c) 2025 EFramework Innovation. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using NUnit.Framework;
using EFramework.Unity.Puer;

/// <summary>
/// TestXPuerConstants 是 XPuer.Constants 的单元测试。
/// </summary>
public class TestXPuerConstants
{
    [TestCase("myscript.jsc")]     // 单脚本路径
    [TestCase("myfolder/myscript.jsc")]    // 包含空格
    [TestCase("myfolder/my#script.jsc")]    // 包含#号
    [TestCase("myfolder/my[script].jsc")]   // 包含方括号
    [TestCase("myfolder\\myscript.jsc")]   // 包含反斜杠
    public void Tag(string path)
    {
        var expected = path.Contains("myfolder") ? "myfolder.jsc" : "default.jsc";
        var result = XPuer.Constants.GenTag(path);

        // 验证标签是否正确
        Assert.That(result, Is.EqualTo(expected), "GenTag应返回正确的标签名称");
    }

    [TestCase(true, true)]
    [TestCase(false, false)]
    public void Mode(bool releaseMode, bool debugMode)
    {
        // 测试模式
        XPuer.Constants.bReleaseMode = true;
        XPuer.Constants.bDebugMode = true;
        XPuer.Constants.releaseMode = releaseMode;
        XPuer.Constants.debugMode = debugMode;

        Assert.That(XPuer.Constants.ReleaseMode, Is.EqualTo(releaseMode), "ReleaseMode属性应与设置的值一致");
        Assert.That(XPuer.Constants.DebugMode, Is.EqualTo(debugMode), "DebugMode属性应与设置的值一致");
    }

    [Test]
    public void Path()
    {
        var originLocalPath = XPuer.Constants.LocalPath;
        // 测试路径
        var localPath = "localPath";
        XPuer.Constants.bLocalPath = true;
        XPuer.Constants.localPath = localPath;

        Assert.That(XPuer.Constants.LocalPath, Is.EqualTo(localPath), "LocalPath属性应与设置的路径一致");

        // 恢复本地路径
        XPuer.Constants.localPath = originLocalPath;
    }

    [Test]
    public void Escape()
    {
        // 测试排除字符
        Assert.That(XPuer.Constants.escapeChars["_"], Is.EqualTo("_"), "下划线应映射为下划线。");
        Assert.That(XPuer.Constants.escapeChars[" "], Is.EqualTo(""), "空格应映射为空字符串。");
        Assert.That(XPuer.Constants.escapeChars["#"], Is.EqualTo(""), "井号应映射为空字符串。");
        Assert.That(XPuer.Constants.escapeChars["["], Is.EqualTo(""), "左方括号应映射为空字符串。");
        Assert.That(XPuer.Constants.escapeChars["]"], Is.EqualTo(""), "右方括号应映射为空字符串。");
    }
}
