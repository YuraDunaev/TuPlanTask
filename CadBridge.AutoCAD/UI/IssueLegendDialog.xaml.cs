using System.Windows;
using CadBridge.AutoCAD.Drawing;

namespace CadBridge.AutoCAD.UI;

public partial class IssueLegendDialog : Window
{
    public IssueLegendDialog(IReadOnlyList<IssueLegendRow> rows)
    {
        InitializeComponent();

        ArgumentNullException.ThrowIfNull(rows);

        if (rows.Count == 0)
            EmptyText.Visibility = Visibility.Visible;
        else
            LegendGrid.ItemsSource = rows;
    }

    private void OnCloseClick(object sender, RoutedEventArgs e) => DialogResult = false;
}
