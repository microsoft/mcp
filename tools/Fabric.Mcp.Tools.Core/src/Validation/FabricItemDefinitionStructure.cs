// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// cspell:words ipynb tmdl tmsl pbism pbir

using Fabric.Mcp.Tools.Core.Services;

namespace Fabric.Mcp.Tools.Core.Validation;

internal static class FabricItemDefinitionStructure
{
    // Curated minimum part/format checks from the corresponding bundled notebook,
    // datapipeline, semantic-model, and report definition articles in Fabric.Mcp.Tools.Docs.
    // These are not content schemas; Fabric validates content and other/future item types.
    internal static void Validate(string itemType, IEnumerable<string> partPaths, string? format)
    {
        var paths = partPaths.ToHashSet(StringComparer.Ordinal);
        if (itemType.Equals("Notebook", StringComparison.OrdinalIgnoreCase))
        {
            ValidateNotebook(paths, format);
        }
        else if (itemType.Equals("DataPipeline", StringComparison.OrdinalIgnoreCase))
        {
            Require(paths.Contains("pipeline-content.json"), "DataPipeline definitions require pipeline-content.json.");
        }
        else if (itemType.Equals("SemanticModel", StringComparison.OrdinalIgnoreCase))
        {
            Require(paths.Contains("definition.pbism"), "SemanticModel definitions require definition.pbism.");
            ValidateAlternativeFormats(paths, format, "TMDL", "TMSL", "model.bim", "SemanticModel");
        }
        else if (itemType.Equals("Report", StringComparison.OrdinalIgnoreCase))
        {
            Require(paths.Contains("definition.pbir"), "Report definitions require definition.pbir.");
            ValidateAlternativeFormats(paths, format, "PBIR", "PBIR-Legacy", "report.json", "Report");
        }
    }

    private static void ValidateNotebook(HashSet<string> paths, string? format)
    {
        string[] sourcePaths = ["notebook-content.py", "notebook-content.sql", "notebook-content.scala", "notebook-content.r"];
        var sourceCount = sourcePaths.Count(paths.Contains);
        var notebookCount = paths.Count(static path => path.EndsWith(".ipynb", StringComparison.Ordinal));
        if (string.Equals(format, "ipynb", StringComparison.OrdinalIgnoreCase))
        {
            Require(notebookCount == 1 && sourceCount == 0,
                "Notebook ipynb definitions require one .ipynb content part and no FabricGitSource content parts.");
        }
        else
        {
            Require(format is null || format.Equals("FabricGitSource", StringComparison.OrdinalIgnoreCase),
                "Notebook format must be ipynb or FabricGitSource (the default).");
            Require(sourceCount == 1 && notebookCount == 0,
                "Notebook FabricGitSource definitions require one notebook-content.py, .sql, .scala, or .r part. For .ipynb input, specify format ipynb.");
        }
    }

    private static void ValidateAlternativeFormats(
        HashSet<string> paths, string? format, string directoryFormat, string fileFormat, string filePath, string itemType)
    {
        var hasDirectory = paths.Any(static path => path.StartsWith("definition/", StringComparison.Ordinal));
        var hasFile = paths.Contains(filePath);
        Require(!(hasDirectory && hasFile), $"{itemType} definitions cannot mix {directoryFormat} and {fileFormat} parts.");
        if (string.Equals(format, fileFormat, StringComparison.OrdinalIgnoreCase))
        {
            Require(hasFile, $"{itemType} {fileFormat} definitions require {filePath}.");
        }
        else
        {
            Require(format is null || format.Equals(directoryFormat, StringComparison.OrdinalIgnoreCase),
                $"{itemType} format must be {directoryFormat} (the default) or {fileFormat}.");
            Require(hasDirectory, $"{itemType} {directoryFormat} definitions require files under definition/. For {filePath}, specify format {fileFormat}.");
        }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new FabricItemDefinitionInputException(message);
        }
    }
}
