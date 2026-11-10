// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Net.ServerSentEvents;
using System.Text.Json;
using System.Text.Json.Nodes;
using ModelContextProtocol.Protocol;
using Xunit;

namespace Fabric.Mcp.Server.Tests.Infrastructure;

public class ServerStartupTests
{
    private const string StatelessProtocolVersion = "2026-07-28";

    /// <summary>
    /// Builds a <see cref="StringContent"/> for a 2026-07-28 stateless JSON-RPC request
    /// containing the required <c>_meta</c> envelope fields.
    /// </summary>
    private static StringContent CreateStateless2026RequestContent(string method, int id = 1)
    {
        var body = new JsonObject
        {
            ["jsonrpc"] = "2.0",
            ["id"] = id,
            ["method"] = method,
            ["params"] = new JsonObject
            {
                ["_meta"] = new JsonObject
                {
                    ["io.modelcontextprotocol/protocolVersion"] = StatelessProtocolVersion,
                    ["io.modelcontextprotocol/clientInfo"] = new JsonObject { ["name"] = "test-client", ["version"] = "1.0" },
                    ["io.modelcontextprotocol/clientCapabilities"] = new JsonObject()
                }
            }
        };
        return new StringContent(body.ToJsonString(), System.Text.Encoding.UTF8, "application/json");
    }

