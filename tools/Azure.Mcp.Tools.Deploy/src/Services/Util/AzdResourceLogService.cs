using Azure.Core;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace Azure.Mcp.Tools.Deploy.Services.Util;

public static class AzdResourceLogService
{
    private const string AzureYamlFileName = "azure.yaml";

    public static async Task<string> GetAzdResourceLogsAsync(
        TokenCredential credential,
        string workspaceFolder,
        string azdEnvName,
        string subscriptionId,
        int? limit = null)
    {
        var toolErrorLogs = new List<string>();
        var appLogs = new List<string>();

        try
        {
            var azdAppLogRetriever = new AzdAppLogRetriever(credential, subscriptionId, azdEnvName);
            await azdAppLogRetriever.InitializeAsync();
            await azdAppLogRetriever.GetLogAnalyticsWorkspacesInfoAsync();

            var services = GetServicesFromAzureYaml(workspaceFolder);

            foreach (var (serviceName, service) in services)
            {
                try
                {
                    if (service.Host != null)
                    {
                        var resourceType = ResourceTypeExtensions.GetResourceTypeFromHost(service.Host);
                        var logs = await azdAppLogRetriever.QueryAppLogsAsync(resourceType, serviceName, limit);
                        appLogs.Add(logs);
                    }
                }
                catch (Exception ex)
                {
                    toolErrorLogs.Add($"Error finding app logs for service {serviceName}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            toolErrorLogs.Add(ex.Message);
        }

        if (appLogs.Count > 0)
        {
            return $"App logs retrieved:\n{string.Join("\n\n", appLogs)}";
        }

        if (toolErrorLogs.Count > 0)
        {
            return $"Error during retrieval of app logs of azd project:\n{string.Join("\n", toolErrorLogs)}";
        }

        return "No logs found.";
    }

    private static Dictionary<string, Service> GetServicesFromAzureYaml(string workspaceFolder)
    {
        var azureYamlPath = Path.Combine(workspaceFolder, AzureYamlFileName);

        if (!File.Exists(azureYamlPath))
        {
            throw new FileNotFoundException($"Azure YAML file not found at {azureYamlPath}");
        }

        var yamlContent = File.ReadAllText(azureYamlPath);

        using var stringReader = new StringReader(yamlContent);
        var parser = new Parser(stringReader);

        return ParseAzureYamlServices(parser);
    }

    private static Dictionary<string, Service> ParseAzureYamlServices(Parser parser)
    {
        var result = new Dictionary<string, Service>();

        parser.Consume<StreamStart>();

        parser.Consume<DocumentStart>();

        parser.Consume<MappingStart>();

        while (parser.Accept<MappingEnd>(out _) == false)
        {
            var key = parser.Consume<Scalar>().Value;

            if (key == "services")
            {
                parser.Consume<MappingStart>();

                while (parser.Accept<MappingEnd>(out _) == false)
                {
                    var serviceName = parser.Consume<Scalar>().Value;

                    parser.Consume<MappingStart>();

                    string? host = null;
                    string? project = null;
                    string? language = null;

                    while (parser.Accept<MappingEnd>(out _) == false)
                    {
                        var propertyKey = parser.Consume<Scalar>().Value;
                        // Only accept properties host, project, and language which are scalars
                        if (parser.Accept<Scalar>(out _))
                        {
                            var propertyValue = parser.Consume<Scalar>().Value;
                            switch (propertyKey)
                            {
                                case "host":
                                    host = propertyValue;
                                    break;
                                case "project":
                                    project = propertyValue;
                                    break;
                                case "language":
                                    language = propertyValue;
                                    break;
                            }
                        }
                        else
                        {
                            SkipValue(parser);
                        }
                    }

                    parser.Consume<MappingEnd>();

                    result[serviceName] = new Service(
                        Host: host,
                        Project: project,
                        Language: language
                    );
                }

                parser.Consume<MappingEnd>();
            }
            else
            {
                SkipValue(parser);
            }
        }

        if (result.Count == 0)
        {
            throw new InvalidOperationException("No services section found in azure.yaml");
        }

        return result;
    }

    private static void SkipValue(Parser parser)
    {
        if (parser.Accept<Scalar>(out _))
        {
            parser.Consume<Scalar>();
        }
        else if (parser.Accept<MappingStart>(out _))
        {
            parser.Consume<MappingStart>();
            while (!parser.Accept<MappingEnd>(out _))
            {
                SkipValue(parser);
                SkipValue(parser);
            }
            parser.Consume<MappingEnd>();
        }
        else if (parser.Accept<SequenceStart>(out _))
        {
            parser.Consume<SequenceStart>();
            while (!parser.Accept<SequenceEnd>(out _))
            {
                SkipValue(parser);
            }
            parser.Consume<SequenceEnd>();
        }
    }
}

public record Service(
    string? Host = null,
    string? Project = null,
    string? Language = null
);
