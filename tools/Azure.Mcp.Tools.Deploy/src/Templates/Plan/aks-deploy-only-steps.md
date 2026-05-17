1. Env setup for Azure CLI:
    1. Install Azure CLI if not installed.
    2. Ensure there is a default subscription set. If provided, override the default subscription with the provided subscription ID.
    3. Subscription ID: Use default subscription
2. Check Azure resources existence:
    1. Azure Kubernetes Service:
        - AKS cluster name: <>, resource group: <>, subscription: <>, provisioningState: Succeeded, node count: <>, vmSize: <e.g. Standard_D4ds_v5>, osType: <e.g. Linux>. Check with 'az aks show'
        - Check dependencies existence for all the resources connected to this service in the following format:
            1. <azure-resource-type>: name: <>, resource group: <>.
            2. <azure-resource-type(one of mysql-flexible/postgres-flexible/sql)>: name: <>, database: <>, resource group: <>. connection string: <>.
            If not exists, ask if user want to 1) create a built-in resource in Kubernetes cluster (default). 2) have an Azure resource and provide its id, then get the resource information with Azure CLI command
    2. Azure Container Registry:
        - login server: <>. Check with 'az acr show -o json'
    3. Create missing resources:
        - If any resource is missing, ask user to provide the resource id or create a new one, then get the resource information with Azure CLI command
        - If user want to create new resources, generate a script to do so using Azure CLI command. Run the script and confirms all resources are ready.
3. Deployment:
    1. Azure Kubernetes Service Deployment:
        1. If Kubernetes manifests/Helm charts exist:
            - Existing file paths: <filePaths>
            - Check if the existing files match what we required in the plan and list the necessary changes, e.g. using Azure Container Registry for image
        2. If expected files do not exist:
            - Check if the project prefer to use Kubernetes Manifests(default) or Helm charts, and create the files accordingly.
        3. Prepare the deployment script (build + push image to ACR, deploy to AKS with kubectl/helm).
        4. Deploy with the files and verify the output. If errors occur, **fix the files until it works**
        5. Output: Kubernetes YAML files/Helm Charts, deployment script
    2. Deployment Validation:
        1. Check the deployed application is running.
