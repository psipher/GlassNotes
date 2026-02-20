using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LucidNotes.Models;
using LucidNotes.Services;

namespace LucidNotes.ViewModels;

/// <summary>
/// Main ViewModel for the application
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly NoteService _noteService;
    private readonly SettingsService _settingsService;
    private readonly DispatcherTimer _autoSaveTimer;
    private readonly System.Collections.Generic.Dictionary<Guid, NoteWindow> _openNoteWindows = new();

    [ObservableProperty]
    private ObservableCollection<Note> _notes = new();

    [ObservableProperty]
    private Note? _currentNote;

    [ObservableProperty]
    private string _currentNoteContent = string.Empty;

    [ObservableProperty]
    private AppSettings _settings = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    public ICollectionView FilteredNotes { get; }

    [ObservableProperty]
    private bool _isSettingsPanelOpen;

    [ObservableProperty]
    private bool _areNotesVisible = true;

    public MainViewModel()
    {
        _noteService = new NoteService();
        _settingsService = new SettingsService();

        // Setup auto-save timer (save every 3 seconds after changes)
        _autoSaveTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(3)
        };
        _autoSaveTimer.Tick += AutoSaveTimer_Tick;

        // Load settings and notes
        LoadSettings();
        LoadNotes();

        FilteredNotes = CollectionViewSource.GetDefaultView(Notes);
        FilteredNotes.Filter = FilterNotes;
    }

    private bool FilterNotes(object obj)
    {
        if (obj is not Note note) return false;
        if (string.IsNullOrWhiteSpace(SearchText)) return true;

        return note.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
               note.Content.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
    }

    partial void OnSearchTextChanged(string value)
    {
        FilteredNotes.Refresh();
    }

    private void LoadSettings()
    {
        Settings = _settingsService.LoadSettings();
    }

    private void LoadNotes()
    {
        var loadedNotes = _noteService.LoadAllNotes();
        
        // Sort notes based on settings
        var sortedNotes = Settings.NoteSortOrder == "OldestFirst"
            ? loadedNotes.OrderBy(n => n.ModifiedAt).ToList()
            : loadedNotes.OrderByDescending(n => n.ModifiedAt).ToList();
        
        Notes = new ObservableCollection<Note>(sortedNotes);

        // Select the last opened note or the first note
        if (Settings.LastOpenedNoteId.HasValue)
        {
            CurrentNote = Notes.FirstOrDefault(n => n.Id == Settings.LastOpenedNoteId.Value);
        }

        CurrentNote ??= Notes.FirstOrDefault();

        if (CurrentNote != null)
        {
            CurrentNoteContent = CurrentNote.Content;
        }
    }

    partial void OnCurrentNoteContentChanged(string value)
    {
        if (CurrentNote != null)
        {
            CurrentNote.Content = value;
            
            // Update title based on first line of content
            var firstLine = value.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(firstLine))
            {
                CurrentNote.Title = firstLine.Length > 30 ? firstLine.Substring(0, 30) + "..." : firstLine;
            }
            else
            {
                CurrentNote.Title = "Untitled Note";
            }
            
            // Restart auto-save timer
            _autoSaveTimer.Stop();
            _autoSaveTimer.Start();
        }
    }

    partial void OnCurrentNoteChanged(Note? value)
    {
        if (value != null)
        {
            CurrentNoteContent = value.Content;
            Settings.LastOpenedNoteId = value.Id;
            SaveSettings();
        }
    }

    private void AutoSaveTimer_Tick(object? sender, EventArgs e)
    {
        _autoSaveTimer.Stop();
        SaveCurrentNote();
    }

    [RelayCommand]
    private void SaveCurrentNote()
    {
        if (CurrentNote != null)
        {
            _noteService.SaveNote(CurrentNote);
        }
    }

    [RelayCommand]
    private void CreateNewNote()
    {
        var newNote = new Note
        {
            Title = $"Note {Notes.Count + 1}",
            Content = string.Empty
        };

        // Add note based on sort order
        if (Settings.NoteSortOrder == "OldestFirst")
        {
            Notes.Add(newNote); // Add at bottom for oldest first
        }
        else
        {
            Notes.Insert(0, newNote); // Add at top for newest first
        }
        
        CurrentNote = newNote;
        _noteService.SaveNote(newNote);
    }

    [RelayCommand]
    private void DeleteCurrentNote()
    {
        if (CurrentNote != null && Notes.Count > 1)
        {
            var noteToDelete = CurrentNote;
            var index = Notes.IndexOf(noteToDelete);
            
            // Remove the note first
            Notes.Remove(noteToDelete);
            
            // Select another note - if we deleted the last one, select the new last one
            // Otherwise select the note at the same index (which is now the next note)
            if (index >= Notes.Count)
            {
                CurrentNote = Notes[Notes.Count - 1];
            }
            else
            {
                CurrentNote = Notes[index];
            }
            
            _noteService.DeleteNote(noteToDelete.Id);
        }
    }

    [RelayCommand]
    private void OpenNoteInNewWindow(Note? note)
    {
        if (note == null) return;

        if (_openNoteWindows.TryGetValue(note.Id, out var existingWindow))
        {
            existingWindow.Activate();
            existingWindow.Focus();
            return;
        }

        var noteWindow = new NoteWindow(note, Settings);
        _openNoteWindows[note.Id] = noteWindow;
        
        noteWindow.Closed += (s, e) => _openNoteWindows.Remove(note.Id);
        noteWindow.Show();
    }

    [RelayCommand]
    private void ToggleSettingsPanel()
    {
        IsSettingsPanelOpen = !IsSettingsPanelOpen;
    }

    [RelayCommand]
    public void ToggleAllNotesVisibility()
    {
        AreNotesVisible = !AreNotesVisible;
        
        foreach (System.Windows.Window window in System.Windows.Application.Current.Windows)
        {
            if (window is MainWindow || window is NoteWindow)
            {
                if (AreNotesVisible)
                {
                    window.Show();
                    // Optional: Bring to front when unhiding
                    if (window.Topmost) window.Topmost = true; 
                }
                else
                {
                    window.Hide();
                }
            }
        }
    }

    public void SaveSettings()
    {
        _settingsService.SaveSettings(Settings);
    }

    public void Cleanup()
    {
        _autoSaveTimer.Stop();
        SaveCurrentNote();
        SaveSettings();
    }
}
