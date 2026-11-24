// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Storage.Options.Table;

public class TableListOptions : ICursorOption
{
    [Option(Description = ICursorOption.CursorDescription)]
    public string? Cursor { get; set; }

    [Option(Description = "The name of the Azure Storage account. This is the unique name you chose for your storage account (e.g., 'mystorageaccount').")]
    public required string Account { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

}
