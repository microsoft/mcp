// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Commands;

public static class ToolOperationPlaneExtensions
{
    public static string ToJsonValue(this ToolOperationPlane operationPlane) => operationPlane switch
    {
        ToolOperationPlane.Unspecified => "unspecified",
        ToolOperationPlane.Data => "data",
        ToolOperationPlane.Control => "control",
        ToolOperationPlane.Both => "both",
        ToolOperationPlane.NotApplicable => "notApplicable",
        _ => throw new ArgumentOutOfRangeException(nameof(operationPlane), operationPlane, "Unknown tool operation plane.")
    };

    public static bool TryParseJsonValue(string? value, out ToolOperationPlane operationPlane)
    {
        operationPlane = value switch
        {
            "unspecified" => ToolOperationPlane.Unspecified,
            "data" => ToolOperationPlane.Data,
            "control" => ToolOperationPlane.Control,
            "both" => ToolOperationPlane.Both,
            "notApplicable" => ToolOperationPlane.NotApplicable,
            _ => ToolOperationPlane.Unspecified
        };

        return value is "unspecified" or "data" or "control" or "both" or "notApplicable";
    }

    public static ToolOperationPlane Aggregate(IEnumerable<ToolOperationPlane> operationPlanes)
    {
        var hasData = false;
        var hasControl = false;
        var hasNotApplicable = false;

        foreach (var operationPlane in operationPlanes)
        {
            switch (operationPlane)
            {
                case ToolOperationPlane.Unspecified:
                    return ToolOperationPlane.Unspecified;
                case ToolOperationPlane.Data:
                    hasData = true;
                    break;
                case ToolOperationPlane.Control:
                    hasControl = true;
                    break;
                case ToolOperationPlane.Both:
                    hasData = true;
                    hasControl = true;
                    break;
                case ToolOperationPlane.NotApplicable:
                    hasNotApplicable = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(operationPlanes), operationPlane, "Unknown tool operation plane.");
            }
        }

        if (hasData && hasControl)
        {
            return ToolOperationPlane.Both;
        }

        if (hasData)
        {
            return ToolOperationPlane.Data;
        }

        if (hasControl)
        {
            return ToolOperationPlane.Control;
        }

        return hasNotApplicable ? ToolOperationPlane.NotApplicable : ToolOperationPlane.Unspecified;
    }

    public static ToolOperationPlane Aggregate(CommandGroup commandGroup)
        => Aggregate(commandGroup, static _ => true);

    public static ToolOperationPlane Aggregate(CommandGroup commandGroup, Predicate<ToolMetadata> predicate)
        => Aggregate(GetOperationPlanes(commandGroup, predicate));

    private static IEnumerable<ToolOperationPlane> GetOperationPlanes(
        CommandGroup commandGroup,
        Predicate<ToolMetadata> predicate)
    {
        foreach (var command in commandGroup.Commands.Values)
        {
            if (predicate(command.Metadata))
            {
                yield return command.Metadata.OperationPlane;
            }
        }

        foreach (var subGroup in commandGroup.SubGroup)
        {
            foreach (var operationPlane in GetOperationPlanes(subGroup, predicate))
            {
                yield return operationPlane;
            }
        }
    }
}
