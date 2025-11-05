"""
Ejemplos básicos de uso del sistema de agentes MCP.
"""

import asyncio
import sys
from pathlib import Path

# Agregar el directorio src al path
sys.path.insert(0, str(Path(__file__).parent.parent))

from src.core.agent_base import AgentSystem
from src.agents.azure_storage import AzureStorageAgent
from src.agents.azure_security import AzureSecurityAgent
from src.agents.fabric import FabricAgent
from src.agents.orchestrator import OrchestratorAgent
from src.utils.logger import setup_logger


async def example_1_storage_operations():
    """Ejemplo 1: Operaciones básicas con Azure Storage."""
    print("\n" + "="*60)
    print("EJEMPLO 1: Operaciones con Azure Storage")
    print("="*60 + "\n")

    # Crear sistema de agentes
    system = AgentSystem(config_path="../config/mcp_config.json")

    # Registrar agentes
    storage_agent = AzureStorageAgent(system.client_pool)
    system.register_agent(storage_agent)

    # Inicializar sistema
    print("Inicializando sistema de agentes...")
    if not await system.initialize():
        print("Error: No se pudo inicializar el sistema")
        return

    try:
        # Listar contenedores
        print("\n1. Listando contenedores de storage...")
        result = await system.execute_task(
            "azure_storage",
            "Listar todos los contenedores de storage",
            storage_account="mystorageaccount"
        )

        if result and result.result:
            print(f"   Encontrados {result.result.get('count', 0)} contenedores")

        # Crear contenedor
        print("\n2. Creando nuevo contenedor...")
        result = await system.execute_task(
            "azure_storage",
            "Crear un contenedor llamado 'demo-data'",
            container_name="demo-data",
            storage_account="mystorageaccount"
        )

        if result and not result.error:
            print(f"   Contenedor creado: {result.result.get('container_name')}")
        else:
            print(f"   Error: {result.error}")

    finally:
        # Cerrar sistema
        await system.shutdown()
        print("\nSistema cerrado correctamente")


async def example_2_keyvault_secrets():
    """Ejemplo 2: Gestión de secretos en Key Vault."""
    print("\n" + "="*60)
    print("EJEMPLO 2: Gestión de secretos en Key Vault")
    print("="*60 + "\n")

    system = AgentSystem(config_path="../config/mcp_config.json")

    # Registrar agente de seguridad
    security_agent = AzureSecurityAgent(system.client_pool)
    system.register_agent(security_agent)

    print("Inicializando sistema de agentes...")
    if not await system.initialize():
        print("Error: No se pudo inicializar el sistema")
        return

    try:
        # Crear secreto
        print("\n1. Creando secreto en Key Vault...")
        result = await system.execute_task(
            "azure_security",
            "Crear un secreto llamado 'api-key' con valor 'secret123'",
            vault_name="my-keyvault",
            secret_name="api-key",
            secret_value="secret123"
        )

        if result and not result.error:
            print(f"   Secreto creado: {result.result.get('secret_name')}")
        else:
            print(f"   Error: {result.error}")

        # Listar secretos
        print("\n2. Listando todos los secretos...")
        result = await system.execute_task(
            "azure_security",
            "Listar todos los secretos en el vault",
            vault_name="my-keyvault"
        )

        if result and result.result:
            secrets = result.result.get('secrets', [])
            print(f"   Encontrados {len(secrets)} secretos:")
            for secret in secrets[:5]:  # Mostrar solo los primeros 5
                print(f"     - {secret}")

    finally:
        await system.shutdown()
        print("\nSistema cerrado correctamente")


