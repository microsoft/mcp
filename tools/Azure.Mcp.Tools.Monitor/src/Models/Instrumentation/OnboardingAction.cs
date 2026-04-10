namespace Azure.Mcp.Tools.Monitor.Models;

public record OnboardingAction
{
    public string Id { get; init; } = null!;
    public ActionType Type { get; init; }
    public string Description { get; init; } = null!;
    public Dictionary<string, object> Details { get; init; } = [];
    public int Order { get; init; }
    public List<string> DependsOn { get; init; } = [];
}

// Action Details - Strongly-typed alternatives to Dictionary
public abstract record ActionDetails;

public record ReviewEducationDetails(List<string> Resources) : ActionDetails;

public record AddPackageDetails(
    string PackageManager,
    string Package,
    string Version,
    string TargetProject
) : ActionDetails;

public record ModifyCodeDetails(
    string File,
    string CodeSnippet,
    string InsertAfter,
    string RequiredUsing
) : ActionDetails;

public record AddConfigDetails(
    string File,
    string JsonPath,
    string Value,
    string? EnvVarAlternative = null
) : ActionDetails;

public record ManualStepDetails(
    string Instructions,
    List<string>? Links = null
) : ActionDetails;
