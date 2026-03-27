# DeepLearning.AI LangChain Course in C# with Semantic Kernel

This repository is a C# translation path of the DeepLearning.AI LangChain short course, implemented with Microsoft's Semantic Kernel and current NuGet packages.

## Goals

- Keep the original lesson progression (L1 to L6).
- Implement each lesson in C# console apps.
- Use Semantic Kernel with OpenAI or Azure OpenAI.
- Keep data assets from the original Python repo for RAG-oriented lessons.

## Lesson Mapping

| # | Project | Original Notebook | Status |
| --- | --- | --- | --- |
| 1 | src/L1.ModelPromptParser | L1-Model_prompt_parser.ipynb | Starter implemented |
| 2 | src/L2.Memory | L2-Memory.ipynb | Starter implemented |
| 3 | src/L3.Chains | L3-Chains.ipynb | Starter implemented |
| 4 | src/L4.CreateDb | L4-CreateDB.ipynb | Starter implemented |
| 4 | src/L4.QnA | L4-QnA.ipynb | Starter implemented |
| 5 | src/L5.Evaluation | L5-Evaluation.ipynb | Starter implemented |
| 6 | src/L6.Agents | L6-Agents.ipynb | Starter implemented |

## Structure

- src/SkCourse.Shared: Shared Semantic Kernel bootstrap and settings.
- src/L*: One console app per lesson.
- data/: Shared datasets copied from the original repo.
- docs/: Notes and migration docs.

## Prerequisites

- .NET SDK 8+
- OpenAI or Azure OpenAI credentials

## Configure Credentials

Set environment variables from .env.example:

- OPENAI_API_KEY and optionally OPENAI_MODEL
- or AZURE_OPENAI_ENDPOINT, AZURE_OPENAI_API_KEY, AZURE_OPENAI_DEPLOYMENT

PowerShell example:

```powershell
$env:OPENAI_API_KEY = "your-key"
$env:OPENAI_MODEL = "gpt-4o-mini"
```

## Use with LM Studio

You can run this repository against a local LM Studio server using OpenAI-compatible settings.

Recommended models:

- Chat model: `google/gemma-3-4b`
- Embedding model: `text-embedding-bge-m3-embeddings`

### 1) Start LM Studio local server

In LM Studio:

- Download and load the models above.
- Start the OpenAI-compatible local server (default endpoint is `http://127.0.0.1:1234/v1`).

### 2) Configure this repo

Set environment variables in PowerShell:

```powershell
$env:OPENAI_API_KEY = "lm-studio"
$env:OPENAI_BASE_URL = "http://127.0.0.1:1234/v1"
$env:OPENAI_MODEL = "google/gemma-3-4b"
$env:OPENAI_EMBEDDING_MODEL = "text-embedding-bge-m3-embeddings"
```

Notes:

- `OPENAI_API_KEY` can be any non-empty value for LM Studio.
- You can also place the same keys in `local.settings.json` at the solution root or lesson project folder.
- Environment variables take precedence over `local.settings.json`.

## Run a Lesson

```powershell
dotnet run --project src/L1.ModelPromptParser/L1.ModelPromptParser.csproj
```

Run any other lesson by changing the project path:

- src/L2.Memory/L2.Memory.csproj
- src/L3.Chains/L3.Chains.csproj
- src/L4.CreateDb/L4.CreateDb.csproj
- src/L4.QnA/L4.QnA.csproj
- src/L5.Evaluation/L5.Evaluation.csproj
- src/L6.Agents/L6.Agents.csproj

## Semantic Kernel Packages

Shared package references live in src/SkCourse.Shared/SkCourse.Shared.csproj.

- Microsoft.SemanticKernel
- Microsoft.SemanticKernel.Connectors.OpenAI
- Microsoft.Extensions.Configuration.*

## Next Translation Steps

1. Port each original Python notebook workflow into corresponding C# project logic.
2. Add vector store support for L4 (Chroma equivalent in C#, or alternative store such as Qdrant/Azure AI Search/Postgres pgvector).
3. Add integration tests under tests/ to validate behavior parity with original notebooks.
4. Add benchmark and evaluation harness for L5.

## Acknowledgements

Original course and notebook design by DeepLearning.AI LangChain short course.
