// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using System.Text;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Attributes;
using Azure.Mcp.Tests.Client.Helpers;
using Azure.Mcp.Tests.Generated.Models;
using Azure.Mcp.Tests.Helpers;
using Xunit;

namespace Azure.Mcp.Core.LiveTests.RecordingFramework;

/// <summary>
/// Harness for testing RecordedCommandTestsBase functionality. Intended for proper abstraction of livetest settings etc to allow both record and playback modes in the same test for full roundtrip testing.
/// </summary>
/// <param name="output"></param>
/// <param name="fixture"></param>
internal sealed class RecordedCommandTestHarness(ITestOutputHelper output, TestProxyFixture fixture) : RecordedCommandTestsBase(output, fixture)
{
    public TestMode DesiredMode { get; set; } = TestMode.Record;

    public IReadOnlyDictionary<string, string> Variables => TestVariables;

    public string GetRecordingAbsolutePath(string displayName)
    {
        var sanitized = RecordingPathResolver.Sanitize(displayName);
        var relativeDirectory = _pathResolver.GetSessionDirectory(GetType(), variantSuffix: null)
            .Replace('/', Path.DirectorySeparatorChar);
        var fileName = RecordingPathResolver.BuildFileName(sanitized, IsAsync, VersionQualifier);
        var absoluteDirectory = Path.Combine(_pathResolver.RepositoryRoot, relativeDirectory);
        Directory.CreateDirectory(absoluteDirectory);
        return Path.Combine(absoluteDirectory, fileName);
    }

    protected override async ValueTask LoadSettingsAsync()
    {
        await base.LoadSettingsAsync().ConfigureAwait(false);

        Settings.TestMode = DesiredMode;
        Settings.ResourceBaseName = string.IsNullOrWhiteSpace(Settings.ResourceBaseName)
            ? "RecordedHarness"
            : Settings.ResourceBaseName;
        Settings.SettingsDirectory = string.IsNullOrWhiteSpace(Settings.SettingsDirectory)
            ? _pathResolver.RepositoryRoot
            : Settings.SettingsDirectory;

        TestMode = DesiredMode;
    }

    public void ResetVariables()
    {
        TestVariables.Clear();
    }

    [CustomMatcher(IgnoreQueryOrdering = true, CompareBodies = true)]
    public void PerTestMatcherAttributeAppliesWhenPresent()
    {
        // Marker method used so that RecordedCommandTestsBase can locate the CustomMatcherAttribute via reflection.
    }
}
