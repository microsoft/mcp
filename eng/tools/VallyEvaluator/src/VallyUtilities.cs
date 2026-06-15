using McpToolEvaluator.Core.Models;
using VallyEvaluator.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace VallyEvaluator;

internal class VallyUtilities
{
    private static readonly System.Text.RegularExpressions.Regex AngleBracketPlaceholderRegex =
        new("<[^<>\\s]+>", System.Text.RegularExpressions.RegexOptions.Compiled);

    internal static readonly ISerializer Serializer =
        new StaticSerializerBuilder(new VallyYamlStaticContext())
            .EnsureRoundtrip()
            .WithIndentedSequences()
            .WithNamingConvention(HyphenatedNamingConvention.Instance)
            .Build();

    internal static readonly Dictionary<string, string> Replacements = new()
    {
        { "<service-name>", "Azure Monitor" },
        { "<agent-name>", "Agent Tester" },
        { "<query>", "pricing information" },
        { "<search_term>", "agentic" },
        { "<index-name>", "contents" },
        { "<file_path>", "C:\\test.md" },
        { "<endpoint-id>", "http://foo.com" },
        { "<key_name>", "test-key" },
        { "<subscription>", "00000000-0000-0000-0000-000000000000" },
        { "<source-name>", "my-knowledge-source" },
        { "<endpoint>", "https://myopenai.openai.azure.com" },
        { "<connection_string>", "Server=tcp:myserver.database.windows.net;Database=mydb" },
        { "<database_name>", "sample-database" },
        { "<app_name>", "my-web-app" },
        { "<resource_group>", "my-resource-group" },
        { "<resource_group_name>", "my-resource-group" },
        { "<database_server>", "mydbserver" },
        { "<webapp>", "my-webapp" },
        { "<detector_name>", "availability-detector" },
        { "<start_time>", "2024-01-01T00:00:00Z" },
        { "<end_time>", "2024-01-31T23:59:59Z" },
        { "<interval>", "1h" },
        { "<app>", "my-app" },
        { "<deployment-id>", "deployment-123" },
        { "<setting-name>", "APP_SETTING_KEY" },
        { "<setting-value>", "APP_SETTING_VALUE" },
        { "<resource_id>", "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/my-rg/providers/Microsoft.Compute/virtualMachines/myvm" },
        { "<location>", "eastus" },
        { "<resource_type>", "Microsoft.Compute/virtualMachines" },
        { "<vault_name>", "my-backup-vault" },
        { "<job_id>", "job-12345678" },
        { "<policy_name>", "daily-backup-policy" },
        { "<item_name>", "protected-vm-001" },
        { "<datasource_id>", "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/my-rg/providers/Microsoft.Compute/virtualMachines/myvm" },
        { "<key_vault_uri>", "https://mykeyvault.vault.azure.net" },
        { "<identity_id>", "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/my-rg/providers/Microsoft.ManagedIdentity/userAssignedIdentities/myidentity" },
        { "<resource_guard_id>", "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/my-rg/providers/Microsoft.DataProtection/resourceGuards/myguard" },
        { "<account_name>", "mystorageaccount" },
        { "<email-address>", "user@example.com" },
        { "<email-address-1>", "user1@example.com" },
        { "<email-address-2>", "user2@example.com" },
        { "<subject>", "Important Notification" },
        { "<sender-name>", "Azure Notification Service" },
        { "<phone-number>", "+1234567890" },
        { "<phone-number-1>", "+1234567890" },
        { "<phone-number-2>", "+0987654321" },
        { "<vm-name>", "my-virtual-machine" },
        { "<resource-group-name>", "my-resource-group" },
        { "<instance-id>", "0" },
        { "<vmss-name>", "my-scale-set" },
        { "<disk-name>", "my-disk" },
        { "<resource-group>", "my-resource-group" },
        { "<snapshot-resource-id>", "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/my-rg/providers/Microsoft.Compute/snapshots/mysnapshot" },
        { "<blob-uri>", "https://mystorageaccount.blob.core.windows.net/vhds/mydisk.vhd" },
        { "<disk-encryption-set-id>", "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/my-rg/providers/Microsoft.Compute/diskEncryptionSets/mydes" },
        { "<image-version-resource-id>", "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/my-rg/providers/Microsoft.Compute/galleries/mygallery/images/myimage/versions/1.0.0" },
        { "<disk-access-resource-id>", "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/my-rg/providers/Microsoft.Compute/diskAccesses/mydiskaccess" },
        { "<ledger_name>", "my-confidential-ledger" },
        { "<collection_id>", "audit-logs" },
        { "<transaction_id>", "1.23" },
        { "<container_name>", "items" },
        { "<document_id>", "doc-12345" },
        { "<partition_key>", "category-A" },
        { "<search_property>", "title" },
        { "<search_phrase>", "azure" },
        { "<vector_property>", "embedding" },
        { "<deployment>", "text-embedding-ada-002" },
        { "<count>", "10" },
        { "<embedding_dimensions>", "1536" },
        { "<properties_to_select>", "id, title, description" },
        { "<sample_size>", "100" },
        { "<cluster_name>", "my-kusto-cluster" },
        { "<table_name>", "events" },
        { "<server>", "myserver" },
        { "<database>", "mydatabase" },
        { "<table>", "mytable" },
        { "<topic_name>", "my-event-grid-topic" },
        { "<event_schema>", "EventGridSchema" },
        { "<event_data>", "{\"id\":\"1\",\"subject\":\"test\",\"data\":{\"message\":\"hello\"},\"eventType\":\"Test.Event\",\"eventTime\":\"2024-01-01T00:00:00Z\"}" },
        { "<consumer_group_name>", "my-consumer-group" },
        { "<event_hub_name>", "my-event-hub" },
        { "<namespace_name>", "my-eventhub-namespace" },
        { "<file_share_name>", "my-file-share" },
        { "<snapshot_id>", "snapshot-12345" },
        { "<connection_name>", "my-private-endpoint-connection" },
        { "<function_app_name>", "my-function-app" },
        { "<key_vault_account_name>", "my-key-vault" },
        { "<setting_name>", "AllowKeyManagement" },
        { "<certificate_name>", "my-certificate" },
        { "<key_type>", "RSA" },
        { "<secret_name>", "my-secret" },
        { "<secret_value>", "super-secret-value" },
        { "<cluster-name>", "my-aks-cluster" },
        { "<nodepool-name>", "nodepool1" },
        { "<test-url>", "https://example.com/api" },
        { "<sample-name>", "Sample Load Test" },
        { "<test-id>", "test-12345" },
        { "<load-test-resource>", "my-loadtest-resource" },
        { "<test-resource>", "my-loadtest-resource" },
        { "<testrun-id>", "testrun-67890" },
        { "<load-testing-resource>", "my-loadtest-resource" },
        { "<display-name>", "My Test Run" },
        { "<description>", "Load test for API endpoints" },
        { "<filesystem_name>", "my-lustre-fs" },
        { "<filesystem_size>", "8000" },
        { "<sku>", "AMLFS-Durable-Premium-125" },
        { "<subnet_id>", "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/my-rg/providers/Microsoft.Network/virtualNetworks/myvnet/subnets/mysubnet" },
        { "<zone>", "1" },
        { "<maintenance_window_day>", "Sunday" },
        { "<maintenance_window_time>", "03:00" },
        { "<subscription_name>", "My Azure Subscription" },
        { "<job_name>", "job-12345" },
        { "<product_name>", "Azure SQL Database" },
        { "<publisher_name>", "Microsoft" },
        { "<migrate-project-name>", "my-migrate-project" },
        { "<resource_name>", "my-resource" },
        { "<entity_id>", "entity-12345" },
        { "<health_model_name>", "availability-health-model" },
        { "<resource_path>", "/onboarding/getting-started.md" },
        { "<session_id>", "session-12345" },
        { "<completion_note>", "Completed previous step successfully" },
        { "<workspace_path>", "C:\\projects\\myapp" },
        { "<findings_json>", "{\"telemetry\":{\"traces\":10,\"metrics\":5}}" },
        { "<enhancement_keys>", "[\"enable-distributed-tracing\", \"add-custom-metrics\"]" },
        { "<aggregation_type>", "Average" },
        { "<metric_name>", "requests/duration" },
        { "<time_period>", "24 hours" },
        { "<workspace_name>", "my-log-analytics-workspace" },
        { "<webtest_resource_name>", "my-web-test" },
        { "<appinsights_component>", "my-appinsights" },
        { "<resource_types>", "Microsoft.Compute/virtualMachines,Microsoft.Storage/storageAccounts" },
        { "<region>", "eastus" },
        { "<sku_name>", "Premium" },
        { "<storage_account_name>", "mystorageaccount" },
        { "<vm_name>", "my-vm" },
        { "<subscription_id>", "00000000-0000-0000-0000-000000000000" },
        { "<bicep_template>", "resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = { name: 'mystorageaccount' }" },
        { "<service_bus_name>", "my-servicebus" },
        { "<queue_name>", "my-queue" },
        { "<node_name>", "node-0" },
        { "<node_name_1>", "node-0" },
        { "<node_name_2>", "node-1" },
        { "<node_type_name>", "primary" },
        { "<signalr_name>", "my-signalr" },
        { "<server_name>", "myserver" },
        { "<new_database_name>", "renamed-database" },
        { "<admin_user>", "sqladmin" },
        { "<rule_name>", "allow-office-ip" },
        { "<start_ip>", "1.2.3.4" },
        { "<end_ip>", "1.2.3.10" },
        { "<agent_name>", "my-sre-agent" },
        { "<name>", "sub-agent-1" },
        { "<tool_name>", "custom-tool-1" },
        { "<skill_name>", "deploy-infrastructure" },
        { "<connector_name>", "kusto-connector-1" },
        { "<hook_name>", "alert-hook" },
        { "<thread_id>", "thread-12345" },
        { "<issue>", "High CPU usage on VM" },
        { "<task_id>", "task-67890" },
        { "<title>", "Investigate database timeout errors" },
        { "<filter>", "severity='critical'" },
        { "<handler>", "incident-response-playbook" },
        { "<topic>", "getting-started" },
        { "<text>", "how to configure monitoring" },
        { "<requirements>", "Multi-region deployment with automated failover" },
        { "<prompt_name>", "diagnose-performance" },
        { "<account>", "mystorageaccount" },
        { "<container>", "mycontainer" },
        { "<blob>", "myfile.txt" },
        { "<local-file-path>", "C:\\temp\\upload.txt" },
        { "<prefix>", "logs/" },
        { "<server-name>", "myserver" },
        { "<syncgroup-name>", "my-sync-group" },
        { "<endpoint-name>", "cloud-endpoint-1" },
        { "<share-name>", "my-file-share" },
        { "<local-path>", "D:\\SyncData" },
        { "<path>", "/documents" },
        { "<registry_name>", "mycontainerregistry" },
        { "<hostpool_name>", "my-hostpool" },
        { "<sessionhost_name>", "session-host-1" },
        { "<workbook_name>", "my-workbook" },
        { "<workbook_resource_id>", "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/my-rg/providers/Microsoft.Insights/workbooks/workbook-12345" },
    };

