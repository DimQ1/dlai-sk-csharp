using System.Text.Json;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Memory;
using SkCourse.Shared;

Console.WriteLine("=== L5 - Evaluation ===\n");

var kernel = KernelFactory.CreateKernel(out var settings);
Console.WriteLine($"Model: {settings.ModelId}\n");

// ---------------------------------------------------------------
// Populate the vector store (same as L4-CreateDB)
// ---------------------------------------------------------------
ISemanticTextMemory memory = KernelFactory.CreateMemory(settings);
string csvPath = CatalogLoader.ResolveDataPath("OutdoorClothingCatalog_1000.csv");
var records = CatalogLoader.ReadCsv(csvPath);

Console.WriteLine($"Populating vector store with {records.Count} products...");
await CatalogLoader.PopulateMemoryAsync(memory, records,
    progress: (i, total) => { if (i % 100 == 0 || i == total) Console.WriteLine($"  {i}/{total}"); });
Console.WriteLine("Done.\n");

// ---------------------------------------------------------------
// Hard-coded evaluation examples (from the notebook)
// ---------------------------------------------------------------
Console.WriteLine("--- Hard-coded Examples ---");
Console.WriteLine($"Record 10: {records[10].Name}");
Console.WriteLine($"Record 11: {records[11].Name}\n");

var examples = new List<Dictionary<string, string>>
{
    new() { ["query"] = "Do the Cozy Comfort Pullover Set have side pockets?", ["answer"] = "Yes" },
    new() { ["query"] = "What collection is the Ultra-Lofty 850 Stretch Down Hooded Jacket from?", ["answer"] = "The DownTek collection" }
};

// ---------------------------------------------------------------
// LLM-generated examples from first 3 DB records
// ---------------------------------------------------------------
Console.WriteLine("--- LLM-Generated Examples ---");

// Fetch records 0-2 text
var sampleTexts = new List<string>();
for (int i = 0; i < 3 && i < records.Count; i++)
    sampleTexts.Add($"{records[i].Name} : {records[i].Description}");
string qdocs = string.Join("\n```\n", sampleTexts);

string genPrompt = """
    {{$qdocs}}

    Question: Please generate one question and answer for each of above records
    delimited by triple backticks and return results in a well formed JSON list
    with fields named as query and answer.
    """;

var genResult = await kernel.InvokePromptAsync(genPrompt,
    new KernelArguments { ["qdocs"] = qdocs });

string genJson = genResult.ToString()!.Replace("```json", "").Replace("```", "").Trim();
Console.WriteLine($"Generated:\n{genJson}\n");

var generated = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(genJson,
    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
if (generated != null) examples.AddRange(generated);

Console.WriteLine($"Total evaluation examples: {examples.Count}\n");

// ---------------------------------------------------------------
// RAG QnA (same as L4-QnA)
// ---------------------------------------------------------------
async Task<string> RagQnAAsync(string query, int limit = 3)
{
    var docs = new List<string>();
    await foreach (var r in memory.SearchAsync(CatalogLoader.Collection, query, limit: limit, minRelevanceScore: 0.3))
        docs.Add(r.Metadata.Text);

    string docsBlock = string.Join("\n```\n", docs);
    string prompt = """
        {{$qdocs}}

        Use the above documents delimited by triple backticks and
        answer the following question: {{$query}}
        """;

    var response = await kernel.InvokePromptAsync(prompt,
        new KernelArguments { ["qdocs"] = docsBlock, ["query"] = query });
    return response.ToString()!;
}

// ---------------------------------------------------------------
// Manual Evaluation — run each question through RAG, store predicted answer
// ---------------------------------------------------------------
Console.WriteLine("--- Manual Evaluation ---\n");

foreach (var example in examples)
{
    string predicted = await RagQnAAsync(example["query"], 3);
    example["predicted"] = predicted;
}

// Print results table
Console.WriteLine($"{"#",-3} {"Query",-55} {"Expected",-30} {"Predicted",-60}");
Console.WriteLine(new string('-', 148));
for (int i = 0; i < examples.Count; i++)
{
    string q = Truncate(examples[i]["query"], 52);
    string a = Truncate(examples[i]["answer"], 27);
    string p = Truncate(examples[i]["predicted"], 57);
    Console.WriteLine($"{i,-3} {q,-55} {a,-30} {p,-60}");
}

// ---------------------------------------------------------------
// LLM-Assisted Evaluation (exercise hint from notebook)
// ---------------------------------------------------------------
Console.WriteLine("\n--- LLM-Assisted Evaluation ---\n");

string evalTemplate = """
    Given the question, expected answer, and predicted answer below,
    grade the predicted answer on a scale of CORRECT, PARTIALLY CORRECT, or INCORRECT.
    Return a single word grade followed by a brief rationale.

    Question: {{$query}}
    Expected: {{$expected}}
    Predicted: {{$predicted}}
    """;

foreach (var example in examples)
{
    var grade = await kernel.InvokePromptAsync(evalTemplate, new KernelArguments
    {
        ["query"] = example["query"],
        ["expected"] = example["answer"],
        ["predicted"] = example["predicted"]
    });
    Console.WriteLine($"Q: {Truncate(example["query"], 60)}");
    Console.WriteLine($"Grade: {grade}\n");
}

static string Truncate(string s, int max) =>
    s.Length <= max ? s : s[..(max - 3)] + "...";
