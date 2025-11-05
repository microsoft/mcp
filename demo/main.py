"""
Sistema de Agentes Multi-MCP de Microsoft
Punto de entrada principal
"""

import asyncio
import sys
from pathlib import Path

import click
from rich.console import Console
from rich.panel import Panel
from rich.table import Table

from src.core.agent_base import AgentSystem
from src.agents.azure_storage import AzureStorageAgent
from src.agents.azure_security import AzureSecurityAgent
from src.agents.fabric import FabricAgent
from src.agents.orchestrator import OrchestratorAgent
from src.utils.logger import setup_logger


console = Console()


@click.group()
@click.option('--config', default='config/mcp_config.json', help='Ruta al archivo de configuración MCP')
@click.option('--log-level', default='INFO', help='Nivel de logging')
@click.pass_context
def cli(ctx, config, log_level):
    """Sistema de Agentes Multi-MCP de Microsoft."""
    ctx.ensure_object(dict)
    ctx.obj['config'] = config
    ctx.obj['log_level'] = log_level

    # Configurar logger
    setup_logger(
        name="agent_system",
        level=getattr(__import__('logging'), log_level),
        log_file="logs/agent_system.log"
    )


@cli.command()
@click.pass_context
def info(ctx):
    """Muestra información del sistema de agentes."""
    console.print(Panel.fit(
        "[bold cyan]Sistema de Agentes Multi-MCP de Microsoft[/bold cyan]\n\n"
        "Un sistema modular de agentes que utiliza los servidores MCP\n"
        "de Microsoft para automatizar tareas en Azure y Fabric.\n\n"
        "[yellow]Agentes disponibles:[/yellow]\n"
        "  • Azure Storage Agent\n"
        "  • Azure Security Agent (Key Vault)\n"
        "  • Azure AI Agent\n"
        "  • Azure Data Agent\n"
        "  • Azure Infrastructure Agent\n"
        "  • Fabric Agent\n"
        "  • Orchestrator Agent\n\n"
        "[yellow]Workflows disponibles:[/yellow]\n"
        "  • data_pipeline - Pipeline de datos completo\n"
        "  • secure_app - Aplicación segura\n"
        "  • intelligent_search - Búsqueda inteligente\n"
        "  • fabric_infrastructure - Infraestructura Fabric",
        border_style="cyan"
    ))


@cli.command()
@click.option('--agent', required=True, help='Nombre del agente')
@click.pass_context
def list_capabilities(ctx, agent):
    """Lista las capacidades de un agente específico."""
    async def _list():
        system = AgentSystem(config_path=ctx.obj['config'])

        # Registrar agentes
        agents_map = {
            'azure_storage': AzureStorageAgent(system.client_pool),
            'azure_security': AzureSecurityAgent(system.client_pool),
            'fabric': FabricAgent(system.client_pool)
        }

        if agent not in agents_map:
            console.print(f"[red]Agente no encontrado: {agent}[/red]")
            console.print(f"Agentes disponibles: {', '.join(agents_map.keys())}")
            return

        selected_agent = agents_map[agent]
        system.register_agent(selected_agent)

        if await system.initialize():
            table = Table(title=f"Capacidades de {agent}")
            table.add_column("Capacidad", style="cyan")
            table.add_column("Descripción", style="green")

            for cap in selected_agent.capabilities:
                table.add_row(cap.name, cap.description)

            console.print(table)

            await system.shutdown()
        else:
            console.print("[red]Error al inicializar el sistema[/red]")

    asyncio.run(_list())


@cli.command()
@click.option('--workflow', required=True, help='Nombre del workflow')
@click.option('--params', default='{}', help='Parámetros en formato JSON')
@click.pass_context
def run_workflow(ctx, workflow, params):
    """Ejecuta un workflow predefinido."""
    import json

    async def _run():
        console.print(f"\n[yellow]Ejecutando workflow: {workflow}[/yellow]\n")

        system = AgentSystem(config_path=ctx.obj['config'])

        # Registrar todos los agentes
        system.register_agent(AzureStorageAgent(system.client_pool))
        system.register_agent(AzureSecurityAgent(system.client_pool))
        system.register_agent(FabricAgent(system.client_pool))

        orchestrator = OrchestratorAgent(system)
        system.register_agent(orchestrator)

        if await system.initialize():
            try:
                workflow_params = json.loads(params)
                result = await orchestrator.execute_workflow(workflow, workflow_params)

                console.print(Panel(
                    f"[bold green]Workflow completado[/bold green]\n\n"
                    f"Workflow: {result['workflow']}\n"
                    f"Estado: {result['status']}\n"
                    f"Operaciones: {len(result.get('results', {}))}",
                    border_style="green"
                ))

            except Exception as e:
                console.print(f"[red]Error al ejecutar workflow: {e}[/red]")
            finally:
                await system.shutdown()
        else:
            console.print("[red]Error al inicializar el sistema[/red]")

    asyncio.run(_run())


