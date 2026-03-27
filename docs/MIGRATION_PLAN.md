# Migration Plan: LangChain Python to Semantic Kernel C# #

## Principles ##

- Keep lesson intent unchanged.
- Prefer explicit, testable C# code over notebook-only flow.
- Make provider configuration external via environment variables.

## Technical Translation Guide ##

- PromptTemplate -> Kernel prompt invocation and prompt files.
- Chain patterns -> function composition and pipeline-style orchestration.
- Memory -> conversation state wrappers and persisted stores.
- Retrieval -> embedding + vector DB connector integration.
- Agents -> plugin-based tool use and planning patterns.

## Suggested Execution Order ##

1. L1 and L2 for baseline prompt and memory behavior.
2. L3 to establish composition patterns.
3. L4 for data ingestion and RAG.
4. L5 for automated evaluation scaffolding.
5. L6 for agentic orchestration and tools.
