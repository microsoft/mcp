// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.EventGrid.Options.Topic;

public class TopicCreateOptions : BaseEventGridOptions
{
    public string? Topic { get; set; }
    public string? Location { get; set; }
}