@cli.command()
@click.option('--agent', required=True, help='Nombre del agente')
@click.option('--task', required=True, help='Descripción de la tarea')
@click.option('--params', default='{}', help='Parámetros en formato JSON')
@click.pass_context
def execute(ctx, agent, task, params):
    """Ejecuta una tarea en un agente específico."""
    import json

    async def _execute():
        console.print(f"\n[yellow]Ejecutando tarea en {agent}...[/yellow]\n")

        system = AgentSystem(config_path=ctx.obj['config'])

        # Registrar el agente solicitado
        agents_map = {
            'azure_storage': AzureStorageAgent(system.client_pool),
            'azure_security': AzureSecurityAgent(system.client_pool),
            'fabric': FabricAgent(system.client_pool)
        }

        if agent not in agents_map:
            console.print(f"[red]Agente no encontrado: {agent}[/red]")
            return

        selected_agent = agents_map[agent]
        system.register_agent(selected_agent)

        if await system.initialize():
            try:
                task_params = json.loads(params)
                result = await selected_agent.process_task(task, **task_params)

                if result.error:
                    console.print(f"[red]Error: {result.error}[/red]")
                else:
                    console.print(Panel(
                        f"[bold green]Tarea completada[/bold green]\n\n"
                        f"Agente: {result.agent_name}\n"
                        f"Estado: {result.status.value}\n\n"
                        f"Resultado:\n{result.result}",
                        border_style="green"
                    ))

            except Exception as e:
                console.print(f"[red]Error: {e}[/red]")
            finally:
                await system.shutdown()
        else:
            console.print("[red]Error al inicializar el sistema[/red]")

    asyncio.run(_execute())


@cli.command()
@click.pass_context
def interactive(ctx):
    """Modo interactivo para ejecutar tareas."""
    async def _interactive():
        console.print(Panel.fit(
            "[bold cyan]Modo Interactivo[/bold cyan]\n"
            "Escribe comandos en lenguaje natural",
            border_style="cyan"
        ))

        system = AgentSystem(config_path=ctx.obj['config'])

        # Registrar todos los agentes
        system.register_agent(AzureStorageAgent(system.client_pool))
        system.register_agent(AzureSecurityAgent(system.client_pool))
        system.register_agent(FabricAgent(system.client_pool))

        orchestrator = OrchestratorAgent(system)
        system.register_agent(orchestrator)

        if not await system.initialize():
            console.print("[red]Error al inicializar el sistema[/red]")
            return

        try:
            console.print("\n[green]Sistema inicializado. Escribe 'exit' para salir.[/green]\n")

            while True:
                try:
                    command = console.input("[bold cyan]> [/bold cyan]")

                    if command.lower() in ['exit', 'quit', 'salir']:
                        break

                    if not command.strip():
                        continue

                    # Procesar comando con orchestrator
                    result = await orchestrator.process_task(command)

                    if result.error:
                        console.print(f"[red]Error: {result.error}[/red]")
                    else:
                        console.print(f"[green]Resultado:[/green] {result.result}\n")

                except KeyboardInterrupt:
                    break
                except Exception as e:
                    console.print(f"[red]Error: {e}[/red]")

        finally:
            await system.shutdown()
            console.print("\n[yellow]Sistema cerrado[/yellow]")

    asyncio.run(_interactive())


@cli.command()
def examples():
    """Ejecuta los ejemplos básicos."""
    console.print("[yellow]Ejecutando ejemplos básicos...[/yellow]\n")

    import subprocess
    result = subprocess.run(
        [sys.executable, "examples/basic_usage.py"],
        cwd=Path(__file__).parent
    )

    sys.exit(result.returncode)


@cli.command()
@click.option('--scenario', type=click.Choice(['data_pipeline', 'secure_app', 'intelligent_search', 'fabric_infrastructure']), required=True)
def demo(scenario):
    """Ejecuta un escenario de demostración."""
    console.print(f"[yellow]Ejecutando escenario: {scenario}[/yellow]\n")

    scenario_files = {
        'data_pipeline': 'src/scenarios/data_pipeline.py'
    }

    if scenario in scenario_files:
        import subprocess
        result = subprocess.run(
            [sys.executable, scenario_files[scenario]],
            cwd=Path(__file__).parent
        )
        sys.exit(result.returncode)
    else:
        console.print(f"[red]Escenario no implementado: {scenario}[/red]")
        console.print("[yellow]Disponible: data_pipeline[/yellow]")


def main():
    """Punto de entrada principal."""
    cli(obj={})


if __name__ == '__main__':
    main()
