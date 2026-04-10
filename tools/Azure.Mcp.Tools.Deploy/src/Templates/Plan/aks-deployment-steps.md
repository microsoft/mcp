4. Deployment:
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
