// Copyright (c) 2025 EFramework Innovation. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using NUnit.Framework;
using EFramework.Unity.Utility;
using EFramework.Unity.Editor;
using UnityEngine.TestTools;
using static EFramework.Unity.Puer.Editor.XPuer;

/// <summary>
/// TestXPuerBuilder 是 XPuer.Builder 的单元测试。
/// </summary>
public class TestXPuerBuilder : IPrebuildSetup
{
    void IPrebuildSetup.Setup()
    {
        // 创建处理器
        var handler = new Builder() { ID = "Test/Build Test Scripts" };

        var buildDir = XFile.PathJoin(XPrefs.GetString(Builder.Preferences.Output, Builder.Preferences.OutputDefault), XPrefs.GetString(XEnv.Preferences.Channel, XEnv.Preferences.ChannelDefault), XEnv.Platform.ToString());
        var manifestFile = XFile.PathJoin(buildDir, XMani.Default);

        var report = XEditor.Tasks.Execute(handler);

        Assert.That(XEditor.Tasks.Result.Succeeded, Is.EqualTo(report.Result), "资源构建应当成功");
        Assert.That(XFile.HasFile(manifestFile), Is.True, "资源清单应当生成成功");

        var manifest = new XMani.Manifest();
        Assert.That(manifest.Read(manifestFile)(), Is.True, "资源清单应当读取成功");

        foreach (var file in manifest.Files)
        {
            var path = XFile.PathJoin(buildDir, file.Name);
            Assert.That(XFile.HasFile(path), Is.True, "文件应当存在于本地：" + file.Name);
            Assert.That(XFile.FileMD5(path), Is.EqualTo(file.MD5), "文件MD5应当一致：" + file.Name);
            Assert.That(XFile.FileSize(path), Is.EqualTo(file.Size), "文件大小应当一致：" + file.Name);
        }
    }

    [Test]
    public void Process() { }
}
