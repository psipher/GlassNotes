using System.Windows.Media;

namespace LucidNotes.Helpers;

public static class ColorHelper
{
    public static SolidColorBrush GetBackgroundBrush(string colorName)
    {
        return colorName switch
        {
            "Cream" => new SolidColorBrush(Color.FromRgb(255, 253, 208)), // #FFFDD0
            "White" => new SolidColorBrush(Colors.White),
            "Black" => new SolidColorBrush(Color.FromRgb(30, 30, 30)), // #1E1E1E
            "Light Gray" => new SolidColorBrush(Color.FromRgb(245, 245, 245)), // #F5F5F5
            "Light Blue" => new SolidColorBrush(Color.FromRgb(230, 240, 255)), // Very pale blue
            "Light Green" => new SolidColorBrush(Color.FromRgb(235, 255, 235)), // Very pale green
            "Light Pink" => new SolidColorBrush(Color.FromRgb(255, 235, 240)), // Very pale pink
            "Light Yellow" => new SolidColorBrush(Color.FromRgb(255, 255, 224)), // #FFFFE0
            _ => new SolidColorBrush(Color.FromRgb(255, 253, 208)) // Default to Cream
        };
    }

    public static SolidColorBrush GetTextBrush(string colorName)
    {
        return colorName switch
        {
            "Black" => new SolidColorBrush(Colors.Black),
            "Dark Gray" => new SolidColorBrush(Color.FromRgb(64, 64, 64)), // #404040
            "Gray" => new SolidColorBrush(Color.FromRgb(128, 128, 128)), // #808080
            "White" => new SolidColorBrush(Colors.White),
            "Dark Blue" => new SolidColorBrush(Color.FromRgb(0, 0, 139)), // #00008B
            "Dark Green" => new SolidColorBrush(Color.FromRgb(0, 100, 0)), // #006400
            "Dark Red" => new SolidColorBrush(Color.FromRgb(139, 0, 0)), // #8B0000
            "Purple" => new SolidColorBrush(Color.FromRgb(128, 0, 128)), // #800080
            _ => new SolidColorBrush(Colors.Black) // Default to Black
        };
    }
}
