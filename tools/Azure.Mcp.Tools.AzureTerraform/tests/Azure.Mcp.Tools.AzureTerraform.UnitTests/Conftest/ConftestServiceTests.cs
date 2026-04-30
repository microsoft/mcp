// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureTerraform.Services;
using Xunit;

namespace Azure.Mcp.Tools.AzureTerraform.UnitTests;

public class ConftestServiceTests
{
    private readonly ConftestService _service = new();

    [Fact]
    public void GenerateWorkspaceValidationCommand_DefaultPolicySet_ReturnsCorrectCommand()
    {
        var result = _service.GenerateWorkspaceValidationCommand("/home/user/project");

        Assert.True(result.ConftestFound);
        Assert.Equal("conftest", result.Command);
        Assert.Contains("test", result.Args);
        Assert.Contains("--all-namespaces", result.Args);
        Assert.Contains("--output", result.Args);
        Assert.Contains("json", result.Args);
        Assert.Contains("-p", result.Args);
        Assert.Contains("./policy", result.Args);
        Assert.Equal(".", result.Args[^1]);
        Assert.Equal("/home/user/project", result.WorkspaceFolder);
        Assert.Equal("/home/user/project", result.WorkingDirectory);
        Assert.Equal("all", result.PolicySet);
    }

    [Fact]
    public void GenerateWorkspaceValidationCommand_AprlPolicySet_UsesCorrectPath()
    {
        var result = _service.GenerateWorkspaceValidationCommand(
            "/home/user/project",
            policySet: "Azure-Proactive-Resiliency-Library-v2");

        Assert.Contains("./policy/Azure-Proactive-Resiliency-Library-v2", result.Args);
        Assert.DoesNotContain("./policy/avmsec", result.Args);
    }

    [Fact]
    public void GenerateWorkspaceValidationCommand_AvmsecPolicySet_UsesCorrectPath()
    {
        var result = _service.GenerateWorkspaceValidationCommand(
            "/home/user/project",
            policySet: "avmsec");

        Assert.Contains("./policy/avmsec", result.Args);
        Assert.Equal("avmsec", result.PolicySet);
    }

    [Fact]
    public void GenerateWorkspaceValidationCommand_AvmsecWithSeverityFilter_IncludesSeverityFile()
    {
        var result = _service.GenerateWorkspaceValidationCommand(
            "/home/user/project",
            policySet: "avmsec",
            severityFilter: "high");

        Assert.Contains("./policy/avmsec", result.Args);
        Assert.Contains(".conftest_severity_high.rego", result.Args);
    }

    [Fact]
    public void GenerateWorkspaceValidationCommand_NonAvmsecWithSeverity_IgnoresSeverityFilter()
    {
        var result = _service.GenerateWorkspaceValidationCommand(
            "/home/user/project",
            policySet: "all",
            severityFilter: "high");

        Assert.DoesNotContain(".conftest_severity_high.rego", result.Args);
    }

    [Fact]
    public void GenerateWorkspaceValidationCommand_WithCustomPolicies_IncludesAllPaths()
    {
        var result = _service.GenerateWorkspaceValidationCommand(
            "/home/user/project",
            customPolicies: "/path/to/policy1,/path/to/policy2");

        Assert.Contains("/path/to/policy1", result.Args);
        Assert.Contains("/path/to/policy2", result.Args);
    }

    [Fact]
    public void GenerateWorkspaceValidationCommand_CustomPoliciesWithSpaces_TrimsEntries()
    {
        var result = _service.GenerateWorkspaceValidationCommand(
            "/home/user/project",
            customPolicies: " /path/to/policy1 , /path/to/policy2 ");

        Assert.Contains("/path/to/policy1", result.Args);
        Assert.Contains("/path/to/policy2", result.Args);
    }

    [Fact]
    public void GeneratePlanValidationCommand_DefaultPolicySet_ReturnsCorrectCommand()
    {
        var result = _service.GeneratePlanValidationCommand("/home/user/project");

        Assert.True(result.ConftestFound);
        Assert.Equal("conftest", result.Command);
        Assert.Contains("test", result.Args);
        Assert.Contains("--all-namespaces", result.Args);
        Assert.Contains("--output", result.Args);
        Assert.Contains("json", result.Args);
        Assert.Contains("-p", result.Args);
        Assert.Contains("./policy", result.Args);
        Assert.Equal("tfplan.json", result.Args[^1]);
        Assert.Equal("/home/user/project", result.WorkspaceFolder);
        Assert.Equal("/home/user/project", result.WorkingDirectory);
        Assert.Equal("all", result.PolicySet);
    }

    [Fact]
    public void GeneratePlanValidationCommand_AvmsecWithSeverity_IncludesSeverityFile()
    {
        var result = _service.GeneratePlanValidationCommand(
            "/home/user/project",
            policySet: "avmsec",
            severityFilter: "medium");

        Assert.Contains("./policy/avmsec", result.Args);
        Assert.Contains(".conftest_severity_medium.rego", result.Args);
        Assert.Equal("tfplan.json", result.Args[^1]);
    }

