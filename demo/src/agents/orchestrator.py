"""
Agente Orquestador que coordina todos los demás agentes.
"""

import re
from typing import Any, Dict, List, Optional
from src.core.agent_base import Agent, AgentCapability, Task, TaskStatus, AgentSystem
from src.core.mcp_client import MCPClientPool
from src.utils.logger import AgentLogger


class OrchestratorAgent(Agent):
    """
    Agente Orquestador que coordina las interacciones entre todos los agentes.

    Este agente es responsable de:
    - Analizar tareas complejas y dividirlas en subtareas
    - Asignar subtareas a agentes especializados
    - Coordinar la ejecución secuencial o paralela de tareas
    - Agregar y presentar resultados al usuario
    - Manejar errores y reintentos
    """

    def __init__(self, agent_system: AgentSystem):
        # El orchestrator no usa directamente un servidor MCP
        # En su lugar, coordina otros agentes
        super().__init__(
            name="orchestrator",
            description="Agente coordinador de todos los agentes",
            mcp_server_name="",  # No usa servidor MCP directamente
            client_pool=agent_system.client_pool
        )
        self.agent_system = agent_system
        self.logger = AgentLogger("OrchestratorAgent")
        self._agent_capabilities_map: Dict[str, List[str]] = {}

    async def _load_capabilities(self):
        """Carga las capacidades del orchestrator."""
        self.capabilities = [
            AgentCapability(
                name="coordinate_workflow",
                description="Coordina flujos de trabajo complejos entre múltiples agentes",
                required_tools=[]
            ),
            AgentCapability(
                name="task_decomposition",
                description="Descompone tareas complejas en subtareas simples",
                required_tools=[]
            ),
            AgentCapability(
                name="agent_selection",
                description="Selecciona el agente apropiado para cada tarea",
                required_tools=[]
            ),
            AgentCapability(
                name="result_aggregation",
                description="Agrega resultados de múltiples agentes",
                required_tools=[]
            )
        ]

        # Mapear capacidades de todos los agentes
        await self._build_agent_capabilities_map()

    async def _build_agent_capabilities_map(self):
        """Construye un mapa de capacidades de todos los agentes."""
        self.logger.info("Construyendo mapa de capacidades de agentes")

        for agent_name in self.agent_system.list_agents():
            if agent_name == "orchestrator":
                continue

            agent = self.agent_system.get_agent(agent_name)
            if agent:
                self._agent_capabilities_map[agent_name] = [
                    cap.name for cap in agent.capabilities
                ]

        self.logger.info(
            f"Mapa de capacidades creado con {len(self._agent_capabilities_map)} agentes"
        )

    async def execute_task(self, task: Task) -> Task:
        """Ejecuta una tarea compleja coordinando múltiples agentes."""
        self.logger.task_start(task.description)

        try:
            # Analizar la tarea y determinar qué agentes necesitamos
            subtasks = self._decompose_task(task)

            self.logger.info(
                f"Tarea descompuesta en {len(subtasks)} subtareas"
            )

            # Ejecutar subtareas
            results = []
            for subtask in subtasks:
                subtask_result = await self._execute_subtask(subtask)
                results.append(subtask_result)

                # Si una subtarea falla, decidir si continuar o abortar
                if subtask_result.status == TaskStatus.FAILED:
                    self.logger.warning(
                        f"Subtarea falló: {subtask_result.description}"
                    )
                    # Por ahora, continuamos con las demás tareas

            # Agregar resultados
            task.result = {
                "subtasks": [
                    {
                        "description": st.description,
                        "agent": st.agent_name,
                        "status": st.status.value,
                        "result": st.result,
                        "error": st.error
                    }
                    for st in results
                ],
                "summary": self._generate_summary(results)
            }

            self.logger.task_complete(task.description, 0)

        except Exception as e:
            task.error = str(e)
            self.logger.task_failed(task.description, str(e))

        return task

    def _decompose_task(self, task: Task) -> List[Task]:
        """
        Descompone una tarea compleja en subtareas más simples.

        Esta función analiza la descripción de la tarea y extrae
        las acciones individuales que deben realizarse.
        """
        description = task.description.lower()
        subtasks = []

        # Buscar patrones comunes en la descripción
        patterns = {
            r"(crear|subir|upload).*contenedor": {
                "agent": "azure_storage",
                "action": "create_container"
            },
            r"(crear|set).*secreto": {
                "agent": "azure_security",
                "action": "create_secret"
            },
            r"(subir|upload).*archivo": {
                "agent": "azure_storage",
                "action": "upload_blob"
            },
            r"(crear|generar).*lakehouse": {
                "agent": "fabric",
                "action": "generate_resource"
            },
            r"(crear|generar).*fabric": {
                "agent": "fabric",
                "action": "generate_resource"
            },
            r"listar.*contenedor": {
                "agent": "azure_storage",
                "action": "list_containers"
            },
            r"listar.*secreto": {
                "agent": "azure_security",
                "action": "list_secrets"
            }
        }

        # Intentar match con patrones
        matched = False
        for pattern, info in patterns.items():
            if re.search(pattern, description):
                subtasks.append(
                    Task(
                        description=task.description,
                        agent_name=info["agent"],
                        params=task.params,
                        parent_task_id=task.id
                    )
                )
                matched = True
                break

        # Si no hay match, intentar inferir del contexto
        if not matched:
            subtasks = self._infer_subtasks_from_context(task)

        return subtasks

    def _infer_subtasks_from_context(self, task: Task) -> List[Task]:
        """Infiere subtareas basándose en el contexto y parámetros."""
        subtasks = []
        description = task.description.lower()
        params = task.params

        # Analizar palabras clave para determinar agentes
        agent_keywords = {
            "azure_storage": ["storage", "blob", "contenedor", "container", "archivo", "file"],
            "azure_security": ["key vault", "secreto", "secret", "clave", "key", "certificado"],
            "fabric": ["fabric", "lakehouse", "notebook", "pipeline", "workload"]
        }

        # Encontrar el agente más apropiado
        best_agent = None
        max_matches = 0

        for agent_name, keywords in agent_keywords.items():
            matches = sum(1 for keyword in keywords if keyword in description)
            if matches > max_matches:
                max_matches = matches
                best_agent = agent_name

        if best_agent:
            subtasks.append(
                Task(
                    description=task.description,
                    agent_name=best_agent,
                    params=params,
                    parent_task_id=task.id
                )
            )
        else:
            # Si no podemos determinar, crear una subtarea genérica
            self.logger.warning(
                f"No se pudo determinar el agente apropiado para: {task.description}"
            )
            subtasks.append(
                Task(
                    description=task.description,
                    agent_name="unknown",
                    params=params,
                    parent_task_id=task.id
                )
            )

        return subtasks

    async def _execute_subtask(self, subtask: Task) -> Task:
        """Ejecuta una subtarea en el agente apropiado."""
        agent = self.agent_system.get_agent(subtask.agent_name)

        if not agent:
            self.logger.error(f"Agente no encontrado: {subtask.agent_name}")
            subtask.status = TaskStatus.FAILED
            subtask.error = f"Agente no encontrado: {subtask.agent_name}"
            return subtask

        self.logger.info(
            f"Ejecutando subtarea en {subtask.agent_name}: {subtask.description}"
        )

        try:
            result = await agent.process_task(
                subtask.description,
                **subtask.params
            )
            return result

        except Exception as e:
            self.logger.error(
                f"Error al ejecutar subtarea en {subtask.agent_name}: {e}"
            )
            subtask.status = TaskStatus.FAILED
            subtask.error = str(e)
            return subtask

    def _generate_summary(self, results: List[Task]) -> Dict[str, Any]:
        """Genera un resumen de los resultados de todas las subtareas."""
        total = len(results)
        completed = sum(1 for r in results if r.status == TaskStatus.COMPLETED)
        failed = sum(1 for r in results if r.status == TaskStatus.FAILED)

        return {
            "total_subtasks": total,
            "completed": completed,
            "failed": failed,
            "success_rate": (completed / total * 100) if total > 0 else 0,
            "agents_used": list(set(r.agent_name for r in results))
        }

    async def execute_workflow(
        self,
        workflow_name: str,
        params: Dict[str, Any]
    ) -> Dict[str, Any]:
        """
        Ejecuta un flujo de trabajo predefinido.

        Args:
            workflow_name: Nombre del flujo de trabajo
            params: Parámetros del flujo de trabajo

        Returns:
            Resultados del flujo de trabajo
        """
        self.logger.info(f"Ejecutando workflow: {workflow_name}")

        workflows = {
            "data_pipeline": self._workflow_data_pipeline,
            "secure_app": self._workflow_secure_app,
            "intelligent_search": self._workflow_intelligent_search,
            "fabric_infrastructure": self._workflow_fabric_infrastructure
        }

        if workflow_name not in workflows:
            raise ValueError(f"Workflow no encontrado: {workflow_name}")

        workflow_func = workflows[workflow_name]
        return await workflow_func(params)

    async def _workflow_data_pipeline(
        self,
        params: Dict[str, Any]
    ) -> Dict[str, Any]:
        """
        Workflow: Pipeline de datos completo.

        Steps:
        1. Crear contenedor en Storage
        2. Subir datos
        3. Crear base de datos Cosmos
        4. Indexar en AI Search
        5. Generar definición de Lakehouse
        """
        self.logger.info("Iniciando workflow: Pipeline de datos")

        results = {}

        # Step 1: Crear contenedor
        storage_agent = self.agent_system.get_agent("azure_storage")
        if storage_agent:
            task = await storage_agent.process_task(
                "Crear contenedor de almacenamiento",
                container_name=params.get("container_name", "data-pipeline"),
                storage_account=params.get("storage_account", "")
            )
            results["storage_container"] = task.result

        # Step 2: Generar definición de Fabric
        fabric_agent = self.agent_system.get_agent("fabric")
        if fabric_agent:
            task = await fabric_agent.process_task(
                "Generar definición de Lakehouse",
                workload_type="Lakehouse",
                resource_name=params.get("lakehouse_name", "analytics-lakehouse")
            )
            results["fabric_lakehouse"] = task.result

        return {
            "workflow": "data_pipeline",
            "status": "completed",
            "results": results
        }

    async def _workflow_secure_app(
        self,
        params: Dict[str, Any]
    ) -> Dict[str, Any]:
        """
        Workflow: Aplicación segura.

        Steps:
        1. Crear secretos en Key Vault
        2. Crear contenedor para archivos estáticos
        3. Desplegar aplicación (simulado)
        """
        self.logger.info("Iniciando workflow: Aplicación segura")

        results = {}

        # Step 1: Crear secreto
        security_agent = self.agent_system.get_agent("azure_security")
        if security_agent:
            task = await security_agent.process_task(
                "Crear secreto en Key Vault",
                vault_name=params.get("vault_name", "my-vault"),
                secret_name=params.get("secret_name", "api-key"),
                secret_value=params.get("secret_value", "secret123")
            )
            results["keyvault_secret"] = task.result

        # Step 2: Crear storage
        storage_agent = self.agent_system.get_agent("azure_storage")
        if storage_agent:
            task = await storage_agent.process_task(
                "Crear contenedor para archivos estáticos",
                container_name=params.get("static_container", "app-static")
            )
            results["static_storage"] = task.result

        return {
            "workflow": "secure_app",
            "status": "completed",
            "results": results
        }

    async def _workflow_intelligent_search(
        self,
        params: Dict[str, Any]
    ) -> Dict[str, Any]:
        """Workflow: Búsqueda inteligente."""
        self.logger.info("Iniciando workflow: Búsqueda inteligente")

        # Implementación simplificada
        return {
            "workflow": "intelligent_search",
            "status": "completed",
            "results": {}
        }

    async def _workflow_fabric_infrastructure(
        self,
        params: Dict[str, Any]
    ) -> Dict[str, Any]:
        """Workflow: Infraestructura Fabric."""
        self.logger.info("Iniciando workflow: Infraestructura Fabric")

        results = {}

        # Generar recursos de Fabric
        fabric_agent = self.agent_system.get_agent("fabric")
        if fabric_agent:
            # Listar workloads disponibles
            task1 = await fabric_agent.process_task(
                "Listar workloads disponibles",
                {}
            )
            results["available_workloads"] = task1.result

            # Generar definición de Lakehouse
            task2 = await fabric_agent.process_task(
                "Generar definición de Lakehouse",
                workload_type="Lakehouse",
                resource_name=params.get("lakehouse_name", "data-warehouse")
            )
            results["lakehouse_definition"] = task2.result

        return {
            "workflow": "fabric_infrastructure",
            "status": "completed",
            "results": results
        }
