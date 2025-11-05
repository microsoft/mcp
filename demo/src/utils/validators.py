"""
Validadores para el sistema de agentes.
"""

import re
from typing import Any, Dict, List, Optional
from pathlib import Path


class ValidationError(Exception):
    """Excepción para errores de validación."""
    pass


class Validators:
    """Colección de validadores para parámetros comunes."""

    @staticmethod
    def validate_azure_resource_name(name: str, resource_type: str = "resource") -> bool:
        """
        Valida un nombre de recurso de Azure.

        Args:
            name: Nombre a validar
            resource_type: Tipo de recurso

        Returns:
            True si es válido

        Raises:
            ValidationError: Si el nombre no es válido
        """
        if not name:
            raise ValidationError(f"El nombre de {resource_type} no puede estar vacío")

        if len(name) < 3:
            raise ValidationError(
                f"El nombre de {resource_type} debe tener al menos 3 caracteres"
            )

        if len(name) > 24:
            raise ValidationError(
                f"El nombre de {resource_type} no puede tener más de 24 caracteres"
            )

        if not re.match(r'^[a-z0-9][a-z0-9-]*[a-z0-9]$', name):
            raise ValidationError(
                f"El nombre de {resource_type} solo puede contener letras minúsculas, "
                "números y guiones, y debe comenzar y terminar con letra o número"
            )

        return True

    @staticmethod
    def validate_storage_container_name(name: str) -> bool:
        """
        Valida un nombre de contenedor de Azure Storage.

        Args:
            name: Nombre a validar

        Returns:
            True si es válido

        Raises:
            ValidationError: Si el nombre no es válido
        """
        if not name:
            raise ValidationError("El nombre del contenedor no puede estar vacío")

        if len(name) < 3 or len(name) > 63:
            raise ValidationError(
                "El nombre del contenedor debe tener entre 3 y 63 caracteres"
            )

        if not re.match(r'^[a-z0-9]([a-z0-9-]*[a-z0-9])?$', name):
            raise ValidationError(
                "El nombre del contenedor solo puede contener letras minúsculas, "
                "números y guiones, y debe comenzar y terminar con letra o número"
            )

        if '--' in name:
            raise ValidationError(
                "El nombre del contenedor no puede contener guiones consecutivos"
            )

        return True

    @staticmethod
    def validate_key_vault_secret_name(name: str) -> bool:
        """
        Valida un nombre de secreto de Key Vault.

        Args:
            name: Nombre a validar

        Returns:
            True si es válido

        Raises:
            ValidationError: Si el nombre no es válido
        """
        if not name:
            raise ValidationError("El nombre del secreto no puede estar vacío")

        if len(name) > 127:
            raise ValidationError(
                "El nombre del secreto no puede tener más de 127 caracteres"
            )

        if not re.match(r'^[a-zA-Z0-9-]+$', name):
            raise ValidationError(
                "El nombre del secreto solo puede contener letras, "
                "números y guiones"
            )

        return True

    @staticmethod
    def validate_cosmos_database_name(name: str) -> bool:
        """
        Valida un nombre de base de datos de Cosmos DB.

        Args:
            name: Nombre a validar

        Returns:
            True si es válido

        Raises:
            ValidationError: Si el nombre no es válido
        """
        if not name:
            raise ValidationError(
                "El nombre de la base de datos no puede estar vacío"
            )

        if len(name) > 255:
            raise ValidationError(
                "El nombre de la base de datos no puede tener más de 255 caracteres"
            )

        invalid_chars = ['/', '\\', '#', '?']
        for char in invalid_chars:
            if char in name:
                raise ValidationError(
                    f"El nombre de la base de datos no puede contener '{char}'"
                )

        if name.endswith(' '):
            raise ValidationError(
                "El nombre de la base de datos no puede terminar con espacio"
            )

        return True

    @staticmethod
    def validate_file_path(path: str, must_exist: bool = False) -> bool:
        """
        Valida una ruta de archivo.

        Args:
            path: Ruta a validar
            must_exist: Si True, verifica que el archivo exista

        Returns:
            True si es válido

        Raises:
            ValidationError: Si la ruta no es válida
        """
        if not path:
            raise ValidationError("La ruta del archivo no puede estar vacía")

        file_path = Path(path)

        if must_exist and not file_path.exists():
            raise ValidationError(f"El archivo no existe: {path}")

        if must_exist and not file_path.is_file():
            raise ValidationError(f"La ruta no es un archivo: {path}")

        return True

    @staticmethod
    def validate_json_object(obj: Any, required_fields: List[str]) -> bool:
        """
        Valida que un objeto JSON tenga los campos requeridos.

        Args:
            obj: Objeto a validar
            required_fields: Lista de campos requeridos

        Returns:
            True si es válido

        Raises:
            ValidationError: Si faltan campos
        """
        if not isinstance(obj, dict):
            raise ValidationError("El objeto debe ser un diccionario")

        missing_fields = [
            field for field in required_fields if field not in obj
        ]

        if missing_fields:
            raise ValidationError(
                f"Faltan campos requeridos: {', '.join(missing_fields)}"
            )

        return True

    @staticmethod
    def validate_connection_string(conn_str: str) -> bool:
        """
        Valida una cadena de conexión.

        Args:
            conn_str: Cadena de conexión a validar

        Returns:
            True si es válido

        Raises:
            ValidationError: Si la cadena no es válida
        """
        if not conn_str:
            raise ValidationError("La cadena de conexión no puede estar vacía")

        if len(conn_str) < 10:
            raise ValidationError("La cadena de conexión es demasiado corta")

        # Verificar que tenga formato key=value
        if '=' not in conn_str:
            raise ValidationError(
                "La cadena de conexión debe tener formato key=value"
            )

        return True

    @staticmethod
    def sanitize_input(text: str, max_length: int = 1000) -> str:
        """
        Sanitiza una entrada de texto.

        Args:
            text: Texto a sanitizar
            max_length: Longitud máxima

        Returns:
            Texto sanitizado
        """
        if not text:
            return ""

        # Truncar si es muy largo
        if len(text) > max_length:
            text = text[:max_length]

        # Eliminar caracteres de control
        text = ''.join(char for char in text if ord(char) >= 32 or char == '\n')

        return text.strip()


