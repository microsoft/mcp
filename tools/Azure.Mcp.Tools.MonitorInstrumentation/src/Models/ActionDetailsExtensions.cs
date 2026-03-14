namespace Azure.Mcp.Tools.MonitorInstrumentation.Models;

/// <summary>
/// Extension methods for converting between typed details and dictionary
/// </summary>
public static class ActionDetailsExtensions
{
    public static Dictionary<string, object> ToDictionary(this ActionDetails details)
    {
        return details switch
        {
            ReviewEducationDetails red => new Dictionary<string, object>
            {
                ["resources"] = red.Resources
            },
            AddPackageDetails apd => new Dictionary<string, object>
            {
                ["packageManager"] = apd.PackageManager,
                ["package"] = apd.Package,
                ["version"] = apd.Version,
                ["targetProject"] = apd.TargetProject
            },
            ModifyCodeDetails mcd => new Dictionary<string, object>
            {
                ["file"] = mcd.File,
                ["codeSnippet"] = mcd.CodeSnippet,
                ["insertAfter"] = mcd.InsertAfter,
                ["requiredUsing"] = mcd.RequiredUsing
            },
            AddConfigDetails acd => new Dictionary<string, object>
            {
                ["file"] = acd.File,
                ["jsonPath"] = acd.JsonPath,
                ["value"] = acd.Value,
                ["envVarAlternative"] = acd.EnvVarAlternative ?? ""
            },
            ManualStepDetails msd => new Dictionary<string, object>
            {
                ["instructions"] = msd.Instructions,
                ["links"] = msd.Links ?? new List<string>()
            },
            _ => new Dictionary<string, object>()
        };
    }

    public static OnboardingAction CreateAction(
        string id,
        ActionType type,
        string description,
        ActionDetails details,
        int order,
        params string[] dependsOn)
    {
        return new OnboardingAction
        {
            Id = id,
            Type = type,
            Description = description,
            Details = details.ToDictionary(),
            Order = order,
            DependsOn = dependsOn.ToList()
        };
    }
}
