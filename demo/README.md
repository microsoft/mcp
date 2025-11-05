# Sistema de Agentes Multi-MCP de Microsoft

## Descripción General

Este proyecto demuestra un sistema de agentes inteligentes que utiliza los servidores MCP (Model Context Protocol) de Microsoft para automatizar tareas complejas en Azure y Microsoft Fabric.

## Arquitectura del Sistema

El sistema está compuesto por varios agentes especializados que trabajan juntos:

### 1. **Agente Orquestador** (`OrchestratorAgent`)
- Coordina las interacciones entre todos los agentes
- Distribuye tareas a los agentes especializados
- Agrega y presenta resultados al usuario

### 2. **Agente de Azure Storage** (`AzureStorageAgent`)
- Gestiona operaciones con Azure Storage (blobs, contenedores)
- Sube y descarga archivos
- Lista y organiza contenedores

### 3. **Agente de Azure AI** (`AzureAIAgent`)
- Interactúa con Azure AI Search
- Gestiona índices de búsqueda
- Realiza búsquedas semánticas

### 4. **Agente de Azure Data** (`AzureDataAgent`)
- Gestiona Azure Cosmos DB
- Realiza consultas SQL en Azure SQL Database
- Administra datos estructurados y no estructurados

### 5. **Agente de Azure Security** (`AzureSecurityAgent`)
- Gestiona secretos en Azure Key Vault
- Administra claves y certificados
- Implementa mejores prácticas de seguridad

### 6. **Agente de Azure Infrastructure** (`AzureInfraAgent`)
- Gestiona recursos de Azure (grupos de recursos, suscripciones)
- Despliega aplicaciones en Azure App Service
- Administra Azure Functions

### 7. **Agente de Fabric** (`FabricAgent`)
- Accede a APIs de Microsoft Fabric
- Genera definiciones de recursos (Lakehouse, notebooks, pipelines)
- Proporciona mejores prácticas de Fabric

## Escenarios de Demostración

### Escenario 1: Pipeline de Datos Completo
**Objetivo:** Crear un pipeline end-to-end desde la ingesta hasta el análisis

**Flujo:**
1. **AzureStorageAgent**: Sube datos CSV a Azure Blob Storage
2. **AzureDataAgent**: Carga datos en Azure Cosmos DB
3. **AzureAIAgent**: Indexa datos en Azure AI Search
4. **FabricAgent**: Genera definición de Lakehouse para análisis
5. **OrchestratorAgent**: Coordina todo el proceso

### Escenario 2: Aplicación Segura
**Objetivo:** Desplegar una aplicación web con secretos seguros

**Flujo:**
1. **AzureSecurityAgent**: Crea secretos en Key Vault (API keys, connection strings)
2. **AzureInfraAgent**: Despliega aplicación en App Service
3. **AzureStorageAgent**: Configura almacenamiento para archivos estáticos
4. **OrchestratorAgent**: Verifica que todo esté correctamente configurado

### Escenario 3: Análisis de Búsqueda Inteligente
**Objetivo:** Crear un sistema de búsqueda semántica

**Flujo:**
1. **AzureStorageAgent**: Carga documentos a procesar
2. **AzureAIAgent**: Crea índice de búsqueda con vectores
3. **AzureDataAgent**: Almacena metadatos en Cosmos DB
4. **OrchestratorAgent**: Ejecuta consultas de búsqueda y presenta resultados

### Escenario 4: Infraestructura como Código con Fabric
**Objetivo:** Generar y desplegar infraestructura de datos

**Flujo:**
1. **FabricAgent**: Genera definiciones de recursos de Fabric
2. **AzureInfraAgent**: Crea grupos de recursos necesarios
3. **AzureDataAgent**: Configura conexiones de datos
4. **OrchestratorAgent**: Valida y despliega toda la infraestructura

## Características Técnicas

### Comunicación entre Agentes
- **Protocolo:** Model Context Protocol (MCP)
- **Formato:** JSON
- **Transporte:** stdio (entrada/salida estándar)

### Capacidades de los Agentes
- Procesamiento de lenguaje natural para interpretar instrucciones
- Ejecución autónoma de tareas con manejo de errores
- Validación y logging de todas las operaciones
- Rollback automático en caso de fallos

### Seguridad
- Autenticación con Azure Identity
- Uso de managed identities cuando sea posible
- Secretos almacenados en Key Vault
- Logging de auditoría de todas las operaciones

## Estructura del Proyecto