    [Theory]
    [InlineData(false, null, false)]
    [InlineData(true, null, false)]
    [InlineData(true, "core", false)]
    [InlineData(true, "jobs", false)]
    [InlineData(true, "core,jobs", false)]
    [InlineData(true, "core", true)]
    [InlineData(true, "jobs", true)]
    [InlineData(true, "core,jobs", true)]
    public async Task Server_Should_List_Tools_Over_Http_Root_Endpoint(bool listAllTools, string? namespaces, bool readOnly)
    {
        var exeName = OperatingSystem.IsWindows() ? "fabmcp.exe" : "fabmcp";
        var fabmcpPath = Path.Combine(AppContext.BaseDirectory, exeName);

        Assert.True(File.Exists(fabmcpPath), $"Executable not found at {fabmcpPath}");

        var port = GetAvailablePort();
        var processStartInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = fabmcpPath,
            Arguments = $"server start --transport http --dangerously-disable-http-incoming-auth{(listAllTools ? " --mode all" : "")}{NamespaceArguments(namespaces)}{(readOnly ? " --read-only" : "")}",
            UseShellExecute = false,
            RedirectStandardInput = false,
            RedirectStandardOutput = false,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        processStartInfo.Environment["ASPNETCORE_URLS"] = $"http://127.0.0.1:{port}";

        using var process = System.Diagnostics.Process.Start(processStartInfo);
        Assert.NotNull(process);

        var stderrBuilder = new System.Text.StringBuilder();
        process.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                stderrBuilder.AppendLine(e.Data);
            }
        };
        process.BeginErrorReadLine();

        try
        {
            using var client = new HttpClient();
            using var request = new HttpRequestMessage(HttpMethod.Post, $"http://127.0.0.1:{port}/")
            {
                Content = CreateStateless2026RequestContent(RequestMethods.ToolsList)
            };
            request.Headers.TryAddWithoutValidation("Accept", "application/json, text/event-stream");
            request.Headers.TryAddWithoutValidation("MCP-Protocol-Version", StatelessProtocolVersion);
            request.Headers.TryAddWithoutValidation("Mcp-Method", RequestMethods.ToolsList);
            request.Headers.TryAddWithoutValidation("Mcp-Name", RequestMethods.ToolsList);

            var response = await SendWithRetryAsync(client, request, TestContext.Current.CancellationToken);
            var content = await ReadToolResponseAsync(response, TestContext.Current.CancellationToken);

            var errorOutput = stderrBuilder.ToString();
            Assert.DoesNotContain("Unable to resolve service", errorOutput);
            Assert.DoesNotContain("InvalidOperationException", errorOutput);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("\"result\"", content, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("\"tools\"", content, StringComparison.OrdinalIgnoreCase);
            if (listAllTools)
            {
                AssertCoreAndJobsDiscovery(content, namespaces, localFiles: false, readOnly);
            }
        }
        finally
        {
            if (!process.HasExited)
            {
                process.Kill();
            }
        }
    }

    [Theory]
    [InlineData(false, null, false)]
    [InlineData(true, null, false)]
    [InlineData(true, "core", false)]
    [InlineData(true, "jobs", false)]
    [InlineData(true, "core,jobs", false)]
    [InlineData(true, "core", true)]
    [InlineData(true, "jobs", true)]
    [InlineData(true, "core,jobs", true)]
    public async Task Server_Should_List_Tools_Without_Initialize_And_Without_DI_Errors(bool listAllTools, string? namespaces, bool readOnly)
    {
        // Arrange
        var exeName = OperatingSystem.IsWindows() ? "fabmcp.exe" : "fabmcp";
        var fabmcpPath = Path.Combine(AppContext.BaseDirectory, exeName);

        Assert.True(File.Exists(fabmcpPath), $"Executable not found at {fabmcpPath}");

        var processStartInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = fabmcpPath,
            Arguments = (listAllTools ? "server start --mode all" : "server start") + NamespaceArguments(namespaces) + (readOnly ? " --read-only" : ""),
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using var process = System.Diagnostics.Process.Start(processStartInfo);
        Assert.NotNull(process);

        // Collect stderr asynchronously
        var stderrBuilder = new System.Text.StringBuilder();
        process.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                stderrBuilder.AppendLine(e.Data);
            }
        };
        process.BeginErrorReadLine();

        try
        {
            await Task.Delay(500, TestContext.Current.CancellationToken);

            var listToolsRequest = """
                {"jsonrpc":"2.0","id":1,"method":"tools/list","params":{}}
                """;
            await process.StandardInput.WriteLineAsync(listToolsRequest);
            await process.StandardInput.FlushAsync(TestContext.Current.CancellationToken);

            // Read response - should get valid JSON, not an exception
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var response = await process.StandardOutput.ReadLineAsync(cts.Token);

            // Check stderr for DI exceptions
            var errorOutput = stderrBuilder.ToString();
            Assert.DoesNotContain("Unable to resolve service", errorOutput);
            Assert.DoesNotContain("InvalidOperationException", errorOutput);

            // Verify we got a valid response
            Assert.NotNull(response);
            Assert.Contains("\"result\"", response, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("\"tools\"", response, StringComparison.OrdinalIgnoreCase);
            if (listAllTools)
            {
                AssertCoreAndJobsDiscovery(response, namespaces, localFiles: true, readOnly);
            }
        }
        finally
        {
            if (!process.HasExited)
            {
                process.Kill();
            }
        }
    }

    [Fact]
    public async Task Server_Should_Interop_With_Legacy_Initialize_Without_DI_Errors()
    {
        // Arrange
        var exeName = OperatingSystem.IsWindows() ? "fabmcp.exe" : "fabmcp";
        var fabmcpPath = Path.Combine(AppContext.BaseDirectory, exeName);

        Assert.True(File.Exists(fabmcpPath), $"Executable not found at {fabmcpPath}");

        var processStartInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = fabmcpPath,
            Arguments = "server start",
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using var process = System.Diagnostics.Process.Start(processStartInfo);
        Assert.NotNull(process);

        var stderrBuilder = new System.Text.StringBuilder();
        process.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                stderrBuilder.AppendLine(e.Data);
            }
        };
        process.BeginErrorReadLine();

        try
        {
            await Task.Delay(500, TestContext.Current.CancellationToken);

            var initRequest = """
                {"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2025-11-25","capabilities":{},"clientInfo":{"name":"test","version":"1.0"}}}
                """;
            await process.StandardInput.WriteLineAsync(initRequest);
            await process.StandardInput.FlushAsync(TestContext.Current.CancellationToken);

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var response = await process.StandardOutput.ReadLineAsync(cts.Token);

            var errorOutput = stderrBuilder.ToString();
            Assert.DoesNotContain("Unable to resolve service", errorOutput);
            Assert.DoesNotContain("InvalidOperationException", errorOutput);

            Assert.NotNull(response);
            Assert.Contains("\"result\"", response, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (!process.HasExited)
            {
                process.Kill();
            }
        }
    }

    private static string NamespaceArguments(string? namespaces) =>
        namespaces is null ? "" : string.Concat(namespaces.Split(',').Select(value => " --namespace " + value));

    private static void AssertCoreAndJobsDiscovery(string response, string? namespaces, bool localFiles, bool readOnly)
    {
        using var document = JsonDocument.Parse(response);
        var tools = document.RootElement.GetProperty("result").GetProperty("tools").EnumerateArray().ToArray();
        var names = tools.Select(tool => tool.GetProperty("name").GetString()).ToArray();
        Assert.Equal(names.Length, names.Distinct(StringComparer.Ordinal).Count());
        Assert.DoesNotContain("core_get-item-job-instance", names);
        Assert.DoesNotContain("core_get-item-schedule", names);
        Assert.DoesNotContain("core_delete-item-schedule", names);
        var expectedJobs = namespaces != "core";
        Assert.Equal(expectedJobs && !readOnly, names.Contains("jobs_cancel-item-job-instance"));
        Assert.Equal(expectedJobs && !readOnly, names.Contains("jobs_create-item-schedule"));
        Assert.Equal(expectedJobs && !readOnly, names.Contains("jobs_delete-item-schedule"));
        Assert.Equal(expectedJobs, names.Contains("jobs_get-item-job-instance"));
        Assert.Equal(expectedJobs, names.Contains("jobs_get-item-schedule"));
        Assert.Equal(expectedJobs, names.Contains("jobs_list-item-job-instances"));
        Assert.Equal(expectedJobs, names.Contains("jobs_list-item-schedules"));
        Assert.Equal(expectedJobs && !readOnly, names.Contains("jobs_run-on-demand-item-job"));
        Assert.Equal(expectedJobs && !readOnly, names.Contains("jobs_update-item-schedule"));
        Assert.Equal(namespaces != "jobs", names.Contains("core_get-item"));
        Assert.Equal(namespaces != "jobs" && !readOnly, names.Contains("core_add-workspace-role-assignment"));
        Assert.Equal(namespaces != "jobs" && !readOnly, names.Contains("core_create-folder"));
        Assert.Equal(namespaces != "jobs" && !readOnly, names.Contains("core_delete-folder"));
        Assert.Equal(namespaces != "jobs", names.Contains("core_get-folder"));
        Assert.Equal(namespaces != "jobs", names.Contains("core_get-workspace-role-assignment"));
        Assert.Equal(namespaces != "jobs", names.Contains("core_list-folders"));
        Assert.Equal(namespaces != "jobs", names.Contains("core_list-workspace-role-assignments"));
        Assert.Equal(namespaces != "jobs" && !readOnly, names.Contains("core_move-folder"));
        Assert.Equal(namespaces != "jobs" && !readOnly, names.Contains("core_move-item"));
        Assert.Equal(namespaces != "jobs" && !readOnly, names.Contains("core_update-folder"));
        Assert.DoesNotContain("jobs_create-folder", names);
        Assert.DoesNotContain("jobs_add-workspace-role-assignment", names);
        Assert.DoesNotContain("jobs_delete-folder", names);
        Assert.DoesNotContain("jobs_get-folder", names);
        Assert.DoesNotContain("jobs_get-workspace-role-assignment", names);
        Assert.DoesNotContain("jobs_list-folders", names);
        Assert.DoesNotContain("jobs_list-workspace-role-assignments", names);
        Assert.DoesNotContain("jobs_move-folder", names);
        Assert.DoesNotContain("jobs_move-item", names);
        Assert.DoesNotContain("jobs_update-folder", names);
        if (namespaces != "jobs" && !readOnly)
        {
            var addRole = Assert.Single(tools, tool => tool.GetProperty("name").GetString() == "core_add-workspace-role-assignment");
            var addRoleAnnotations = addRole.GetProperty("annotations");
            Assert.False(addRoleAnnotations.GetProperty("readOnlyHint").GetBoolean());
            Assert.False(addRoleAnnotations.GetProperty("idempotentHint").GetBoolean());
            Assert.False(addRoleAnnotations.GetProperty("destructiveHint").GetBoolean());
            Assert.False(addRoleAnnotations.GetProperty("openWorldHint").GetBoolean());
            var addRoleSchema = addRole.GetProperty("inputSchema");
            Assert.Equal(["principal", "role", "workspace-id"],
                addRoleSchema.GetProperty("required").EnumerateArray().Select(value => value.GetString()).Order());
            Assert.Equal(["principal", "role", "workspace-id"],
                addRoleSchema.GetProperty("properties").EnumerateObject().Select(property => property.Name).Order());
            Assert.Contains("exactly one POST", addRole.GetProperty("description").GetString());
            Assert.Contains("Members can grant Member or lower", addRole.GetProperty("description").GetString());
            Assert.Contains("EntireTenant explicitly grants access to all tenant users", addRole.GetProperty("description").GetString());
            var createFolder = Assert.Single(tools, tool => tool.GetProperty("name").GetString() == "core_create-folder");
            var annotations = createFolder.GetProperty("annotations");
            Assert.False(annotations.GetProperty("readOnlyHint").GetBoolean());
            Assert.False(annotations.GetProperty("idempotentHint").GetBoolean());
            Assert.False(annotations.GetProperty("destructiveHint").GetBoolean());
            Assert.False(annotations.GetProperty("openWorldHint").GetBoolean());
            var schema = createFolder.GetProperty("inputSchema");
            Assert.Equal(["display-name", "workspace-id"], schema.GetProperty("required").EnumerateArray().Select(value => value.GetString()).Order());
            Assert.Equal(["display-name", "parent-folder-id", "workspace-id"],
                schema.GetProperty("properties").EnumerateObject().Select(property => property.Name).Order());
            Assert.Contains("exactly one POST", createFolder.GetProperty("description").GetString());
            var deleteFolder = Assert.Single(tools, tool => tool.GetProperty("name").GetString() == "core_delete-folder");
            var deleteAnnotations = deleteFolder.GetProperty("annotations");
            Assert.False(deleteAnnotations.GetProperty("readOnlyHint").GetBoolean());
            Assert.True(deleteAnnotations.GetProperty("idempotentHint").GetBoolean());
            Assert.True(deleteAnnotations.GetProperty("destructiveHint").GetBoolean());
            Assert.False(deleteAnnotations.GetProperty("openWorldHint").GetBoolean());
            var deleteSchema = deleteFolder.GetProperty("inputSchema");
            Assert.Equal(["folder-id", "workspace-id"],
                deleteSchema.GetProperty("required").EnumerateArray().Select(value => value.GetString()).Order());
            Assert.Equal(["folder-id", "workspace-id"],
                deleteSchema.GetProperty("properties").EnumerateObject().Select(property => property.Name).Order());
            Assert.Contains("exactly one DELETE", deleteFolder.GetProperty("description").GetString());
            Assert.Contains("empty logical", deleteFolder.GetProperty("description").GetString());
            Assert.DoesNotContain("Preview", deleteFolder.GetProperty("description").GetString(), StringComparison.OrdinalIgnoreCase);
            var moveFolder = Assert.Single(tools, tool => tool.GetProperty("name").GetString() == "core_move-folder");
            var moveAnnotations = moveFolder.GetProperty("annotations");
            Assert.False(moveAnnotations.GetProperty("readOnlyHint").GetBoolean());
            Assert.True(moveAnnotations.GetProperty("idempotentHint").GetBoolean());
            Assert.True(moveAnnotations.GetProperty("destructiveHint").GetBoolean());
            Assert.False(moveAnnotations.GetProperty("openWorldHint").GetBoolean());
            var moveSchema = moveFolder.GetProperty("inputSchema");
            Assert.Equal(["folder-id", "workspace-id"],
                moveSchema.GetProperty("required").EnumerateArray().Select(value => value.GetString()).Order());
            Assert.Equal(["folder-id", "target-folder-id", "workspace-id"],
                moveSchema.GetProperty("properties").EnumerateObject().Select(property => property.Name).Order());
            Assert.False(moveSchema.GetProperty("properties").GetProperty("target-folder-id").TryGetProperty("default", out _));
            Assert.Contains("exactly one POST", moveFolder.GetProperty("description").GetString());
            Assert.DoesNotContain("Preview", moveFolder.GetProperty("description").GetString(), StringComparison.OrdinalIgnoreCase);
            var moveItem = Assert.Single(tools, tool => tool.GetProperty("name").GetString() == "core_move-item");
            var moveItemAnnotations = moveItem.GetProperty("annotations");
            Assert.False(moveItemAnnotations.GetProperty("readOnlyHint").GetBoolean());
            Assert.True(moveItemAnnotations.GetProperty("idempotentHint").GetBoolean());
            Assert.True(moveItemAnnotations.GetProperty("destructiveHint").GetBoolean());
            Assert.False(moveItemAnnotations.GetProperty("openWorldHint").GetBoolean());
            var moveItemSchema = moveItem.GetProperty("inputSchema");
            Assert.Equal(["item-id", "workspace-id"],
                moveItemSchema.GetProperty("required").EnumerateArray().Select(value => value.GetString()).Order());
            Assert.Equal(["item-id", "target-folder-id", "workspace-id"],
                moveItemSchema.GetProperty("properties").EnumerateObject().Select(property => property.Name).Order());
            Assert.False(moveItemSchema.GetProperty("properties").GetProperty("target-folder-id").TryGetProperty("default", out _));
            Assert.Contains("exactly one POST", moveItem.GetProperty("description").GetString());
            Assert.Contains("child items", moveItem.GetProperty("description").GetString());
            Assert.DoesNotContain("Preview", moveItem.GetProperty("description").GetString(), StringComparison.OrdinalIgnoreCase);
            var updateFolder = Assert.Single(tools, tool => tool.GetProperty("name").GetString() == "core_update-folder");
            var updateAnnotations = updateFolder.GetProperty("annotations");
            Assert.False(updateAnnotations.GetProperty("readOnlyHint").GetBoolean());
            Assert.True(updateAnnotations.GetProperty("idempotentHint").GetBoolean());
            Assert.True(updateAnnotations.GetProperty("destructiveHint").GetBoolean());
            Assert.False(updateAnnotations.GetProperty("openWorldHint").GetBoolean());
            var updateSchema = updateFolder.GetProperty("inputSchema");
            Assert.Equal(["display-name", "folder-id", "workspace-id"],
                updateSchema.GetProperty("required").EnumerateArray().Select(value => value.GetString()).Order());
            Assert.Equal(["display-name", "folder-id", "workspace-id"],
                updateSchema.GetProperty("properties").EnumerateObject().Select(property => property.Name).Order());
            Assert.Contains("exactly one PATCH", updateFolder.GetProperty("description").GetString());
            Assert.DoesNotContain("Preview", updateFolder.GetProperty("description").GetString(), StringComparison.OrdinalIgnoreCase);
        }
        if (namespaces != "jobs")
        {
            var getFolder = Assert.Single(tools, tool => tool.GetProperty("name").GetString() == "core_get-folder");
            var annotations = getFolder.GetProperty("annotations");
            Assert.True(annotations.GetProperty("readOnlyHint").GetBoolean());
            Assert.True(annotations.GetProperty("idempotentHint").GetBoolean());
            Assert.False(annotations.GetProperty("destructiveHint").GetBoolean());
            Assert.False(annotations.GetProperty("openWorldHint").GetBoolean());
            var schema = getFolder.GetProperty("inputSchema");
            Assert.Equal(["folder-id", "workspace-id"], schema.GetProperty("required").EnumerateArray().Select(value => value.GetString()).Order());
            Assert.Equal(["folder-id", "workspace-id"], schema.GetProperty("properties").EnumerateObject().Select(property => property.Name).Order());
            Assert.Contains("exactly one GET", getFolder.GetProperty("description").GetString());
            var folders = Assert.Single(tools, tool => tool.GetProperty("name").GetString() == "core_list-folders");
            Assert.True(folders.GetProperty("annotations").GetProperty("readOnlyHint").GetBoolean());
            Assert.True(folders.GetProperty("annotations").GetProperty("idempotentHint").GetBoolean());
            Assert.False(folders.GetProperty("annotations").GetProperty("destructiveHint").GetBoolean());
            Assert.False(folders.GetProperty("annotations").GetProperty("openWorldHint").GetBoolean());
            Assert.Equal("workspace-id", Assert.Single(folders.GetProperty("inputSchema").GetProperty("required").EnumerateArray()).GetString());
            var options = folders.GetProperty("inputSchema").GetProperty("properties");
            Assert.Equal(["continuation-token", "recursive", "root-folder-id", "workspace-id"],
                options.EnumerateObject().Select(property => property.Name).Order());
            Assert.Equal(["boolean", "null"], options.GetProperty("recursive").GetProperty("type").EnumerateArray().Select(value => value.GetString()));
            Assert.False(options.GetProperty("recursive").TryGetProperty("default", out _));
            var roles = Assert.Single(tools, tool => tool.GetProperty("name").GetString() == "core_list-workspace-role-assignments");
            Assert.True(roles.GetProperty("annotations").GetProperty("readOnlyHint").GetBoolean());
            Assert.True(roles.GetProperty("annotations").GetProperty("idempotentHint").GetBoolean());
            Assert.False(roles.GetProperty("annotations").GetProperty("destructiveHint").GetBoolean());
            Assert.False(roles.GetProperty("annotations").GetProperty("openWorldHint").GetBoolean());
            var roleSchema = roles.GetProperty("inputSchema");
            Assert.Equal("workspace-id", Assert.Single(roleSchema.GetProperty("required").EnumerateArray()).GetString());
            Assert.Equal(["continuation-token", "workspace-id"],
                roleSchema.GetProperty("properties").EnumerateObject().Select(property => property.Name).Order());
            Assert.Contains("exactly one GET", roles.GetProperty("description").GetString());
            Assert.Contains("Member or higher", roles.GetProperty("description").GetString());
            var assignment = Assert.Single(tools, tool => tool.GetProperty("name").GetString() == "core_get-workspace-role-assignment");
            Assert.True(assignment.GetProperty("annotations").GetProperty("readOnlyHint").GetBoolean());
            Assert.True(assignment.GetProperty("annotations").GetProperty("idempotentHint").GetBoolean());
            Assert.False(assignment.GetProperty("annotations").GetProperty("destructiveHint").GetBoolean());
            Assert.False(assignment.GetProperty("annotations").GetProperty("openWorldHint").GetBoolean());
            var assignmentSchema = assignment.GetProperty("inputSchema");
            Assert.Equal(["workspace-id", "workspace-role-assignment-id"],
                assignmentSchema.GetProperty("required").EnumerateArray().Select(value => value.GetString()).Order());
            Assert.Equal(["workspace-id", "workspace-role-assignment-id"],
                assignmentSchema.GetProperty("properties").EnumerateObject().Select(property => property.Name).Order());
            Assert.Contains("exactly one GET", assignment.GetProperty("description").GetString());
            Assert.Contains("Assignment and principal IDs are independent", assignment.GetProperty("description").GetString());
            Assert.Contains("Member or higher", assignment.GetProperty("description").GetString());
        }
        if (namespaces is not null)
        {
            Assert.All(names, name => Assert.Contains(name!.Split('_')[0], namespaces.Split(',')));
        }
        if (namespaces != "jobs" && !readOnly)
        {
            AssertRepresentativeCoreTools(response, localFiles);
            AssertListItemsTool(response);
        }
        if (expectedJobs)
        {
            if (!readOnly)
            {
                var delete = Assert.Single(tools, tool => tool.GetProperty("name").GetString() == "jobs_delete-item-schedule");
                Assert.False(delete.GetProperty("annotations").GetProperty("readOnlyHint").GetBoolean());
                Assert.True(delete.GetProperty("annotations").GetProperty("idempotentHint").GetBoolean());
                Assert.True(delete.GetProperty("annotations").GetProperty("destructiveHint").GetBoolean());
                Assert.False(delete.GetProperty("annotations").GetProperty("openWorldHint").GetBoolean());
                Assert.Equal(["item-id", "job-type", "schedule-id", "workspace-id"],
                    delete.GetProperty("inputSchema").GetProperty("required").EnumerateArray().Select(value => value.GetString()).Order());
                Assert.Equal(["item-id", "job-type", "schedule-id", "workspace-id"],
                    delete.GetProperty("inputSchema").GetProperty("properties").EnumerateObject().Select(property => property.Name).Order());
                Assert.Contains("Only HTTP 200 confirms deletion", delete.GetProperty("description").GetString());
                var create = Assert.Single(tools, tool => tool.GetProperty("name").GetString() == "jobs_create-item-schedule");
                Assert.False(create.GetProperty("annotations").GetProperty("readOnlyHint").GetBoolean());
                Assert.False(create.GetProperty("annotations").GetProperty("idempotentHint").GetBoolean());
                Assert.True(create.GetProperty("annotations").GetProperty("destructiveHint").GetBoolean());
                Assert.Equal(["configuration", "enabled", "item-id", "job-type", "workspace-id"],
                    create.GetProperty("inputSchema").GetProperty("required").EnumerateArray().Select(value => value.GetString()).Order());
                var createOptions = create.GetProperty("inputSchema").GetProperty("properties");
                Assert.Equal(["configuration", "enabled", "execution-data", "item-id", "job-type", "workspace-id"],
                    createOptions.EnumerateObject().Select(property => property.Name).Order());
                Assert.Equal("boolean", createOptions.GetProperty("enabled").GetProperty("type").GetString());
                Assert.False(createOptions.GetProperty("enabled").TryGetProperty("default", out _));
                var update = Assert.Single(tools, tool => tool.GetProperty("name").GetString() == "jobs_update-item-schedule");
                Assert.False(update.GetProperty("annotations").GetProperty("readOnlyHint").GetBoolean());
                Assert.False(update.GetProperty("annotations").GetProperty("idempotentHint").GetBoolean());
                Assert.True(update.GetProperty("annotations").GetProperty("destructiveHint").GetBoolean());
                Assert.Equal(["configuration", "enabled", "item-id", "job-type", "schedule-id", "workspace-id"],
                    update.GetProperty("inputSchema").GetProperty("required").EnumerateArray().Select(value => value.GetString()).Order());
                var updateOptions = update.GetProperty("inputSchema").GetProperty("properties");
                Assert.Equal(["configuration", "enabled", "execution-data", "item-id", "job-type", "schedule-id", "workspace-id"],
                    updateOptions.EnumerateObject().Select(property => property.Name).Order());
                Assert.Equal("boolean", updateOptions.GetProperty("enabled").GetProperty("type").GetString());
                Assert.False(updateOptions.GetProperty("enabled").TryGetProperty("default", out _));
                var cancel = Assert.Single(tools, tool => tool.GetProperty("name").GetString() == "jobs_cancel-item-job-instance");
                Assert.False(cancel.GetProperty("annotations").GetProperty("readOnlyHint").GetBoolean());
                Assert.True(cancel.GetProperty("annotations").GetProperty("idempotentHint").GetBoolean());
                Assert.True(cancel.GetProperty("annotations").GetProperty("destructiveHint").GetBoolean());
                Assert.Equal(["item-id", "job-instance-id", "workspace-id"], cancel.GetProperty("inputSchema").GetProperty("required").EnumerateArray()
                    .Select(value => value.GetString()).Order());
                Assert.Equal(["item-id", "job-instance-id", "workspace-id"], cancel.GetProperty("inputSchema").GetProperty("properties").EnumerateObject()
                    .Select(property => property.Name).Order());
                var run = Assert.Single(tools, tool => tool.GetProperty("name").GetString() == "jobs_run-on-demand-item-job");
                Assert.False(run.GetProperty("annotations").GetProperty("readOnlyHint").GetBoolean());
                Assert.False(run.GetProperty("annotations").GetProperty("idempotentHint").GetBoolean());
                Assert.True(run.GetProperty("annotations").GetProperty("destructiveHint").GetBoolean());
                Assert.Equal(["item-id", "job-type", "workspace-id"], run.GetProperty("inputSchema").GetProperty("required").EnumerateArray()
                    .Select(value => value.GetString()).Order());
                Assert.Equal(["execution-data", "item-id", "job-parameters", "job-type", "workspace-id"],
                    run.GetProperty("inputSchema").GetProperty("properties").EnumerateObject().Select(property => property.Name).Order());
            }
            var job = Assert.Single(tools, tool => tool.GetProperty("name").GetString() == "jobs_get-item-job-instance");
            Assert.True(job.GetProperty("annotations").GetProperty("readOnlyHint").GetBoolean());
            Assert.True(job.GetProperty("annotations").GetProperty("idempotentHint").GetBoolean());
            Assert.False(job.GetProperty("annotations").GetProperty("destructiveHint").GetBoolean());
            Assert.Equal(["item-id", "job-instance-id", "workspace-id"], job.GetProperty("inputSchema").GetProperty("required").EnumerateArray()
                .Select(value => value.GetString()).Order());
            Assert.Equal(["item-id", "job-instance-id", "workspace-id"], job.GetProperty("inputSchema").GetProperty("properties").EnumerateObject()
                .Select(property => property.Name).Order());
            var list = Assert.Single(tools, tool => tool.GetProperty("name").GetString() == "jobs_list-item-job-instances");
            Assert.True(list.GetProperty("annotations").GetProperty("readOnlyHint").GetBoolean());
            Assert.True(list.GetProperty("annotations").GetProperty("idempotentHint").GetBoolean());
            Assert.False(list.GetProperty("annotations").GetProperty("destructiveHint").GetBoolean());
            Assert.Equal(["item-id", "workspace-id"], list.GetProperty("inputSchema").GetProperty("required").EnumerateArray()
                .Select(value => value.GetString()).Order());
            Assert.Equal(["continuation-token", "item-id", "workspace-id"], list.GetProperty("inputSchema").GetProperty("properties").EnumerateObject()
                .Select(property => property.Name).Order());
            var schedule = Assert.Single(tools, tool => tool.GetProperty("name").GetString() == "jobs_get-item-schedule");
            Assert.True(schedule.GetProperty("annotations").GetProperty("readOnlyHint").GetBoolean());
            Assert.True(schedule.GetProperty("annotations").GetProperty("idempotentHint").GetBoolean());
            Assert.False(schedule.GetProperty("annotations").GetProperty("destructiveHint").GetBoolean());
            Assert.False(schedule.GetProperty("annotations").GetProperty("openWorldHint").GetBoolean());
            Assert.Contains("disclosure consent, even in read-only mode", schedule.GetProperty("description").GetString());
            Assert.Equal(["item-id", "job-type", "schedule-id", "workspace-id"],
                schedule.GetProperty("inputSchema").GetProperty("required").EnumerateArray().Select(value => value.GetString()).Order());
            var scheduleOptions = schedule.GetProperty("inputSchema").GetProperty("properties");
            Assert.Equal(["item-id", "job-type", "schedule-id", "workspace-id"],
                scheduleOptions.EnumerateObject().Select(property => property.Name).Order());
            Assert.All(scheduleOptions.EnumerateObject(), property =>
            {
                Assert.Equal("string", property.Value.GetProperty("type").GetString());
                Assert.False(property.Value.TryGetProperty("default", out _));
            });
            var schedules = Assert.Single(tools, tool => tool.GetProperty("name").GetString() == "jobs_list-item-schedules");
            Assert.True(schedules.GetProperty("annotations").GetProperty("readOnlyHint").GetBoolean());
            Assert.True(schedules.GetProperty("annotations").GetProperty("idempotentHint").GetBoolean());
            Assert.False(schedules.GetProperty("annotations").GetProperty("destructiveHint").GetBoolean());
            Assert.Equal(["item-id", "job-type", "workspace-id"], schedules.GetProperty("inputSchema").GetProperty("required").EnumerateArray()
                .Select(value => value.GetString()).Order());
            Assert.Equal(["continuation-token", "item-id", "job-type", "workspace-id"], schedules.GetProperty("inputSchema").GetProperty("properties").EnumerateObject()
                .Select(property => property.Name).Order());
        }
    }

    private static void AssertRepresentativeCoreTools(string response, bool localFiles)
    {
        using var document = JsonDocument.Parse(response);
        var tools = document.RootElement.GetProperty("result").GetProperty("tools").EnumerateArray().ToArray();
        var names = tools.Select(tool => tool.GetProperty("name").GetString()).ToArray();
        Assert.Equal(names.Length, names.Distinct(StringComparer.Ordinal).Count());
        var itemGet = Assert.Single(tools, tool => tool.GetProperty("name").GetString() == "core_get-item");
        Assert.True(itemGet.GetProperty("annotations").GetProperty("readOnlyHint").GetBoolean());
        Assert.True(itemGet.GetProperty("annotations").GetProperty("idempotentHint").GetBoolean());
        Assert.False(itemGet.GetProperty("annotations").GetProperty("destructiveHint").GetBoolean());
        Assert.Equal(["item-id", "workspace-id"], itemGet.GetProperty("inputSchema").GetProperty("required").EnumerateArray()
            .Select(value => value.GetString()).Order());
        var assignment = Assert.Single(tools, tool => tool.GetProperty("name").GetString() == "core_assign-workspace-to-capacity");
        Assert.False(assignment.GetProperty("annotations").GetProperty("readOnlyHint").GetBoolean());
        Assert.True(assignment.GetProperty("annotations").GetProperty("destructiveHint").GetBoolean());
        Assert.False(assignment.GetProperty("annotations").GetProperty("idempotentHint").GetBoolean());
        Assert.Equal(["capacity-id", "workspace-id"], assignment.GetProperty("inputSchema").GetProperty("required").EnumerateArray()
            .Select(value => value.GetString()).Order());
        var creation = Assert.Single(tools, tool => tool.GetProperty("name").GetString() == "core_create-workspace");
        Assert.False(creation.GetProperty("annotations").GetProperty("readOnlyHint").GetBoolean());
        Assert.False(creation.GetProperty("annotations").GetProperty("idempotentHint").GetBoolean());
        Assert.Equal(["display-name"], creation.GetProperty("inputSchema").GetProperty("required").EnumerateArray()
            .Select(value => value.GetString()));
        var update = Assert.Single(tools, tool => tool.GetProperty("name").GetString() == "core_update-workspace");
        Assert.False(update.GetProperty("annotations").GetProperty("readOnlyHint").GetBoolean());
        Assert.True(update.GetProperty("annotations").GetProperty("destructiveHint").GetBoolean());
        Assert.True(update.GetProperty("annotations").GetProperty("idempotentHint").GetBoolean());
        Assert.Equal(["workspace-id"], update.GetProperty("inputSchema").GetProperty("required").EnumerateArray()
            .Select(value => value.GetString()));
        var itemUpdate = Assert.Single(tools, tool => tool.GetProperty("name").GetString() == "core_update-item");
        Assert.False(itemUpdate.GetProperty("annotations").GetProperty("readOnlyHint").GetBoolean());
        Assert.True(itemUpdate.GetProperty("annotations").GetProperty("destructiveHint").GetBoolean());
        Assert.True(itemUpdate.GetProperty("annotations").GetProperty("idempotentHint").GetBoolean());
        Assert.Equal(["item-id", "workspace-id"], itemUpdate.GetProperty("inputSchema").GetProperty("required").EnumerateArray()
            .Select(value => value.GetString()).Order());
        var itemDelete = Assert.Single(tools, tool => tool.GetProperty("name").GetString() == "core_delete-item");
        Assert.False(itemDelete.GetProperty("annotations").GetProperty("readOnlyHint").GetBoolean());
        Assert.True(itemDelete.GetProperty("annotations").GetProperty("destructiveHint").GetBoolean());
        Assert.True(itemDelete.GetProperty("annotations").GetProperty("idempotentHint").GetBoolean());
        Assert.Equal(["item-id", "workspace-id"], itemDelete.GetProperty("inputSchema").GetProperty("required").EnumerateArray()
            .Select(value => value.GetString()).Order());
        var workspaceDelete = Assert.Single(tools, tool => tool.GetProperty("name").GetString() == "core_delete-workspace");
        Assert.False(workspaceDelete.GetProperty("annotations").GetProperty("readOnlyHint").GetBoolean());
        Assert.True(workspaceDelete.GetProperty("annotations").GetProperty("destructiveHint").GetBoolean());
        Assert.True(workspaceDelete.GetProperty("annotations").GetProperty("idempotentHint").GetBoolean());
        Assert.Equal(["workspace-id"], workspaceDelete.GetProperty("inputSchema").GetProperty("required").EnumerateArray()
            .Select(value => value.GetString()));
        var hardDelete = itemDelete.GetProperty("inputSchema").GetProperty("properties").GetProperty("hard-delete");
        Assert.Equal(["boolean", "null"], hardDelete.GetProperty("type").EnumerateArray().Select(value => value.GetString()));
        Assert.False(hardDelete.TryGetProperty("default", out _));
        foreach (var name in new[] { "core_get-operation-state", "core_get-operation-result" })
        {
            var operation = Assert.Single(tools, tool => tool.GetProperty("name").GetString() == name);
            Assert.True(operation.GetProperty("annotations").GetProperty("readOnlyHint").GetBoolean());
            Assert.True(operation.GetProperty("annotations").GetProperty("idempotentHint").GetBoolean());
            Assert.False(operation.GetProperty("annotations").GetProperty("destructiveHint").GetBoolean());
            Assert.Equal("operation-id", Assert.Single(operation.GetProperty("inputSchema").GetProperty("required").EnumerateArray()).GetString());
            Assert.Equal("operation-id", Assert.Single(operation.GetProperty("inputSchema").GetProperty("properties").EnumerateObject()).Name);
        }
        var itemCreate = Assert.Single(tools, tool => tool.GetProperty("name").GetString() == "core_create-item");
        var creationOptions = itemCreate.GetProperty("inputSchema").GetProperty("properties");
        Assert.Equal("string", creationOptions.GetProperty("definition").GetProperty("type").GetString());
        foreach (var option in new[] { "folder-id", "creation-payload", "options", "sensitivity-label-settings" })
        {
            Assert.Equal("string", creationOptions.GetProperty(option).GetProperty("type").GetString());
        }
        foreach (var option in new[] { "definition-directory", "definition-files", "definition-format" })
        {
            Assert.Equal(localFiles, creationOptions.TryGetProperty(option, out _));
        }
        Assert.Equal("boolean", creationOptions.GetProperty("sync").GetProperty("type").GetString());
        Assert.Equal("boolean", creationOptions.GetProperty("early-poll").GetProperty("type").GetString());
        Assert.Equal("number", creationOptions.GetProperty("max-wait-seconds").GetProperty("type").GetString());
        Assert.Equal(["display-name", "item-type"], itemCreate.GetProperty("inputSchema").GetProperty("required").EnumerateArray()
            .Select(value => value.GetString()).Order());
        var definitionGet = Assert.Single(tools, tool => tool.GetProperty("name").GetString() == "core_get-item-definition");
        Assert.False(definitionGet.GetProperty("annotations").GetProperty("readOnlyHint").GetBoolean());
        Assert.False(definitionGet.GetProperty("annotations").GetProperty("destructiveHint").GetBoolean());
        Assert.True(definitionGet.GetProperty("annotations").GetProperty("idempotentHint").GetBoolean());
        var definitionGetOptions = definitionGet.GetProperty("inputSchema").GetProperty("properties");
        foreach (var option in new[] { "workspace-id", "item-id", "operation-id", "format" })
        {
            Assert.Equal("string", definitionGetOptions.GetProperty(option).GetProperty("type").GetString());
        }
        Assert.Equal(localFiles, definitionGetOptions.TryGetProperty("output-directory", out _));
        Assert.False(definitionGetOptions.TryGetProperty("definition", out _));
        Assert.False(definitionGetOptions.TryGetProperty("update-metadata", out _));
        var definitionUpdate = Assert.Single(tools, tool => tool.GetProperty("name").GetString() == "core_update-item-definition");
        Assert.False(definitionUpdate.GetProperty("annotations").GetProperty("readOnlyHint").GetBoolean());
        Assert.True(definitionUpdate.GetProperty("annotations").GetProperty("destructiveHint").GetBoolean());
        Assert.True(definitionUpdate.GetProperty("annotations").GetProperty("idempotentHint").GetBoolean());
        Assert.Equal(["item-id", "workspace-id"], definitionUpdate.GetProperty("inputSchema").GetProperty("required").EnumerateArray()
            .Select(value => value.GetString()).Order());
        var definitionUpdateOptions = definitionUpdate.GetProperty("inputSchema").GetProperty("properties");
        Assert.Equal("string", definitionUpdateOptions.GetProperty("definition").GetProperty("type").GetString());
        Assert.Equal("string", definitionUpdateOptions.GetProperty("options").GetProperty("type").GetString());
        Assert.False(definitionUpdateOptions.TryGetProperty("definition-item-type", out _));
        Assert.Equal(["boolean", "null"], definitionUpdateOptions.GetProperty("update-metadata").GetProperty("type").EnumerateArray()
            .Select(value => value.GetString()));
        Assert.False(definitionUpdateOptions.GetProperty("update-metadata").TryGetProperty("default", out _));
        foreach (var option in new[] { "definition-directory", "definition-files", "definition-format" })
        {
            Assert.Equal(localFiles, definitionUpdateOptions.TryGetProperty(option, out _));
        }
    }

    private static async Task<string> ReadToolResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.Content.Headers.ContentType?.MediaType != "text/event-stream")
        {
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var messages = new List<string>();
        await foreach (var item in SseParser.Create(stream).EnumerateAsync(cancellationToken))
        {
            messages.Add(item.Data);
        }
        return Assert.Single(messages);
    }

    private static void AssertListItemsTool(string response)
    {
        using var document = JsonDocument.Parse(response);
        var tools = document.RootElement.GetProperty("result").GetProperty("tools").EnumerateArray().ToArray();
        var tool = Assert.Single(tools, tool => tool.GetProperty("name").GetString() == "core_list-items");
        Assert.True(tool.GetProperty("annotations").GetProperty("readOnlyHint").GetBoolean());
        Assert.Equal(["workspace-id"], tool.GetProperty("inputSchema").GetProperty("required").EnumerateArray()
            .Select(value => value.GetString()));
    }

    private static int GetAvailablePort()
    {
        using var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        return ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
    }

    private static async Task<HttpResponseMessage> SendWithRetryAsync(HttpClient client, HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var deadline = DateTime.UtcNow.AddSeconds(10);
        Exception? lastException = null;

        while (DateTime.UtcNow < deadline)
        {
            try
            {
                using var clonedRequest = CloneRequest(request);
                var response = await client.SendAsync(clonedRequest, cancellationToken);
                if (response.StatusCode != HttpStatusCode.NotFound)
                {
                    return response;
                }

                // Dispose the 404 response before retrying to avoid socket/handler leaks.
                response.Dispose();
            }
            catch (HttpRequestException ex)
            {
                lastException = ex;
            }

            await Task.Delay(250, cancellationToken);
        }

        throw new InvalidOperationException("Timed out waiting for Fabric MCP HTTP endpoint to accept requests.", lastException);
    }

    private static HttpRequestMessage CloneRequest(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Content = request.Content == null ? null : new StringContent(request.Content.ReadAsStringAsync().GetAwaiter().GetResult(), System.Text.Encoding.UTF8, request.Content.Headers.ContentType?.MediaType)
        };

        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return clone;
    }
}
