// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using AzureMcp.Core.Options;

namespace AzureMcp.Communication.Options;

public class BaseCommunicationOptions : GlobalOptions
{
    [JsonPropertyName(CommunicationOptionDefinitions.ConnectionStringName)]
    public string? ConnectionString { get; set; }
}