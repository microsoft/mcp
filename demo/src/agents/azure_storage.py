"""
Agente especializado para Azure Storage.
"""

from typing import Any, Dict, List
from src.core.agent_base import Agent, AgentCapability, Task, TaskStatus
from src.core.mcp_client import MCPClientPool
from src.utils.logger import AgentLogger
from src.utils.validators import Validators, ParameterValidator


class AzureStorageAgent(Agent):
    """
    Agente especializado para operaciones con Azure Blob Storage.

    Capacidades:
    - Listar contenedores de almacenamiento
    - Crear contenedores
    - Subir archivos a blob storage
    - Descargar archivos de blob storage
    - Eliminar blobs y contenedores
    """

    def __init__(self, client_pool: MCPClientPool):
        super().__init__(
            name="azure_storage",
            description="Agente para gestión de Azure Blob Storage",
            mcp_server_name="azure-mcp",
            client_pool=client_pool
        )
        self.logger = AgentLogger("AzureStorageAgent")

    async def _load_capabilities(self):
        """Carga las capacidades del agente."""
        self.capabilities = [
            AgentCapability(
                name="list_containers",
                description="Lista contenedores de storage",
                required_tools=["azure.storage.list"]
            ),
            AgentCapability(
                name="create_container",
                description="Crea un nuevo contenedor",
                required_tools=["azure.storage.create"],
                parameters={
                    "container_name": "string",
                    "storage_account": "string"
                }
            ),
            AgentCapability(
                name="upload_blob",
                description="Sube un archivo a blob storage",
                required_tools=["azure.storage.upload"],
                parameters={
                    "container_name": "string",
                    "blob_name": "string",
                    "file_path": "string"
                }
            ),
            AgentCapability(
                name="download_blob",
                description="Descarga un archivo de blob storage",
                required_tools=["azure.storage.download"],
                parameters={
                    "container_name": "string",
                    "blob_name": "string",
                    "destination_path": "string"
                }
            ),
            AgentCapability(
                name="delete_blob",
                description="Elimina un blob",
                required_tools=["azure.storage.delete"],
                parameters={
                    "container_name": "string",
                    "blob_name": "string"
                }
            )
        ]

    async def execute_task(self, task: Task) -> Task:
        """Ejecuta una tarea de storage."""
        self.logger.task_start(task.description)

        try:
            # Analizar la descripción de la tarea para determinar la acción
            description_lower = task.description.lower()

            if "listar" in description_lower or "list" in description_lower:
                if "contenedor" in description_lower or "container" in description_lower:
                    task.result = await self._list_containers(task.params)
                else:
                    task.result = await self._list_blobs(task.params)

            elif "crear" in description_lower or "create" in description_lower:
                task.result = await self._create_container(task.params)

            elif "subir" in description_lower or "upload" in description_lower:
                task.result = await self._upload_blob(task.params)

            elif "descargar" in description_lower or "download" in description_lower:
                task.result = await self._download_blob(task.params)

            elif "eliminar" in description_lower or "delete" in description_lower:
                if "contenedor" in description_lower or "container" in description_lower:
                    task.result = await self._delete_container(task.params)
                else:
                    task.result = await self._delete_blob(task.params)

            else:
                task.error = "No se pudo determinar la acción a realizar"
                return task

            self.logger.task_complete(task.description, 0)

        except Exception as e:
            task.error = str(e)
            self.logger.task_failed(task.description, str(e))

        return task

    async def _list_containers(self, params: Dict[str, Any]) -> Dict[str, Any]:
        """Lista contenedores de storage."""
        self.logger.info("Listando contenedores de Azure Storage")

        response = await self.call_mcp_tool(
            "azure.storage.listContainers",
            {
                "storageAccount": params.get("storage_account", ""),
                "subscriptionId": params.get("subscription_id", "")
            }
        )

        if not response.success:
            raise Exception(f"Error al listar contenedores: {response.error}")

        return {
            "action": "list_containers",
            "containers": response.data.get("containers", []),
            "count": len(response.data.get("containers", []))
        }

    async def _list_blobs(self, params: Dict[str, Any]) -> Dict[str, Any]:
        """Lista blobs en un contenedor."""
        container_name = params.get("container_name")

        if not container_name:
            raise Exception("Se requiere 'container_name'")

        self.logger.info(f"Listando blobs en contenedor: {container_name}")

        response = await self.call_mcp_tool(
            "azure.storage.listBlobs",
            {
                "containerName": container_name,
                "storageAccount": params.get("storage_account", "")
            }
        )

        if not response.success:
            raise Exception(f"Error al listar blobs: {response.error}")

        return {
            "action": "list_blobs",
            "container": container_name,
            "blobs": response.data.get("blobs", []),
            "count": len(response.data.get("blobs", []))
        }

    async def _create_container(self, params: Dict[str, Any]) -> Dict[str, Any]:
        """Crea un nuevo contenedor."""
        container_name = params.get("container_name")

        if not container_name:
            raise Exception("Se requiere 'container_name'")

        # Validar nombre del contenedor
        Validators.validate_storage_container_name(container_name)

        self.logger.info(f"Creando contenedor: {container_name}")

        response = await self.call_mcp_tool(
            "azure.storage.createContainer",
            {
                "containerName": container_name,
                "storageAccount": params.get("storage_account", ""),
                "publicAccess": params.get("public_access", "None")
            }
        )

        if not response.success:
            raise Exception(f"Error al crear contenedor: {response.error}")

        return {
            "action": "create_container",
            "container_name": container_name,
            "status": "created",
            "url": response.data.get("url", "")
        }

    async def _upload_blob(self, params: Dict[str, Any]) -> Dict[str, Any]:
        """Sube un archivo a blob storage."""
        container_name = params.get("container_name")
        blob_name = params.get("blob_name")
        file_path = params.get("file_path")

        if not all([container_name, blob_name, file_path]):
            raise Exception(
                "Se requiere 'container_name', 'blob_name' y 'file_path'"
            )

        # Validar archivo
        Validators.validate_file_path(file_path, must_exist=True)

        self.logger.info(
            f"Subiendo archivo {file_path} a {container_name}/{blob_name}"
        )

        response = await self.call_mcp_tool(
            "azure.storage.uploadBlob",
            {
                "containerName": container_name,
                "blobName": blob_name,
                "filePath": file_path,
                "storageAccount": params.get("storage_account", ""),
                "overwrite": params.get("overwrite", True)
            }
        )

        if not response.success:
            raise Exception(f"Error al subir archivo: {response.error}")

        return {
            "action": "upload_blob",
            "container": container_name,
            "blob_name": blob_name,
            "status": "uploaded",
            "url": response.data.get("url", "")
        }

    async def _download_blob(self, params: Dict[str, Any]) -> Dict[str, Any]:
        """Descarga un archivo de blob storage."""
        container_name = params.get("container_name")
        blob_name = params.get("blob_name")
        destination_path = params.get("destination_path")

        if not all([container_name, blob_name, destination_path]):
            raise Exception(
                "Se requiere 'container_name', 'blob_name' y 'destination_path'"
            )

        self.logger.info(
            f"Descargando {container_name}/{blob_name} a {destination_path}"
        )

        response = await self.call_mcp_tool(
            "azure.storage.downloadBlob",
            {
                "containerName": container_name,
                "blobName": blob_name,
                "destinationPath": destination_path,
                "storageAccount": params.get("storage_account", "")
            }
        )

        if not response.success:
            raise Exception(f"Error al descargar archivo: {response.error}")

        return {
            "action": "download_blob",
            "container": container_name,
            "blob_name": blob_name,
            "destination": destination_path,
            "status": "downloaded"
        }

    async def _delete_blob(self, params: Dict[str, Any]) -> Dict[str, Any]:
        """Elimina un blob."""
        container_name = params.get("container_name")
        blob_name = params.get("blob_name")

        if not all([container_name, blob_name]):
            raise Exception("Se requiere 'container_name' y 'blob_name'")

        self.logger.info(f"Eliminando blob {container_name}/{blob_name}")

        response = await self.call_mcp_tool(
            "azure.storage.deleteBlob",
            {
                "containerName": container_name,
                "blobName": blob_name,
                "storageAccount": params.get("storage_account", "")
            }
        )

        if not response.success:
            raise Exception(f"Error al eliminar blob: {response.error}")

        return {
            "action": "delete_blob",
            "container": container_name,
            "blob_name": blob_name,
            "status": "deleted"
        }

    async def _delete_container(self, params: Dict[str, Any]) -> Dict[str, Any]:
        """Elimina un contenedor."""
        container_name = params.get("container_name")

        if not container_name:
            raise Exception("Se requiere 'container_name'")

        self.logger.info(f"Eliminando contenedor {container_name}")

        response = await self.call_mcp_tool(
            "azure.storage.deleteContainer",
            {
                "containerName": container_name,
                "storageAccount": params.get("storage_account", "")
            }
        )

        if not response.success:
            raise Exception(f"Error al eliminar contenedor: {response.error}")

        return {
            "action": "delete_container",
            "container_name": container_name,
            "status": "deleted"
        }
