1. Env setup for AzCLI:
    1. Install AZ CLI if not installed.
    2. Ensure there is a default subscription set. If provided, override the default subscription with the provided subscription ID.
    3. Subscription ID: Use default subscription
    (ONLY WHEN the project is using passwordless connection to mysql-flexible/postgres-flexible/sql) 4. Install Service Connector AzCLI extension: az extension add --name serviceconnector-passwordless --upgrade
2. Check Azure resources existence:
    1. For each service in this project, check {{AzureComputeHost}} for app <service-name>:
        - name: <>, resource group: <>, subscription: <>, provisioningState: Succeeded, runningStatus: Running. Check with 'az {{TargetAppCommandTitle}} show -o json'
        - Check dependencies existence for all the resources connected to this service in the following format:
            1. <azure-resource-type>: name: <>, resource group: <>.
            2. <azure-resource-type>: name: <>, database: <>, resource group: <>.
        {{ACRDependencyCheck}}
    2. Create missing resources:
        - If any resource is missing, ask user to provide the resource id or create a new one, then get the resource information with Az CLI command
        - If user want to create new resources, generate a script to do so using Azure CLI command. Run the script and confirms all resources are ready.
3. Deployment:
    1. {{AzureComputeHost}} Deployment:
        {{DeploymentSteps}}
    2. Deployment Validation:
        1. Check the deployed application is running.
4. Summarize Result:
    1. Generate files: .azure/summary.copilotmd to summarize the deployment result.
