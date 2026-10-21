using Android.Runtime;
using Android.Window;

namespace KackZug.Android;

/// <summary>Faengt "Zurueck" ab Android 13 ab, damit der Alarm nur mit "Gesehen" verschwindet</summary>
public sealed class BlockedBackCallback : Java.Lang.Object, IOnBackInvokedCallback
{
    public void OnBackInvoked()
    {
    }
}
