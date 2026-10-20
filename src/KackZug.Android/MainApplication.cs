using Android.App;
using Android.Runtime;
using Avalonia.Android;

namespace KackZug.Android;

/// <summary>Android-Anwendung, startet Avalonia mit der gemeinsamen App</summary>
[Application(Label = "KackZug", Icon = "@drawable/icon")]
public class MainApplication : AvaloniaAndroidApplication<App>
{
    protected MainApplication(nint javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
    {
    }
}
