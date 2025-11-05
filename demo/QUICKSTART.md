# Guía de Inicio Rápido

## Instalación en 5 minutos

### 1. Requisitos previos

- Python 3.10 o superior
- .NET 10 SDK o Node.js 20+ (para servidores MCP)
- Azure CLI (para autenticación)
- Cuenta de Azure con una suscripción activa

### 2. Clonar y configurar

```bash
# Ya estás en el repositorio
cd demo

# Instalar dependencias Python
pip install -r requirements.txt

# Autenticar con Azure
az login
```

### 3. Compilar servidores MCP

#### Opción A: Usar .NET (recomendado)

```bash
# Compilar Azure MCP Server
cd ../servers/Azure.Mcp.Server
dotnet build --configuration Release

# Compilar Fabric MCP Server
cd ../Fabric.Mcp.Server
dotnet build --configuration Release

cd ../../demo
```

#### Opción B: Usar NPM

```bash
# Instalar Azure MCP Server desde NPM
npm install -g @azure/mcp@latest
```

### 4. Ejecutar tu primera demo

```bash
# Ver información del sistema
python main.py info

# Ejecutar ejemplos básicos
python main.py examples

# O ejecutar un escenario específico
python main.py demo --scenario data_pipeline
```

## Comandos Básicos

### Mostrar información del sistema
```bash
python main.py info
```

### Listar capacidades de un agente
```bash
python main.py list-capabilities --agent azure_storage
python main.py list-capabilities --agent azure_security
python main.py list-capabilities --agent fabric
```

### Ejecutar una tarea específica
```bash
python main.py execute \
  --agent azure_storage \
  --task "Listar todos los contenedores" \
  --params '{"storage_account": "mystorageaccount"}'
```

### Ejecutar un workflow
```bash
python main.py run-workflow \
  --workflow data_pipeline \
  --params '{"container_name": "demo-data", "lakehouse_name": "analytics"}'
```

### Modo interactivo
```bash
python main.py interactive
```

En modo interactivo puedes escribir comandos en lenguaje natural:
```
> Crear un contenedor llamado 'demo-data'
> Listar todos los secretos en mi Key Vault
> Generar una definición de Lakehouse para ventas
> exit
```

## Ejemplos Rápidos

### 1. Trabajar con Azure Storage

```python
from src.core.agent_base import AgentSystem
from src.agents.azure_storage import AzureStorageAgent

async def demo():
    system = AgentSystem(config_path="config/mcp_config.json")
    storage_agent = AzureStorageAgent(system.client_pool)
    system.register_agent(storage_agent)

    await system.initialize()

    # Crear contenedor
    result = await storage_agent.process_task(
        "Crear un contenedor llamado 'test-data'",
        container_name="test-data"
    )

    print(result.result)

    await system.shutdown()

import asyncio
asyncio.run(demo())
```

### 2. Gestionar secretos en Key Vault

```python
from src.agents.azure_security import AzureSecurityAgent

async def demo():
    system = AgentSystem(config_path="config/mcp_config.json")
    security_agent = AzureSecurityAgent(system.client_pool)
    system.register_agent(security_agent)

    await system.initialize()

    # Crear secreto
    result = await security_agent.process_task(
        "Crear secreto 'db-password' con valor 'SecurePassword123!'",
        vault_name="my-vault",
        secret_name="db-password",
        secret_value="SecurePassword123!"
    )

    print(result.result)

    await system.shutdown()

asyncio.run(demo())
```

### 3. Explorar Microsoft Fabric

```python
from src.agents.fabric import FabricAgent

async def demo():
    system = AgentSystem(config_path="config/mcp_config.json")
    fabric_agent = FabricAgent(system.client_pool)
    system.register_agent(fabric_agent)

    await system.initialize()

    # Listar workloads
    result = await fabric_agent.process_task(
        "Listar todos los workloads disponibles",
        {}
    )

    print(f"Workloads: {result.result['workloads']}")

    # Generar definición de Lakehouse
    result = await fabric_agent.process_task(
        "Generar definición de Lakehouse",
        workload_type="Lakehouse",
        resource_name="my-lakehouse"
    )

    print(result.result)

    await system.shutdown()

asyncio.run(demo())
```

### 4. Usar el Orchestrator para workflows complejos

```python
from src.agents.orchestrator import OrchestratorAgent

async def demo():
    system = AgentSystem(config_path="config/mcp_config.json")

    # Registrar todos los agentes
    from src.agents.azure_storage import AzureStorageAgent
    from src.agents.azure_security import AzureSecurityAgent
    from src.agents.fabric import FabricAgent

    system.register_agent(AzureStorageAgent(system.client_pool))
    system.register_agent(AzureSecurityAgent(system.client_pool))
    system.register_agent(FabricAgent(system.client_pool))

    orchestrator = OrchestratorAgent(system)
    system.register_agent(orchestrator)

    await system.initialize()

    # Ejecutar workflow
    result = await orchestrator.execute_workflow(
        "data_pipeline",
        {
            "container_name": "pipeline-data",
            "lakehouse_name": "analytics-warehouse"
        }
    )

    print(f"Workflow: {result['workflow']}")
    print(f"Estado: {result['status']}")
    print(f"Resultados: {result['results']}")

    await system.shutdown()

asyncio.run(demo())
```

## Escenarios de Demostración

### Escenario 1: Pipeline de Datos Completo
```bash
python main.py demo --scenario data_pipeline
```

Crea un pipeline end-to-end:
- Contenedor de Storage para datos raw
- Definición de Lakehouse en Fabric
- Configuración de tablas y schemas
- Best practices aplicadas

### Escenario 2: Aplicación Segura
```bash
python main.py demo --scenario secure_app
```

Despliega una aplicación con seguridad:
- Secretos en Key Vault
- Contenedor para archivos estáticos
- Configuración de aplicación

## Solución de Problemas

### Error: "El servidor MCP no está ejecutándose"

Asegúrate de que los servidores MCP están compilados:
```bash
cd ../servers/Azure.Mcp.Server
dotnet build --configuration Release
```

### Error: "No se pudo autenticar con Azure"

Ejecuta:
```bash
az login
az account show
```

### Error: "ModuleNotFoundError"

Instala las dependencias:
```bash
pip install -r requirements.txt
```

### Los logs no se generan

Crea el directorio de logs:
```bash
mkdir -p logs
```

## Próximos Pasos

1. Explora los ejemplos en `examples/basic_usage.py`
2. Lee la documentación completa en `README.md`
3. Personaliza los agentes en `src/agents/`
4. Crea tus propios workflows en `src/scenarios/`
5. Integra con tus aplicaciones existentes

## Recursos

- [Documentación Azure MCP](https://learn.microsoft.com/azure/developer/azure-mcp-server/)
- [Microsoft Fabric Docs](https://learn.microsoft.com/fabric/)
- [MCP Protocol](https://modelcontextprotocol.io)
- [Repositorio GitHub](https://github.com/microsoft/mcp)

## Soporte

¿Problemas o preguntas?
- Abre un issue en: https://github.com/microsoft/mcp/issues
- Revisa el troubleshooting: `../servers/Azure.Mcp.Server/TROUBLESHOOTING.md`

---

**¡Disfruta construyendo con los MCPs de Microsoft!** 🚀
