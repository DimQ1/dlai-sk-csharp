using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Memory;

namespace SkCourse.Shared;

public static class KernelFactory
{
    public static Kernel CreateKernel(out CourseSettings settings)
    {
        settings = CourseSettings.FromEnvironment();

        var builder = Kernel.CreateBuilder();


        if (IsAzureConfigured(settings))
        {
            builder.AddAzureOpenAIChatCompletion(
                deploymentName: settings.AzureOpenAIDeploymentName!,
                endpoint: settings.AzureOpenAIEndpoint!,
                apiKey: settings.AzureOpenAIApiKey!);
            return builder.Build();
        }

        if (!string.IsNullOrWhiteSpace(settings.OpenAIApiKey))
        {
            var httpClient = CreateOpenAIHttpClient(settings.OpenAIBaseUrl);

            builder.AddOpenAIChatCompletion(
                modelId: settings.ModelId,
                apiKey: settings.OpenAIApiKey,
                httpClient: httpClient);
            return builder.Build();
        }

        throw new InvalidOperationException(
            "No LLM credentials found. Set OPENAI_API_KEY or AZURE_OPENAI_ENDPOINT, AZURE_OPENAI_API_KEY, and AZURE_OPENAI_DEPLOYMENT.");
    }

    public static ISemanticTextMemory CreateMemory(CourseSettings settings)
    {
        var memoryBuilder = new MemoryBuilder();
        var httpClient = CreateOpenAIHttpClient(settings.OpenAIBaseUrl);

#pragma warning disable CS0618 // MemoryBuilder extensions are obsolete but functional
        if (IsAzureConfigured(settings) &&
            !string.IsNullOrWhiteSpace(settings.AzureOpenAIEmbeddingDeploymentName))
        {
            memoryBuilder.WithOpenAITextEmbeddingGeneration(
                modelId: settings.AzureOpenAIEmbeddingDeploymentName,
                apiKey: settings.AzureOpenAIApiKey!);
        }
        else if (!string.IsNullOrWhiteSpace(settings.OpenAIApiKey))
        {
            memoryBuilder.WithOpenAITextEmbeddingGeneration(
                modelId: settings.EmbeddingModelId,
                apiKey: settings.OpenAIApiKey, httpClient: httpClient);
        }
        else
        {
            throw new InvalidOperationException("No embedding credentials found.");
        }

        memoryBuilder.WithMemoryStore(new VolatileMemoryStore());
        return memoryBuilder.Build();
    }

    private static bool IsAzureConfigured(CourseSettings s) =>
        !string.IsNullOrWhiteSpace(s.AzureOpenAIEndpoint) &&
        !string.IsNullOrWhiteSpace(s.AzureOpenAIApiKey) &&
        !string.IsNullOrWhiteSpace(s.AzureOpenAIDeploymentName);

    private static HttpClient? CreateOpenAIHttpClient(string? baseUrl)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return null;
        }

        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var endpoint))
        {
            throw new InvalidOperationException("OPENAI_BASE_URL must be a valid absolute URL.");
        }

        return new HttpClient
        {
            BaseAddress = endpoint
        };
    }
}
