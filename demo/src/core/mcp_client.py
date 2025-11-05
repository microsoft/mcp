"""
Cliente MCP para comunicación con servidores MCP de Microsoft.
"""

import asyncio
import json
import logging
import subprocess
from dataclasses import dataclass
from typing import Any, Dict, List, Optional
from pathlib import Path


logger = logging.getLogger(__name__)


@dataclass
class MCPRequest:
    """Representa una solicitud MCP."""
    method: str
    params: Dict[str, Any]
    request_id: Optional[str] = None


@dataclass
class MCPResponse:
    """Representa una respuesta MCP."""
    success: bool
    data: Optional[Any] = None
    error: Optional[str] = None
    request_id: Optional[str] = None


class MCPClient:
    """
    Cliente para comunicarse con servidores MCP a través de stdio.

    Implementa el protocolo MCP para enviar solicitudes y recibir respuestas
    de los servidores MCP de Azure y Fabric.
    """

    def __init__(self, server_config: Dict[str, Any]):
        """
        Inicializa el cliente MCP.

        Args:
            server_config: Configuración del servidor MCP
        """
        self.server_config = server_config
        self.process: Optional[subprocess.Popen] = None
        self.request_counter = 0
        self._lock = asyncio.Lock()

    async def start(self) -> bool:
        """
        Inicia el proceso del servidor MCP.

        Returns:
            True si el servidor se inició correctamente
        """
        try:
            command = self.server_config["command"]
            args = self.server_config.get("args", [])

            logger.info(f"Iniciando servidor MCP: {command} {' '.join(args)}")

            self.process = subprocess.Popen(
                [command] + args,
                stdin=subprocess.PIPE,
                stdout=subprocess.PIPE,
                stderr=subprocess.PIPE,
                text=True,
                bufsize=1
            )

            # Dar tiempo al servidor para iniciar
            await asyncio.sleep(1)

            # Verificar que el proceso está vivo
            if self.process.poll() is not None:
                stderr_output = self.process.stderr.read() if self.process.stderr else ""
                logger.error(f"El servidor MCP falló al iniciar: {stderr_output}")
                return False

            logger.info("Servidor MCP iniciado correctamente")
            return True

        except Exception as e:
            logger.error(f"Error al iniciar servidor MCP: {e}")
            return False

    async def stop(self):
        """Detiene el proceso del servidor MCP."""
        if self.process:
            try:
                self.process.terminate()
                await asyncio.sleep(0.5)

                if self.process.poll() is None:
                    self.process.kill()

                logger.info("Servidor MCP detenido")
            except Exception as e:
                logger.error(f"Error al detener servidor MCP: {e}")

    async def send_request(self, method: str, params: Dict[str, Any]) -> MCPResponse:
        """
        Envía una solicitud al servidor MCP y espera la respuesta.

        Args:
            method: Método MCP a invocar
            params: Parámetros de la solicitud

        Returns:
            Respuesta del servidor MCP
        """
        async with self._lock:
            try:
                if not self.process or self.process.poll() is not None:
                    logger.error("El servidor MCP no está ejecutándose")
                    return MCPResponse(
                        success=False,
                        error="El servidor MCP no está ejecutándose"
                    )

                self.request_counter += 1
                request_id = f"req_{self.request_counter}"

                # Construir solicitud JSON-RPC 2.0
                request = {
                    "jsonrpc": "2.0",
                    "id": request_id,
                    "method": method,
                    "params": params
                }

                logger.debug(f"Enviando solicitud MCP: {request}")

                # Enviar solicitud
                request_json = json.dumps(request) + "\n"
                self.process.stdin.write(request_json)
                self.process.stdin.flush()

                # Leer respuesta
                response_line = self.process.stdout.readline()

                if not response_line:
                    return MCPResponse(
                        success=False,
                        error="No se recibió respuesta del servidor MCP",
                        request_id=request_id
                    )

                response_data = json.loads(response_line)
                logger.debug(f"Respuesta MCP recibida: {response_data}")

                # Procesar respuesta
                if "error" in response_data:
                    return MCPResponse(
                        success=False,
                        error=str(response_data["error"]),
                        request_id=request_id
                    )

                return MCPResponse(
                    success=True,
                    data=response_data.get("result"),
                    request_id=request_id
                )

            except json.JSONDecodeError as e:
                logger.error(f"Error al parsear respuesta JSON: {e}")
                return MCPResponse(
                    success=False,
                    error=f"Error al parsear respuesta: {e}"
                )
            except Exception as e:
                logger.error(f"Error en send_request: {e}")
                return MCPResponse(
                    success=False,
                    error=str(e)
                )

    async def list_tools(self) -> List[Dict[str, Any]]:
        """
        Lista todas las herramientas disponibles en el servidor MCP.

        Returns:
            Lista de herramientas disponibles
        """
        response = await self.send_request("tools/list", {})

        if response.success and response.data:
            return response.data.get("tools", [])

        return []

    async def call_tool(
        self,
        tool_name: str,
        arguments: Dict[str, Any]
    ) -> MCPResponse:
        """
        Llama a una herramienta específica del servidor MCP.

        Args:
            tool_name: Nombre de la herramienta
            arguments: Argumentos para la herramienta

        Returns:
            Respuesta de la herramienta
        """
        return await self.send_request(
            "tools/call",
            {
                "name": tool_name,
                "arguments": arguments
            }
        )

    def is_running(self) -> bool:
        """Verifica si el servidor MCP está ejecutándose."""
        return self.process is not None and self.process.poll() is None


class MCPClientPool:
    """
    Pool de clientes MCP para gestionar múltiples servidores.
    """

    def __init__(self, config_path: str = "config/mcp_config.json"):
        """
        Inicializa el pool de clientes MCP.

        Args:
            config_path: Ruta al archivo de configuración MCP
        """
        self.config_path = Path(config_path)
        self.clients: Dict[str, MCPClient] = {}
        self._load_config()

    def _load_config(self):
        """Carga la configuración de los servidores MCP."""
        try:
            with open(self.config_path, 'r', encoding='utf-8') as f:
                config = json.load(f)

            for server_name, server_config in config["mcpServers"].items():
                self.clients[server_name] = MCPClient(server_config)

            logger.info(f"Configuración MCP cargada: {len(self.clients)} servidores")

        except Exception as e:
            logger.error(f"Error al cargar configuración MCP: {e}")
            raise

    async def start_all(self) -> bool:
        """
        Inicia todos los servidores MCP.

        Returns:
            True si todos los servidores se iniciaron correctamente
        """
        results = []

        for name, client in self.clients.items():
            logger.info(f"Iniciando servidor MCP: {name}")
            result = await client.start()
            results.append(result)

            if not result:
                logger.error(f"Falló al iniciar servidor: {name}")

        return all(results)

    async def stop_all(self):
        """Detiene todos los servidores MCP."""
        for name, client in self.clients.items():
            logger.info(f"Deteniendo servidor MCP: {name}")
            await client.stop()

    def get_client(self, server_name: str) -> Optional[MCPClient]:
        """
        Obtiene un cliente MCP específico.

        Args:
            server_name: Nombre del servidor MCP

        Returns:
            Cliente MCP o None si no existe
        """
        return self.clients.get(server_name)

    async def list_all_tools(self) -> Dict[str, List[Dict[str, Any]]]:
        """
        Lista todas las herramientas de todos los servidores.

        Returns:
            Diccionario con las herramientas por servidor
        """
        all_tools = {}

        for name, client in self.clients.items():
            if client.is_running():
                tools = await client.list_tools()
                all_tools[name] = tools

        return all_tools
