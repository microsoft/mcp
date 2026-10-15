// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using StorageMcp.Generator;
using Xunit;

namespace StorageMcp.Generator.Tests;

public sealed class ManifestTests
{
    private static StorageManifest Manifest => StorageManifest.Load(Path.Combine(AppContext.BaseDirectory, "storage.tools.json"));

    [Fact]
    public void ReviewedManifestPreservesExistingIdentityAndComposites()
    {
        var manifest = Manifest;
        var tool = Assert.Single(manifest.Operations);
        Assert.Equal("f5088334-e630-4df0-a5be-ac87787acad0", tool.Id);
        Assert.Equal("StoragePilot.Containers.create", tool.OperationId);
        Assert.Equal(["storage", "blob", "container"], tool.Group);
        Assert.Equal(["account", "tenant"], tool.AmbientParameters);
        Assert.True(tool.UseDescriptionOverride);
        Assert.Contains(manifest.Composites, c => c.Path == "storage account get");
        Assert.Contains(manifest.Composites, c => c.Path == "storage blob container get");
        Assert.Contains(manifest.Composites, c => c.Path == "storage blob get");
    }

    [Fact]
    public void OperationSelectionIsExactAndIgnoresUnselectedOperations()
    {
        Manifest.ValidateOperationIds(["StoragePilot.Containers.create", "StoragePilot.Containers.get"]);
        Assert.Throws<InvalidOperationException>(() => Manifest.ValidateOperationIds(["StoragePilot.Containers.get"]));
        Assert.Throws<InvalidOperationException>(() => Manifest.ValidateOperationIds(
            ["StoragePilot.Containers.create", "StoragePilot.Containers.create"]));
    }

    [Theory]
    [InlineData("GET", false, true, true)]
    [InlineData("head", false, true, true)]
    [InlineData("OPTIONS", false, true, true)]
    [InlineData("PUT", true, true, false)]
    [InlineData("DELETE", true, true, false)]
    [InlineData("POST", true, false, false)]
    [InlineData("PATCH", true, false, false)]
    public void SafetyDefaultsAreConservative(string verb, bool destructive, bool idempotent, bool readOnly)
    {
        var flags = ToolSafety.FromHttpMethod(verb);
        Assert.Equal(destructive, flags.Destructive);
        Assert.Equal(idempotent, flags.Idempotent);
        Assert.Equal(readOnly, flags.ReadOnly);
        Assert.True(flags.OpenWorld);
    }

    [Fact]
    public void ReviewedSafetyOverridesWinAndMissingOverridesKeepDefaults()
    {
        var defaults = ToolSafety.FromHttpMethod("PUT");
        var flags = Manifest.Operations[0].Safety.Apply(defaults);
        Assert.True(flags.Destructive);
        Assert.False(flags.Idempotent);
        Assert.False(flags.ReadOnly);
        Assert.False(flags.OpenWorld);
        Assert.Equal(defaults, new SafetyOverrides(null, null, null, null).Apply(defaults));
        Assert.Throws<InvalidOperationException>(() => ToolSafety.FromHttpMethod("UNKNOWN"));
    }

    [Theory]
    [InlineData("operation")]
    [InlineData("id")]
    [InlineData("group")]
    [InlineData("name")]
    [InlineData("binding")]
    [InlineData("ambient")]
    [InlineData("safety")]
    [InlineData("empty")]
    [InlineData("multiple")]
    [InlineData("duplicate-composite")]
    [InlineData("colliding-composite")]
    public void InvalidManifestIsRejected(string change)
    {
        var manifest = Manifest;
        var tool = manifest.Operations[0];
        var invalid = change switch
        {
            "operation" => manifest with { Operations = [tool with { OperationId = "" }] },
            "id" => manifest with { Operations = [tool with { Id = "not-a-guid" }] },
            "group" => manifest with { Operations = [tool with { Group = ["storage", "wrong"] }] },
            "name" => manifest with { Operations = [tool with { Name = "delete" }] },
            "binding" => manifest with { Operations = [tool with { Binding = "unreviewed-reflection" }] },
            "ambient" => manifest with { Operations = [tool with { AmbientParameters = ["subscription"] }] },
            "safety" => manifest with { Operations = [tool with { Safety = null! }] },
            "empty" => manifest with { Operations = [] },
            "multiple" => manifest with { Operations = [tool, tool] },
            "duplicate-composite" => manifest with { Composites = [manifest.Composites[0], manifest.Composites[0]] },
            "colliding-composite" => manifest with { Composites = [new("storage blob container create", "Collision")] },
            _ => throw new InvalidOperationException(change)
        };
        Assert.Throws<InvalidOperationException>(invalid.Validate);
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("null")]
    [InlineData("{\"operations\":[],\"composites\":[],\"typo\":true}")]
    [InlineData("{\"operations\":[],\"operations\":[],\"composites\":[]}")]
    [InlineData("{\"operations\":null,\"composites\":[]}")]
    public void MalformedJsonDoesNotSilentlyBecomeAnEmptyToolset(string json)
    {
        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllText(path, json);
            var error = Record.Exception(() => StorageManifest.Load(path));
            Assert.NotNull(error);
            Assert.True(error is JsonException or InvalidOperationException);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
