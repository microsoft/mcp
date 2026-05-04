using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Monitor.Models;

[JsonConverter(typeof(JsonStringEnumConverter<Language>))]
public enum Language
{
    DotNet,
    Java,
    Python,
    NodeJs,
    Go,
    Unknown
}

[JsonConverter(typeof(JsonStringEnumConverter<AppType>))]
public enum AppType
{
    // .NET
    AspNetCore,
    Worker,
    AzureFunctions,
    Console,
    Library,
    AspNetClassic,
    AspNetMvc,
    AspNetWebForms,
    WcfService,
    OwinApp,
    // Node.js
    Express,
    Fastify,
    NestJs,
    NextJs,
    LangchainJs,
    // Node.js Database Integrations
    PostgresNodeJs,
    MongoDBNodeJs,
    RedisNodeJs,
    MySQLNodeJs,
    // Node.js Logging Integrations
    WinstonNodeJs,
    BunyanNodeJs,
    ConsoleNodeJs,
    // Python
    Django,
    Flask,
    FastAPI,
    Falcon,
    Starlette,
    GenAI,
    Unknown
}

[JsonConverter(typeof(JsonStringEnumConverter<InstrumentationState>))]
public enum InstrumentationState
{
    Greenfield,
    Brownfield
}

[JsonConverter(typeof(JsonStringEnumConverter<InstrumentationType>))]
public enum InstrumentationType
{
    ApplicationInsightsSdk,
    OpenTelemetry,
    AzureMonitorDistro,
    Other
}

[JsonConverter(typeof(JsonStringEnumConverter<ActionType>))]
public enum ActionType
{
    ReviewEducation,
    AddPackage,
    ModifyCode,
    AddConfig,
    ManualStep
}

[JsonConverter(typeof(JsonStringEnumConverter<HostingPattern>))]
public enum HostingPattern
{
    /// <summary>
    /// ASP.NET Core minimal API pattern: WebApplication.CreateBuilder
    /// </summary>
    MinimalApi,

    /// <summary>
    /// Generic host pattern: Host.CreateDefaultBuilder or Host.CreateApplicationBuilder
    /// </summary>
    GenericHost,

    /// <summary>
    /// Could not determine hosting pattern
    /// </summary>
    Unknown
}