class ParameterValidator:
    """
    Validador de parámetros para llamadas a herramientas MCP.
    """

    def __init__(self, tool_name: str):
        """
        Inicializa el validador.

        Args:
            tool_name: Nombre de la herramienta
        """
        self.tool_name = tool_name
        self.errors: List[str] = []

    def add_error(self, error: str):
        """Agrega un error de validación."""
        self.errors.append(error)

    def validate_required(self, params: Dict[str, Any], required: List[str]):
        """
        Valida que existan parámetros requeridos.

        Args:
            params: Parámetros a validar
            required: Lista de parámetros requeridos
        """
        for param in required:
            if param not in params:
                self.add_error(f"Falta parámetro requerido: {param}")

    def validate_type(
        self,
        params: Dict[str, Any],
        param_name: str,
        expected_type: type
    ):
        """
        Valida el tipo de un parámetro.

        Args:
            params: Parámetros
            param_name: Nombre del parámetro
            expected_type: Tipo esperado
        """
        if param_name in params:
            if not isinstance(params[param_name], expected_type):
                self.add_error(
                    f"Parámetro '{param_name}' debe ser de tipo "
                    f"{expected_type.__name__}"
                )

    def validate_enum(
        self,
        params: Dict[str, Any],
        param_name: str,
        valid_values: List[Any]
    ):
        """
        Valida que un parámetro esté en una lista de valores válidos.

        Args:
            params: Parámetros
            param_name: Nombre del parámetro
            valid_values: Lista de valores válidos
        """
        if param_name in params:
            if params[param_name] not in valid_values:
                self.add_error(
                    f"Parámetro '{param_name}' debe ser uno de: "
                    f"{', '.join(map(str, valid_values))}"
                )

    def is_valid(self) -> bool:
        """Retorna True si no hay errores de validación."""
        return len(self.errors) == 0

    def get_errors(self) -> str:
        """Retorna todos los errores como string."""
        return "; ".join(self.errors)

    def raise_if_invalid(self):
        """Lanza ValidationError si hay errores."""
        if not self.is_valid():
            raise ValidationError(
                f"Validación fallida para herramienta '{self.tool_name}': "
                f"{self.get_errors()}"
            )
