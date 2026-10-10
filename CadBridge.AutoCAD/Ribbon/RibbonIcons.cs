using System;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace CadBridge.AutoCAD.Ribbon;

internal static class RibbonIcons
{
    public static BitmapImage Load(string resourceName, int pixelSize)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var image = new BitmapImage();
        image.BeginInit();
        image.StreamSource = assembly.GetManifestResourceStream(resourceName);
        image.DecodePixelHeight = pixelSize;
        image.DecodePixelWidth = pixelSize;
        image.EndInit();
        image.Freeze();
        return image;
    }
}
