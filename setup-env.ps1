# react-agent environment setup
# Run this once on any new machine after installing:
#   1. Rancher Desktop  https://rancherdesktop.io
#   2. Ollama           https://ollama.com
#   3. Visual Studio 2022/2026 with .NET 8 SDK

Write-Host "=== react-agent environment setup ===" -ForegroundColor Cyan

# --- Check prerequisites ---
Write-Host "`n[1/4] Checking prerequisites..." -ForegroundColor Yellow

if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    Write-Host "ERROR: Docker not found. Install Rancher Desktop first." -ForegroundColor Red
    exit 1
}

if (-not (Get-Command ollama -ErrorAction SilentlyContinue)) {
    Write-Host "ERROR: Ollama not found. Install from https://ollama.com" -ForegroundColor Red
    exit 1
}

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Host "ERROR: dotnet not found. Install .NET 8 SDK first." -ForegroundColor Red
    exit 1
}

Write-Host "Prerequisites OK" -ForegroundColor Green

# --- Start containers ---
Write-Host "`n[2/4] Starting containers..." -ForegroundColor Yellow

# Kafka / Redpanda
$kafka = docker ps -q -f name=kafka
if (-not $kafka) {
    Write-Host "  Starting Kafka (Redpanda)..."
    docker run -d --name kafka -p 9092:9092 `
        redpandadata/redpanda:latest `
        redpanda start --overprovisioned --smp 1 --memory 512M `
        --reserve-memory 0M --node-id 0 --check=false
} else {
    Write-Host "  Kafka already running"
}

# Redis
$redis = docker ps -q -f name=redis
if (-not $redis) {
    Write-Host "  Starting Redis..."
    docker run -d --name redis -p 6379:6379 `
        redis redis-server --requirepass LPLRedis2024!
} else {
    Write-Host "  Redis already running"
}

# MongoDB
$mongo = docker ps -q -f name=mongo
if (-not $mongo) {
    Write-Host "  Starting MongoDB..."
    docker run -d --name mongo -p 27017:27017 `
        -e MONGO_INITDB_ROOT_USERNAME=admin `
        -e MONGO_INITDB_ROOT_PASSWORD=LPLMongo2024! `
        mongo
} else {
    Write-Host "  MongoDB already running"
}

# OpenSearch
$opensearch = docker ps -q -f name=opensearch
if (-not $opensearch) {
    Write-Host "  Starting OpenSearch..."
    docker run -d --name opensearch -p 9200:9200 `
        -e discovery.type=single-node `
        -e OPENSEARCH_INITIAL_ADMIN_PASSWORD=MySearch@7890# `
        -e "DISABLE_SECURITY_PLUGIN=true" `
        opensearchproject/opensearch:latest
} else {
    Write-Host "  OpenSearch already running"
}

Write-Host "Containers started" -ForegroundColor Green

# --- Pull Ollama models ---
Write-Host "`n[3/4] Pulling Ollama models (this takes a few minutes first time)..." -ForegroundColor Yellow

Write-Host "  Pulling llama3.2..."
ollama pull llama3.2

Write-Host "  Pulling nomic-embed-text..."
ollama pull nomic-embed-text

Write-Host "  Pulling llava (4.7GB - vision model)..."
ollama pull llava

Write-Host "Ollama models ready" -ForegroundColor Green

# --- Restore .NET packages ---
Write-Host "`n[4/4] Restoring .NET packages..." -ForegroundColor Yellow
dotnet restore ReactAgent.Core\ReactAgent.Core.csproj
Write-Host ".NET packages restored" -ForegroundColor Green

# --- Done ---
Write-Host "`n=== Setup complete ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Connection strings:" -ForegroundColor White
Write-Host "  Kafka:      localhost:9092"
Write-Host "  Redis:      localhost:6379  password: LPLRedis2024!"
Write-Host "  MongoDB:    localhost:27017  admin/LPLMongo2024!"
Write-Host "  OpenSearch: localhost:9200"
Write-Host "  Ollama:     localhost:11434"
Write-Host ""
Write-Host "To run: open ReactAgent.Core.sln in Visual Studio" -ForegroundColor Cyan
Write-Host "        set ReactAgent.Core as startup project -> F5"