```
demo/
├── README.md                          # Este archivo
├── config/
│   ├── mcp_config.json               # Configuración de servidores MCP
│   └── agents_config.json            # Configuración de agentes
├── src/
│   ├── core/
│   │   ├── agent_base.py             # Clase base para todos los agentes
│   │   ├── mcp_client.py             # Cliente MCP
│   │   └── message_bus.py            # Sistema de mensajería entre agentes
│   ├── agents/
│   │   ├── orchestrator.py           # Agente orquestador
│   │   ├── azure_storage.py          # Agente de Storage
│   │   ├── azure_ai.py               # Agente de AI
│   │   ├── azure_data.py             # Agente de Data
│   │   ├── azure_security.py         # Agente de Security
│   │   ├── azure_infra.py            # Agente de Infrastructure
│   │   └── fabric.py                 # Agente de Fabric
│   ├── scenarios/
│   │   ├── data_pipeline.py          # Escenario 1
│   │   ├── secure_app.py             # Escenario 2
│   │   ├── intelligent_search.py     # Escenario 3
│   │   └── fabric_infrastructure.py  # Escenario 4
│   └── utils/
│       ├── logger.py                 # Sistema de logging
│       └── validators.py             # Validadores
├── tests/
│   ├── test_agents.py                # Tests de agentes
│   └── test_scenarios.py             # Tests de escenarios
├── examples/
│   ├── basic_usage.py                # Ejemplos básicos
│   └── advanced_usage.py             # Ejemplos avanzados
└── requirements.txt                   # Dependencias Python
```

## Requisitos Previos

1. **Azure:**
   - Suscripción de Azure activa
   - Azure CLI instalado
   - Credenciales configuradas (`az login`)

2. **Microsoft Fabric:**
   - Acceso a un workspace de Fabric (opcional para demos)

3. **Software:**
   - Python 3.10+
   - .NET 10 SDK (para Azure MCP Server)
   - Node.js 20+ (alternativa para Azure MCP Server)

## Instalación

```bash
# 1. Clonar el repositorio (ya hecho)
cd /home/user/Microsoft_MCPs/demo

# 2. Instalar dependencias Python
pip install -r requirements.txt

# 3. Compilar servidores MCP
cd ../servers/Azure.Mcp.Server
dotnet build --configuration Release

cd ../Fabric.Mcp.Server
dotnet build --configuration Release

# 4. Configurar credenciales de Azure
az login
```

## Uso Rápido

### Ejecutar un escenario completo:

```bash
# Escenario 1: Pipeline de datos
python src/scenarios/data_pipeline.py

# Escenario 2: Aplicación segura
python src/scenarios/secure_app.py

# Escenario 3: Búsqueda inteligente
python src/scenarios/intelligent_search.py

# Escenario 4: Infraestructura Fabric
python src/scenarios/fabric_infrastructure.py
```

### Interacción directa con agentes:

```python
from src.core.agent_base import AgentSystem
from src.agents.orchestrator import OrchestratorAgent

# Inicializar sistema
system = AgentSystem()
orchestrator = OrchestratorAgent(system)

# Ejecutar tarea
result = orchestrator.execute(
    "Crea un contenedor de almacenamiento llamado 'demo-data' "
    "y sube el archivo 'datos.csv'"
)

print(result)
```

## Beneficios del Sistema

1. **Modularidad:** Agentes especializados que se pueden combinar
2. **Escalabilidad:** Fácil agregar nuevos agentes y capacidades
3. **Reutilización:** Componentes que se pueden usar en diferentes escenarios
4. **Observabilidad:** Logging completo de todas las operaciones
5. **Resiliencia:** Manejo de errores y reintentos automáticos

## Ejemplos de Comandos

### Usar múltiples servicios Azure:
```python
orchestrator.execute("""
    1. Crea un secreto en Key Vault llamado 'api-key' con valor 'secret123'
    2. Crea un contenedor de storage llamado 'app-data'
    3. Despliega una función en Azure Functions que use ese secreto
""")
```

### Generar recursos de Fabric:
```python
fabric_agent.execute("""
    Genera una definición de Lakehouse con las siguientes tablas:
    - customers: id (int), name (string), email (string)
    - orders: id (int), customer_id (int), amount (decimal), date (datetime)
""")
```

### Pipeline completo:
```python
orchestrator.execute("""
    Crea un pipeline completo para análisis de ventas:
    1. Storage: contenedor 'raw-sales-data'
    2. Cosmos DB: base de datos 'sales-analytics'
    3. AI Search: índice 'sales-search' con embeddings
    4. Fabric: Lakehouse 'sales-warehouse'
""")
```

## Próximos Pasos

1. Ejecutar los escenarios de ejemplo
2. Explorar las capacidades de cada agente
3. Crear tus propios escenarios personalizados
4. Integrar con tus aplicaciones existentes

## Soporte y Contribuciones

- **Issues:** [GitHub Issues](https://github.com/microsoft/mcp/issues)
- **Documentación:** [Learn Microsoft](https://learn.microsoft.com/azure/developer/azure-mcp-server/)
- **Comunidad:** [MCP Community](https://modelcontextprotocol.io)

---

**¡Disfruta explorando las capacidades de los MCPs de Microsoft!** 🚀
