"""
Escenario 1: Pipeline de Datos Completo

Este escenario demuestra cómo crear un pipeline end-to-end
desde la ingesta de datos hasta el análisis, usando múltiples
servicios de Azure y Microsoft Fabric.
"""

import asyncio
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).parent.parent.parent))

from src.core.agent_base import AgentSystem
from src.agents.azure_storage import AzureStorageAgent
from src.agents.fabric import FabricAgent
from src.agents.orchestrator import OrchestratorAgent
from src.utils.logger import setup_logger
from rich.console import Console
from rich.table import Table
from rich.panel import Panel
from rich.progress import Progress


console = Console()


async def run_data_pipeline_scenario():
    """
    Escenario: Pipeline de Datos Completo

    Flujo:
    1. Crear contenedor en Azure Storage para datos raw
    2. Subir archivos de datos (simulado)
    3. Generar definición de Lakehouse en Fabric
    4. Mostrar schema y best practices
    5. Generar reportes de configuración
    """
    console.print(Panel.fit(
        "[bold cyan]Escenario 1: Pipeline de Datos Completo[/bold cyan]\n"
        "Creando un pipeline end-to-end con Azure y Fabric",
        border_style="cyan"
    ))

    # Inicializar sistema
    system = AgentSystem(config_path="config/mcp_config.json")

    # Registrar agentes
    storage_agent = AzureStorageAgent(system.client_pool)
    fabric_agent = FabricAgent(system.client_pool)

    system.register_agent(storage_agent)
    system.register_agent(fabric_agent)

    orchestrator = OrchestratorAgent(system)
    system.register_agent(orchestrator)

    console.print("\n[yellow]Inicializando sistema de agentes...[/yellow]")

    with Progress() as progress:
        task = progress.add_task("[cyan]Iniciando servidores MCP...", total=100)

        if await system.initialize():
            progress.update(task, completed=100)
            console.print("[green]✓ Sistema inicializado correctamente[/green]\n")
        else:
            console.print("[red]✗ Error al inicializar el sistema[/red]")
            return

    try:
        # Paso 1: Crear infraestructura de Storage
        console.print(Panel(
            "[bold]Paso 1: Crear infraestructura de Storage[/bold]",
            border_style="blue"
        ))

        result = await storage_agent.process_task(
            "Crear contenedor para datos raw",
            container_name="sales-data-raw",
            storage_account="analytics"
        )

        if not result.error:
            console.print(f"[green]✓ Contenedor creado: {result.result.get('container_name')}[/green]")
        else:
            console.print(f"[red]✗ Error: {result.error}[/red]")

        # Paso 2: Listar workloads de Fabric
        console.print(Panel(
            "[bold]Paso 2: Explorar workloads de Fabric[/bold]",
            border_style="blue"
        ))

        result = await fabric_agent.process_task(
            "Listar workloads de Fabric",
            {}
        )

        if not result.error and result.result:
            workloads = result.result.get('workloads', [])
            console.print(f"[green]✓ Encontrados {len(workloads)} workloads[/green]")

            # Mostrar workloads en tabla
            table = Table(title="Workloads Disponibles")
            table.add_column("Workload", style="cyan")

            for workload in workloads[:10]:
                table.add_row(workload)

            console.print(table)

        # Paso 3: Generar definición de Lakehouse
        console.print(Panel(
            "[bold]Paso 3: Generar definición de Lakehouse[/bold]",
            border_style="blue"
        ))

        result = await fabric_agent.process_task(
            "Generar definición completa de Lakehouse",
            workload_type="Lakehouse",
            resource_name="sales-analytics-lakehouse",
            properties={
                "description": "Lakehouse para análisis de ventas",
                "tables": {
                    "sales_transactions": {
                        "columns": [
                            {"name": "transaction_id", "type": "string"},
                            {"name": "customer_id", "type": "string"},
                            {"name": "product_id", "type": "string"},
                            {"name": "amount", "type": "decimal"},
                            {"name": "timestamp", "type": "timestamp"}
                        ]
                    },
                    "customers": {
                        "columns": [
                            {"name": "customer_id", "type": "string"},
                            {"name": "name", "type": "string"},
                            {"name": "email", "type": "string"},
                            {"name": "region", "type": "string"}
                        ]
                    }
                }
            }
        )

        if not result.error:
            console.print("[green]✓ Definición de Lakehouse generada[/green]")

            # Mostrar schema generado
            definition = result.result.get('generated_definition', {})
            console.print(f"\n[cyan]Nombre del recurso:[/cyan] {definition.get('displayName')}")
            console.print(f"[cyan]Tipo:[/cyan] {definition.get('type')}")

        # Paso 4: Obtener best practices
        console.print(Panel(
            "[bold]Paso 4: Obtener mejores prácticas[/bold]",
            border_style="blue"
        ))

        result = await fabric_agent.process_task(
            "Obtener mejores prácticas para Lakehouse",
            workload_type="Lakehouse"
        )

        if not result.error and result.result:
            practices = result.result.get('practices', [])
            if practices:
                console.print("[green]✓ Mejores prácticas obtenidas[/green]\n")
                for i, practice in enumerate(practices[:5], 1):
                    console.print(f"  {i}. {practice}")

        # Paso 5: Ejecutar workflow completo con orchestrator
        console.print(Panel(
            "[bold]Paso 5: Ejecutar workflow completo[/bold]",
            border_style="blue"
        ))

        result = await orchestrator.execute_workflow(
            "data_pipeline",
            {
                "container_name": "pipeline-data",
                "lakehouse_name": "complete-analytics"
            }
        )

        console.print(f"\n[green]✓ Workflow '{result['workflow']}' completado[/green]")

        # Mostrar resumen
        console.print(Panel(
            "[bold green]Pipeline de Datos Completado[/bold green]\n\n"
            "Componentes creados:\n"
            "  • Contenedor de Storage: sales-data-raw\n"
            "  • Lakehouse: sales-analytics-lakehouse\n"
            "  • Schema: 2 tablas definidas\n"
            "  • Best practices aplicadas\n\n"
            "[yellow]Próximos pasos:[/yellow]\n"
            "  1. Cargar datos en el contenedor\n"
            "  2. Configurar data pipelines\n"
            "  3. Crear notebooks para análisis\n"
            "  4. Configurar alertas y monitoreo",
            border_style="green"
        ))

    finally:
        await system.shutdown()
        console.print("\n[yellow]Sistema cerrado correctamente[/yellow]")


def main():
    """Punto de entrada del escenario."""
    setup_logger(name="agent_system", log_file="logs/data_pipeline.log")

    try:
        asyncio.run(run_data_pipeline_scenario())
    except KeyboardInterrupt:
        console.print("\n[red]Escenario interrumpido por el usuario[/red]")
    except Exception as e:
        console.print(f"\n[red]Error: {e}[/red]")


if __name__ == "__main__":
    main()
