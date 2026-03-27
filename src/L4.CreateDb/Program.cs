using Microsoft.SemanticKernel.Memory;
using SkCourse.Shared;

Console.WriteLine("=== L4 - Create DB (Vector Store) ===\n");

var settings = CourseSettings.FromEnvironment();
Console.WriteLine($"Embedding model: {settings.EmbeddingModelId}\n");

// ---------------------------------------------------------------
// Load CSV
// ---------------------------------------------------------------
string csvPath = CatalogLoader.ResolveDataPath("OutdoorClothingCatalog_1000.csv");
var records = CatalogLoader.ReadCsv(csvPath);
Console.WriteLine($"Loaded {records.Count} products from CSV.\n");

// ---------------------------------------------------------------
// Create memory store with embeddings and populate
// ---------------------------------------------------------------
ISemanticTextMemory memory = KernelFactory.CreateMemory(settings);

Console.WriteLine("Populating vector store (this calls the embedding API for each row)...");
await CatalogLoader.PopulateMemoryAsync(memory, records,
    progress: (i, total) =>
    {
        if (i % 50 == 0 || i == total)
            Console.WriteLine($"  {i}/{total}");
    });

Console.WriteLine("Done.\n");

// ---------------------------------------------------------------
// Test a query
// ---------------------------------------------------------------
string query = "Please suggest a shirt with sunblocking";
Console.WriteLine($"Query: {query}");

var results = memory.SearchAsync(CatalogLoader.Collection, query, limit: 1, minRelevanceScore: 0.3);
await foreach (var result in results)
{
    Console.WriteLine($"\nMatch (relevance {result.Relevance:F3}):");
    Console.WriteLine(result.Metadata.Text[..Math.Min(200, result.Metadata.Text.Length)] + "...");
}

Console.WriteLine("\nVector store populated. Ready for L4-QnA and L5-Evaluation.");
