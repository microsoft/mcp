- MANDATORY: Add a Microsoft.Authorization/roleAssignments resource to assign the AcrPull (7f951dda-4ed3-4680-a7ca-43fe172d538d) role to the user-assigned managed identity (Only one instance is required per-container registry. Define this BEFORE any container apps.)
- Use this identity (NOT system) to connect to the container registry. A registry connection needs to be created even if we are using a template base image.
- Container Apps MUST use base container image mcr.microsoft.com/azuredocs/containerapps-helloworld:latest. The property is set via properties.template.containers.image.
- Use 'properties.configuration.registries' to set the container registry connection.
- Enable CORS via properties.configuration.ingress.corsPolicy.
- Define all used secrets; Use Key Vault if possible.
- MANDATORY: You MUST add Key Vault secrets and their related role assignment in explicit dependencies to use these secrets.
- IMPORTANT NOTES: If you meet deployment error: "Field 'configuration.secrets' is invalid with details: Invalid value: <secret-name>: unable to get value using managed identity". DO NOT use key vault references first, use connection string directly in container app environment variable and deploy.
  After deployment is successful, get IP from the container app, switch to use key vault references in secrets, and 1) add the IP to key vault network rules 2) set defaultAction: 'Deny'. This is a workaround for the issue that managed identity cannot access key vault when key vault has network rules enabled.
- Container App Environment must be connected to Log Analytics Workspace. Use logAnalyticsConfiguration -> customerId=logAnalytics.properties.customerId and sharedKey=logAnalytics.listKeys().primarySharedKey.
