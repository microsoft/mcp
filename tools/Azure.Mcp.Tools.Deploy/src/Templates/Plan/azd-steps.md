1. Provision Azure Infrastructure{{DeployTitle}}:
    1. Agent must call tool #mcp_azure_mcp_azd with input command='plan_init' to get instructions for AZD project initialization. And call tool #mcp_azure_mcp_azd with input command='discovery_analysis' to get instructions for performing comprehensive discovery and analysis of application components to prepare for azd initialization. {{IaCRuleTool}}
    2. Generate IaC ({{IacType}} files) for required azure resources based on the plan.
    3. Pre-check: use `get_errors` tool to check generated Bicep grammar errors. Fix the errors if exist.
    4. Run the AZD command `azd up` to provision the resources and confirm each resource is created or already exists.
    5. Check the deployment output to ensure the resources are provisioned successfully.
    {{CheckLog}}
