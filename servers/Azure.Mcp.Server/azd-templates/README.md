# Azure MCP remote hosting templates

Azure MCP can be hosted as a remote MCP server in an Azure Container App. Here are some Azure Developer CLI (azd) templates that can deploy such such a remotely self-hosted Azure MCP server. You may extend these templates for your own needs or learn from them on how Azure MCP remote hosting works.

## ACA with Foundry agent using managed identity

This template deploys an Azure Container App to run Azure MCP as a remote MCP server using a managed identity assigned to the container app. THe MCP server will have the permissions to call downstream APIs granted to the assigned managed identity. This hosting option is a good fit if you plan to give the same level of access to all users of the MCP server.

This template also gives instructions on how to connect to this server from a Foundry Agent. Follow the instructions in https://github.com/Azure-Samples/azmcp-foundry-aca-mi to deploy such a a remotely self-hosted Azure MCP Server.

## ACA with Copilot Studio agent using managed identity

This template deploys an Azure Container App to run Azure MCP as a remote MCP server using a managed identity assigned to the container app. The MCP server will have the permissions to call downstream APIs granted to the assigned managed identity. This hosting option is a good fit if you plan to give the same level of access to all users of the MCP server.

This template also gives instructions on how to connect to this server from a Copilot Studio Agent. Follow the instructions in https://github.com/Azure-Samples/azmcp-copilot-studio-aca-mi to deploy such a remotely self-hosted Azure MCP Server.

## ACA with On-Behalf-Of flow

This template deploys an Azure Container App to run Azure MCP as a remote MCP server that perform On-Behalf-Of flow. The MCP server will have permissions to call downstream APIs granted to the user principal accessing the MCP server. This hosting option is a good fit if you plan to grant users with different levels of permissions access to the MCP server and limit what they can do by their permissions.

This template also gives instructions on how to connect from a C# console app, instructions on how to connect from a Foundry Agent, and instructions on how to connect from a Copilot Studio Agent. Follow the instructions in https://github.com/Azure-Samples/azmcp-obo-template to deploy such a remotely self-hosted Azure MCP Server.