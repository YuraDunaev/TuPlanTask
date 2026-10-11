using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CadBridge.Core.Models;
using CadBridge.Core.Serialization;
using CadBridge.Revit.Adapters;

namespace CadBridge.Revit.UI;

public sealed class ImportRoomsViewModel : INotifyPropertyChanged
{
    private readonly Func<string?> _pickFile;
    private readonly List<Level> _levels = new();
    private Document? _document;
    private string _filePath = string.Empty;
    private string? _selectedLevel;
    private int _totalCount;
    private int _newCount;
    private int _updateCount;
    private int _errorCount;

    public ImportRoomsViewModel(Func<string?>? pickFile = null)
    {
        _pickFile = pickFile ?? (() => null);
        BrowseCommand = new RelayCommand(_ => PickFile());
        ImportCommand = new RelayCommand(_ => RequestClose?.Invoke(true), _ => CanImport);
        CancelCommand = new RelayCommand(_ => RequestClose?.Invoke(false));
        Rooms.CollectionChanged += (_, _) => UpdateCounters();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public event Action<bool>? RequestClose;

    public string FilePath
    {
        get => _filePath;
        set
        {
            if (Set(ref _filePath, value))
            {
                ImportCommand.NotifyCanExecuteChanged();

                if (System.IO.File.Exists(value))
                    LoadRoomsFile();
            }
        }
    }

    public ObservableCollection<string> Levels { get; } = new();

    public string? SelectedLevel
    {
        get => _selectedLevel;
        set
        {
            if (Set(ref _selectedLevel, value))
                ImportCommand.NotifyCanExecuteChanged();
        }
    }

    public ObservableCollection<RoomPreviewRow> Rooms { get; } = new();

    public RelayCommand BrowseCommand { get; }

    public RelayCommand ImportCommand { get; }

    public RelayCommand CancelCommand { get; }

    public int TotalCount
    {
        get => _totalCount;
        private set => Set(ref _totalCount, value);
    }

    public int NewCount
    {
        get => _newCount;
        private set => Set(ref _newCount, value);
    }

    public int UpdateCount
    {
        get => _updateCount;
        private set => Set(ref _updateCount, value);
    }

    public int ErrorCount
    {
        get => _errorCount;
        private set => Set(ref _errorCount, value);
    }

    public string Summary => $"{TotalCount} rooms · {NewCount} new · {UpdateCount} update · {ErrorCount} error";

    private bool CanImport => !string.IsNullOrWhiteSpace(FilePath) && SelectedLevel is not null;

    private void PickFile()
    {
        var path = _pickFile();
        if (!string.IsNullOrWhiteSpace(path))
            FilePath = path;
    }

    private void UpdateCounters()
    {
        TotalCount = Rooms.Count;
        NewCount = Rooms.Count(row => row.Status == "New");
        UpdateCount = Rooms.Count(row => row.Status == "Update");
        ErrorCount = Rooms.Count(row => row.Status == "Error");
        OnPropertyChanged(nameof(Summary));
    }

    private bool Set<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public void RefreshLevels(Document document)
    {
        _document = document;
        Levels.Clear();
        _levels.Clear();

        var collector = new FilteredElementCollector(document)
            .OfClass(typeof(Level))
            .Cast<Level>()
            .OrderBy(l => l.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var level in collector)
        {
            _levels.Add(level);
            Levels.Add(level.Name);
        }

        SelectedLevel = Levels.FirstOrDefault();

        if (Levels.Count == 0)
            TaskDialog.Show(
                "CadBridge",
                "No levels found in the model (FilteredElementCollector returned 0 elements).");
    }

    public Level? GetSelectedLevel() =>
        _levels.FirstOrDefault(l => l.Name == SelectedLevel);

    private static string ResolveStatus(RevitRoomReader? reader, Room room)
    {
        if (room.Id.IsEmpty)
            return "Error";

        try
        {
            return reader is not null && reader.GetExisting(room.Id) is not null ? "Update" : "New";
        }
        catch
        {
            return "New";
        }
    }

    public void LoadRoomsFile()
    {
        if (string.IsNullOrWhiteSpace(FilePath))
            return;

        RoomsDocument? document;
        try
        {
            var json = System.IO.File.ReadAllText(FilePath);
            document = RoomsJsonSerializer.Deserialize(json);
        }
        catch (System.Exception ex) when (ex is System.IO.IOException or System.IO.InvalidDataException or System.Text.Json.JsonException)
        {
            Rooms.Clear();
            TaskDialog.Show("CadBridge", $"Failed to load rooms.json: {ex.Message}");
            return;
        }

        if (document is null)
        {
            Rooms.Clear();
            return;
        }

        Rooms.Clear();
        var reader = _document is null ? null : new RevitRoomReader(_document);
        foreach (var room in document.Rooms)
        {
            Rooms.Add(new RoomPreviewRow(
                room.Id.Value,
                room.Number ?? string.Empty,
                room.Name ?? string.Empty,
                room.Area,
                ResolveStatus(reader, room)));
        }

        TotalCount = Rooms.Count;
        OnPropertyChanged(nameof(Summary));
    }
}
