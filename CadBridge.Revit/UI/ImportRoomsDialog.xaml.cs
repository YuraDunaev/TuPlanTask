using System.Windows;
using Autodesk.Revit.DB;

namespace CadBridge.Revit.UI;

public partial class ImportRoomsDialog : Window
{
    public ImportRoomsViewModel ViewModel { get; }

    public ImportRoomsDialog(Document document)
    {
        InitializeComponent();

        ViewModel = new ImportRoomsViewModel(PickFile);
        ViewModel.RequestClose += result => DialogResult = result;
        DataContext = ViewModel;
        ViewModel.RefreshLevels(document);
    }

    private static string? PickFile()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "rooms.json|*.json|All files|*.*",
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    private void OnCloseClick(object sender, RoutedEventArgs e) => DialogResult = false;
}

