# {{Title}}

{Agent should fill in and polish the markdown template below to generate a deployment plan for the project. Then save it to '.azure/plan.copilotmd' file. Don't add cost estimation! Don't add extra validation steps unless it is required! Don't change the tool name!}

## **Goal**
{{Goal}}

## **Project Information**
{
briefly summarize the project structure, services, and configurations, example:
AppName: web
- **Technology Stack**: ASP.NET Core 7.0 Razor Pages application
- **Application Type**: Task Manager web application with client-side JavaScript
- **Containerization**: Ready for deployment with existing Dockerfile
- **Dependencies**: No external dependencies detected (database, APIs, etc.)
- **Hosting Recommendation**: Azure Container Apps for scalable, serverless container hosting
}

## **Azure Resources Architecture**
> **Install the mermaid extension in IDE to view the architecture.**
{a mermaid graph of following recommended azure resource architecture. Only keep the most important edges to make structure clear and readable.}
{
List how data flows between the services, example:
- The container app gets its image from the Azure Container Registry.
- The container app gets requests and interacts with the Azure SQL Database for data storage and retrieval.
}
Sample mermaid:
{{SampleMermaid}}

{{ResourceInfo}}
## **Execution Step**
> **Below are the steps for Copilot to follow; ask Copilot to update or execute this plan.**
{{ExecutionSteps}}
