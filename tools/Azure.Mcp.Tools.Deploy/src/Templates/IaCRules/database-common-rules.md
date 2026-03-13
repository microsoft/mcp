- Add firewall rules to allow traffic from Azure Services (allow IP 0.0.0.0).
- If username and password are required, you MUST leave them as params.
- Create secrets in Key Vault to store the connection string or credentials, and assign `Key Vault Secrets User` role to the user-assigned managed identity.
- If app used Managed Identity to connect the database, MUST add a post-provision step to use Service Connector create a connection between containerapp and database: 
    1) Prerequisites: Ensure the Service Connector extension installed in local environment: 'az extension add --name serviceconnector-passwordless'. Check if the app uses user-assigned managed identity or system-assigned managed identity.
    2) Run Command: az containerapp connection create mysql-flexible/postgres-flexible/sql --connection connectionname --user-identity client-id=XX subs-id=XX --source-id <azure resource id > --tg <db resource group> --server servername --database dbname -y. 
    3) Command parameter: use '--user-identity client-id=$clientId subs-id=$subsId' or '--system-identity' based on the container app's configuration. Don't use both. For param '--client-type', it can be 'springBoot, java, nodejs, dotnet, python, none'. For container app, add param '-c containername'. For AKS, add '--kube-namespace <>' and use the deployment's namespace. Don't mock and add param in command that doesn't exist. 
    4) Check command output: 'configurations.name' property name means an env variable name and you should use it in your application to connect to the database.
    5) If app is Java spring boot app connected to database with Managed Identity, Service Connector connection will set the database configuration automatically, don't add SPRING_DATASOURCE env variable in container app.
