using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace LucidNotes.Models;

/// <summary>
/// Represents a single note with content and metadata
/// </summary>
public partial class Note : ObservableObject
{
    public Guid Id { get; set; }
    
    [ObservableProperty]
    private string _title = "Untitled Note";
    
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }

    public Note()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.Now;
        ModifiedAt = DateTime.Now;
    }
}
