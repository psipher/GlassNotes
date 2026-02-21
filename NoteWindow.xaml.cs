using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using LucidNotes.Helpers;
using LucidNotes.Models;
using LucidNotes.Services;

namespace LucidNotes
{
    public partial class NoteWindow : Window
    {
        private readonly NoteService _noteService;
        private readonly DispatcherTimer _autoSaveTimer;

        public NoteWindow(Note note, AppSettings settings)
        {
            InitializeComponent();
            DataContext = note;

            _noteService = new NoteService();
            _autoSaveTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
            _autoSaveTimer.Tick += (s, e) => SaveNote();

            ApplySettings(settings);

            Closing += NoteWindow_Closing;

            NoteTextBox.TextChanged += (s, e) =>
            {
                _autoSaveTimer.Stop();
                _autoSaveTimer.Start();

                var firstLine = NoteTextBox.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(firstLine))
                {
                    note.Title = firstLine.Length > 30 ? firstLine.Substring(0, 30) + "..." : firstLine;
                }
                else
                {
                    note.Title = "Untitled Note";
                }
            };
        }

        private void SaveNote()
        {
            _autoSaveTimer.Stop();
            if (DataContext is Note note)
            {
                _noteService.SaveNote(note);
            }
        }

        private void NoteWindow_Closing(object? sender, CancelEventArgs e)
        {
            SaveNote();
        }

        public void ApplySettings(AppSettings settings)
        {
            // Colors
            var bgBrush = ColorHelper.GetBackgroundBrush(settings.BackgroundColor);
            bgBrush.Opacity = settings.Opacity;

            var textBrush = ColorHelper.GetTextBrush(settings.TextColor);
            Resources["NoteBackgroundBrush"] = bgBrush;
            Resources["NoteForegroundBrush"] = textBrush;

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
