using Android.Content;

namespace KackZug.Android;

/// <summary>Datenordner der App auf dem Handy, auch fuer Receiver ohne laufende Oberflaeche</summary>
public static class AndroidPaths
{
    public static string DataDirectory(Context context)
    {
        return System.IO.Path.Combine(context.FilesDir!.AbsolutePath, "KackZug");
    }
}
