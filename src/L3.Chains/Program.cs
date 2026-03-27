using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.VisualBasic.FileIO;
using SkCourse.Shared;

Console.WriteLine("=== L3 - Chains ===\n");

var kernel = KernelFactory.CreateKernel(out var settings);
Console.WriteLine($"Model: {settings.ModelId}\n");

// ---------------------------------------------------------------
// Load Data.csv (same as the original notebook)
// ---------------------------------------------------------------
string csvPath = CatalogLoader.ResolveDataPath("Data.csv");
var reviews = new List<(string Product, string Review)>();
using (var parser = new TextFieldParser(csvPath))
{
    parser.TextFieldType = FieldType.Delimited;
    parser.SetDelimiters(",");
    parser.HasFieldsEnclosedInQuotes = true;
    parser.ReadFields(); // skip header
    while (!parser.EndOfData)
    {
        var f = parser.ReadFields()!;
        if (f.Length >= 2) reviews.Add((f[0], f[1]));
    }
}

Console.WriteLine($"Loaded {reviews.Count} reviews from Data.csv");
Console.WriteLine($"Review[5]: {reviews[5].Product}\n");

// ---------------------------------------------------------------
// Part 1: LLM Chain — single prompt with one variable
// ---------------------------------------------------------------
Console.WriteLine("--- Part 1: LLM Chain ---");

var productnamer = kernel.CreateFunctionFromPrompt(
    "What is the best name to describe a company that makes {{$input}}?",
    new OpenAIPromptExecutionSettings { Temperature = 0.9 });

string product = "Queen Size Sheet Set";
var nameResult = await kernel.InvokeAsync(productnamer, new KernelArguments { ["input"] = product });
Console.WriteLine($"Product: {product}\nCompany name: {nameResult}\n");

// ---------------------------------------------------------------
// Part 2: Simple Sequential Chain — output of func1 → input of func2
// ---------------------------------------------------------------
Console.WriteLine("--- Part 2: Simple Sequential Chain ---");

var funcOne = kernel.CreateFunctionFromPrompt(
    "What is the best name to describe a company that makes {{$input}}?",
    new OpenAIPromptExecutionSettings { Temperature = 0.9 });

var funcTwo = kernel.CreateFunctionFromPrompt(
    "Write a 20 words description for the following company: {{$input}}",
    new OpenAIPromptExecutionSettings { Temperature = 0.9 });

// Chain: funcOne → funcTwo  (output of first becomes input of second)
var step1 = await kernel.InvokeAsync(funcOne, new KernelArguments { ["input"] = "Queen Size Sheet Set" });
var step2 = await kernel.InvokeAsync(funcTwo, new KernelArguments { ["input"] = step1.ToString() });
Console.WriteLine($"Sequential chain result:\n{step2}\n");

// ---------------------------------------------------------------
// Part 3: Complex chain with context variables
//   Review → translate → summarize → detect language → write followup
//   (mirrors the CopyContext native functions from the Python notebook)
// ---------------------------------------------------------------
Console.WriteLine("--- Part 3: Sequential Chain with Context ---");

string review = reviews[5].Review; // French espresso review
Console.WriteLine($"Original review:\n{review}\n");

// Step A: Translate the review to English
var translateFunc = kernel.CreateFunctionFromPrompt(
    "Translate the following review to english:\n{{$review}}",
    new OpenAIPromptExecutionSettings { Temperature = 0.9 });

var englishReview = await kernel.InvokeAsync(translateFunc, new KernelArguments { ["review"] = review });
Console.WriteLine($"English translation:\n{englishReview}\n");

// Step B: Summarize the English review
var summarizeFunc = kernel.CreateFunctionFromPrompt(
    "Can you summarize the following review in 1 sentence:\n{{$English_Review}}",
    new OpenAIPromptExecutionSettings { Temperature = 0.9 });

var summary = await kernel.InvokeAsync(summarizeFunc,
    new KernelArguments { ["English_Review"] = englishReview.ToString() });
Console.WriteLine($"Summary:\n{summary}\n");

// Step C: Detect the original language
var languageFunc = kernel.CreateFunctionFromPrompt(
    "What language is the following review:\n\n{{$review}}",
    new OpenAIPromptExecutionSettings { Temperature = 0.9 });

var language = await kernel.InvokeAsync(languageFunc, new KernelArguments { ["review"] = review });
Console.WriteLine($"Detected language: {language}\n");

// Step D: Write a follow-up in the original language, referencing the summary
var followupFunc = kernel.CreateFunctionFromPrompt(
    """
    Write a follow up response to the following summary in the specified language:
    Summary: {{$summary}}
    Language: {{$language}}
    """,
    new OpenAIPromptExecutionSettings { Temperature = 0.9 });

var followup = await kernel.InvokeAsync(followupFunc,
    new KernelArguments { ["summary"] = summary.ToString(), ["language"] = language.ToString() });
Console.WriteLine($"Follow-up message:\n{followup}\n");
