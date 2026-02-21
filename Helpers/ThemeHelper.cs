using System.Windows;
using System.Windows.Media;

namespace LucidNotes.Helpers;

/// <summary>
/// Helper for managing application themes
/// </summary>
public static class ThemeHelper
{
    /// <summary>
    /// Apply a theme to the application
    /// </summary>
    public static void ApplyTheme(string theme)
    {
        var app = Application.Current;

        // Remove existing theme dictionaries
        var existingTheme = app.Resources.MergedDictionaries
            .FirstOrDefault(d => d.Source?.OriginalString.Contains("Themes/") == true);

        if (existingTheme != null)
        {
            app.Resources.MergedDictionaries.Remove(existingTheme);
        }

        // Add new theme
        var themeUri = theme.ToLower() switch
        {
            "light" => new Uri("Resources/Themes/LightTheme.xaml", UriKind.Relative),
            "dark" => new Uri("Resources/Themes/DarkTheme.xaml", UriKind.Relative),
            _ => new Uri("Resources/Themes/DarkTheme.xaml", UriKind.Relative)
        };

        try
        {
            var themeDict = new ResourceDictionary { Source = themeUri };
            app.Resources.MergedDictionaries.Add(themeDict);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error applying theme: {ex.Message}");
        }
    }

    /// <summary>
    /// Get a color from hex string
    /// </summary>
    public static Color GetColorFromHex(string hex)
    {
        try
        {
            return (Color)ColorConverter.ConvertFromString(hex);
        }
        catch
        {
            return Colors.Blue;
        }
    }

    /// <summary>
    /// Apply accent color to the application
    /// </summary>
    public static void ApplyAccentColor(string hexColor)
    {
        var color = GetColorFromHex(hexColor);
        var brush = new SolidColorBrush(color);

        Application.Current.Resources["AccentColor"] = color;
        Application.Current.Resources["AccentBrush"] = brush;
    }
}
