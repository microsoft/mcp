# Azure MCP Server - ACA with Copilot Studio agent

This document explains how to deploy the [Azure MCP Server 2.0-beta](https://mcr.microsoft.com/product/azure-sdk/azure-mcp) as a remote MCP server accessible over HTTPS. This enables AI agents from [Azure AI Foundry](https://azure.microsoft.com/products/ai-foundry) and [Microsoft Copilot Studio](https://www.microsoft.com/microsoft-copilot/microsoft-copilot-studio) to securely invoke MCP tools that perform Azure operations on your behalf. It also explains how to configure a Copilot Studio Agent to connect to such a remotely self-hosted Azure MCP server.

## Prerequisite

- License to use Power Platform features
  - Copilot Studio
  - Power Apps
- An Azure subscription with **Owner** or **User Access Administrator** permissions
- [Azure Developer CLI (azd)](https://learn.microsoft.com/azure/developer/azure-developer-cli/install-azd)
- The list of Azure MCP Server tool areas (namespaces) you wish to enable (see [azmcp-commands.md](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/azmcp-commands.md)). This reference template uses the `storage` namespace.

## Quick start

This reference template deploys the Azure MCP Server with **read-only** Azure Storage tools enabled, accessible over HTTPS transport. For details on customizing server startup flags and configuration, see [Azure MCP Server documentation](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/azmcp-commands.md).

```bash
azd up
```

For the first time running this command, you will be prompted to sign in to your Azure account and give the following information:

- **Environment Name** - A user friendly name for managing azd deployments.
- **Azure Subscription** - The Azure subscription in which to create the resources.
- **Resource Group** - The resource group in which to create the resources. You can create a new resource group on demand during this step.

## What gets deployed

- **Container App** - Runs Azure MCP Server with storage namespace.
- **User-assigned managed identity** - A user-assigned managed identity with the Subscription Reader role for the subscription the resources are deployed to. This managed identity will be assigned to the container app and used by the Azure MCP server to make tool calls. 
- **Entra App Registration (Azure MCP Server)** - For incoming OAuth 2.0 authentication from clients (e.g. custom connector) with `Mcp.Tools.ReadWrite` scope. This scope will be requested by the client app accessing the server.
- **Entra App Registration (Client)** - For Power Apps custom connector to connect to the remote Azure MCP Server.
- **Application Insights** - Telemetry and monitoring.

> Note: this template creates the server app registration and the client app registration in the same tenant. It also sets the client app as a pre-authorized app of the server app. This allows it to bypass the explicit consent flow when using the client app to access the server app. If you prefer explicitly giving consent for the client app, please refer to [known issues](#known-issues) for available options.

### Deployment outputs

After deployment, retrieve `azd` outputs:

```bash
azd env get-values
```

Among the output there are useful values for the subsequent steps. Here is an example of these values.

```
AZURE_RESOURCE_GROUP="<your_resource_group_name>"
AZURE_SUBSCRIPTION_ID="<your_subscription_id>"
AZURE_TENANT_ID="<your_tenant_id>"
CONTAINER_APP_NAME="azure-mcp-storage-server"
CONTAINER_APP_URL="https://azure-mcp-storage-server.whitewave-ff25acf5.westus3.azurecontainerapps.io"
ENTRA_APP_CLIENT_CLIENT_ID="<your_client_app_registration_client_id>"
ENTRA_APP_SERVER_CLIENT_ID="<your_server_app_registration_client_id>"
```

You also need to add the created API scope as one of the permissions of the client app registration. Go to Azure Portal and search for the client app registration using the ENTRA_APP_CLIENT_CLIENT_ID output value. In the `API permissions` blade, click `Add a permission`, select server app registration in the `My APIs` tab and add the 'Mcp.Tools.ReadWrite' scope.

## Calling tools from Copilot Studio Agent

Copilot Studio Agent connect to MCP servers via a custom connector.

### Configure a custom connector

Login to [Power Apps](https://make.powerapps.com) and select the environment to host the custom connector. Create a custom connector following the steps in the UI. Here we need to select the `Create from blank` option. To learn more about custom connector configuration, refer to [create custom connector from scratch](https://learn.microsoft.com/connectors/custom-connectors/define-blank).

#### General

- Give a descriptive name and description for the custom connector.
- Set `Scheme` to be `HTTPS`.
- Set the `Host` to be the Container App URL from the CONTAINER_APP_URL output value.

![custom-connector-general](./images/custom-connector-general.png)

#### Swagger editor

Skip the Security step for now and click the `Swagger editor` to enter the swagger editor view. In the swagger editor view

- Set the path such that a POST method is exposed at the root with a custom `x-ms-agentic-protocol: mcp-streamable-1.0` property. This custom property is necessary for the custom connector to interact with this API using the MCP protocol. Refer to [custom connector swagger example](./custom-connector-swagger-example.yaml) as an example.

![custom-connector-swagger-editor](./images/custom-connector-swagger-editor.png)

#### Security

Go to the Security step.

- Select `OAuth 2.0` as the Authentication type.
- Select `Azure Active Directory` as the Identity Provider.
- Set `Client ID` as the client ID of the client app registration provisioned before. You can get this from the ENTRA_APP_CLIENT_CLIENT_ID output value.
- Choose `Use client secret` or `Use managed identity` as the `Secret options`.
  - If you choose to use client secret, go to Azure Portal and create a client secret under the client app registration. Then copy the client secret value and paste it into the client secret field in the Security step.
  - If you choose to use managed identity. Proceed with the rest of the steps until the custom connector is created.
- Keep Authorization URL as `https://login.microsoftonline.com`.
- Set `Tenant ID` to the tenant ID of the client app registration. You can get this from the AZURE_TENANT_ID output value.
- Set `Resource URL` to the client ID of the server app registration. You can get this from the ENTRA_APP_SERVER_CLIENT_ID output value.
- Set `Enable on-behalf-of login` to true.
- Set `Scope` to `<server app registration client ID>/.default`.

![custom-connector-security](./images/custom-connector-security.png)

#### Create the connector

- Click `Create connector` and wait for it to complete. After the custom connector is created, it will give you a Redirect URL, and optionally a Managed Identity if you chose to use managed identity as the secret options.
- Go to Azure Portal and add a redirect URI under the Web platform in the client app registration.
- If you chose to use managed identity as the secret options, create a Federated Credentials in the client app registration. In the creation UI, select `Other issuer` as the `Federated credential scenario`. Then copy paste the `issuer` and the `subject` of the Federated Credentials value from the custom connectors to corresponding fields in the credential creation UI. Give it a descriptive name and description, and then click `Add`.

![client-app-redirect-uri](./images/client-app-redirect-uri.png)
![client-app-client-credential](./images/client-app-client-credential.png)

#### Test connection

- Open the created custom connector, click `Edit` and go to the `Test` step.
- Select any operation and click the `New connection` button in the UI.
- A new window should pop up to have you sign in to your user account. Sign in to the user account you plan to use to access the MCP tools. You might see the dialog asking you give consent to grant the client app registration access or telling you that you need an admin to give consent. If you don't know what you should do, please refer to the [known issues](#known-issues) for more details.

If everything works fine, after signing into the user account, the UI should indicate a connection is created successfully. If you encounter any error message during the sign-in, please refer to the [known issues](#known-issues) section, troubleshoot with your tenant admin or let us know.

![custom-connector-created-connection](./images/custom-connector-created-connection.png)

### Call Azure MCP tool in Copilot Studio test playground

- Login to [Copilot Studio](https://copilotstudio.microsoft.com) and select the environment to host the Copilot Studio Agent. You may create a new Agent or use an existing one.
- Click to view the details of the Agent and navigate to its `Tools` tab.
- Click `Add a tool`.
- Search for your custom connector name and select to add it.
- After adding the custom connector, the Copilot Studio Agent will attempt to list the tools from the MCP server. If everything works fine, you should see the correct list of tools show up in the details under the added custom connector.
- Click the `Test` button to start a test playground session.
- You can prompt the agent to call the MCP tools, such as asking it to list storage accounts in the subscription.

![copilot-studio-tools-tab](./images/copilot-studio-tools-tab.png)
![copilot-studio-call-tools](./images/copilot-studio-call-tools.png)

## Clean Up

If you no longer need the Azure resources created by this template, run this command to delete them.

```bash
azd down
```

If you need to clean up the Power Platform resources, please use the Power Platform UI to delete the Copilot Studio Agent, Power Apps custom connector and connection.

## Template Structure

The `azd` template consists of the following Bicep modules:

- **`main.bicep`** - Orchestrates the deployment of all resources.
- **`aca-storage-managed-identity.bicep`** - Create a user-assigned managed identity
- **`aca-storage-subscription-role.bicep`** - Assigns an Azure RBAC role to the user-assigned managed identity, which defaults to Subscription Reader.
- **`aca-infrastructure.bicep`** - Deploys Container App hosting the Azure MCP Server.
- **`entra-app.bicep`** - Creates Entra App registrations.
- **`application-insights.bicep`** - Deploys Application Insights for telemetry and monitoring (conditional deployment).

## Known issues

- Power Apps custom connector doesn't support authenticating users from multiple tenants. As a result, the client app registration must be configured to only accept users from its tenant.
- As a part of the authentication flow, the user/tenant admin needs to give explicit consent to grant the client app access to their data. To learn more about the consent experience, refer to [application consent experience](https://learn.microsoft.com/entra/identity-platform/application-consent-experience). There are multiple ways to give consent to the client app.
  - A user may give consent in the sign-in process just for this user. This may be prohibited by tenant security policy.
  - A tenant admin may give consent for all users in the tenant in the client app registration under the `API permissions` blade in Azure Portal.
  - The server app registration can add the client app registration as an pre-authorized client app under the `Expose an API` blade in Azure Portal.
- If the client app registration and server app registration are in different tenants, you may see the following error when trying to create the connection.
  - "The app is trying to access a service 'server_app_registration_client_id'(server_app_registration_display_name) that your organization 'client_app_registration_tenant' lacks a service principal for". In this case, a tenant admin of the client app registration must provision a service principal for the server app registration in that tenant. This can be done via an Azure CLI command `az ad sp create --id <server_app_registration_client_id>`. After the service principal is provisioned, trying to create the connection again should trigger the consent flow.
- If the Power Apps environment has tenant isolation policy, it will block the data flow if the client app registration or the server app registration are in different tenants. To learn more about how to add exception rules to allow these data flow, refer to [cross tenant restrictions](https://learn.microsoft.com/power-platform/admin/cross-tenant-restrictions).