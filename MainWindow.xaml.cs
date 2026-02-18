using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LucidNotes.Helpers;
using LucidNotes.ViewModels;

namespace LucidNotes;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private MainViewModel ViewModel => (MainViewModel)DataContext;

    public MainWindow()
    {
        DataContext = new MainViewModel();
        InitializeComponent();
        
        // Load saved window position and size
        LoadWindowState();
        
        Loaded += MainWindow_Loaded;
        Closing += MainWindow_Closing;
    }

    private void MainWindow_SourceInitialized(object? sender, EventArgs e)
    {
        // Enable full edge/corner resizing for transparent windows via WM_NCHITTEST hook
        ResizeHelper.Attach(this);

        // Apply Windows 10/11 dark mode title bar
        try
        {
            WindowBlurHelper.EnableDarkMode(this, ViewModel.Settings.Theme == "Dark");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error applying dark mode: {ex.Message}");
        }
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Apply theme
        ThemeHelper.ApplyTheme(ViewModel.Settings.Theme);
        
        // Set always on top
        Topmost = ViewModel.Settings.AlwaysOnTop;
        
        // Set initial opacity
        if (MainBorder != null)
        {
            MainBorder.Opacity = ViewModel.Settings.Opacity;
        }
        
        // Initialize ComboBoxes
        InitializeComboBoxes();
    }
    
    private void InitializeComboBoxes()
    {
        // Apply initial colors
        ApplyBackgroundColor(ViewModel.Settings.BackgroundColor);
        ApplyTextColor(ViewModel.Settings.TextColor);
    }

    private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        // Save window state
        SaveWindowState();
        
        // Cleanup ViewModel
        ViewModel.Cleanup();
    }

    private void LoadWindowState()
    {
        const double minW = 480, minH = 360;

        if (ViewModel.Settings.WindowWidth >= minW && ViewModel.Settings.WindowHeight >= minH)
        {
            Width  = ViewModel.Settings.WindowWidth;
            Height = ViewModel.Settings.WindowHeight;
        }
        else
        {
            Width  = 680;
            Height = 520;
        }

        // Restore position only if it's plausibly on-screen
        if (ViewModel.Settings.WindowLeft >= 0 && ViewModel.Settings.WindowTop >= 0)
        {
            Left = ViewModel.Settings.WindowLeft;
            Top  = ViewModel.Settings.WindowTop;
        }
    }

    private void SaveWindowState()
    {
        ViewModel.Settings.WindowWidth = Width;
        ViewModel.Settings.WindowHeight = Height;
        ViewModel.Settings.WindowLeft = Left;
        ViewModel.Settings.WindowTop = Top;
        ViewModel.SaveSettings();
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            // Double-click to maximize/restore (optional)
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }
        else
        {
            // Drag to move window
            DragMove();
        }
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    protected override void OnStateChanged(EventArgs e)
    {
        base.OnStateChanged(e);
        if (MaximizeButton != null)
        {
            MaximizeButton.Content = WindowState == WindowState.Maximized ? "❐" : "☐";
            MaximizeButton.ToolTip = WindowState == WindowState.Maximized ? "Restore" : "Maximize";
        }
    }

    // ── Settings Overlay ─────────────────────────────────────────────────────
    private string _soActiveTab = "General";
    private bool _soInitialized = false;

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        if (SettingsOverlay.Visibility == Visibility.Visible)
        {
            SettingsOverlay.Visibility = Visibility.Collapsed;
        }
        else
        {
            if (!_soInitialized)
            {
                SO_LoadSettings();
                SO_AttachEventHandlers();
                _soInitialized = true;
            }
            SettingsOverlay.Visibility = Visibility.Visible;
            SO_SelectTab("General");
        }
    }

    private void SettingsHeader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // Dragging the settings header moves the MAIN window
        if (e.ButtonState == MouseButtonState.Pressed)
            DragMove();
    }

    private void SettingsCloseButton_Click(object sender, RoutedEventArgs e)
    {
        SettingsOverlay.Visibility = Visibility.Collapsed;
    }

    private void SOTab_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string tabName)
            SO_SelectTab(tabName);
    }

    private void SO_SelectTab(string tabName)
    {
        _soActiveTab = tabName;

        SO_PanelGeneral.Visibility = tabName == "General" ? Visibility.Visible : Visibility.Collapsed;
        SO_PanelColors.Visibility  = tabName == "Colors"  ? Visibility.Visible : Visibility.Collapsed;
        SO_PanelFont.Visibility    = tabName == "Font"    ? Visibility.Visible : Visibility.Collapsed;
        SO_PanelAbout.Visibility   = tabName == "About"   ? Visibility.Visible : Visibility.Collapsed;

        var gray  = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x88, 0x88, 0x88));
        var black = System.Windows.Media.Brushes.Black;
        SO_TabGeneral.Foreground = tabName == "General" ? black : gray;
        SO_TabColors.Foreground  = tabName == "Colors"  ? black : gray;
        SO_TabFont.Foreground    = tabName == "Font"    ? black : gray;
        SO_TabAbout.Foreground   = tabName == "About"   ? black : gray;

        // Move underline
        Button? targetButton = tabName switch
        {
            "General" => SO_TabGeneral,
            "Colors"  => SO_TabColors,
            "Font"    => SO_TabFont,
            "About"   => SO_TabAbout,
            _         => null
        };

        if (targetButton != null)
        {
            // Ensure layout is up to date to get accurate position
            if (SettingsOverlay.Visibility == Visibility.Visible)
            {
                SettingsOverlay.UpdateLayout();
            }

            // Calculate position relative to the first tab (General) to determine left offset
            // The tabs are in a StackPanel, so we can just use TranslatePoint relative to the first tab
            // Offset of General is 0.
            
            try 
            {
                // We want the position relative to the container (StackPanel)
                // The underline is in a generic Grid below, which shares the same left alignment as the StackPanel
                // So the X position of the button within the StackPanel is the Left Margin for the underline.
                
                // Fallback for initial layout if needed
                if (targetButton.ActualWidth == 0 && tabName == "General")
                {
                    SO_TabUnderline.Margin = new Thickness(0, 0, 0, 0);
                    SO_TabUnderline.Width = 60; // Default estimate
                }
                else
                {
                     // Get parent stackpanel
                    var parent = System.Windows.Media.VisualTreeHelper.GetParent(targetButton) as UIElement;
                    if (parent != null)
                    {
                        var offset = targetButton.TranslatePoint(new Point(0, 0), parent);
                        SO_TabUnderline.Margin = new Thickness(offset.X, 0, 0, 0);
                        SO_TabUnderline.Width = targetButton.ActualWidth > 0 ? targetButton.ActualWidth : 60;
                    }
                }
            }
            catch 
            {
                // Fallback if visual tree is not ready
            }
        }
    }

    private void SO_LoadSettings()
    {
        SO_TransparencySlider.Value = ViewModel.Settings.Opacity;
        SO_TransparencyLabel.Text   = $"{(int)(ViewModel.Settings.Opacity * 100)}%";
        SO_AlwaysOnTopCheckBox.IsChecked = ViewModel.Settings.AlwaysOnTop;
        SO_FontSizeSlider.Value = ViewModel.Settings.FontSize;
        SO_FontSizeLabel.Text   = $"{(int)ViewModel.Settings.FontSize}px";
        SelectComboByTag(SO_BackgroundColorComboBox, ViewModel.Settings.BackgroundColor);
        SelectComboByTag(SO_TextColorComboBox,       ViewModel.Settings.TextColor);
        SelectComboByTag(SO_FontFamilyComboBox,      ViewModel.Settings.FontFamily);
    }

    private static void SelectComboByTag(ComboBox box, string? value)
    {
        foreach (ComboBoxItem item in box.Items)
            if (item.Tag?.ToString() == value) { box.SelectedItem = item; return; }
    }

    private void SO_AttachEventHandlers()
    {
        SO_TransparencySlider.ValueChanged += (s, e) =>
        {
            ViewModel.Settings.Opacity = e.NewValue;
            SO_TransparencyLabel.Text  = $"{(int)(e.NewValue * 100)}%";
            MainBorder.Opacity = e.NewValue;
            ViewModel.SaveSettings();
        };

        SO_AlwaysOnTopCheckBox.Checked   += (s, e) => { Topmost = true;  ViewModel.Settings.AlwaysOnTop = true;  ViewModel.SaveSettings(); };
        SO_AlwaysOnTopCheckBox.Unchecked += (s, e) => { Topmost = false; ViewModel.Settings.AlwaysOnTop = false; ViewModel.SaveSettings(); };

        SO_FontSizeSlider.ValueChanged += (s, e) =>
        {
            ViewModel.Settings.FontSize = e.NewValue;
            SO_FontSizeLabel.Text       = $"{(int)e.NewValue}px";
            ViewModel.SaveSettings();
        };

        SO_BackgroundColorComboBox.SelectionChanged += (s, e) =>
        {
            if (SO_BackgroundColorComboBox.SelectedItem is ComboBoxItem item)
            {
                var color = item.Tag?.ToString() ?? "White";
                ViewModel.Settings.BackgroundColor = color;
                ApplyBackgroundColor(color);
                ViewModel.SaveSettings();
            }
        };

        SO_TextColorComboBox.SelectionChanged += (s, e) =>
        {
            if (SO_TextColorComboBox.SelectedItem is ComboBoxItem item)
            {
                var color = item.Tag?.ToString() ?? "Black";
                ViewModel.Settings.TextColor = color;
                ApplyTextColor(color);
                ViewModel.SaveSettings();
            }
        };

        SO_FontFamilyComboBox.SelectionChanged += (s, e) =>
        {
            if (SO_FontFamilyComboBox.SelectedItem is ComboBoxItem item)
            {
                ViewModel.Settings.FontFamily = item.Tag?.ToString() ?? "Segoe UI";
                ViewModel.SaveSettings();
            }
        };
    }
    
    public void UpdateOpacity(double opacity)
    {
        this.Opacity = opacity;
    }
    
    public void UpdateBackgroundColor(string colorName)
    {
        ApplyBackgroundColor(colorName);
    }
    
    public void UpdateTextColor(string colorName)
    {
        ApplyTextColor(colorName);
    }
    
    private void ApplyBackgroundColor(string colorName)
    {
        var brush = Helpers.ColorHelper.GetBackgroundBrush(colorName);
        
        // Apply to the main content grid
        var grid = this.FindName("ContentGrid") as Grid;
        if (grid != null)
        {
            grid.Background = brush;
        }
        
        // Apply to sidebar
        var notesSidebar = this.FindName("NotesSidebar") as Border;
        if (notesSidebar != null)
        {
            notesSidebar.Background = brush;
        }
        
        // Keep text editor transparent
        if (NoteTextBox != null)
        {
            NoteTextBox.Background = System.Windows.Media.Brushes.Transparent;
        }
    }
    
    private void ApplyTextColor(string colorName)
    {
        if (NoteTextBox != null)
        {
            NoteTextBox.Foreground = Helpers.ColorHelper.GetTextBrush(colorName);
        }
    }
    
    // Drag and drop support
    private Models.Note? _draggedNote;
    
    private void NotesListBox_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed && NotesListBox.SelectedItem is Models.Note note)
        {
            _draggedNote = note;
            DragDrop.DoDragDrop(NotesListBox, note, DragDropEffects.Move);
        }
    }
    
    private void NotesListBox_Drop(object sender, DragEventArgs e)
    {
        if (_draggedNote != null && e.Data.GetDataPresent(typeof(Models.Note)))
        {
            var targetItem = GetNoteFromPoint(e.GetPosition(NotesListBox));
            if (targetItem != null && targetItem != _draggedNote)
            {
                int oldIndex = ViewModel.Notes.IndexOf(_draggedNote);
                int newIndex = ViewModel.Notes.IndexOf(targetItem);
                
                if (oldIndex != -1 && newIndex != -1)
                {
                    ViewModel.Notes.Move(oldIndex, newIndex);
                    
                    // Save custom order
                    ViewModel.Settings.CustomNoteOrder = ViewModel.Notes.Select(n => n.Id).ToList();
                    ViewModel.SaveSettings();
                }
            }
            _draggedNote = null;
        }
    }
    
    private Models.Note? GetNoteFromPoint(System.Windows.Point point)
    {
        var element = NotesListBox.InputHitTest(point) as DependencyObject;
        while (element != null)
        {
            if (element is ListBoxItem item)
            {
                return item.DataContext as Models.Note;
            }
            element = System.Windows.Media.VisualTreeHelper.GetParent(element);
        }
        return null;
    }

    private void NotesListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (NotesListBox.SelectedItem is Models.Note note)
        {
            ViewModel.OpenNoteInNewWindowCommand.Execute(note);
        }
    }
}
