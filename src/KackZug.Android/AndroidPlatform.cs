using Android.Content;
using KackZug.Core.Checking;
using KackZug.Core.Planning;
using KackZug.Services;

namespace KackZug.Android;

/// <summary>Plattform fuer Android: Datenordner im App-Speicher, Alarme und Meldungen folgen spaeter</summary>
public sealed class AndroidPlatform : IPlatform
{
    private readonly Context _context;

    public AndroidPlatform(Context context)
    {
        _context = context;
        DataDirectory = System.IO.Path.Combine(context.FilesDir!.AbsolutePath, "KackZug");
    }

    public string DataDirectory { get; }

    public bool IsMobile
    {
        get { return true; }
    }

    public void ScheduleChecks(IReadOnlyList<PlannedCheck> checks)
    {
    }

    public void ShowAlert(TripAlert alert)
    {
    }

    public void OpenUrl(string url)
    {
        var intent = new Intent(Intent.ActionView, global::Android.Net.Uri.Parse(url));
        intent.AddFlags(ActivityFlags.NewTask);
        _context.StartActivity(intent);
    }
}
