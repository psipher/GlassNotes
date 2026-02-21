using CommunityToolkit.Mvvm.ComponentModel;

namespace LucidNotes.Models;

/// <summary>
/// Application settings for theme, appearance, and window state
/// </summary>
public partial class AppSettings : ObservableObject
{
    // Theme settings
    [ObservableProperty]
    private string _theme = "Light"; // "Light", "Dark", or "Auto"

    [ObservableProperty]
    private double _opacity = 0.95;

    // Font settings
    [ObservableProperty]
    private string _fontFamily = "Segoe UI";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SmallFontSize))]
    private double _fontSize = 14;

    public double SmallFontSize => FontSize * 0.85; // Slightly smaller for secondary text

    // Color settings
    [ObservableProperty]
    private string _backgroundColor = "Cream"; // Cream, White, Black, Light Gray, Light Blue, Light Green, Light Pink

    [ObservableProperty]
    private string _textColor = "Black"; // Black, Dark Gray, Gray, White, Dark Blue, Dark Green, Dark Red, Purple

    // Window settings
    [ObservableProperty]
    private bool _alwaysOnTop = true;

    public double WindowWidth { get; set; } = 680;
    public double WindowHeight { get; set; } = 520;
    public double WindowLeft { get; set; } = 100;
    public double WindowTop { get; set; } = 100;

    // Last opened note
    public Guid? LastOpenedNoteId { get; set; }

    // Note sorting
    [ObservableProperty]
    private string _noteSortOrder = "NewestFirst"; // "NewestFirst" or "OldestFirst"

    // Custom note order (for drag-drop)
    public List<Guid> CustomNoteOrder { get; set; } = new();

    // Accent color
    [ObservableProperty]
    private string _accentColor = "#FFD700"; // Gold/Yellow for selected notes
}
