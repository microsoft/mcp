# {{Title}}

{Agent should fill in and polish the markdown template below to generate a deployment plan for the project. Then save it to 'docs/plan.copilotmd' file. Don't add cost estimation! Don't add extra validation steps unless it is required! Don't change the tool name!}

## **Goal**
Based on the project to provide a plan to deploy the project to {{AzureComputeHost}} using Azure CLI.

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

## **Recommended Azure Resources**

Recommended App service hosting the project //agent should fulfill this for each app instance
- Application {{ProjectName}}
  - Hosting Service Type: {{AzureComputeHost}} // it can be Azure Container Apps, Web App Service, Azure Functions, Azure Kubernetes Service. Recommend one based on the project.
  - SKU // recommend a sku based on the project, show its performance. Don't estimate the cost.
  - Configuration:
    - language: {language}  //detect from the project, it can be nodejs, python, dotnet, etc.
    - dockerFilePath: {dockerFilePath}// fulfill this if service.azureComputeHost is ContainerApp
    - dockerContext: {dockerContext}// fulfill this if service.azureComputeHost is ContainerApp
    - Environment Variables: [] // the env variables that are used in the project/required by service
  - Dependencies Resource
    - Dependency Name
    - SKU // recommend a sku, show its performance.
    - Service Type // it can be Azure SQL, Azure Cosmos DB, Azure Storage, etc.
    - Connection Type // it can be connection string, managed identity, etc.
    - Environment Variables: [] // the env variables that are used in the project/required by dependency

Recommended Supporting Services
- Application Insights
- Log Analytics Workspace: set all app service to connect to this
- Key Vault(Optional): If there are dependencies such as postgresql/sql/mysql, create a Key Vault to store connection string. If not, the resource should not show.
If there is a Container App, the following resources are required:
- Container Registry


Recommended Security Configurations
If there is a Container App
- User managed identity: Must be assigned to the container app.
- AcrPull role assignment: User managed identity must have **AcrPull** role ("7f951dda-4ed3-4680-a7ca-43fe172d538d") assigned to the container registry.

## **Execution Step**
> **Below are the steps for Copilot to follow; ask Copilot to update or execute this plan.**
{{ExecutionSteps}}
