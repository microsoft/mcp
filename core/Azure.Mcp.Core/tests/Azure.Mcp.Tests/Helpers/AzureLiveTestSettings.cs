// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tests.Client.Helpers;

public class AzureLiveTestSettings : LiveTestSettingsBase
{
    public override string ServerExecutableName => "azmcp";

    public string SubscriptionId { get; set; } = string.Empty;
    public string SubscriptionName { get; set; } = string.Empty;
    public string ResourceGroupName { get; set; } = string.Empty;
    public string ResourceBaseName { get; set; } = string.Empty;

    protected override async Task InitializeAsync()
    {
        await base.InitializeAsync().ConfigureAwait(false);

        if (TestMode == Tests.Helpers.TestMode.Playback)
        {
            SubscriptionId = "00000000-0000-0000-0000-000000000000";
            SubscriptionName = "Sanitized";
            ResourceBaseName = "Sanitized";
            ResourceGroupName = "Sanitized";
        }
    }
}
