using System.ComponentModel;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SkCourse.Shared;

Console.WriteLine("=== L6 - Agents (Auto Function Calling) ===\n");

var kernel = KernelFactory.CreateKernel(out var settings);
Console.WriteLine($"Model: {settings.ModelId}\n");

// ---------------------------------------------------------------
// Register plugins (equivalent to MathSkill, TimeSkill, HttpSkill)
// ---------------------------------------------------------------
kernel.Plugins.AddFromType<MathPlugin>();
kernel.Plugins.AddFromType<TimePlugin>();
kernel.Plugins.AddFromType<WriterPlugin>();

Console.WriteLine("Registered plugins: MathPlugin, TimePlugin, WriterPlugin\n");

// Auto function calling settings (modern equivalent of BasicPlanner)
var executionSettings = new OpenAIPromptExecutionSettings
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
};

// ---------------------------------------------------------------
// Ask 1: Math — should choose MathPlugin.Add
// ---------------------------------------------------------------
Console.WriteLine("--- Ask 1: Math ---");
string ask1 = "What is the sum of 2319 and 86988?";
Console.WriteLine($"Ask: {ask1}");
var result1 = await kernel.InvokePromptAsync(ask1, new KernelArguments(executionSettings));
Console.WriteLine($"Answer: {result1}\n");

// ---------------------------------------------------------------
// Ask 2: Time — should choose TimePlugin.GetCurrentDateAndTime
// ---------------------------------------------------------------
Console.WriteLine("--- Ask 2: Time ---");
string ask2 = "What's the date today?";
Console.WriteLine($"Ask: {ask2}");
var result2 = await kernel.InvokePromptAsync(ask2, new KernelArguments(executionSettings));
Console.WriteLine($"Answer: {result2}\n");

// ---------------------------------------------------------------
// Ask 3: Writer — should choose WriterPlugin.ShortPoem
// ---------------------------------------------------------------
Console.WriteLine("--- Ask 3: Writer ---");
string ask3 = "Write a short poem about the mountains.";
Console.WriteLine($"Ask: {ask3}");
var result3 = await kernel.InvokePromptAsync(ask3, new KernelArguments(executionSettings));
Console.WriteLine($"Answer: {result3}\n");

// ---------------------------------------------------------------
// Ask 4: Multi-step — may chain multiple plugins
// ---------------------------------------------------------------
Console.WriteLine("--- Ask 4: Multi-step ---");
string ask4 = "What day of the week is today, and also compute 1234 * 5678?";
Console.WriteLine($"Ask: {ask4}");
var result4 = await kernel.InvokePromptAsync(ask4, new KernelArguments(executionSettings));
Console.WriteLine($"Answer: {result4}\n");

// ===================== Plugin Definitions =====================

/// <summary>Equivalent to the Python MathSkill.</summary>
public class MathPlugin
{
    [KernelFunction, Description("Adds two numbers together")]
    public static double Add(
        [Description("The first number")] double a,
        [Description("The second number")] double b) => a + b;

    [KernelFunction, Description("Subtracts the second number from the first")]
    public static double Subtract(
        [Description("The first number")] double a,
        [Description("The second number")] double b) => a - b;

    [KernelFunction, Description("Multiplies two numbers")]
    public static double Multiply(
        [Description("The first number")] double a,
        [Description("The second number")] double b) => a * b;

    [KernelFunction, Description("Divides the first number by the second")]
    public static double Divide(
        [Description("The dividend")] double a,
        [Description("The divisor")] double b) => b != 0 ? a / b : double.NaN;
}

/// <summary>Equivalent to the Python TimeSkill.</summary>
public class TimePlugin
{
    [KernelFunction, Description("Gets the current date and time")]
    public static string GetCurrentDateAndTime() => DateTime.Now.ToString("F");

    [KernelFunction, Description("Gets today's date")]
    public static string GetTodayDate() => DateTime.Today.ToString("D");

    [KernelFunction, Description("Gets the current day of the week")]
    public static string GetDayOfWeek() => DateTime.Now.DayOfWeek.ToString();
}

/// <summary>Equivalent to the Python WriterSkill loaded from the skills directory.</summary>
public class WriterPlugin
{
    [KernelFunction, Description("Writes a short poem on a given topic")]
    public static string ShortPoem([Description("The topic of the poem")] string topic) =>
        $"(A poem about {topic} — the LLM should compose this creatively.)";

    [KernelFunction, Description("Translates text to the specified language")]
    public static string Translate(
        [Description("The text to translate")] string text,
        [Description("The target language")] string language) =>
        $"(Translate '{text}' to {language} — the LLM should handle this.)";
}
