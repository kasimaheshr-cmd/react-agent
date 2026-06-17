# Setup guide

## Prerequisites

Install these three tools first:

| Tool | Download | Purpose |
|---|---|---|
| Rancher Desktop | https://rancherdesktop.io | Docker runtime (free, no license) |
| Ollama | https://ollama.com | Local LLM runtime |
| Visual Studio 2022+ | https://visualstudio.microsoft.com | .NET 8 SDK included |

---

## One-command setup

After installing the prerequisites, open PowerShell in the repo root and run:

```powershell
.\setup-env.ps1
```

This script:
- Starts Kafka, Redis, MongoDB, OpenSearch as Docker containers
- Pulls llama3.2, nomic-embed-text, and llava models via Ollama
- Restores all .NET NuGet packages

First run takes 10–15 minutes (model downloads). Subsequent runs are instant.

---

## Connection strings

```
Kafka:       localhost:9092
Redis:       localhost:6379    password: LPLRedis2024!
MongoDB:     localhost:27017   admin / LPLMongo2024!
OpenSearch:  localhost:9200
Ollama:      localhost:11434
```

---

## Running the projects

Open `ReactAgent.Core.sln` in Visual Studio. Each project is a standalone runnable — right-click → Set as Startup Project → F5.

| Project | What it runs | Week |
|---|---|---|
| `ReactAgent.Core` | ReAct agent + Semantic Kernel demo | 5, 14 |
| `ReactAgent.McpServer` | MCP tool server (run alongside Core) | 6 |
| `ReactAgent.Workflow` | LangGraph-style stateful workflow | 7 |
| `ReactAgent.MultiAgent` | Orchestrator + specialist agents | 8 |
| `ReactAgent.Multimodal` | LLaVA vision pipeline + PDF routing | 9 |

---

## Architecture

```
User query
    ↓
ReactAgent.Workflow        — classifies query, picks execution path
    ├── ReactAgent.Core    — ReAct loop (single agent, dynamic tools)
    ├── ReactAgent.MultiAgent  — orchestrator delegates to specialists
    └── ReactAgent.Multimodal  — handles PDF/image inputs via LLaVA
              ↓
    ReactAgent.McpServer   — FINRA tool provider (separate process)
              ↓
    Ollama (llama3.2)      — local LLM, no API key needed
    MongoDB                — trajectory logging + audit trail
```

---

## Local stack vs production mapping

| Local | Production equivalent |
|---|---|
| Kafka (Redpanda) | AWS MSK |
| Redis | AWS ElastiCache |
| MongoDB | MongoDB Atlas / DynamoDB |
| OpenSearch | AWS OpenSearch |
| Ollama llama3.2 | AWS Bedrock Claude |
| OTel + Grafana | AWS CloudWatch + X-Ray |

---

## Troubleshooting

**Ollama model not found**
```powershell
ollama serve        # start Ollama if not running
ollama pull llama3.2
```

**Docker containers not starting**
Open Rancher Desktop, wait for status to show "Running", then re-run the script.

**NuGet restore fails**
```powershell
dotnet restore --force
```

**Semantic Kernel NuGet conflict**
```powershell
Install-Package Azure.AI.OpenAI -Version 2.9.0-beta.1
Install-Package Microsoft.SemanticKernel -Version 1.77.0
```
