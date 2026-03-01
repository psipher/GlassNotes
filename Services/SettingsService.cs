using System.IO;
using GlassNotes.Models;
using Newtonsoft.Json;

namespace GlassNotes.Services;

/// <summary>
/// Service for loading and saving application settings
/// </summary>
public class SettingsService
{
    private static readonly string SettingsFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "GlassNotes"
    );

    private static readonly string SettingsFilePath = Path.Combine(SettingsFolder, "settings.json");

    /// <summary>
    /// Load settings from disk, or return default settings if file doesn't exist
    /// </summary>
    public AppSettings LoadSettings()
    {
        try
        {
            if (File.Exists(SettingsFilePath))
            {
                var json = File.ReadAllText(SettingsFilePath);
                return JsonConvert.DeserializeObject<AppSettings>(json) ?? new AppSettings();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
        }

        return new AppSettings();
    }

    /// <summary>
    /// Save settings to disk
    /// </summary>
    public void SaveSettings(AppSettings settings)
    {
        try
        {
            // Ensure directory exists
            Directory.CreateDirectory(SettingsFolder);

            var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText(SettingsFilePath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex.Message}");
        }
    }
}
