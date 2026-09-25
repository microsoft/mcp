// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Compute.Utilities;
using Xunit;

namespace Azure.Mcp.Tools.Compute.Tests;

public class ComputeUtilitiesTests
{
    [Theory]
    // Known Windows aliases (prefix-based matching)
    [InlineData("Win2022Datacenter", "windows")]
    [InlineData("Win11Pro", "windows")]
    [InlineData("Win10Pro", "windows")]
    [InlineData("Win2022Datacenter1P", "windows")]
    // Windows URN / offer names (substring matching on "windows")
    [InlineData("MicrosoftWindowsServer:WindowsServer2022:2022-datacenter-azure-edition:latest", "windows")]
    [InlineData("MicrosoftWindowsDesktop:windows-11:win11-22h2-pro:latest", "windows")]
    // Visual Studio on Windows — "win" only appears inside a SKU token (not at string start, no "windows" anywhere)
    [InlineData("MicrosoftVisualStudio:visualstudio2022:vs-2022-comm-latest-win11-n-gen2:latest", "windows")]
    [InlineData("MicrosoftVisualStudio:visualstudio2022:vs-2022-ent-latest-win10-n-gen2:latest", "windows")]
    public void DetermineOsType_WindowsImages_ReturnsWindows(string image, string expected)
    {
        Assert.Equal(expected, ComputeUtilities.DetermineOsType(null, image));
    }

    [Theory]
    // Linux aliases
    [InlineData("Ubuntu2604", "linux")]
    [InlineData("Ubuntu2404", "linux")]
    [InlineData("Debian12", "linux")]
    [InlineData("RHEL9", "linux")]
    // Image names that contain "win" as a non-leading substring — original bug false-positive guard
    [InlineData("twin-ubuntu-lts", "linux")]     // "twin" does not start with "win"
    [InlineData("swingbench-linux", "linux")]    // "swingbench" does not start with "win"
    // Linux URN
    [InlineData("Canonical:ubuntu-24_04-lts:server:latest", "linux")]
    public void DetermineOsType_LinuxImages_ReturnsLinux(string image, string expected)
    {
        Assert.Equal(expected, ComputeUtilities.DetermineOsType(null, image));
    }

    [Theory]
    [InlineData("windows", "windows")]
    [InlineData("linux", "linux")]
    [InlineData("Windows", "Windows")]
    [InlineData("Linux", "Linux")]
    public void DetermineOsType_ExplicitOsType_ReturnsExplicitValue(string osType, string expected)
    {
        // Explicit osType overrides image-based detection
        Assert.Equal(expected, ComputeUtilities.DetermineOsType(osType, "Ubuntu2404"));
    }

    [Fact]
    public void DetermineOsType_NullImageAndNoOsType_DefaultsToLinux()
    {
        Assert.Equal("linux", ComputeUtilities.DetermineOsType(null, null));
    }

    [Fact]
    public void DetermineOsType_EmptyImageAndNoOsType_DefaultsToLinux()
    {
        Assert.Equal("linux", ComputeUtilities.DetermineOsType(null, ""));
    }
}
