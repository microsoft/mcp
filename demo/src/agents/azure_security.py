"""
Agente especializado para Azure Key Vault (Security).
"""

from typing import Any, Dict, List
from src.core.agent_base import Agent, AgentCapability, Task
from src.core.mcp_client import MCPClientPool
from src.utils.logger import AgentLogger
from src.utils.validators import Validators


class AzureSecurityAgent(Agent):
    """
    Agente especializado para operaciones con Azure Key Vault.

    Capacidades:
    - Gestión de secretos (crear, leer, eliminar)
    - Gestión de claves (crear, leer, eliminar)
    - Gestión de certificados (importar, leer, eliminar)
    - Listar secretos, claves y certificados
    """

    def __init__(self, client_pool: MCPClientPool):
        super().__init__(
            name="azure_security",
            description="Agente para gestión de Azure Key Vault",
            mcp_server_name="azure-mcp",
            client_pool=client_pool
        )
        self.logger = AgentLogger("AzureSecurityAgent")

    async def _load_capabilities(self):
        """Carga las capacidades del agente."""
        self.capabilities = [
            AgentCapability(
                name="create_secret",
                description="Crea o actualiza un secreto",
                required_tools=["azure.keyvault.setSecret"],
                parameters={
                    "vault_name": "string",
                    "secret_name": "string",
                    "secret_value": "string"
                }
            ),
            AgentCapability(
                name="get_secret",
                description="Obtiene el valor de un secreto",
                required_tools=["azure.keyvault.getSecret"],
                parameters={
                    "vault_name": "string",
                    "secret_name": "string"
                }
            ),
            AgentCapability(
                name="list_secrets",
                description="Lista todos los secretos",
                required_tools=["azure.keyvault.listSecrets"],
                parameters={
                    "vault_name": "string"
                }
            ),
            AgentCapability(
                name="delete_secret",
                description="Elimina un secreto",
                required_tools=["azure.keyvault.deleteSecret"],
                parameters={
                    "vault_name": "string",
                    "secret_name": "string"
                }
            ),
            AgentCapability(
                name="create_key",
                description="Crea una nueva clave criptográfica",
                required_tools=["azure.keyvault.createKey"],
                parameters={
                    "vault_name": "string",
                    "key_name": "string",
                    "key_type": "string"
                }
            )
        ]

    async def execute_task(self, task: Task) -> Task:
        """Ejecuta una tarea de seguridad."""
        self.logger.task_start(task.description)

        try:
            description_lower = task.description.lower()

            if "secreto" in description_lower or "secret" in description_lower:
                if "crear" in description_lower or "create" in description_lower or "set" in description_lower:
                    task.result = await self._create_secret(task.params)
                elif "obtener" in description_lower or "get" in description_lower or "leer" in description_lower:
                    task.result = await self._get_secret(task.params)
                elif "listar" in description_lower or "list" in description_lower:
                    task.result = await self._list_secrets(task.params)
                elif "eliminar" in description_lower or "delete" in description_lower:
                    task.result = await self._delete_secret(task.params)

            elif "clave" in description_lower or "key" in description_lower:
                if "crear" in description_lower or "create" in description_lower:
                    task.result = await self._create_key(task.params)
                elif "listar" in description_lower or "list" in description_lower:
                    task.result = await self._list_keys(task.params)
                elif "eliminar" in description_lower or "delete" in description_lower:
                    task.result = await self._delete_key(task.params)

            elif "certificado" in description_lower or "certificate" in description_lower:
                if "importar" in description_lower or "import" in description_lower:
                    task.result = await self._import_certificate(task.params)
                elif "listar" in description_lower or "list" in description_lower:
                    task.result = await self._list_certificates(task.params)

            else:
                task.error = "No se pudo determinar la operación de Key Vault"
                return task

            self.logger.task_complete(task.description, 0)

        except Exception as e:
            task.error = str(e)
            self.logger.task_failed(task.description, str(e))

        return task

    async def _create_secret(self, params: Dict[str, Any]) -> Dict[str, Any]:
        """Crea o actualiza un secreto en Key Vault."""
        vault_name = params.get("vault_name")
        secret_name = params.get("secret_name")
        secret_value = params.get("secret_value")

        if not all([vault_name, secret_name, secret_value]):
            raise Exception(
                "Se requiere 'vault_name', 'secret_name' y 'secret_value'"
            )

        # Validar nombre del secreto
        Validators.validate_key_vault_secret_name(secret_name)

        self.logger.info(f"Creando secreto '{secret_name}' en vault '{vault_name}'")

        response = await self.call_mcp_tool(
            "azure.keyvault.setSecret",
            {
                "vaultName": vault_name,
                "secretName": secret_name,
                "value": secret_value,
                "contentType": params.get("content_type", "text/plain")
            }
        )

        if not response.success:
            raise Exception(f"Error al crear secreto: {response.error}")

        return {
            "action": "create_secret",
            "vault_name": vault_name,
            "secret_name": secret_name,
            "status": "created",
            "version": response.data.get("version", "")
        }

    async def _get_secret(self, params: Dict[str, Any]) -> Dict[str, Any]:
        """Obtiene un secreto de Key Vault."""
        vault_name = params.get("vault_name")
        secret_name = params.get("secret_name")

        if not all([vault_name, secret_name]):
            raise Exception("Se requiere 'vault_name' y 'secret_name'")

        self.logger.info(f"Obteniendo secreto '{secret_name}' de vault '{vault_name}'")

        response = await self.call_mcp_tool(
            "azure.keyvault.getSecret",
            {
                "vaultName": vault_name,
                "secretName": secret_name,
                "version": params.get("version", "")
            }
        )

        if not response.success:
            raise Exception(f"Error al obtener secreto: {response.error}")

        return {
            "action": "get_secret",
            "vault_name": vault_name,
            "secret_name": secret_name,
            "value": response.data.get("value", ""),
            "version": response.data.get("version", "")
        }

    async def _list_secrets(self, params: Dict[str, Any]) -> Dict[str, Any]:
        """Lista todos los secretos en un Key Vault."""
        vault_name = params.get("vault_name")

        if not vault_name:
            raise Exception("Se requiere 'vault_name'")

        self.logger.info(f"Listando secretos en vault '{vault_name}'")

        response = await self.call_mcp_tool(
            "azure.keyvault.listSecrets",
            {
                "vaultName": vault_name
            }
        )

        if not response.success:
            raise Exception(f"Error al listar secretos: {response.error}")

        secrets = response.data.get("secrets", [])

        return {
            "action": "list_secrets",
            "vault_name": vault_name,
            "secrets": [s.get("name") for s in secrets],
            "count": len(secrets)
        }

    async def _delete_secret(self, params: Dict[str, Any]) -> Dict[str, Any]:
        """Elimina un secreto de Key Vault."""
        vault_name = params.get("vault_name")
        secret_name = params.get("secret_name")

        if not all([vault_name, secret_name]):
            raise Exception("Se requiere 'vault_name' y 'secret_name'")

        self.logger.info(f"Eliminando secreto '{secret_name}' de vault '{vault_name}'")

        response = await self.call_mcp_tool(
            "azure.keyvault.deleteSecret",
            {
                "vaultName": vault_name,
                "secretName": secret_name
            }
        )

        if not response.success:
            raise Exception(f"Error al eliminar secreto: {response.error}")

        return {
            "action": "delete_secret",
            "vault_name": vault_name,
            "secret_name": secret_name,
            "status": "deleted"
        }

    async def _create_key(self, params: Dict[str, Any]) -> Dict[str, Any]:
        """Crea una nueva clave criptográfica."""
        vault_name = params.get("vault_name")
        key_name = params.get("key_name")
        key_type = params.get("key_type", "RSA")

        if not all([vault_name, key_name]):
            raise Exception("Se requiere 'vault_name' y 'key_name'")

        self.logger.info(f"Creando clave '{key_name}' en vault '{vault_name}'")

        response = await self.call_mcp_tool(
            "azure.keyvault.createKey",
            {
                "vaultName": vault_name,
                "keyName": key_name,
                "keyType": key_type,
                "keySize": params.get("key_size", 2048)
            }
        )

        if not response.success:
            raise Exception(f"Error al crear clave: {response.error}")

        return {
            "action": "create_key",
            "vault_name": vault_name,
            "key_name": key_name,
            "key_type": key_type,
            "status": "created"
        }

    async def _list_keys(self, params: Dict[str, Any]) -> Dict[str, Any]:
        """Lista todas las claves en un Key Vault."""
        vault_name = params.get("vault_name")

        if not vault_name:
            raise Exception("Se requiere 'vault_name'")

        self.logger.info(f"Listando claves en vault '{vault_name}'")

        response = await self.call_mcp_tool(
            "azure.keyvault.listKeys",
            {
                "vaultName": vault_name
            }
        )

        if not response.success:
            raise Exception(f"Error al listar claves: {response.error}")

        keys = response.data.get("keys", [])

        return {
            "action": "list_keys",
            "vault_name": vault_name,
            "keys": [k.get("name") for k in keys],
            "count": len(keys)
        }

    async def _delete_key(self, params: Dict[str, Any]) -> Dict[str, Any]:
        """Elimina una clave de Key Vault."""
        vault_name = params.get("vault_name")
        key_name = params.get("key_name")

        if not all([vault_name, key_name]):
            raise Exception("Se requiere 'vault_name' y 'key_name'")

        self.logger.info(f"Eliminando clave '{key_name}' de vault '{vault_name}'")

        response = await self.call_mcp_tool(
            "azure.keyvault.deleteKey",
            {
                "vaultName": vault_name,
                "keyName": key_name
            }
        )

        if not response.success:
            raise Exception(f"Error al eliminar clave: {response.error}")

        return {
            "action": "delete_key",
            "vault_name": vault_name,
            "key_name": key_name,
            "status": "deleted"
        }

    async def _import_certificate(self, params: Dict[str, Any]) -> Dict[str, Any]:
        """Importa un certificado a Key Vault."""
        vault_name = params.get("vault_name")
        cert_name = params.get("cert_name")
        cert_path = params.get("cert_path")

        if not all([vault_name, cert_name, cert_path]):
            raise Exception("Se requiere 'vault_name', 'cert_name' y 'cert_path'")

        Validators.validate_file_path(cert_path, must_exist=True)

        self.logger.info(f"Importando certificado '{cert_name}' a vault '{vault_name}'")

        response = await self.call_mcp_tool(
            "azure.keyvault.importCertificate",
            {
                "vaultName": vault_name,
                "certificateName": cert_name,
                "certificatePath": cert_path,
                "password": params.get("password", "")
            }
        )

        if not response.success:
            raise Exception(f"Error al importar certificado: {response.error}")

        return {
            "action": "import_certificate",
            "vault_name": vault_name,
            "cert_name": cert_name,
            "status": "imported"
        }

    async def _list_certificates(self, params: Dict[str, Any]) -> Dict[str, Any]:
        """Lista todos los certificados en un Key Vault."""
        vault_name = params.get("vault_name")

        if not vault_name:
            raise Exception("Se requiere 'vault_name'")

        self.logger.info(f"Listando certificados en vault '{vault_name}'")

        response = await self.call_mcp_tool(
            "azure.keyvault.listCertificates",
            {
                "vaultName": vault_name
            }
        )

        if not response.success:
            raise Exception(f"Error al listar certificados: {response.error}")

        certs = response.data.get("certificates", [])

        return {
            "action": "list_certificates",
            "vault_name": vault_name,
            "certificates": [c.get("name") for c in certs],
            "count": len(certs)
        }
