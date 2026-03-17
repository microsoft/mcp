1. Containerization:
    - If Dockerfiles exist, list their paths.
    - If missing: Create dockerfiles for each application/service.
        - Define a valid base image based on the project's language.
        - Copy the required files into the container.
        - Set the working directory.
        - Install dependencies and build the project.
        - Expose the listening ports.
        - Define an entrypoint
        - Create .dockerignore if required.
    - Ensure the dockerfiles can be built using 'docker build'. Keep track of each dockerfile created, and its required docker build context path. Agent must create Dockerfile first before deployment!
    - Output: Docker artifacts
2. Env setup for Azure CLI:
    1. Install Azure CLI if not installed.
    2. Ensure there is a default subscription set. If provided, override the default subscription with the provided subscription ID.
    3. Subscription ID: Use default subscription
3. Provision Azure Infrastructure with Azure CLI:
    1. Provisioning tool: Azure CLI. Expected files: terraform.
    2. Grab current subscriptionID, call tool quota_region_availability_list and quota_usage_check to get available region and SKUs for all needed Azure resource types in this project.You must check Microsoft.Compute/virtualMachines quota is available for AKS nodes.
    3. If expected files exist, please follow the below steps:
        - Existing file paths: <filePaths>
        - Check if the Azure resources in the existing files match what we required in the plan. If not, add steps to update the existing files with missing Azure resource.
    4. If expected files do not exist, please follow the below steps:
        - Generate the terraform files in infra/ folder. Call tool deploy_iac_rules_get to get IaC rules
        - Add a provisioning script to provision resources with `terraform` command.
        - Ensure to run `terraform init` and `terraform validate` before `terraform apply` to initialize the working directory.
    5. Validate all generated files to ensure they are runnable and free of syntax errors:
        - Call 'get_errors' on all generated files and iterate until all errors are resolved.
        - Call 'terraform validate' and fix any errors.
    6. Run the generated Terraform script to provision the resources and confirm the result.
