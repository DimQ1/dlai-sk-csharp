using Microsoft.Extensions.Configuration;

namespace SkCourse.Shared;

public sealed class CourseSettings
{
    public string ModelId { get; init; } = "gpt-4o-mini";
    public string EmbeddingModelId { get; init; } = "text-embedding-3-small";
    public string? OpenAIApiKey { get; init; }
    public string? OpenAIBaseUrl { get; init; }
    public string? AzureOpenAIEndpoint { get; init; }
    public string? AzureOpenAIApiKey { get; init; }
    public string? AzureOpenAIDeploymentName { get; init; }
    public string? AzureOpenAIEmbeddingDeploymentName { get; init; }

    public static CourseSettings FromEnvironment()
    {
        var projectSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "local.settings.json");
        var solutionDirectory = FindSolutionDirectory(Directory.GetCurrentDirectory());
        var sharedSettingsPath = solutionDirectory is null
            ? null
            : Path.Combine(solutionDirectory, "local.settings.json");

        var projectConfiguration = BuildJsonConfiguration(projectSettingsPath);
        var sharedConfiguration = sharedSettingsPath is null
            ? null
            : BuildJsonConfiguration(sharedSettingsPath);

        return new CourseSettings
        {
            ModelId = GetSetting("OPENAI_MODEL", projectConfiguration, sharedConfiguration) ?? "gpt-4o-mini",
            EmbeddingModelId = GetSetting("OPENAI_EMBEDDING_MODEL", projectConfiguration, sharedConfiguration) ?? "text-embedding-3-small",
            OpenAIApiKey = GetSetting("OPENAI_API_KEY", projectConfiguration, sharedConfiguration),
            OpenAIBaseUrl = GetSetting("OPENAI_BASE_URL", projectConfiguration, sharedConfiguration),
            AzureOpenAIEndpoint = GetSetting("AZURE_OPENAI_ENDPOINT", projectConfiguration, sharedConfiguration),
            AzureOpenAIApiKey = GetSetting("AZURE_OPENAI_API_KEY", projectConfiguration, sharedConfiguration),
            AzureOpenAIDeploymentName = GetSetting("AZURE_OPENAI_DEPLOYMENT", projectConfiguration, sharedConfiguration),
            AzureOpenAIEmbeddingDeploymentName = GetSetting("AZURE_OPENAI_EMBEDDING_DEPLOYMENT", projectConfiguration, sharedConfiguration)
        };
    }

    private static IConfiguration BuildJsonConfiguration(string path) =>
        new ConfigurationBuilder()
            .AddJsonFile(path, optional: true, reloadOnChange: false)
            .Build();

    private static string? GetSetting(string key, IConfiguration projectConfiguration, IConfiguration? sharedConfiguration)
    {
        var environmentValue = Environment.GetEnvironmentVariable(key);
        if (!string.IsNullOrWhiteSpace(environmentValue))
        {
            return environmentValue;
        }

        var projectValue = GetValue(projectConfiguration, key);
        if (!string.IsNullOrWhiteSpace(projectValue))
        {
            return projectValue;
        }

        var sharedValue = sharedConfiguration is null ? null : GetValue(sharedConfiguration, key);
        return string.IsNullOrWhiteSpace(sharedValue) ? null : sharedValue;
    }

    private static string? GetValue(IConfiguration configuration, string key) =>
        configuration[key] ?? configuration[$"Values:{key}"];

    private static string? FindSolutionDirectory(string startDirectory)
    {
        var directory = new DirectoryInfo(startDirectory);

        while (directory is not null)
        {
            if (directory.EnumerateFiles("*.sln").Any())
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return null;
    }
}
