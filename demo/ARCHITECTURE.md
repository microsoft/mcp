# Arquitectura del Sistema de Agentes Multi-MCP

## Visión General

Este sistema implementa una arquitectura de agentes especializados que se comunican con servidores MCP (Model Context Protocol) de Microsoft para automatizar tareas en Azure y Microsoft Fabric.

## Diagrama de Arquitectura

```
┌─────────────────────────────────────────────────────────────────┐
│                         Usuario / CLI                            │
│                        (main.py)                                 │
└─────────────────────────┬───────────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────────────┐
│                    Orchestrator Agent                            │
│              (Coordina todos los agentes)                        │
└─────────────────────────┬───────────────────────────────────────┘
                          │
        ┌─────────────────┼─────────────────┬──────────────┐
        │                 │                 │              │
        ▼                 ▼                 ▼              ▼
┌───────────────┐  ┌───────────────┐  ┌──────────┐  ┌──────────┐
│Azure Storage  │  │Azure Security │  │Azure AI  │  │  Fabric  │
│    Agent      │  │    Agent      │  │  Agent   │  │  Agent   │
└───────┬───────┘  └───────┬───────┘  └────┬─────┘  └────┬─────┘
        │                  │                │             │
        │                  │                │             │
        ▼                  ▼                ▼             ▼
┌───────────────────────────────────────────────────────────────┐
│                    MCP Client Pool                            │
│              (Gestiona conexiones a servidores MCP)           │
└─────────────────────────┬─────────────────────────────────────┘
                          │
        ┌─────────────────┼─────────────────┐
        │                 │                 │
        ▼                 ▼                 ▼
┌──────────────┐  ┌──────────────┐  ┌──────────────┐
│ Azure MCP    │  │ Fabric MCP   │  │  Other MCP   │
│   Server     │  │   Server     │  │   Servers    │
│   (stdio)    │  │   (stdio)    │  │   (stdio)    │
└──────┬───────┘  └──────┬───────┘  └──────┬───────┘
       │                 │                 │
       ▼                 ▼                 ▼
┌─────────────────────────────────────────────────┐
│          Servicios de Azure / Fabric            │
│   (Storage, Key Vault, Cosmos DB, etc.)         │
└─────────────────────────────────────────────────┘
```

## Componentes Principales

### 1. Core Components (`src/core/`)

#### `agent_base.py`
- **Agent**: Clase abstracta base para todos los agentes
- **AgentSystem**: Sistema que gestiona el ciclo de vida de todos los agentes
- **Task**: Representa una tarea que un agente debe ejecutar
- **AgentCapability**: Define las capacidades de un agente

#### `mcp_client.py`
- **MCPClient**: Cliente para comunicarse con un servidor MCP individual
- **MCPClientPool**: Pool que gestiona múltiples clientes MCP
- Implementa el protocolo JSON-RPC 2.0 sobre stdio

### 2. Specialized Agents (`src/agents/`)

#### `azure_storage.py` - AzureStorageAgent
**Responsabilidades:**
- Gestión de contenedores de blob storage
- Subida y descarga de archivos
- Listado y eliminación de blobs

**Capacidades:**
- `list_containers`: Lista contenedores
- `create_container`: Crea contenedores
- `upload_blob`: Sube archivos
- `download_blob`: Descarga archivos
- `delete_blob`: Elimina blobs

#### `azure_security.py` - AzureSecurityAgent
**Responsabilidades:**
- Gestión de Azure Key Vault
- Operaciones con secretos, claves y certificados
- Seguridad y compliance

**Capacidades:**
- `create_secret`: Crea/actualiza secretos
- `get_secret`: Obtiene secretos
- `list_secrets`: Lista secretos
- `delete_secret`: Elimina secretos
- `create_key`: Crea claves criptográficas

#### `fabric.py` - FabricAgent
**Responsabilidades:**
- Interacción con APIs de Microsoft Fabric
- Generación de definiciones de recursos
- Obtención de best practices

