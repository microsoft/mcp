#!/bin/bash

# Script de configuración para el Sistema de Agentes Multi-MCP
# Este script automatiza la instalación y configuración del sistema

set -e  # Salir si hay error

echo "=================================================="
echo "Sistema de Agentes Multi-MCP de Microsoft"
echo "Script de Configuración"
echo "=================================================="
echo ""

# Colores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Función para imprimir mensajes
print_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# 1. Verificar Python
print_info "Verificando Python..."
if command -v python3 &> /dev/null; then
    PYTHON_VERSION=$(python3 --version | cut -d' ' -f2)
    print_info "Python encontrado: $PYTHON_VERSION"
else
    print_error "Python 3 no está instalado"
    exit 1
fi

# 2. Verificar .NET o Node.js
print_info "Verificando .NET SDK..."
if command -v dotnet &> /dev/null; then
    DOTNET_VERSION=$(dotnet --version)
    print_info ".NET SDK encontrado: $DOTNET_VERSION"
    USE_DOTNET=true
elif command -v node &> /dev/null; then
    NODE_VERSION=$(node --version)
    print_info "Node.js encontrado: $NODE_VERSION (se usará NPM)"
    USE_DOTNET=false
else
    print_error "Necesitas .NET 10+ o Node.js 20+ instalado"
    exit 1
fi

# 3. Crear estructura de directorios
print_info "Creando directorios necesarios..."
mkdir -p logs
mkdir -p config
mkdir -p data
print_info "Directorios creados"

# 4. Instalar dependencias Python
print_info "Instalando dependencias Python..."
if [ -f "requirements.txt" ]; then
    python3 -m pip install -r requirements.txt --quiet
    print_info "Dependencias Python instaladas"
else
    print_warning "requirements.txt no encontrado"
fi

# 5. Compilar servidores MCP
print_info "Compilando servidores MCP..."

if [ "$USE_DOTNET" = true ]; then
    # Compilar Azure MCP Server
    print_info "Compilando Azure MCP Server..."
    cd ../servers/Azure.Mcp.Server
    dotnet build --configuration Release > /dev/null 2>&1
    if [ $? -eq 0 ]; then
        print_info "Azure MCP Server compilado correctamente"
    else
        print_warning "Error al compilar Azure MCP Server"
    fi

    # Compilar Fabric MCP Server
    print_info "Compilando Fabric MCP Server..."
    cd ../Fabric.Mcp.Server
    dotnet build --configuration Release > /dev/null 2>&1
    if [ $? -eq 0 ]; then
        print_info "Fabric MCP Server compilado correctamente"
    else
        print_warning "Error al compilar Fabric MCP Server"
    fi

    cd ../../demo
else
    print_info "Usando Azure MCP Server desde NPM..."
    npm install -g @azure/mcp@latest --silent
fi

# 6. Verificar Azure CLI
print_info "Verificando Azure CLI..."
if command -v az &> /dev/null; then
    print_info "Azure CLI encontrado"

    # Verificar autenticación
    if az account show &> /dev/null; then
        ACCOUNT=$(az account show --query name -o tsv)
        print_info "Autenticado con Azure: $ACCOUNT"
    else
        print_warning "No estás autenticado con Azure"
        print_info "Ejecuta: az login"
    fi
else
    print_warning "Azure CLI no está instalado"
    print_info "Descárgalo desde: https://docs.microsoft.com/cli/azure/install-azure-cli"
fi

# 7. Crear archivo .env de ejemplo
print_info "Creando archivo .env de ejemplo..."
cat > .env.example << EOF
# Configuración de Azure
AZURE_SUBSCRIPTION_ID=your-subscription-id
AZURE_TENANT_ID=your-tenant-id

# Configuración de Storage
AZURE_STORAGE_ACCOUNT=your-storage-account

# Configuración de Key Vault
AZURE_KEYVAULT_NAME=your-keyvault-name

# Configuración de Logging
LOG_LEVEL=INFO

# Configuración de MCP
MCP_TIMEOUT=30000
MCP_MAX_RETRIES=3
EOF
print_info "Archivo .env.example creado"

# 8. Verificar instalación
print_info "Verificando instalación..."
python3 main.py info > /dev/null 2>&1
if [ $? -eq 0 ]; then
    print_info "Sistema instalado correctamente"
else
    print_warning "Hubo algunos errores durante la instalación"
fi

echo ""
echo "=================================================="
echo -e "${GREEN}Instalación Completada${NC}"
echo "=================================================="
echo ""
echo "Próximos pasos:"
echo "  1. Autenticar con Azure: az login"
echo "  2. Ver información: python main.py info"
echo "  3. Ejecutar ejemplos: python main.py examples"
echo "  4. Ejecutar demo: python main.py demo --scenario data_pipeline"
echo ""
echo "Para más información, lee QUICKSTART.md"
echo ""
