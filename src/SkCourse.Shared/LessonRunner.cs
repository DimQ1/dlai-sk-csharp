using Microsoft.SemanticKernel;

namespace SkCourse.Shared;

public static class LessonRunner
{
    public static async Task RunPromptAsync(string lessonName, string prompt)
    {
        Console.WriteLine($"=== {lessonName} ===");

        var kernel = KernelFactory.CreateKernel(out var settings);
        Console.WriteLine($"Model: {settings.ModelId}");

        FunctionResult result = await kernel.InvokePromptAsync(prompt);

        Console.WriteLine();
        Console.WriteLine("Prompt:");
        Console.WriteLine(prompt);
        Console.WriteLine();
        Console.WriteLine("Response:");
        Console.WriteLine(result.ToString());
    }
}
