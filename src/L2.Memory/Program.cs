using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Memory;
using SkCourse.Shared;

Console.WriteLine("=== L2 - Memory ===\n");

var kernel = KernelFactory.CreateKernel(out var settings);
Console.WriteLine($"Model: {settings.ModelId}\n");

// ---------------------------------------------------------------
// Part 1: Chat without memory — LLM doesn't remember prior turns
// ---------------------------------------------------------------
Console.WriteLine("--- Part 1: Chat Without Memory ---");

var r1 = await kernel.InvokePromptAsync("{{$input}}", new KernelArguments { ["input"] = "Hi, my name is Andrew!" });
Console.WriteLine($"User: Hi, my name is Andrew!\nBot:  {r1}\n");

var r2 = await kernel.InvokePromptAsync("{{$input}}", new KernelArguments { ["input"] = "What is 1+1?" });
Console.WriteLine($"User: What is 1+1?\nBot:  {r2}\n");

var r3 = await kernel.InvokePromptAsync("{{$input}}", new KernelArguments { ["input"] = "What is my name?" });
Console.WriteLine($"User: What is my name?\nBot:  {r3}");
Console.WriteLine("(Without memory the LLM does not know previous info shared)\n");

// ---------------------------------------------------------------
// Part 2: Set up volatile memory store with embeddings
// ---------------------------------------------------------------
Console.WriteLine("--- Part 2: Chat With Memory ---\n");

ISemanticTextMemory memory = KernelFactory.CreateMemory(settings);

// Populate memory with facts about "me"
Console.WriteLine("Saving facts to memory...");
await memory.SaveInformationAsync("aboutMe", id: "info1", text: "My name is Andrew");
await memory.SaveInformationAsync("aboutMe", id: "info2", text: "I currently work as a tour guide");
await memory.SaveInformationAsync("aboutMe", id: "info3", text: "I've been living in Seattle since 2005");
await memory.SaveInformationAsync("aboutMe", id: "info4", text: "I visited France and Italy five times since 2015");
await memory.SaveInformationAsync("aboutMe", id: "info5", text: "My family is from New York");
Console.WriteLine("Done.\n");

// ---------------------------------------------------------------
// Part 3: Chat with memory — recall relevant facts per question
// ---------------------------------------------------------------
string[] factQueries =
[
    "what is my name?",
    "where do I live?",
    "where's my family from?",
    "where have I traveled?",
    "what do I do for work?"
];

async Task<string> ChatWithMemoryAsync(string userInput)
{
    // Retrieve relevant facts from memory (like the {{recall}} skill in Python SK)
    var facts = new List<string>();
    foreach (var query in factQueries)
    {
        await foreach (var result in memory.SearchAsync("aboutMe", query, limit: 1, minRelevanceScore: 0.7))
        {
            facts.Add(result.Metadata.Text);
        }
    }

    string factBlock = facts.Count > 0
        ? string.Join("\n", facts.Select(f => $"    - {f}"))
        : "    No relevant information found.";

    string prompt = $"""
        ChatBot can have a conversation with you about any topic.
        It can give explicit instructions or say 'I don't know' if
        it does not have an answer.

        Information about me, from previous conversations:
        {factBlock}

        User: {userInput}
        ChatBot:
        """;

    var response = await kernel.InvokePromptAsync(prompt);
    return response.ToString()!;
}

var a1 = await ChatWithMemoryAsync("What is 1+1?");
Console.WriteLine($"User: What is 1+1?\nBot:  {a1}\n");

var a2 = await ChatWithMemoryAsync("Can you tell me my name?");
Console.WriteLine($"User: Can you tell me my name?\nBot:  {a2}\n");

var a3 = await ChatWithMemoryAsync("Can you tell me where I hail from?");
Console.WriteLine($"User: Can you tell me where I hail from?\nBot:  {a3}\n");

var a4 = await ChatWithMemoryAsync("Can you tell me where I live currently?");
Console.WriteLine($"User: Can you tell me where I live currently?\nBot:  {a4}\n");
