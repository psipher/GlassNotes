# Release Notes - GlassNotes v1.0.7

This release resolves critical data-loss issues related to note auto-saving and introduces static analysis guidelines to prevent future regressions.

## [v1.0.7] - 2026-06-10

### Fixed
* **Note Switching Data Loss**: Fixed a bug where unsaved changes to a note were lost if the user switched to another note before the 3-second auto-save timer fired. Changes are now flushed to disk immediately upon switching.
* **OS Shutdown/Restart Protection**: Handled the `Application.SessionEnding` event. Unsaved note content, window positions, and application settings are now saved cleanly when the operating system shuts down, restarts (e.g., during automatic Windows Updates), or when the user logs off.

### Improved
* **Code Quality & Static Analysis**: Added the `WpfAnalyzers` NuGet Style and Quality Analyzer package to enforce WPF-specific best practices during development.
* **Dependency Property Safety**: Resolved 53 static analysis warnings (`WPF0041`) by converting all direct CLR property assignments to `SetCurrentValue` calls across `MainWindow` and `SettingsWindow`, protecting the application from accidentally overwriting WPF data bindings.