**Capacidades:**
- `list_workloads`: Lista workloads disponibles
- `get_workload_apis`: Obtiene OpenAPI specs
- `get_item_definition`: Obtiene schemas de items
- `get_best_practices`: Obtiene mejores prácticas
- `generate_resource_definition`: Genera definiciones completas

#### `orchestrator.py` - OrchestratorAgent
**Responsabilidades:**
- Coordinación de múltiples agentes
- Descomposición de tareas complejas
- Ejecución de workflows predefinidos
- Agregación de resultados

**Capacidades:**
- `coordinate_workflow`: Coordina flujos complejos
- `task_decomposition`: Descompone tareas
- `agent_selection`: Selecciona agentes apropiados
- `result_aggregation`: Agrega resultados

### 3. Utility Modules (`src/utils/`)

#### `logger.py`
- Sistema de logging con colores
- Logger específico por agente
- Logging a archivo y consola

#### `validators.py`
- Validadores para nombres de recursos Azure
- Validación de parámetros
- Sanitización de inputs

### 4. Scenarios (`src/scenarios/`)

Implementaciones de escenarios completos end-to-end:
- `data_pipeline.py`: Pipeline de datos completo
- `secure_app.py`: Despliegue de aplicación segura
- `intelligent_search.py`: Sistema de búsqueda
- `fabric_infrastructure.py`: Infraestructura Fabric

## Flujo de Ejecución

### Flujo Básico: Ejecutar una Tarea

```
1. Usuario ejecuta comando
   └─> main.py CLI

2. CLI inicializa AgentSystem
   └─> AgentSystem.initialize()
       └─> MCPClientPool.start_all()
           └─> Inicia procesos de servidores MCP

3. CLI ejecuta tarea en agente
   └─> Agent.process_task(description, **params)
       └─> Agent.execute_task(task)
           └─> Agent.call_mcp_tool(tool_name, arguments)
               └─> MCPClient.call_tool()
                   └─> Envía JSON-RPC request
                   └─> Recibe JSON-RPC response

4. Agente procesa respuesta
   └─> Actualiza Task con resultado
   └─> Retorna Task completada

5. CLI muestra resultado al usuario
```

### Flujo Avanzado: Workflow con Orchestrator

```
1. Usuario ejecuta workflow
   └─> main.py run-workflow --workflow data_pipeline

2. Orchestrator recibe workflow
   └─> OrchestratorAgent.execute_workflow()

3. Orchestrator descompone el workflow
   └─> _decompose_task()
       └─> Crea subtareas para cada paso

4. Para cada subtarea:
   └─> Selecciona agente apropiado
   └─> Ejecuta subtarea
       └─> AzureStorageAgent.process_task()
       └─> FabricAgent.process_task()
   └─> Recolecta resultados

5. Orchestrator agrega resultados
   └─> _generate_summary()
   └─> Retorna resultado consolidado

6. CLI muestra resumen al usuario
```

## Protocolo de Comunicación MCP

### Formato de Mensaje

#### Request
```json
{
  "jsonrpc": "2.0",
  "id": "req_123",
  "method": "tools/call",
  "params": {
    "name": "azure.storage.createContainer",
    "arguments": {
      "containerName": "demo-data",
      "storageAccount": "mystorageaccount"
    }
  }
}
```

#### Response (Success)
```json
{
  "jsonrpc": "2.0",
  "id": "req_123",
  "result": {
    "container_name": "demo-data",
    "status": "created",
    "url": "https://mystorageaccount.blob.core.windows.net/demo-data"
  }
}
```

#### Response (Error)
```json
{
  "jsonrpc": "2.0",
  "id": "req_123",
  "error": {
    "code": -32000,
    "message": "Container already exists"
  }
}
```

## Patrones de Diseño

