"""
Sistema de logging para el sistema de agentes.
"""

import logging
import sys
from datetime import datetime
from pathlib import Path
from typing import Optional

from colorama import Fore, Style, init


# Inicializar colorama para Windows
init(autoreset=True)


class ColoredFormatter(logging.Formatter):
    """Formateador personalizado con colores para consola."""

    COLORS = {
        'DEBUG': Fore.CYAN,
        'INFO': Fore.GREEN,
        'WARNING': Fore.YELLOW,
        'ERROR': Fore.RED,
        'CRITICAL': Fore.RED + Style.BRIGHT,
    }

    def format(self, record):
        # Añadir color al nivel
        levelname = record.levelname
        if levelname in self.COLORS:
            record.levelname = (
                f"{self.COLORS[levelname]}{levelname}{Style.RESET_ALL}"
            )

        # Añadir color al nombre del módulo
        record.name = f"{Fore.BLUE}{record.name}{Style.RESET_ALL}"

        return super().format(record)


def setup_logger(
    name: str = "agent_system",
    level: int = logging.INFO,
    log_file: Optional[str] = None
) -> logging.Logger:
    """
    Configura y retorna un logger con formato personalizado.

    Args:
        name: Nombre del logger
        level: Nivel de logging
        log_file: Ruta al archivo de log (opcional)

    Returns:
        Logger configurado
    """
    logger = logging.getLogger(name)
    logger.setLevel(level)

    # Evitar duplicar handlers si ya existe
    if logger.handlers:
        return logger

    # Handler para consola con colores
    console_handler = logging.StreamHandler(sys.stdout)
    console_handler.setLevel(level)

    console_format = ColoredFormatter(
        '%(asctime)s - %(name)s - %(levelname)s - %(message)s',
        datefmt='%Y-%m-%d %H:%M:%S'
    )
    console_handler.setFormatter(console_format)
    logger.addHandler(console_handler)

    # Handler para archivo si se especifica
    if log_file:
        log_path = Path(log_file)
        log_path.parent.mkdir(parents=True, exist_ok=True)

        file_handler = logging.FileHandler(log_file, encoding='utf-8')
        file_handler.setLevel(level)

        file_format = logging.Formatter(
            '%(asctime)s - %(name)s - %(levelname)s - '
            '%(filename)s:%(lineno)d - %(message)s',
            datefmt='%Y-%m-%d %H:%M:%S'
        )
        file_handler.setFormatter(file_format)
        logger.addHandler(file_handler)

    return logger


class AgentLogger:
    """
    Logger específico para agentes con contexto adicional.
    """

    def __init__(self, agent_name: str, base_logger: Optional[logging.Logger] = None):
        """
        Inicializa el logger del agente.

        Args:
            agent_name: Nombre del agente
            base_logger: Logger base (opcional)
        """
        self.agent_name = agent_name
        self.logger = base_logger or logging.getLogger("agent_system")

    def _format_message(self, message: str) -> str:
        """Formatea el mensaje con el nombre del agente."""
        return f"[{self.agent_name}] {message}"

    def debug(self, message: str):
        """Log nivel DEBUG."""
        self.logger.debug(self._format_message(message))

    def info(self, message: str):
        """Log nivel INFO."""
        self.logger.info(self._format_message(message))

    def warning(self, message: str):
        """Log nivel WARNING."""
        self.logger.warning(self._format_message(message))

    def error(self, message: str):
        """Log nivel ERROR."""
        self.logger.error(self._format_message(message))

    def critical(self, message: str):
        """Log nivel CRITICAL."""
        self.logger.critical(self._format_message(message))

    def task_start(self, task_description: str):
        """Log de inicio de tarea."""
        self.info(f"Iniciando tarea: {task_description}")

    def task_complete(self, task_description: str, duration_ms: float):
        """Log de tarea completada."""
        self.info(
            f"Tarea completada: {task_description} "
            f"(duración: {duration_ms:.2f}ms)"
        )

    def task_failed(self, task_description: str, error: str):
        """Log de tarea fallida."""
        self.error(f"Tarea fallida: {task_description} - Error: {error}")

    def tool_call(self, tool_name: str, arguments: dict):
        """Log de llamada a herramienta MCP."""
        self.debug(f"Llamando herramienta: {tool_name} con args: {arguments}")

    def tool_response(self, tool_name: str, success: bool, duration_ms: float):
        """Log de respuesta de herramienta."""
        status = "éxito" if success else "fallo"
        self.debug(
            f"Herramienta {tool_name} completada con {status} "
            f"({duration_ms:.2f}ms)"
        )


# Logger global del sistema
system_logger = setup_logger(
    name="agent_system",
    level=logging.INFO,
    log_file="logs/agent_system.log"
)
