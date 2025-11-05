"""
Clase base para todos los agentes del sistema.
"""

import asyncio
import logging
from abc import ABC, abstractmethod
from dataclasses import dataclass, field
from datetime import datetime
from enum import Enum
from typing import Any, Dict, List, Optional
from uuid import uuid4

from src.core.mcp_client import MCPClient, MCPClientPool, MCPResponse


logger = logging.getLogger(__name__)


class TaskStatus(Enum):
    """Estado de una tarea."""
    PENDING = "pending"
    IN_PROGRESS = "in_progress"
    COMPLETED = "completed"
    FAILED = "failed"
    CANCELLED = "cancelled"


@dataclass
class Task:
    """Representa una tarea que un agente debe ejecutar."""
    id: str = field(default_factory=lambda: str(uuid4()))
    description: str = ""
    agent_name: str = ""
    params: Dict[str, Any] = field(default_factory=dict)
    status: TaskStatus = TaskStatus.PENDING
    result: Optional[Any] = None
    error: Optional[str] = None
    created_at: datetime = field(default_factory=datetime.now)
    started_at: Optional[datetime] = None
    completed_at: Optional[datetime] = None
    parent_task_id: Optional[str] = None
    subtasks: List[str] = field(default_factory=list)


@dataclass
class AgentCapability:
    """Define una capacidad de un agente."""
    name: str
    description: str
    required_tools: List[str]
    parameters: Dict[str, Any] = field(default_factory=dict)


class Agent(ABC):
    """
    Clase base abstracta para todos los agentes.

    Cada agente especializado debe heredar de esta clase e implementar
    los métodos abstractos.
    """

    def __init__(
        self,
        name: str,
        description: str,
        mcp_server_name: str,
        client_pool: MCPClientPool
    ):
        """
        Inicializa el agente.

        Args:
            name: Nombre del agente
            description: Descripción del agente
            mcp_server_name: Nombre del servidor MCP que usa este agente
            client_pool: Pool de clientes MCP
        """
        self.name = name
        self.description = description
        self.mcp_server_name = mcp_server_name
        self.client_pool = client_pool
        self.mcp_client: Optional[MCPClient] = None
        self.capabilities: List[AgentCapability] = []
        self.task_history: List[Task] = []
        self._is_initialized = False

        logger.info(f"Agente creado: {self.name}")

    async def initialize(self) -> bool:
        """
        Inicializa el agente y obtiene el cliente MCP.

        Returns:
            True si la inicialización fue exitosa
        """
        try:
            self.mcp_client = self.client_pool.get_client(self.mcp_server_name)

            if not self.mcp_client:
                logger.error(
                    f"No se encontró el servidor MCP: {self.mcp_server_name}"
                )
                return False

            if not self.mcp_client.is_running():
                logger.error(
                    f"El servidor MCP no está ejecutándose: {self.mcp_server_name}"
                )
                return False

            # Cargar capacidades del agente
            await self._load_capabilities()

            self._is_initialized = True
            logger.info(f"Agente inicializado: {self.name}")
            return True

        except Exception as e:
            logger.error(f"Error al inicializar agente {self.name}: {e}")
            return False

    @abstractmethod
    async def _load_capabilities(self):
        """Carga las capacidades específicas del agente."""
        pass

    @abstractmethod
    async def execute_task(self, task: Task) -> Task:
        """
        Ejecuta una tarea asignada al agente.

        Args:
            task: Tarea a ejecutar

        Returns:
            Tarea actualizada con el resultado
        """
        pass

    async def process_task(self, task_description: str, **params) -> Task:
        """
        Procesa una tarea descrita en lenguaje natural.

        Args:
            task_description: Descripción de la tarea
            **params: Parámetros adicionales

        Returns:
            Tarea completada con resultado
        """
        if not self._is_initialized:
            raise RuntimeError(
                f"El agente {self.name} no está inicializado. "
                f"Llama a initialize() primero."
            )

        task = Task(
            description=task_description,
            agent_name=self.name,
            params=params
        )

        logger.info(
            f"Agente {self.name} procesando tarea: {task.description}"
        )

        # Agregar tarea al historial
        self.task_history.append(task)

        # Cambiar estado a en progreso
        task.status = TaskStatus.IN_PROGRESS
        task.started_at = datetime.now()

        try:
            # Ejecutar tarea
            task = await self.execute_task(task)

            # Si no hubo error, marcar como completada
            if not task.error:
                task.status = TaskStatus.COMPLETED
                task.completed_at = datetime.now()
                logger.info(
                    f"Tarea completada por {self.name}: {task.description}"
                )
            else:
                task.status = TaskStatus.FAILED
                task.completed_at = datetime.now()
                logger.error(
                    f"Tarea fallida en {self.name}: {task.error}"
                )

        except Exception as e:
            task.status = TaskStatus.FAILED
            task.error = str(e)
            task.completed_at = datetime.now()
            logger.error(
                f"Error al ejecutar tarea en {self.name}: {e}"
            )

        return task

    async def call_mcp_tool(
        self,
        tool_name: str,
        arguments: Dict[str, Any]
    ) -> MCPResponse:
        """
        Llama a una herramienta del servidor MCP.

        Args:
            tool_name: Nombre de la herramienta
            arguments: Argumentos de la herramienta

        Returns:
            Respuesta de la herramienta
        """
        if not self.mcp_client:
            return MCPResponse(
                success=False,
                error="Cliente MCP no disponible"
            )

        logger.debug(
            f"{self.name} llamando herramienta MCP: {tool_name} "
            f"con argumentos: {arguments}"
        )

        return await self.mcp_client.call_tool(tool_name, arguments)

    def get_capability(self, capability_name: str) -> Optional[AgentCapability]:
        """
        Obtiene una capacidad específica del agente.

        Args:
            capability_name: Nombre de la capacidad

        Returns:
            Capacidad o None si no existe
        """
        for cap in self.capabilities:
            if cap.name == capability_name:
                return cap
        return None

    def has_capability(self, capability_name: str) -> bool:
        """
        Verifica si el agente tiene una capacidad específica.

        Args:
            capability_name: Nombre de la capacidad

        Returns:
            True si el agente tiene la capacidad
        """
        return self.get_capability(capability_name) is not None

    def get_task_history(self, limit: int = 10) -> List[Task]:
        """
        Obtiene el historial de tareas del agente.

        Args:
            limit: Número máximo de tareas a devolver

        Returns:
            Lista de tareas más recientes
        """
        return self.task_history[-limit:]

    def __str__(self) -> str:
        return (
            f"Agent({self.name}, capabilities={len(self.capabilities)}, "
            f"tasks={len(self.task_history)})"
        )


