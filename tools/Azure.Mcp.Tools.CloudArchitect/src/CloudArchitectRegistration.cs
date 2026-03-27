// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Azure.Mcp.Tools.CloudArchitect.Commands.Design;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Azure.Mcp.Tools.CloudArchitect;

public sealed class CloudArchitectRegistration : IAreaRegistration
{
    public static string AreaName => "cloudarchitect";

    public static string AreaTitle => "Azure Cloud Architecture";

    public static CommandCategory Category => CommandCategory.AzureServices;

    public static CommandGroupDescriptor GetCommandDescriptors() => new()
    {
        Name = AreaName,
        Description = "Cloud Architecture operations - Commands for generating Azure architecture designs and recommendations based on requirements.",
        Title = AreaTitle,
        Commands =
        [
            new CommandDescriptor
            {
                Id = "aa7c2a8b-c664-423b-8fb5-8edfbdadc783",
                Name = "design",
                Description = "Recommends architecture design for cloud services/apps/solutions, such as: file storage, banking, video streaming, e-commerce, SaaS, and more. Use as follows: 1. Ask about user role, business goals, etc (1-2 questions at a time). 2. Track confidence returned by service and update requirements (explicit/implicit/assumed). 3. Repeat steps 1 and 2 as needed until confidence >= 0.7 4. Present architecture with table format, visual organization, ASCII diagrams. 5. Follow Azure Well-Architected Framework principles. 6. Cover all tiers: infrastructure, platform, application, data, security, operations. 7. Provide actionable advice and high-level overview. Note: State tracks components, requirements by category, and confidence factors. Be conservative with suggestions.",
                Title = "Design",
                Annotations = new ToolAnnotations
                {
                    Destructive = false,
                    Idempotent = true,
                    OpenWorld = false,
                    ReadOnly = true,
                    LocalRequired = false,
                    Secret = false,
                },
                Options =
                [
                    new OptionDescriptor
                    {
                        Name = "question",
                        Description = "The current question being asked",
                        TypeName = "string",
                    },
                    new OptionDescriptor
                    {
                        Name = "question-number",
                        Description = "Current question number",
                        TypeName = "string",
                    },
                    new OptionDescriptor
                    {
                        Name = "total-questions",
                        Description = "Estimated total questions needed",
                        TypeName = "string",
                    },
                    new OptionDescriptor
                    {
                        Name = "answer",
                        Description = "The user's response to the question",
                        TypeName = "string",
                    },
                    new OptionDescriptor
                    {
                        Name = "next-question-needed",
                        Description = "Whether another question is needed",
                        TypeName = "string",
                    },
                    new OptionDescriptor
                    {
                        Name = "confidence-score",
                        Description = "A value between 0.0 and 1.0 representing confidence in understanding requirements. When this reaches 0.7 or higher, nextQuestionNeeded should be set to false.",
                        TypeName = "string",
                    },
                    new OptionDescriptor
                    {
                        Name = "state",
                        Description = "The complete architecture state from the previous request as JSON, State input schema: { \"state\":{ \"type\":\"object\", \"description\":\"The complete architecture state from the previous request\", \"properties\":{ \"architectureComponents\":{ \"type\":\"array\", \"description\":\"All architecture components suggested so far\", \"items\":{ \"type\":\"string\" } }, \"architectureTiers\":{ \"type\":\"object\", \"description\":\"Components organized by architecture tier\", \"additionalProperties\":{ \"type\":\"array\", \"items\":{ \"type\":\"string\" } } }, \"thought\":{ \"type\":\"string\", \"description\":\"The calling agent's thoughts on the next question or reasoning process. The calling agent should use the requirements it has gathered to reason about the next question.\" }, \"suggestedHint\":{ \"type\":\"string\", \"description\":\"A suggested interaction hint to show the user, such as 'Ask me to create an ASCII art diagram of this architecture' or 'Ask about how this design handles disaster recovery'.\" }, \"requirements\":{ \"type\":\"object\", \"description\":\"Tracked requirements organized by type\", \"properties\":{ \"explicit\":{ \"type\":\"array\", \"description\":\"Requirements explicitly stated by the user\", \"items\":{ \"type\":\"object\", \"properties\":{ \"category\":{ \"type\":\"string\" }, \"description\":{ \"type\":\"string\" }, \"source\":{ \"type\":\"string\" }, \"importance\":{ \"type\":\"string\", \"enum\":[ \"high\", \"medium\", \"low\" ] }, \"confidence\":{ \"type\":\"number\" } } } }, \"implicit\":{ \"type\":\"array\", \"description\":\"Requirements implied by user responses\", \"items\":{ \"type\":\"object\", \"properties\":{ \"category\":{ \"type\":\"string\" }, \"description\":{ \"type\":\"string\" }, \"source\":{ \"type\":\"string\" }, \"importance\":{ \"type\":\"string\", \"enum\":[ \"high\", \"medium\", \"low\" ] }, \"confidence\":{ \"type\":\"number\" } } } }, \"assumed\":{ \"type\":\"array\", \"description\":\"Requirements assumed based on context/best practices\", \"items\":{ \"type\":\"object\", \"properties\":{ \"category\":{ \"type\":\"string\" }, \"description\":{ \"type\":\"string\" }, \"source\":{ \"type\":\"string\" }, \"importance\":{ \"type\":\"string\", \"enum\":[ \"high\", \"medium\", \"low\" ] }, \"confidence\":{ \"type\":\"number\" } } } } } }, \"confidenceFactors\":{ \"type\":\"object\", \"description\":\"Factors that contribute to the overall confidence score\", \"properties\":{ \"explicitRequirementsCoverage\":{ \"type\":\"number\" }, \"implicitRequirementsCertainty\":{ \"type\":\"number\" }, \"assumptionRisk\":{ \"type\":\"number\" } } } } } }",
                        TypeName = "string",
                    },
                ],
                Kind = CommandKind.Global,
                HandlerType = nameof(DesignCommand)
            },
        ],
    };

    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<DesignCommand>();
    }

    public static IBaseCommand ResolveHandler(string handlerTypeName, IServiceProvider serviceProvider) =>
        handlerTypeName switch
        {
            nameof(DesignCommand) => serviceProvider.GetRequiredService<DesignCommand>(),
            _ => throw new InvalidOperationException($"Unknown handler type '{{handlerTypeName}}' in cloudarchitect area.")
        };
}
