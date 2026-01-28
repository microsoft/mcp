Follow ALL of the following rules to provide CI/CD pipeline files and configuration setup instructions:
*Prerequisite Checks*:
{{PrerequisiteChecksPrompt}}
*Pipeline File Generation Guidance*:
- Generate a CI/CD pipeline file using {{DeploymentTool}} for {{PipelinePlatform}}.
- The CI part should include build (and test if applicable). CI should NOT include Azure-related steps, for example, pushing images to ACR.
- The CD part should include multi-environment deployment steps. Images, if applicable, should be pushed to ACR in different environments.
- **'pull_request' should not start CD(deployment).**
- Use different environments according to user-provided Azure resources. Default to use 'dev', 'staging', and 'production' IF NOT SPECIFIED by user. *If user states specific environments, use those.*
- Pay attention to the dependency order among build and multi-environment deployments.
{{PipelineFilePrompt}}
- **You should ONLY care about deployment-related variables like hosting services' names. Ignore application-level variables like connection strings.**
*How you do configurations*:
- Configure Azure authentication and environment setup for the pipeline as per the instructions below.
- You should use a .azure/pipeline-setup.md file to outline the steps. The file should contain the steps and explain the tasks to be done by the user.
{{SetUpMethodPrompt}}
*Azure Authentication Configuration Guidance*:
{{AzureAuthConfigPrompt}}
*Pipeline Environment Setup Guidance*:
    - Create Github environments and set up approval checks in ALL environments.
    - Configure the Github Actions variables and secrets needed for the deployment pipeline for *different environments*.
    - Refer to the managed identity created in the 'Azure Authentication Configuration Guidance' section for OIDC setup.
    - You can use Github CLI and Github CLI or guide user to use UI to finish the setup.
    - AZD IaC check: if Bicep is using resource group scope, AZURE_RESOURCE_GROUP variable should be set to the environment.
