// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Nodes;
using ModelContextProtocol.Client;

namespace Azure.Mcp.Server.Perf;

internal static class EnumParameterMeasurement
{
    internal static async Task RunAsync(string executable, string samplePath, string outputPath, string? baselinePath)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(2));
        var cancellationToken = timeout.Token;
        var sample = await ReadObjectAsync(samplePath, cancellationToken);
        var selected = sample["tools"]?.AsArray() ?? throw new InvalidDataException("The sample must contain a tools array.");
        var names = selected.Select(entry => entry?["name"]?.GetValue<string>()
            ?? throw new InvalidDataException("Every sample tool must have a name.")).ToArray();
        var baseline = baselinePath is null ? null : await ReadObjectAsync(baselinePath, cancellationToken);
        var baselineTools = baseline?["tools"]?.AsArray().ToDictionary(
            entry => entry!["name"]!.GetValue<string>(), entry => entry!.AsObject(), StringComparer.Ordinal);

        var transport = new StdioClientTransport(new()
        {
            Name = "enum-parameter-measurement",
            Command = executable,
            Arguments = ["server", "start", "--mode", "all", "--tool", .. names],
        });
        await using var client = await McpClient.CreateAsync(transport, cancellationToken: cancellationToken);
        var tools = (await client.ListToolsAsync(cancellationToken: cancellationToken))
            .ToDictionary(tool => tool.Name, StringComparer.Ordinal);

        var measurements = new JsonArray();
        foreach (var entry in selected)
        {
            var name = entry!["name"]!.GetValue<string>();
            if (!tools.TryGetValue(name, out var tool))
            {
                throw new InvalidDataException($"Selected tool '{name}' was not returned by tools/list.");
            }

            var definition = JsonSerializer.SerializeToNode(tool.ProtocolTool, EnumParameterJsonContext.Default.Tool)!;
            var call = new JsonObject
            {
                ["name"] = name,
                ["arguments"] = entry["arguments"]?.DeepClone()
                    ?? throw new InvalidDataException($"Selected tool '{name}' must have sample arguments."),
            };
            var (bytes, tokens, _) = PerfTokenizer.Measure(Serialize(definition));
            var measurement = new JsonObject
            {
                ["name"] = name,
                ["definition"] = definition,
                ["definitionBytes"] = bytes,
                ["definitionTokens"] = tokens,
                ["call"] = call,
                ["callTokens"] = PerfTokenizer.Measure(Serialize(call)).ExactTokens,
            };

            if (baselineTools is not null)
            {
                if (!baselineTools.TryGetValue(name, out var original))
                {
                    throw new InvalidDataException($"Baseline does not contain selected tool '{name}'.");
                }

                var enumOnly = original["definition"]!.DeepClone();
                CopyEnumConstraints(definition, enumOnly);
                measurement["baselineDefinitionTokens"] = original["definitionTokens"]!.DeepClone();
                measurement["enumOnlyDefinitionTokens"] = PerfTokenizer.Measure(Serialize(enumOnly)).ExactTokens;
                measurement["baselineCallTokens"] = original["callTokens"]!.DeepClone();
                if (!JsonNode.DeepEquals(original["call"], call))
                {
                    throw new InvalidDataException($"Sample call for '{name}' differs from the baseline.");
                }
            }

            measurements.Add(measurement);
        }

        var report = new JsonObject
        {
            ["encoding"] = "o200k_base",
            ["method"] = "Local tools/list definitions and synthetic call parameters; no tools executed and no model inference.",
            ["tools"] = measurements,
        };
        await File.WriteAllTextAsync(outputPath, Serialize(report), cancellationToken);
        foreach (var measurement in measurements)
        {
            Console.WriteLine($"{measurement!["name"]}: definition={measurement["definitionTokens"]}, call={measurement["callTokens"]} tokens");
        }
    }

    private static async Task<JsonObject> ReadObjectAsync(string path, CancellationToken cancellationToken) =>
        JsonNode.Parse(await File.ReadAllTextAsync(path, cancellationToken))?.AsObject()
            ?? throw new InvalidDataException($"'{path}' must contain a JSON object.");

    private static string Serialize(JsonNode value) =>
        value.ToJsonString(EnumParameterJsonContext.Default.Options);

    private static void CopyEnumConstraints(JsonNode source, JsonNode target)
    {
        if (source is not JsonObject sourceObject || target is not JsonObject targetObject)
        {
            return;
        }

        foreach (var (key, value) in sourceObject)
        {
            if (key == "enum")
            {
                targetObject[key] = value!.DeepClone();
            }
            else if (value is not null && targetObject[key] is { } targetValue)
            {
                CopyEnumConstraints(value, targetValue);
            }
        }
    }
}