class AgentSystem:
    """
    Sistema que gestiona todos los agentes y su comunicación.
    """

    def __init__(self, config_path: str = "config/mcp_config.json"):
        """
        Inicializa el sistema de agentes.

        Args:
            config_path: Ruta al archivo de configuración MCP
        """
        self.client_pool = MCPClientPool(config_path)
        self.agents: Dict[str, Agent] = {}
        self._is_initialized = False

        logger.info("Sistema de agentes creado")

    async def initialize(self) -> bool:
        """
        Inicializa el sistema de agentes.

        Returns:
            True si la inicialización fue exitosa
        """
        try:
            # Iniciar todos los servidores MCP
            logger.info("Iniciando servidores MCP...")
            if not await self.client_pool.start_all():
                logger.error("Falló al iniciar servidores MCP")
                return False

            # Inicializar todos los agentes registrados
            logger.info("Inicializando agentes...")
            for name, agent in self.agents.items():
                if not await agent.initialize():
                    logger.error(f"Falló al inicializar agente: {name}")
                    return False

            self._is_initialized = True
            logger.info("Sistema de agentes inicializado correctamente")
            return True

        except Exception as e:
            logger.error(f"Error al inicializar sistema de agentes: {e}")
            return False

    async def shutdown(self):
        """Cierra el sistema de agentes."""
        logger.info("Cerrando sistema de agentes...")
        await self.client_pool.stop_all()
        logger.info("Sistema de agentes cerrado")

    def register_agent(self, agent: Agent):
        """
        Registra un nuevo agente en el sistema.

        Args:
            agent: Agente a registrar
        """
        self.agents[agent.name] = agent
        logger.info(f"Agente registrado: {agent.name}")

    def get_agent(self, name: str) -> Optional[Agent]:
        """
        Obtiene un agente específico.

        Args:
            name: Nombre del agente

        Returns:
            Agente o None si no existe
        """
        return self.agents.get(name)

    def list_agents(self) -> List[str]:
        """
        Lista todos los agentes registrados.

        Returns:
            Lista de nombres de agentes
        """
        return list(self.agents.keys())

    async def execute_task(
        self,
        agent_name: str,
        task_description: str,
        **params
    ) -> Optional[Task]:
        """
        Ejecuta una tarea en un agente específico.

        Args:
            agent_name: Nombre del agente
            task_description: Descripción de la tarea
            **params: Parámetros adicionales

        Returns:
            Tarea completada o None si el agente no existe
        """
        if not self._is_initialized:
            raise RuntimeError(
                "El sistema de agentes no está inicializado. "
                "Llama a initialize() primero."
            )

        agent = self.get_agent(agent_name)

        if not agent:
            logger.error(f"Agente no encontrado: {agent_name}")
            return None

        return await agent.process_task(task_description, **params)
