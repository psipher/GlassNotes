using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LucidNotes.ViewModels;

namespace LucidNotes;

public partial class SettingsWindow : Window
{
    private readonly MainViewModel _viewModel;
    private readonly MainWindow _mainWindow;

    // Tab layout: maps tab name → (button, panel, underline left offset)
    private readonly Dictionary<string, (Button Button, StackPanel Panel, double UnderlineLeft)> _tabs;
    private string _activeTab = "General";

    public SettingsWindow(MainViewModel viewModel, MainWindow mainWindow)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _mainWindow = mainWindow;

        _tabs = new Dictionary<string, (Button, StackPanel, double)>
        {
            { "General", (TabGeneral, PanelGeneral,   0)   },
            { "Colors",  (TabColors,  PanelColors,    80)  },
            { "Font",    (TabFont,    PanelFont,       158) },
            { "About",   (TabAbout,   PanelAbout,      224) },
        };

        LoadSettings();
        AttachEventHandlers();
        SelectTab("General");

        // Position settings centered over the main window, constrained to fit inside it
        Loaded += (_, _) => PositionOverMainWindow();
    }

    private void PositionOverMainWindow()
    {
        // Center settings over the main window
        Left = _mainWindow.Left + (_mainWindow.ActualWidth  - ActualWidth)  / 2;
        Top  = _mainWindow.Top  + (_mainWindow.ActualHeight - ActualHeight) / 2;

        // Clamp so it never goes off-screen
        Left = Math.Max(0, Left);
        Top  = Math.Max(0, Top);
    }

    // ── Dragging the settings header moves THIS settings window ─────────────
    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
            DragMove();
    }

    // ── Tab switching ────────────────────────────────────────────────────────
    private void Tab_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string tabName)
            SelectTab(tabName);
    }

    private void SelectTab(string tabName)
    {
        _activeTab = tabName;

        foreach (var (name, (btn, panel, left)) in _tabs)
        {
            bool active = name == tabName;
            panel.Visibility = active ? Visibility.Visible : Visibility.Collapsed;
            
            btn.Foreground = active
                ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x1A, 0x1A, 0x1A))
                : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x66, 0x66, 0x66));
            
            btn.FontWeight = active ? FontWeights.Bold : FontWeights.Normal;
        }

        // Animate underline to the correct position
        var (_, _, targetLeft) = _tabs[tabName];
        TabUnderline.Margin = new Thickness(targetLeft, 0, 0, 0);
    }

    // ── Load saved settings into controls ───────────────────────────────────
    private void LoadSettings()
    {
        TransparencySlider.Value = _viewModel.Settings.Opacity;
        UpdateTransparencyLabel();

        AlwaysOnTopCheckBox.IsChecked = _viewModel.Settings.AlwaysOnTop;

        FontSizeSlider.Value = _viewModel.Settings.FontSize;
        UpdateFontSizeLabel();

        SelectComboItem(BackgroundColorComboBox, _viewModel.Settings.BackgroundColor);
        SelectComboItem(TextColorComboBox,       _viewModel.Settings.TextColor);
        SelectComboItem(FontFamilyComboBox,      _viewModel.Settings.FontFamily);
    }

    private static void SelectComboItem(ComboBox box, string? value)
    {
        foreach (ComboBoxItem item in box.Items)
        {
            if (item.Tag?.ToString() == value)
            {
                box.SelectedItem = item;
                return;
            }
        }
    }

    // ── Wire up live-update event handlers ──────────────────────────────────
    private void AttachEventHandlers()
    {
        TransparencySlider.ValueChanged += (s, e) =>
        {
            _viewModel.Settings.Opacity = e.NewValue;
            UpdateTransparencyLabel();
            
            // Call the correct MainWindow method that applies opacity only to the background brush
            _mainWindow.UpdateOpacity(e.NewValue);
            
            _viewModel.SaveSettings();
        };

        AlwaysOnTopCheckBox.Checked += (s, e) =>
        {
            _viewModel.Settings.AlwaysOnTop = true;
            _mainWindow.Topmost = true;
            _viewModel.SaveSettings();
        };

        AlwaysOnTopCheckBox.Unchecked += (s, e) =>
        {
            _viewModel.Settings.AlwaysOnTop = false;
            _mainWindow.Topmost = false;
            _viewModel.SaveSettings();
        };

        FontSizeSlider.ValueChanged += (s, e) =>
        {
            _viewModel.Settings.FontSize = e.NewValue;
            UpdateFontSizeLabel();
            _viewModel.SaveSettings();
        };

        BackgroundColorComboBox.SelectionChanged += (s, e) =>
        {
            if (BackgroundColorComboBox.SelectedItem is ComboBoxItem item)
            {
                var color = item.Tag?.ToString() ?? "Cream";
                _viewModel.Settings.BackgroundColor = color;
                _mainWindow.UpdateBackgroundColor(color);
                _viewModel.SaveSettings();
            }
        };

        TextColorComboBox.SelectionChanged += (s, e) =>
        {
            if (TextColorComboBox.SelectedItem is ComboBoxItem item)
            {
                var color = item.Tag?.ToString() ?? "Black";
                _viewModel.Settings.TextColor = color;
                _mainWindow.UpdateTextColor(color);
                _viewModel.SaveSettings();
            }
        };

        FontFamilyComboBox.SelectionChanged += (s, e) =>
        {
            if (FontFamilyComboBox.SelectedItem is ComboBoxItem item)
            {
                var font = item.Tag?.ToString() ?? "Segoe UI";
                _viewModel.Settings.FontFamily = font;
                _viewModel.SaveSettings();
            }
        };
    }

    private void UpdateTransparencyLabel()
    {
        TransparencyLabel.Text = $"{(int)(TransparencySlider.Value * 100)}%";
    }

    private void UpdateFontSizeLabel()
    {
        FontSizeLabel.Text = $"{(int)FontSizeSlider.Value}px";
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
