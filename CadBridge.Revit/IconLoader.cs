using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CadBridge.Revit;

public static class IconLoader
{
    private static readonly string Prefix = typeof(IconLoader).Namespace + ".Assets.";

    public static System.Windows.Media.ImageSource Load(string resourceName)
    {
        var assembly = typeof(IconLoader).Assembly;
        var stream = assembly.GetManifestResourceStream(Prefix + resourceName);
        if (stream is null)
            throw new InvalidOperationException($"Icon not found: {resourceName}");

        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.StreamSource = stream;
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.EndInit();
        bitmap.Freeze();
        return bitmap;
    }
}
