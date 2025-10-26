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
        Assert.AreEqual(Preferences.ReleaseMode, "Puer/ReleaseMode");
        Assert.AreEqual(Preferences.DebugMode, "Puer/DebugMode");
        Assert.AreEqual(Preferences.DebugWait, "Puer/DebugWait");
        Assert.AreEqual(Preferences.DebugPort, "Puer/DebugPort");
        Assert.AreEqual(Preferences.AssetUri, "Puer/AssetUri");
        Assert.AreEqual(Preferences.LocalUri, "Puer/LocalUri");
        Assert.AreEqual(Preferences.RemoteUri, "Puer/RemoteUri");
    }

    [Test]
    public void Defaults()
    {
        Assert.AreEqual(Preferences.ReleaseModeDefault, false);
        Assert.AreEqual(Preferences.DebugModeDefault, false);
        Assert.AreEqual(Preferences.DebugWaitDefault, true);
        Assert.AreEqual(Preferences.DebugPortDefault, 9222);
        Assert.AreEqual(Preferences.AssetUriDefault, "Patch@Scripts@TS.zip");
        Assert.AreEqual(Preferences.LocalUriDefault, "Scripts/TS");
        Assert.AreEqual(Preferences.RemoteUriDefault, "Builds/Patch/${Environment.Author}/${Environment.Version}/${Environment.Platform}/Scripts/TS");
    }
}
