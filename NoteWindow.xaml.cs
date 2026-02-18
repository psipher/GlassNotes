using System;
using System.Windows;
using System.Windows.Input;
using LucidNotes.Helpers;
using LucidNotes.Models;

namespace LucidNotes
{
    public partial class NoteWindow : Window
    {
        public NoteWindow(Note note, AppSettings settings)
        {
            InitializeComponent();
            DataContext = note;
            ApplySettings(settings);
        }

        private void ApplySettings(AppSettings settings)
        {
            // Colors
            var bgBrush = ColorHelper.GetBackgroundBrush(settings.BackgroundColor);
            var textBrush = ColorHelper.GetTextBrush(settings.TextColor);
            Resources["NoteBackgroundBrush"] = bgBrush;
            Resources["NoteForegroundBrush"] = textBrush;

            // Transparency
            OuterBorder.Opacity = settings.Opacity;

            // Always on Top
            Topmost = settings.AlwaysOnTop;
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            ResizeHelper.Attach(this);
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
