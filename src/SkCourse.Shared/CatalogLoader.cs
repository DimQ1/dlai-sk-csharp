using Microsoft.SemanticKernel.Memory;
using Microsoft.VisualBasic.FileIO;

namespace SkCourse.Shared;

/// <summary>
/// Loads OutdoorClothingCatalog_1000.csv into an ISemanticTextMemory collection.
/// Shared between L4-CreateDb, L4-QnA and L5-Evaluation.
/// </summary>
public static class CatalogLoader
{
    public const string Collection = "outdoordb";

    public static List<(int Index, string Name, string Description)> ReadCsv(string csvPath)
    {
        var records = new List<(int, string, string)>();

        using var parser = new TextFieldParser(csvPath);
        parser.TextFieldType = FieldType.Delimited;
        parser.SetDelimiters(",");
        parser.HasFieldsEnclosedInQuotes = true;
        parser.ReadFields(); // skip header

        while (!parser.EndOfData)
        {
            var fields = parser.ReadFields()!;
            if (fields.Length >= 3 && int.TryParse(fields[0], out int idx))
            {
                records.Add((idx, fields[1], fields[2]));
            }
        }

        return records;
    }

    public static async Task PopulateMemoryAsync(
        ISemanticTextMemory memory,
        List<(int Index, string Name, string Description)> records,
        Action<int, int>? progress = null)
    {
        for (int i = 0; i < records.Count; i++)
        {
            var (idx, name, description) = records[i];
            string text = $"{name} : {description}";
            await memory.SaveInformationAsync(Collection, id: idx.ToString(), text: text);
            progress?.Invoke(i + 1, records.Count);
        }
    }

    /// <summary>Resolves the data directory relative to the running project.</summary>
    public static string ResolveDataPath(string fileName)
    {
        // Walk up from bin folder to find the data directory
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, "data", fileName);
            if (File.Exists(candidate)) return candidate;
            candidate = Path.Combine(dir.FullName, "..", "..", "data", fileName);
            if (File.Exists(Path.GetFullPath(candidate))) return Path.GetFullPath(candidate);
            dir = dir.Parent;
        }

        throw new FileNotFoundException($"Cannot find {fileName} in data directory.");
    }
}
