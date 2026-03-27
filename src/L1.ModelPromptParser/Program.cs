using System.Text.Json;
using Microsoft.SemanticKernel;
using SkCourse.Shared;

Console.WriteLine("=== L1 - Models, Prompts and Output Parsers ===\n");

var kernel = KernelFactory.CreateKernel(out var settings);
Console.WriteLine($"Model: {settings.ModelId}\n");

// ---------------------------------------------------------------
// Part 1: Direct prompt (equivalent to direct OpenAI API call)
// ---------------------------------------------------------------
Console.WriteLine("--- Part 1: Direct Prompt ---");
var directResult = await kernel.InvokePromptAsync("What is 1+1?");
Console.WriteLine($"Q: What is 1+1?\nA: {directResult}\n");

// ---------------------------------------------------------------
// Part 2: Prompt Template with variables ($style, $text)
// ---------------------------------------------------------------
Console.WriteLine("--- Part 2: Prompt Template ---");

string templateString = """
    Translate the following text into a style that is {{$style}}.
    text: {{$text}}
    """;

string customerEmail = """
    Arrr, I be fuming that me blender lid
    flew off and splattered me kitchen walls
    with smoothie! And to make matters worse,
    the warranty don't cover the cost of
    cleaning up me kitchen. I need yer help
    right now, matey!
    """;

string customerStyle = "American English in a calm and respectful tone";

var translateResult = await kernel.InvokePromptAsync(templateString,
    new KernelArguments { ["style"] = customerStyle, ["text"] = customerEmail });
Console.WriteLine($"Customer email (translated):\n{translateResult}\n");

// ---------------------------------------------------------------
// Part 3: Reuse the same template with a different style
// ---------------------------------------------------------------
Console.WriteLine("--- Part 3: Reuse Template (Pirate Style) ---");

string serviceReply = """
    Hey there customer, the warranty does not cover
    cleaning expenses for your kitchen because it's your fault that
    you misused your blender by forgetting to put the lid on before
    starting the blender. Tough luck! See ya!
    """;

string pirateStyle = "a polite tone that speaks in English Pirate";

var pirateResult = await kernel.InvokePromptAsync(templateString,
    new KernelArguments { ["style"] = pirateStyle, ["text"] = serviceReply });
Console.WriteLine($"Service reply (pirate):\n{pirateResult}\n");

// ---------------------------------------------------------------
// Part 4: Output Parser — extract structured JSON from a review
// ---------------------------------------------------------------
Console.WriteLine("--- Part 4: Output Parser ---");

string customerReview = """
    This leaf blower is pretty amazing. It has four settings:
    candle blower, gentle breeze, windy city, and tornado.
    It arrived in two days, just in time for my wife's
    anniversary present. I think my wife liked it so much she was speechless.
    So far I've been the only one using it, and I've been
    using it every other morning to clear the leaves on our lawn.
    It's slightly more expensive than the other leaf blowers
    out there, but I think it's worth it for the extra features.
    """;

string reviewTemplate = """
    For the following text, extract the following information:

    gift: Was the item purchased as a gift for someone else?
    Answer True if yes, False if not or unknown.

    delivery_days: How many days did it take for the product to arrive?
    If this information is not found, output -1.

    price_value: Extract any sentences about the value or price,
    and output them as a comma separated list.

    Format the output as JSON with the following keys:
    gift
    delivery_days
    price_value

    text: {{$text}}
    """;

var extractResult = await kernel.InvokePromptAsync(reviewTemplate,
    new KernelArguments { ["text"] = customerReview });
Console.WriteLine($"Extracted (raw):\n{extractResult}\n");

// Parse the JSON output
string jsonStr = extractResult.ToString()!
    .Replace("```json", "").Replace("```", "").Trim();
using var parsed = JsonDocument.Parse(jsonStr);
Console.WriteLine("Parsed JSON:");
Console.WriteLine($"  gift:          {parsed.RootElement.GetProperty("gift")}");
Console.WriteLine($"  delivery_days: {parsed.RootElement.GetProperty("delivery_days")}");
Console.WriteLine($"  price_value:   {parsed.RootElement.GetProperty("price_value")}");
Console.WriteLine($"  Type:          {parsed.GetType().Name}");
