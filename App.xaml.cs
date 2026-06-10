using System.Windows;
using GlassNotes.ViewModels;

namespace GlassNotes;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        // Ensure the WPF framework uses the current culture for formatting UI elements
        FrameworkElement.LanguageProperty.OverrideMetadata(
            typeof(FrameworkElement),
            new FrameworkPropertyMetadata(
                System.Windows.Markup.XmlLanguage.GetLanguage(System.Globalization.CultureInfo.CurrentCulture.IetfLanguageTag)));

        SessionEnding += App_SessionEnding;

        base.OnStartup(e);
    }

    private void App_SessionEnding(object sender, SessionEndingCancelEventArgs e)
    {
        try
        {
            if (MainWindow is MainWindow mainWindow && mainWindow.DataContext is MainViewModel viewModel)
            {
                viewModel.Cleanup();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error during session ending cleanup: {ex.Message}");
        }
    }
}

