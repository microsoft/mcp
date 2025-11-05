"""
Agente especializado para Microsoft Fabric.
"""

from typing import Any, Dict, List
from src.core.agent_base import Agent, AgentCapability, Task
from src.core.mcp_client import MCPClientPool
from src.utils.logger import AgentLogger


class FabricAgent(Agent):
    """
    Agente especializado para operaciones con Microsoft Fabric.

    Capacidades:
    - Listar workloads disponibles (notebook, lakehouse, etc.)
    - Obtener OpenAPI specs para workloads
    - Generar definiciones de recursos (item definitions)
    - Obtener mejores prácticas
    - Obtener ejemplos de uso
    """

    def __init__(self, client_pool: MCPClientPool):
        super().__init__(
            name="fabric",
            description="Agente para APIs públicas de Microsoft Fabric",
            mcp_server_name="fabric-mcp",
            client_pool=client_pool
        )
        self.logger = AgentLogger("FabricAgent")

    async def _load_capabilities(self):
        """Carga las capacidades del agente."""
        self.capabilities = [
            AgentCapability(
                name="list_workloads",
                description="Lista todos los tipos de workloads de Fabric",
                required_tools=["fabric.publicapis.list"]
            ),
            AgentCapability(
                name="get_workload_apis",
                description="Obtiene OpenAPI spec para un workload",
                required_tools=["fabric.publicapis.get"],
                parameters={
                    "workload_type": "string"
                }
            ),
            AgentCapability(
                name="get_item_definition",
                description="Obtiene JSON schema de definición de item",
                required_tools=["fabric.publicapis.itemdefinition.get"],
                parameters={
                    "workload_type": "string"
                }
            ),
            AgentCapability(
                name="get_best_practices",
                description="Obtiene mejores prácticas para un workload",
                required_tools=["fabric.publicapis.bestpractices.get"],
                parameters={
                    "workload_type": "string"
                }
            ),
            AgentCapability(
                name="get_examples",
                description="Obtiene ejemplos de uso para un workload",
                required_tools=["fabric.publicapis.examples.get"],
                parameters={
                    "workload_type": "string"
                }
            )
        ]

    async def execute_task(self, task: Task) -> Task:
        """Ejecuta una tarea de Fabric."""
        self.logger.task_start(task.description)

        try:
            description_lower = task.description.lower()

            if "listar" in description_lower or "list" in description_lower:
                if "workload" in description_lower:
                    task.result = await self._list_workloads(task.params)

            elif "api" in description_lower or "openapi" in description_lower:
                task.result = await self._get_workload_apis(task.params)

            elif "definici" in description_lower or "schema" in description_lower or "item" in description_lower:
                task.result = await self._get_item_definition(task.params)

            elif "mejor" in description_lower or "best" in description_lower or "práctica" in description_lower:
                task.result = await self._get_best_practices(task.params)

            elif "ejemplo" in description_lower or "example" in description_lower:
                task.result = await self._get_examples(task.params)

            elif "generar" in description_lower or "generate" in description_lower or "crear" in description_lower:
                # Generar definición completa de recurso
                task.result = await self._generate_resource_definition(task.params)

            else:
                task.error = "No se pudo determinar la operación de Fabric"
                return task

            self.logger.task_complete(task.description, 0)

        except Exception as e:
            task.error = str(e)
            self.logger.task_failed(task.description, str(e))

        return task

    async def _list_workloads(self, params: Dict[str, Any]) -> Dict[str, Any]:
        """Lista todos los workloads disponibles en Fabric."""
        self.logger.info("Listando workloads de Microsoft Fabric")

        response = await self.call_mcp_tool(
            "fabric.publicapis.list",
            {}
        )

        if not response.success:
            raise Exception(f"Error al listar workloads: {response.error}")

        workloads = response.data.get("workloads", [])

        return {
            "action": "list_workloads",
            "workloads": workloads,
            "count": len(workloads)
        }

    async def _get_workload_apis(self, params: Dict[str, Any]) -> Dict[str, Any]:
        """Obtiene las APIs de un workload específico."""
        workload_type = params.get("workload_type")

        if not workload_type:
            raise Exception("Se requiere 'workload_type'")

        self.logger.info(f"Obteniendo APIs para workload: {workload_type}")

        response = await self.call_mcp_tool(
            "fabric.publicapis.get",
            {
                "workloadType": workload_type
            }
        )

        if not response.success:
            raise Exception(f"Error al obtener APIs: {response.error}")

        return {
            "action": "get_workload_apis",
            "workload_type": workload_type,
            "openapi_spec": response.data.get("openapi", {}),
            "operations": response.data.get("operations", [])
        }

    async def _get_item_definition(self, params: Dict[str, Any]) -> Dict[str, Any]:
        """Obtiene la definición de item para un workload."""
        workload_type = params.get("workload_type")

        if not workload_type:
            raise Exception("Se requiere 'workload_type'")

        self.logger.info(f"Obteniendo definición de item para: {workload_type}")

        response = await self.call_mcp_tool(
            "fabric.publicapis.itemdefinition.get",
            {
                "workloadType": workload_type
            }
        )

        if not response.success:
            raise Exception(f"Error al obtener definición: {response.error}")

        return {
            "action": "get_item_definition",
            "workload_type": workload_type,
            "schema": response.data.get("schema", {}),
            "properties": response.data.get("properties", {})
        }

    async def _get_best_practices(self, params: Dict[str, Any]) -> Dict[str, Any]:
        """Obtiene mejores prácticas para un workload."""
        workload_type = params.get("workload_type")

        if not workload_type:
            raise Exception("Se requiere 'workload_type'")

        self.logger.info(f"Obteniendo mejores prácticas para: {workload_type}")

        response = await self.call_mcp_tool(
            "fabric.publicapis.bestpractices.get",
            {
                "workloadType": workload_type
            }
        )

        if not response.success:
            raise Exception(f"Error al obtener mejores prácticas: {response.error}")

        return {
            "action": "get_best_practices",
            "workload_type": workload_type,
            "practices": response.data.get("practices", []),
            "recommendations": response.data.get("recommendations", [])
        }

    async def _get_examples(self, params: Dict[str, Any]) -> Dict[str, Any]:
        """Obtiene ejemplos de uso para un workload."""
        workload_type = params.get("workload_type")

        if not workload_type:
            raise Exception("Se requiere 'workload_type'")

        self.logger.info(f"Obteniendo ejemplos para: {workload_type}")

        response = await self.call_mcp_tool(
            "fabric.publicapis.examples.get",
            {
                "workloadType": workload_type
            }
        )

        if not response.success:
            raise Exception(f"Error al obtener ejemplos: {response.error}")

        return {
            "action": "get_examples",
            "workload_type": workload_type,
            "examples": response.data.get("examples", [])
        }

    async def _generate_resource_definition(
        self,
        params: Dict[str, Any]
    ) -> Dict[str, Any]:
        """
        Genera una definición completa de recurso de Fabric.

        Combina schema, best practices y ejemplos.
        """
        workload_type = params.get("workload_type")

        if not workload_type:
            raise Exception("Se requiere 'workload_type'")

        self.logger.info(
            f"Generando definición completa para: {workload_type}"
        )

        # Obtener schema
        schema_result = await self._get_item_definition(
            {"workload_type": workload_type}
        )

        # Obtener best practices
        practices_result = await self._get_best_practices(
            {"workload_type": workload_type}
        )

        # Obtener ejemplos
        examples_result = await self._get_examples(
            {"workload_type": workload_type}
        )

        return {
            "action": "generate_resource_definition",
            "workload_type": workload_type,
            "schema": schema_result.get("schema", {}),
            "best_practices": practices_result.get("practices", []),
            "examples": examples_result.get("examples", []),
            "generated_definition": self._build_definition(
                schema_result.get("schema", {}),
                params.get("resource_name", f"my-{workload_type}"),
                params.get("properties", {})
            )
        }

    def _build_definition(
        self,
        schema: Dict[str, Any],
        resource_name: str,
        properties: Dict[str, Any]
    ) -> Dict[str, Any]:
        """Construye una definición de recurso basada en schema y propiedades."""
        definition = {
            "displayName": resource_name,
            "type": schema.get("type", ""),
            "definition": {
                "parts": []
            }
        }

        # Agregar propiedades personalizadas
        for key, value in properties.items():
            definition["definition"]["parts"].append({
                "path": key,
                "payload": value
            })

        return definition