    [Fact]
    public void GeneratePlanValidationCommand_WithCustomPolicies_IncludesAllPaths()
    {
        var result = _service.GeneratePlanValidationCommand(
            "/home/user/project",
            customPolicies: "/custom/policy");

        Assert.Contains("/custom/policy", result.Args);
        Assert.Equal("tfplan.json", result.Args[^1]);
    }

    [Fact]
    public void GeneratePlanValidationCommand_Notes_IncludePlanFileInstructions()
    {
        var result = _service.GeneratePlanValidationCommand("/home/user/project");

        Assert.Contains(result.Notes, n => n.Contains("tfplan.json", StringComparison.Ordinal));
        Assert.Contains(result.Notes, n => n.Contains("terraform plan", StringComparison.Ordinal));
    }

    [Fact]
    public void GenerateWorkspaceValidationCommand_Notes_IncludePolicyCloneInstructions()
    {
        var result = _service.GenerateWorkspaceValidationCommand("/home/user/project");

        Assert.Contains(result.Notes, n => n.Contains("git clone", StringComparison.Ordinal));
        Assert.Contains(result.Notes, n => n.Contains("policy-library-avm", StringComparison.Ordinal));
    }

    [Fact]
    public void NotFoundResult_ReturnsInstallationHelp()
    {
        var result = ConftestService.NotFoundResult("Test description");

        Assert.False(result.ConftestFound);
        Assert.Empty(result.Command);
        Assert.Equal("Test description", result.Description);
        Assert.NotNull(result.InstallationHelp);
        Assert.Equal("conftest", result.InstallationHelp.ToolName);
        Assert.NotEmpty(result.InstallationHelp.InstallationMethods);
        Assert.NotEmpty(result.InstallationHelp.DocumentationUrl);
        Assert.NotEmpty(result.InstallationHelp.VerifyCommand);
        Assert.NotEmpty(result.InstallationHelp.AdditionalNotes);
    }

    [Fact]
    public void GetInstallationHelp_ContainsAllPlatforms()
    {
        var help = ConftestService.GetConftestInstallationHelp();

        Assert.Contains(help.InstallationMethods, m => m.Platform == "windows");
        Assert.Contains(help.InstallationMethods, m => m.Platform == "macos");
        Assert.Contains(help.InstallationMethods, m => m.Platform == "linux");
    }

    [Fact]
    public void GetInstallationHelp_ContainsMultipleMethods()
    {
        var help = ConftestService.GetConftestInstallationHelp();

        Assert.Contains(help.InstallationMethods, m => m.Method == "scoop");
        Assert.Contains(help.InstallationMethods, m => m.Method == "choco");
        Assert.Contains(help.InstallationMethods, m => m.Method == "brew");
        Assert.Contains(help.InstallationMethods, m => m.Method == "manual");
    }

    [Fact]
    public void GetInstallationHelp_VerifyCommand_IsConftestVersion()
    {
        var help = ConftestService.GetConftestInstallationHelp();

        Assert.Equal("conftest --version", help.VerifyCommand);
    }

    [Fact]
    public void AddPolicyArgs_AllPolicySet_UsesRootPolicyDirectory()
    {
        var args = new List<string>();
        ConftestService.AddPolicyArgs(args, "all", null, null);

        Assert.Equal(["-p", "./policy"], args);
    }

    [Fact]
    public void AddPolicyArgs_AprlPolicySet_UsesSubdirectory()
    {
        var args = new List<string>();
        ConftestService.AddPolicyArgs(args, "Azure-Proactive-Resiliency-Library-v2", null, null);

        Assert.Equal(["-p", "./policy/Azure-Proactive-Resiliency-Library-v2"], args);
    }

    [Fact]
    public void AddPolicyArgs_AvmsecWithSeverity_IncludesSeverityFile()
    {
        var args = new List<string>();
        ConftestService.AddPolicyArgs(args, "avmsec", "low", null);

        Assert.Contains("-p", args);
        Assert.Contains("./policy/avmsec", args);
        Assert.Contains(".conftest_severity_low.rego", args);
    }

    [Fact]
    public void AddPolicyArgs_AvmsecWithoutSeverity_NoSeverityFile()
    {
        var args = new List<string>();
        ConftestService.AddPolicyArgs(args, "avmsec", null, null);

        Assert.Equal(["-p", "./policy/avmsec"], args);
    }

    [Fact]
    public void WorkspaceCommand_LastArgIsDot()
    {
        var result = _service.GenerateWorkspaceValidationCommand("/home/user/project");

        Assert.Equal(".", result.Args[^1]);
    }

    [Fact]
    public void PlanCommand_LastArgIsTfplanJson()
    {
        var result = _service.GeneratePlanValidationCommand("/home/user/project");

        Assert.Equal("tfplan.json", result.Args[^1]);
    }
}
