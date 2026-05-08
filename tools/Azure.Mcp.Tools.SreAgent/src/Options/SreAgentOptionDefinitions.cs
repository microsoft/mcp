// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.SreAgent.Options;

public static class SreAgentOptionDefinitions
{
    public const string AgentNameName = "agent";
    public const string ThreadIdName = "thread-id";
    public const string TaskIdName = "task-id";
    public const string MessageName = "message";
    public const string ConfirmName = "confirm";
    public const string NameName = "name";
    public const string CronExpressionName = "cron-expression";
    public const string DescriptionName = "description";
    public const string MaxIterationsName = "max-iterations";
    public const string TimeoutSecondsName = "timeout-seconds";

    public static readonly Option<string> Agent = new(
        $"--{AgentNameName}"
    )
    {
        Description = "The name of the Azure SRE Agent resource to target.",
        Required = true
    };

    public static readonly Option<string> ThreadId = new($"--{ThreadIdName}")
    {
        Description = "The thread ID.",
        Required = true
    };

    public static readonly Option<string> TaskId = new($"--{TaskIdName}")
    {
        Description = "The scheduled task ID.",
        Required = true
    };

    public static readonly Option<string> Message = new($"--{MessageName}")
    {
        Description = "The message to send.",
        Required = true
    };

    public static readonly Option<bool> Confirm = new($"--{ConfirmName}")
    {
        Description = "Confirms a destructive operation.",
        Required = true
    };

    public static readonly Option<string> Name = new($"--{NameName}")
    {
        Description = "The name of the scheduled task.",
        Required = true
    };

    public static readonly Option<string> CronExpression = new($"--{CronExpressionName}")
    {
        Description = "The cron expression for the schedule.",
        Required = true
    };

    public static readonly Option<string> Description = new($"--{DescriptionName}")
    {
        Description = "The scheduled task description.",
        Required = false
    };

    public static readonly Option<int> MaxIterations = new($"--{MaxIterationsName}")
    {
        Description = "The maximum number of automatic follow-up iterations.",
        Required = false,
        DefaultValueFactory = _ => 20
    };

    public static readonly Option<int> TimeoutSeconds = new($"--{TimeoutSecondsName}")
    {
        Description = "The investigation timeout in seconds.",
        Required = false,
        DefaultValueFactory = _ => 600
    };
}
