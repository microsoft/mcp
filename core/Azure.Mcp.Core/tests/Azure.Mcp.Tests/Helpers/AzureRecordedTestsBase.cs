// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Microsoft.Mcp.Tests.Generated.Models;
using Xunit;

namespace Azure.Mcp.Tests.Client.Helpers;

public class AzureRecordedTestsBase(ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture<AzureLiveTestSettings> liveServerFixture)
    : RecordedCommandTestsBase<AzureLiveTestSettings>(output, fixture, liveServerFixture)
{
    protected override void PopulateDefaultSanitizers()
    {
        if (EnableDefaultSanitizerAdditions)
        {
            GeneralRegexSanitizers.Add(new GeneralRegexSanitizer(new GeneralRegexSanitizerBody()
            {
                Regex = Settings.ResourceBaseName,
                Value = "Sanitized",
            }));
            GeneralRegexSanitizers.Add(new GeneralRegexSanitizer(new GeneralRegexSanitizerBody()
            {
                Regex = Settings.SubscriptionId,
                Value = EmptyGuid,
            }));
        }
    }
}
