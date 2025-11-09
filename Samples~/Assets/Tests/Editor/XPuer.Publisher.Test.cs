// Copyright (c) 2025 EFramework Innovation. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using NUnit.Framework;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Networking;
using EFramework.Unity.Puer;
using EFramework.Unity.Editor;
using EFramework.Unity.Utility;
using static EFramework.Unity.Puer.Editor.XPuer;

/// <summary>
/// TestXPuerPublisher 是 XPuer.Publisher 的单元测试。
/// </summary>
public class TestXPuerPublisher
{
    [Test]
    [PrebuildSetup(typeof(TestXPuerBuilder))]
    public void Process()
    {
        // 复制资源到本地
        var buildDir = XFile.PathJoin(XPrefs.GetString(Builder.Preferences.Output, Builder.Preferences.OutputDefault), XPrefs.GetString(XEnv.Preferences.Channel, XEnv.Preferences.ChannelDefault), XEnv.Platform.ToString());
        if (XFile.HasDirectory(XPuer.Constants.LocalPath)) XFile.DeleteDirectory(XPuer.Constants.LocalPath);
        XFile.CopyDirectory(buildDir, XPuer.Constants.LocalPath);
        Assert.That(XFile.HasDirectory(XPuer.Constants.LocalPath), Is.True, "本地路径应当存在。");

        // 设置测试环境
        XPrefs.Asset.Set(Publisher.Preferences.Endpoint, "http://localhost:9000");
        XPrefs.Asset.Set(Publisher.Preferences.Bucket, "default");
        XPrefs.Asset.Set(Publisher.Preferences.Access, "admin");
        XPrefs.Asset.Set(Publisher.Preferences.Secret, "adminadmin");
        XPrefs.Asset.Set(XPuer.Preferences.LocalUri, "Scripts");
        XPrefs.Asset.Set(XPuer.Preferences.RemoteUri, $"TestXPuerPublisher/Builds-{XTime.GetMillisecond()}/Scripts");

        var handler = new Publisher() { ID = "Test/TestXPuerPublisher" };

        // 执行推送脚本
        LogAssert.Expect(LogType.Error, new Regex(@"<ERROR> Object does not exist.*"));
        LogAssert.Expect(LogType.Error, new Regex(@"XEditor.Command.Run: finish mc.*"));
        var report = XEditor.Tasks.Execute(handler);

        // 验证Result
        Assert.That(report.Result, Is.EqualTo(XEditor.Tasks.Result.Succeeded), "脚本发布应当成功");

        var manifestUrl = $"{XPrefs.Asset.GetString(Publisher.Preferences.Endpoint)}/{XPrefs.Asset.GetString(Publisher.Preferences.Bucket)}/{XPrefs.Asset.GetString(Publisher.Preferences.RemoteUri)}/{XMani.Default}";
        var req = UnityWebRequest.Get(manifestUrl);
        req.timeout = 10;
        req.SendWebRequest();
        while (!req.isDone) { }
        Assert.That(req.responseCode, Is.EqualTo(200), "资源清单应当请求成功");

        var manifest = new XMani.Manifest();
        Assert.That(manifest.Parse(req.downloadHandler.text, out _), Is.True, "资源清单应当读取成功");
    }
}
