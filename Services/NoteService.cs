using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using LucidNotes.Models;

namespace LucidNotes.Services;

/// <summary>
/// Service for managing notes persistence
/// </summary>
public class NoteService
{
    private static readonly string NotesFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "LucidNotes",
        "Notes"
    );

    public NoteService()
    {
        // Ensure notes directory exists
        Directory.CreateDirectory(NotesFolder);
    }

    /// <summary>
    /// Load all notes from disk
    /// </summary>
    public List<Note> LoadAllNotes()
    {
        var notes = new List<Note>();

        try
        {
            var noteFiles = Directory.GetFiles(NotesFolder, "*.json");
            
            foreach (var file in noteFiles)
            {
                try
                {
                    var json = File.ReadAllText(file);
                    var note = JsonConvert.DeserializeObject<Note>(json);
                    if (note != null)
                    {
                        notes.Add(note);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading note {file}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading notes: {ex.Message}");
        }

        // If no notes exist, create a default one
        if (notes.Count == 0)
        {
            var defaultNote = new Note
            {
                Title = "Welcome to Lucid Notes!",
                Content = "Start typing your notes here...\n\nFeatures:\n• Always on top\n• Transparent background\n• Auto-save\n• Multiple notes\n• Customizable themes"
            };
            notes.Add(defaultNote);
            SaveNote(defaultNote);
        }

        return notes.OrderByDescending(n => n.ModifiedAt).ToList();
    }

    /// <summary>
    /// Save a single note to disk
    /// </summary>
    public void SaveNote(Note note)
    {
        try
        {
            note.ModifiedAt = DateTime.Now;
            var filePath = Path.Combine(NotesFolder, $"{note.Id}.json");
            var json = JsonConvert.SerializeObject(note, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving note: {ex.Message}");
        }
    }

    /// <summary>
    /// Delete a note from disk
    /// </summary>
    public void DeleteNote(Guid noteId)
    {
        try
        {
            var filePath = Path.Combine(NotesFolder, $"{noteId}.json");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error deleting note: {ex.Message}");
        }
    }
}
