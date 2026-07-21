// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Deploy.Schemas;

internal static class DeployAppTopologySchema
{
    public const string Schema = """
    {
        "type": "object",
        "properties": {
            "workspaceFolder": {
                "type": "string",
                "description": "The full path of the workspace folder."
            },
            "projectName": {
                "type": "string",
                "description": "The name of the project. This is used to generate the resource names."
            },
            "services": {
                "type": "array",
                "description": "An array of service parameters.",
                "items": {
                    "type": "object",
                    "properties": {
                        "name": {
                            "type": "string",
                            "description": "The name of the service."
                        },
                        "path": {
                            "type": "string",
                            "description": "The relative path of the service main project folder"
                        },
                        "language": {
                            "type": "string",
                            "description": "The programming language of the service."
                        },
                        "port": {
                            "type": "string",
                            "description": "The port number the service uses. Get this from Dockerfile for container apps. If not available, default to '80'."
                        },
                        "azureComputeHost": {
                            "type": "string",
                            "description": "The appropriate azure service that should be used to host this service. Use containerapp if the service is containerized and has a Dockerfile.",
                            "enum": [
                                "appservice",
                                "containerapp",
                                "function",
                                "staticwebapp",
                                "aks"
                            ]
                        },
                        "dockerSettings": {
                            "type": "object",
                            "description": "Docker settings for the service. This is only needed if the service's azureComputeHost is containerapp.",
                            "properties": {
                                "dockerFilePath": {
                                    "type": "string",
                                    "description": "The absolute path to the Dockerfile for the service. If the service's azureComputeHost is not containerapp, leave blank."
                                },
                                "dockerContext": {
                                    "type": "string",
                                    "description": "The absolute path to the Docker build context for the service. If the service's azureComputeHost is not containerapp, leave blank."
                                }
                            },
                            "required": [
                                "dockerFilePath",
                                "dockerContext"
                            ]
                        },
                        "dependencies": {
                            "type": "array",
                            "description": "An array of dependent services. A compute service may have a dependency on another compute service.",
                            "items": {
                                "type": "object",
                                "properties": {
                                    "name": {
                                        "type": "string",
                                        "description": "The name of the dependent service. Can be arbitrary, or must reference another service in the services array if referencing appservice, containerapp, staticwebapps, aks, or functionapp."
                                    },
                                    "serviceType": {
                                        "type": "string",
                                        "description": "The name of the azure service that can be used for this dependent service.",
                                        "enum": [
                                            "azureaisearch",
                                            "azureaiservices",
                                            "appservice",
                                            "azureapplicationinsights",
                                            "azurebotservice",
                                            "containerapp",
                                            "azurecosmosdb",
                                            "functionapp",
                                            "azurekeyvault",
                                            "aks",
                                            "azuredatabaseformysql",
                                            "azureopenai",
                                            "azuredatabaseforpostgresql",
                                            "azureprivateendpoint",
                                            "azurecacheforredis",
                                            "azuresqldatabase",
                                            "azurestorageaccount",
                                            "staticwebapp",
                                            "azureservicebus",
                                            "azuresignalrservice",
                                            "azurevirtualnetwork",
                                            "azurewebpubsub"
                                        ]
                                    },
                                    "connectionType": {
                                        "type": "string",
                                        "description": "The connection authentication type of the dependency.",
                                        "enum": [
                                            "http",
                                            "secret",
                                            "system-identity",
                                            "user-identity",
                                            "bot-connection"
                                        ]
                                    },
                                    "environmentVariables": {
                                        "type": "array",
                                        "description": "An array of environment variables defined in source code to set up the connection.",
                                        "items": {
                                            "type": "string"
                                        }
                                    }
                                },
                                "required": [
                                    "name",
                                    "serviceType",
                                    "connectionType",
                                    "environmentVariables"
                                ]
                            }
                        },
                        "settings": {
                            "type": "array",
                            "description": "An array of environment variables needed to run this service.  Please search the entire codebase to find environment variables.",
                            "items": {
                                "type": "string"
                            }
                        }
                    },
                    "required": [
                        "name",
                        "path",
                        "azureComputeHost",
                        "language",
                        "port",
                        "dependencies",
                        "settings"
                    ]
                }
            }
        },
        "required": [
            "workspaceFolder",
            "services"
        ]
    }
    """;
}
