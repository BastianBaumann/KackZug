using System.Text.Json;
using KackZug.Core.Models;

namespace KackZug.Core.Storage;

/// <summary>Liest und schreibt alle Daten als eine JSON-Datei, das Schreiben ist atomar</summary>
public sealed class AppDataStore
{
    public const string FileName = "kackzug.json";

    private readonly object _fileLock = new object();

    public AppDataStore(string directory)
    {
        Directory.CreateDirectory(directory);
        FilePath = Path.Combine(directory, FileName);
    }

    public string FilePath { get; }

    public AppData Load()
    {
        lock (_fileLock)
        {
            if (!File.Exists(FilePath))
            {
                return new AppData();
            }

            try
            {
                var json = File.ReadAllText(FilePath);
                var data = JsonSerializer.Deserialize(json, AppJsonContext.Default.AppData);
                if (data is null)
                {
                    return new AppData();
                }

                FillMissingParts(data);
                return data;
            }
            catch (JsonException)
            {
                KeepBrokenFile();
                return new AppData();
            }
        }
    }

    public void Save(AppData data)
    {
        lock (_fileLock)
        {
            var json = JsonSerializer.Serialize(data, AppJsonContext.Default.AppData);
            var temporaryPath = FilePath + ".tmp";
            File.WriteAllText(temporaryPath, json);
            File.Move(temporaryPath, FilePath, true);
        }
    }

    private void KeepBrokenFile()
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        var brokenPath = FilePath + ".broken-" + timestamp;
        File.Move(FilePath, brokenPath, true);
    }

    private static void FillMissingParts(AppData data)
    {
        if (data.Shifts is null)
        {
            data.Shifts = new List<Shift>();
        }

        if (data.Settings is null)
        {
            data.Settings = new Settings();
        }

        if (data.CheckStates is null)
        {
            data.CheckStates = new List<CheckState>();
        }

        if (data.Settings.LeadTimesMinutes is null)
        {
            data.Settings.LeadTimesMinutes = new List<int>();
        }
    }
}
