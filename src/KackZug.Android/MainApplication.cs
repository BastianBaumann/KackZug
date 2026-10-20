using Android.App;
using Android.Runtime;
using Avalonia.Android;

namespace KackZug.Android;

/// <summary>Android-Anwendung, setzt die Plattform und startet Avalonia mit der gemeinsamen App</summary>
[Application(Label = "KackZug", Icon = "@mipmap/icon")]
public class MainApplication : AvaloniaAndroidApplication<App>
{
    protected MainApplication(nint javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
    {
    }

    public override void OnCreate()
    {
        App.Platform = new AndroidPlatform(this);
        base.OnCreate();
    }
}
