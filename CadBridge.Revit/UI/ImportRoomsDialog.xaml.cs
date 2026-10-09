using System.Windows;

namespace CadBridge.Revit.UI;

public partial class ImportRoomsDialog : Window
{
    public ImportRoomsDialog()
    {
        InitializeComponent();

        var viewModel = new ImportRoomsViewModel(PickFile);
        viewModel.RequestClose += result => DialogResult = result;
        DataContext = viewModel;
    }

    private static string? PickFile()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "rooms.json|*.json|All files|*.*",
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}

