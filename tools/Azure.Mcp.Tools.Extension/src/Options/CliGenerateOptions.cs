using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Extension.Options;

public class CliGenerateOptions : GlobalOptions
{
    [JsonPropertyName(ExtensionOptionDefinitions.CliGenerate.IntentName)]
    public string? Intent { get; set; }

    [JsonPropertyName(ExtensionOptionDefinitions.CliGenerate.CliTypeName)]
    public string? CliType { get; set; }
}
