// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Core.Models.Resource;

public class GenericResourceInfo(string name, string id, string type, string location)
{
    public string Name { get; set; } = name;
    public string Id { get; set; } = id;
    public string Type { get; set; } = type;
    public string Location { get; set; } = location;
}
