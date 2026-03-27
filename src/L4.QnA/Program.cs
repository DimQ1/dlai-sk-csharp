using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Memory;
using SkCourse.Shared;

Console.WriteLine("=== L4 - Q&A over Documents (RAG) ===\n");

var kernel = KernelFactory.CreateKernel(out var settings);
Console.WriteLine($"Model: {settings.ModelId}");
Console.WriteLine($"Embedding model: {settings.EmbeddingModelId}\n");

// ---------------------------------------------------------------
// Populate the vector store (in the Python course this re-uses
// the persisted Chroma DB from L4-CreateDB; here we re-populate
// in-memory since VolatileMemoryStore doesn't persist)
// ---------------------------------------------------------------
ISemanticTextMemory memory = KernelFactory.CreateMemory(settings);
string csvPath = CatalogLoader.ResolveDataPath("OutdoorClothingCatalog_1000.csv");
var records = CatalogLoader.ReadCsv(csvPath);

Console.WriteLine($"Populating vector store with {records.Count} products...");
await CatalogLoader.PopulateMemoryAsync(memory, records,
    progress: (i, total) => { if (i % 100 == 0 || i == total) Console.WriteLine($"  {i}/{total}"); });
Console.WriteLine("Done.\n");

// ---------------------------------------------------------------
// RAG pattern: Retrieval → Augmentation → Generation
// (mirrors ragqna() from the Python notebook)
// ---------------------------------------------------------------
async Task<string> RagQnAAsync(string query, int limit = 3)
{
    // Step 1 — Retrieval: search vector store for relevant documents
    var docs = new List<string>();
    await foreach (var result in memory.SearchAsync(CatalogLoader.Collection, query, limit: limit, minRelevanceScore: 0.3))
    {
        docs.Add(result.Metadata.Text);
    }

    // Step 2 — Augmentation: build prompt with retrieved context
    string qdocs = string.Join("\n```\n", docs);

    string prompt = """
        {{$qdocs}}

        Question: Please query above documents delimited by triple backticks for {{$query}}
        and return results in a table in markdown and summarize each one.
        """;

    // Step 3 — Generation: invoke the LLM
    var response = await kernel.InvokePromptAsync(prompt,
        new KernelArguments { ["qdocs"] = qdocs, ["query"] = query });
    return response.ToString()!;
}

// Run a RAG query
string queryText = "shirts with sunblocking";
Console.WriteLine($"RAG Query: \"{queryText}\"\n");

string answer = await RagQnAAsync(queryText, limit: 3);
Console.WriteLine("=== RAG Answer (Markdown) ===");
Console.WriteLine(answer);