### 1. Agent Pattern
Cada agente es autónomo y especializado en un dominio específico.

### 2. Pool Pattern
MCPClientPool gestiona múltiples conexiones a servidores MCP.

### 3. Orchestrator Pattern
Orchestrator coordina múltiples agentes para tareas complejas.

### 4. Strategy Pattern
Cada agente implementa diferentes estrategias para ejecutar tareas.

### 5. Command Pattern
Tasks representan comandos que pueden ser ejecutados, almacenados y auditados.

## Manejo de Errores

### Niveles de Error

1. **Error de Validación**
   - Se captura en `validators.py`
   - Se retorna antes de llamar a MCP

2. **Error de MCP**
   - Se captura en `mcp_client.py`
   - Se retorna como MCPResponse.error

3. **Error de Agente**
   - Se captura en `agent_base.py`
   - Se actualiza Task.error

4. **Error de Sistema**
   - Se captura en `agent_base.py`
   - Se loguea y se propaga

### Estrategia de Reintentos

```python
# Configurado en mcp_config.json
{
  "settings": {
    "maxRetries": 3,
    "timeout": 30000
  }
}
```

## Seguridad

### Autenticación
- Usa Azure Identity SDK
- Soporta DefaultAzureCredential
- No almacena tokens directamente

### Validación
- Validación de inputs en todos los agentes
- Sanitización de datos
- Verificación de permisos

### Logging
- No se loguean secretos
- Auditoría de todas las operaciones
- Logs separados por agente

## Extensibilidad

### Agregar un Nuevo Agente

1. Crear clase que herede de `Agent`
2. Implementar `_load_capabilities()`
3. Implementar `execute_task()`
4. Registrar en `AgentSystem`

```python
class MyCustomAgent(Agent):
    def __init__(self, client_pool):
        super().__init__(
            name="my_custom_agent",
            description="Mi agente personalizado",
            mcp_server_name="azure-mcp",
            client_pool=client_pool
        )

    async def _load_capabilities(self):
        self.capabilities = [...]

    async def execute_task(self, task):
        # Implementación
        pass
```

### Agregar un Nuevo Workflow

Agregar método al `OrchestratorAgent`:

```python
async def _workflow_my_workflow(self, params):
    # Implementación del workflow
    pass
```

## Performance

### Optimizaciones Implementadas

1. **Async/Await**
   - Operaciones asíncronas para I/O
   - Mejor utilización de recursos

2. **Connection Pooling**
   - Reutilización de conexiones MCP
   - Reducción de overhead

3. **Caching**
   - Cache de capacidades de agentes
   - Cache de resultados de herramientas (futuro)

## Métricas y Monitoreo

### Métricas Disponibles

- Número de tareas ejecutadas por agente
- Tiempo de ejecución de tareas
- Tasa de éxito/fallo
- Uso de herramientas MCP

### Logging

Ubicación: `logs/agent_system.log`

Formato:
```
2025-01-15 10:30:45 - agent_system - INFO - [AzureStorageAgent] Iniciando tarea: Crear contenedor
2025-01-15 10:30:46 - agent_system - INFO - [AzureStorageAgent] Tarea completada (1234.56ms)
```

## Testing

### Unit Tests
```bash
pytest tests/test_agents.py
```

### Integration Tests
```bash
pytest tests/test_scenarios.py
```

## Deployment

### Docker (Futuro)
```dockerfile
FROM python:3.10
WORKDIR /app
COPY . .
RUN pip install -r requirements.txt
CMD ["python", "main.py", "interactive"]
```

### Kubernetes (Futuro)
- Deployment para cada agente
- Service mesh para comunicación
- ConfigMaps para configuración

## Referencias

- [MCP Specification](https://spec.modelcontextprotocol.io/)
- [Azure MCP Server](https://learn.microsoft.com/azure/developer/azure-mcp-server/)
- [Microsoft Fabric](https://learn.microsoft.com/fabric/)
