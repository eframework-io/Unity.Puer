// Copyright (c) 2025 EFramework Innovation. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using NUnit.Framework;
using static EFramework.Unity.Puer.XPuer;

/// <summary>
/// TestXPuerPreferences 是 XPuer.Preferences 的单元测试。
/// </summary>
public class TestXPuerPreferences
{
    [Test]
    public void Keys()
    {
        Assert.That(Preferences.ReleaseMode, Is.EqualTo("XPuer/ReleaseMode"), "ReleaseMode 属性应与设置的值一致。");
        Assert.That(Preferences.DebugMode, Is.EqualTo("XPuer/DebugMode"), "DebugMode 属性应与设置的值一致。");
        Assert.That(Preferences.DebugWait, Is.EqualTo("XPuer/DebugWait"), "DebugWait 属性应与设置的值一致。");
        Assert.That(Preferences.DebugPort, Is.EqualTo("XPuer/DebugPort"), "DebugPort 属性应与设置的值一致。");
        Assert.That(Preferences.AssetUri, Is.EqualTo("XPuer/AssetUri"), "AssetUri 属性应与设置的值一致。");
        Assert.That(Preferences.LocalUri, Is.EqualTo("XPuer/LocalUri"), "LocalUri 属性应与设置的值一致。");
        Assert.That(Preferences.RemoteUri, Is.EqualTo("XPuer/RemoteUri"), "RemoteUri 属性应与设置的值一致。");
    }

    [Test]
    public void Defaults()
    {
        Assert.That(Preferences.ReleaseModeDefault, Is.EqualTo(false), "ReleaseModeDefault 属性应与设置的值一致。");
        Assert.That(Preferences.DebugModeDefault, Is.EqualTo(false), "DebugModeDefault 属性应与设置的值一致。");
        Assert.That(Preferences.DebugWaitDefault, Is.EqualTo(true), "DebugWaitDefault 属性应与设置的值一致。");
        Assert.That(Preferences.DebugPortDefault, Is.EqualTo(9222), "DebugPortDefault 属性应与设置的值一致。");
        Assert.That(Preferences.AssetUriDefault, Is.EqualTo("Patch@Scripts@TS.zip"), "AssetUriDefault 属性应与设置的值一致。");
        Assert.That(Preferences.LocalUriDefault, Is.EqualTo("Scripts/TS"), "LocalUriDefault 属性应与设置的值一致。");
        Assert.That(Preferences.RemoteUriDefault, Is.EqualTo("Builds/Patch/${Environment.Author}/${Environment.Version}/${Environment.Platform}/Scripts/TS"), "RemoteUriDefault 属性应与设置的值一致。");
    }
}