async def example_3_fabric_workloads():
    """Ejemplo 3: Explorar workloads de Microsoft Fabric."""
    print("\n" + "="*60)
    print("EJEMPLO 3: Explorar workloads de Microsoft Fabric")
    print("="*60 + "\n")

    system = AgentSystem(config_path="../config/mcp_config.json")

    # Registrar agente de Fabric
    fabric_agent = FabricAgent(system.client_pool)
    system.register_agent(fabric_agent)

    print("Inicializando sistema de agentes...")
    if not await system.initialize():
        print("Error: No se pudo inicializar el sistema")
        return

    try:
        # Listar workloads
        print("\n1. Listando workloads disponibles en Fabric...")
        result = await system.execute_task(
            "fabric",
            "Listar todos los workloads de Fabric",
            {}
        )

        if result and result.result:
            workloads = result.result.get('workloads', [])
            print(f"   Encontrados {len(workloads)} workloads:")
            for workload in workloads:
                print(f"     - {workload}")

        # Generar definición de Lakehouse
        print("\n2. Generando definición de Lakehouse...")
        result = await system.execute_task(
            "fabric",
            "Generar definición de Lakehouse con tablas customers y orders",
            workload_type="Lakehouse",
            resource_name="sales-lakehouse",
            properties={
                "tables": [
                    {
                        "name": "customers",
                        "columns": [
                            {"name": "id", "type": "int"},
                            {"name": "name", "type": "string"},
                            {"name": "email", "type": "string"}
                        ]
                    },
                    {
                        "name": "orders",
                        "columns": [
                            {"name": "id", "type": "int"},
                            {"name": "customer_id", "type": "int"},
                            {"name": "amount", "type": "decimal"},
                            {"name": "date", "type": "datetime"}
                        ]
                    }
                ]
            }
        )

        if result and not result.error:
            print(f"   Definición generada para: {result.result.get('workload_type')}")
        else:
            print(f"   Error: {result.error}")

    finally:
        await system.shutdown()
        print("\nSistema cerrado correctamente")


async def example_4_orchestrator_workflow():
    """Ejemplo 4: Usar el orchestrator para workflows complejos."""
    print("\n" + "="*60)
    print("EJEMPLO 4: Workflows con Orchestrator")
    print("="*60 + "\n")

    system = AgentSystem(config_path="../config/mcp_config.json")

    # Registrar todos los agentes
    system.register_agent(AzureStorageAgent(system.client_pool))
    system.register_agent(AzureSecurityAgent(system.client_pool))
    system.register_agent(FabricAgent(system.client_pool))

    # Crear orchestrator
    orchestrator = OrchestratorAgent(system)
    system.register_agent(orchestrator)

    print("Inicializando sistema de agentes...")
    if not await system.initialize():
        print("Error: No se pudo inicializar el sistema")
        return

    try:
        # Ejecutar workflow de pipeline de datos
        print("\n1. Ejecutando workflow: Pipeline de datos...")
        result = await orchestrator.execute_workflow(
            "data_pipeline",
            {
                "container_name": "data-raw",
                "lakehouse_name": "analytics-lakehouse"
            }
        )

        print(f"   Workflow completado: {result['status']}")
        print(f"   Resultados: {len(result.get('results', {}))} operaciones")

        # Ejecutar workflow de aplicación segura
        print("\n2. Ejecutando workflow: Aplicación segura...")
        result = await orchestrator.execute_workflow(
            "secure_app",
            {
                "vault_name": "app-vault",
                "secret_name": "db-connection",
                "secret_value": "Server=myserver;Database=mydb",
                "static_container": "app-static"
            }
        )

        print(f"   Workflow completado: {result['status']}")

    finally:
        await system.shutdown()
        print("\nSistema cerrado correctamente")


def main():
    """Función principal que ejecuta todos los ejemplos."""
    print("\n" + "="*60)
    print("SISTEMA DE AGENTES MULTI-MCP DE MICROSOFT")
    print("Ejemplos Básicos de Uso")
    print("="*60)

    # Configurar logger
    setup_logger(name="agent_system", log_file="logs/examples.log")

    # Ejecutar ejemplos
    examples = [
        ("Operaciones con Azure Storage", example_1_storage_operations),
        ("Gestión de secretos en Key Vault", example_2_keyvault_secrets),
        ("Explorar workloads de Fabric", example_3_fabric_workloads),
        ("Workflows con Orchestrator", example_4_orchestrator_workflow)
    ]

    print("\nEjemplos disponibles:")
    for i, (name, _) in enumerate(examples, 1):
        print(f"{i}. {name}")

    print("\nEjecutando todos los ejemplos...")
    print("(Nota: Algunos ejemplos pueden fallar si no tienes recursos Azure configurados)\n")

    for name, example_func in examples:
        try:
            asyncio.run(example_func())
        except KeyboardInterrupt:
            print("\n\nEjecución interrumpida por el usuario")
            break
        except Exception as e:
            print(f"\nError en ejemplo '{name}': {e}")

    print("\n" + "="*60)
    print("Ejemplos completados")
    print("="*60 + "\n")


if __name__ == "__main__":
    main()
