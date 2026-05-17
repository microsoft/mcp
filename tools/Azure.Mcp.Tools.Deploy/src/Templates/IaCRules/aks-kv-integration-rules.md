- To Access Key Vault secrets in AKS cluster, you need to export some configuration, then set them in env variables to create some Kubernetes resources. Example:
1) Create a SecretProviderClass. Example:
```
apiVersion: secrets-store.csi.x-k8s.io/v1
kind: SecretProviderClass
metadata:
  name: azure-kvname-user-msi
spec:
  provider: azure
  parameters:
    usePodIdentity: "false"
    useVMManagedIdentity: "true"            # Set to true for using managed identity
    userAssignedIdentityID: ${AKS_KV_IDENTITY_CLIENT_ID}   # The clientID of the AKS Key Vault Secrets Provider identity, it can be got after AKS creation
    keyvaultName: ${KEY_VAULT_NAME}          # The name of the Key Vault
    objects:  |
      array:
        - |
          objectName: postgres-connection-string
          objectType: secret              # object types: secret, key, or cert
    tenantId: ${TENANT_ID}                   # The tenant ID of the key vault
  secretObjects:                          
    - secretName: test-secrets-obj1       # it will create a Kubernetes secret in cluster with the name 'test-secrets-obj1'
      type: Opaque
      data:
      - objectName: postgres-connection-string    # must use the same objectName as above
        key: postgres-connection-string
```
2) In the deployment YAML file, mount the secret to pod. Example:
```
kind: Pod
apiVersion: v1
metadata:
  name: busybox-secrets-store-inline-user-msi
spec:
  containers:
    - name: busybox
      image: registry.k8s.io/e2e-test-images/busybox:1.29-4
      command:
        - "/bin/sleep"
        - "10000"
      env:
      - name: SPRING_DATASOURCE_URL     # Important: set the env variable to use the secret
        valueFrom:
          secretKeyRef:
            name: test-secrets-obj1    # use the secretName defined in secretObjects above
            key: postgres-connection-string  # use the objectName defined in secretObjects above
      volumeMounts:                    # Important: mount the volume
      - name: secrets-store01-inline
        mountPath: "/mnt/secrets-store"
        readOnly: true
  volumes:                            # Important: define the volume
    - name: secrets-store01-inline  
      csi:
        driver: secrets-store.csi.k8s.io
        readOnly: true
        volumeAttributes:
          secretProviderClass: "azure-kvname-user-msi"  # Use the SecretProviderClass name defined above
```
