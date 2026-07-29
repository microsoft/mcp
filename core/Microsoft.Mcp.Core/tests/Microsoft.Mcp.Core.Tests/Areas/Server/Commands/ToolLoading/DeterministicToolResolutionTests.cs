// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;
using ModelContextProtocol.Protocol;
using Xunit;

namespace Microsoft.Mcp.Core.Tests.Areas.Server.Commands.ToolLoading;

/// <summary>
/// Unit tests for <see cref="DeterministicToolResolution"/>.
/// Covers lexical matching, synonym expansion, confidence threshold, and edge cases.
/// </summary>
public sealed class DeterministicToolResolutionTests
{
    private static readonly IReadOnlyList<Tool> s_namespaces =
    [
        new Tool { Name = "storage",          Description = "Azure Storage blobs containers files accounts" },
        new Tool { Name = "compute",          Description = "Azure Compute virtual machines disks snapshots" },
        new Tool { Name = "keyvault",         Description = "Azure Key Vault secrets keys certificates" },
        new Tool { Name = "network",          Description = "Azure Network VNets NSGs subnets firewalls" },
        new Tool { Name = "sql",              Description = "Azure SQL databases servers" },
        new Tool { Name = "aks",              Description = "Azure Kubernetes Service clusters" },
        new Tool { Name = "costmanagement",   Description = "Azure Cost Management budgets spending billing" },
        new Tool { Name = "authorization",    Description = "Azure Authorization role assignments RBAC" },
        new Tool { Name = "appservice",       Description = "Azure App Service web apps sites" },
        new Tool { Name = "resourcegroup",    Description = "Azure resource groups" },
        new Tool { Name = "subscription",     Description = "Azure subscriptions" },
    ];

    // ── Edge cases ─────────────────────────────────────────────────────────────

    [Fact]
    public void ResolveNamespace_NullIntent_ReturnsNull()
    {
        Assert.Null(DeterministicToolResolution.ResolveNamespace(null!, s_namespaces));
    }

    [Fact]
    public void ResolveNamespace_EmptyIntent_ReturnsNull()
    {
        Assert.Null(DeterministicToolResolution.ResolveNamespace("", s_namespaces));
    }

    [Fact]
    public void ResolveNamespace_WhitespaceIntent_ReturnsNull()
    {
        Assert.Null(DeterministicToolResolution.ResolveNamespace("   ", s_namespaces));
    }

    [Fact]
    public void ResolveNamespace_EmptyNamespaces_ReturnsNull()
    {
        Assert.Null(DeterministicToolResolution.ResolveNamespace("list storage accounts", []));
    }

    [Fact]
    public void ResolveNamespace_IntentWithNoSignal_ReturnsNull()
    {
        // Single-character tokens are ignored; no overlap possible
        Assert.Null(DeterministicToolResolution.ResolveNamespace("a b c d", s_namespaces));
    }

    [Fact]
    public void ResolveNamespace_CompletelyUnrelated_ReturnsNull()
    {
        Assert.Null(DeterministicToolResolution.ResolveNamespace("xyzzy foobar quux123", s_namespaces));
    }

    // ── Lexical matches (namespace token in intent) ────────────────────────────

    [Theory]
    [InlineData("list my storage accounts",           "storage")]
    [InlineData("show storage blobs",                 "storage")]
    [InlineData("get sql databases",                  "sql")]
    [InlineData("list keyvault secrets",              "keyvault")]
    [InlineData("show network vnets",                 "network")]
    [InlineData("list aks clusters",                  "aks")]
    [InlineData("list resource groups",               "resourcegroup")]
    [InlineData("show subscriptions",                 "subscription")]
    [InlineData("list appservice web apps",           "appservice")]
    [InlineData("list costmanagement budgets",        "costmanagement")]
    [InlineData("show authorization role assignments","authorization")]
    public void ResolveNamespace_LexicalMatch_ReturnsCorrectNamespace(string intent, string expected)
    {
        Assert.Equal(expected, DeterministicToolResolution.ResolveNamespace(intent, s_namespaces));
    }

    // ── Synonym expansion ──────────────────────────────────────────────────────

    [Theory]
    [InlineData("list my VMs",                  "compute")]
    [InlineData("show virtual machines",        "compute")]
    [InlineData("give me my vms",               "compute")]
    [InlineData("list all instances",           "compute")]
    [InlineData("get secrets from the kv",      "keyvault")]
    [InlineData("show my vault",                "keyvault")]
    [InlineData("list certificates",            "keyvault")]
    [InlineData("show my k8s clusters",         "aks")]
    [InlineData("list kubernetes pods",         "aks")]
    [InlineData("how much am I spending",       "costmanagement")]
    [InlineData("what is my cloud bill",        "costmanagement")]
    [InlineData("check my azure budget",        "costmanagement")]
    [InlineData("list blob containers",         "storage")]
    [InlineData("show nsg rules",               "network")]
    [InlineData("list role assignments",        "authorization")]
    [InlineData("show rbac permissions",        "authorization")]
    [InlineData("list web apps",                "appservice")]
    public void ResolveNamespace_SynonymMatch_ReturnsCorrectNamespace(string intent, string expected)
    {
        Assert.Equal(expected, DeterministicToolResolution.ResolveNamespace(intent, s_namespaces));
    }

    // ── Case insensitivity ─────────────────────────────────────────────────────

    [Theory]
    [InlineData("LIST MY STORAGE ACCOUNTS",     "storage")]
    [InlineData("List My VMs",                  "compute")]
    [InlineData("SHOW K8S CLUSTERS",            "aks")]
    public void ResolveNamespace_IsCaseInsensitive(string intent, string expected)
    {
        Assert.Equal(expected, DeterministicToolResolution.ResolveNamespace(intent, s_namespaces));
    }

    // ── Best match wins when multiple namespaces have signal ──────────────────

    [Fact]
    public void ResolveNamespace_MultiplePartialMatches_ReturnsBestMatch()
    {
        // "storage blobs" has two tokens matching storage; only "blobs" synonym expands to storage.
        // "sql" and "databases" both match sql. Neither matches storage.
        // Intent has strong sql signal.
        var result = DeterministicToolResolution.ResolveNamespace("list sql databases", s_namespaces);
        Assert.Equal("sql", result);
    }

    // ── Confidence threshold: single ambiguous token below threshold ──────────

    [Fact]
    public void ResolveNamespace_SingleAmbiguousToken_MayReturnNullOrLowestScore()
    {
        // "list" doesn't match any namespace token — threshold not reached
        var result = DeterministicToolResolution.ResolveNamespace("list", s_namespaces);
        // Either null (below threshold) or some namespace — should not throw
        // The key property is that no namespace has overlap > 0 for "list" alone
        // since none contain "list" in name or description.
        Assert.Null(result);
    }
}