    public static async Task WritePromptsAsync(List<TestPrompt> prompts,
        string outputFile,
        string environment = "development",
        bool force = false)
    {
        var stimuli = new List<Stimulus>();
        for (int i = 0; i < prompts.Count; i++)
        {
            var p = prompts[i];
            var graders = new List<StimulusGraderConfig>()
            {
                GetToolCallGrader(p.Namespace, p.Tool)
            };
            var stimulus = new Stimulus
            {
                Name = $"{p.Namespace} evaluation {i}",
                Prompt = p.Prompt,
                Environment = environment,
                Graders = graders
            };

            stimuli.Add(stimulus);
        }

        var section = prompts[0].Section;
        var evaluation = new Evaluation
        {
            Name = $"{section} evaluations",
            Description = "Evaluation of prompts in the section " + section,
            Stimuli = stimuli
        };

        var serialized = Serializer.Serialize(evaluation);

        if (File.Exists(outputFile))
        {
            if (!force)
            {
                throw new InvalidOperationException($"Output file {outputFile} already exists.");
            }
            else
            {
                File.Delete(outputFile);
            }
        }

        await File.WriteAllTextAsync(outputFile, serialized);
    }

    internal static StimulusGraderConfig GetToolCallGrader(string toolName, string toolCommand)
    {
        var commandArgsEntry = new GraderConfigEntryPair
        {
            Name = toolName,
        };
        commandArgsEntry.Args.Add("command", toolCommand);

        var graderConfig = new GraderConfigEntry()
        {
            Required = [commandArgsEntry]
        };

        return new StimulusGraderConfig
        {
            Type = "tool-calls",
            Config = graderConfig
        };
    }

    internal static string ReplacePromptPlaceholders(string prompt, Dictionary<string, string> replacements)
    {
        foreach (var kvp in replacements)
        {
            prompt = prompt.Replace($"{{{kvp.Key}}}", kvp.Value);
        }
        return prompt;
    }

    internal static string ReplaceAngleBracketPlaceholders(string text, Dictionary<string, string> replacements)
    {
        if (string.IsNullOrEmpty(text) || replacements.Count == 0)
        {
            return text;
        }

        return AngleBracketPlaceholderRegex.Replace(text, match =>
        {
            if (replacements.TryGetValue(match.Value, out var replacement))
            {
                return replacement;
            }

            var unwrappedKey = match.Value[1..^1];
            return replacements.TryGetValue(unwrappedKey, out replacement)
                ? replacement
                : match.Value;
        });
    }
}
