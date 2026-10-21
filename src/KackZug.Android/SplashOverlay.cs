using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace KackZug.Android;

/// <summary>Logo auf Hintergrundfarbe ueber der Oberflaeche, bis diese ihr erstes Bild gezeichnet hat; sieht aus wie der Startbildschirm des Systems</summary>
/// <remarks>Haengt am DecorView, weil Avalonia seine Ansicht erst spaeter als Inhalt setzt und einen Inhalt ersetzen wuerde.
/// Avalonia meldet sein erstes Bild zu frueh, deshalb schaut die Ebene selbst nach, ob die Zeichenflaeche nicht mehr schwarz ist.</remarks>
public sealed class SplashOverlay : Java.Lang.Object, PixelCopy.IOnPixelCopyFinishedListener
{
    private const int IconSizeDp = 160;
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMilliseconds(150);
    private static readonly TimeSpan MaximumWait = TimeSpan.FromSeconds(20);
    private static readonly TimeSpan FadeDuration = TimeSpan.FromMilliseconds(220);

    private readonly Activity _activity;
    private readonly FrameLayout _layer;
    private readonly Handler _handler = new Handler(Looper.MainLooper!);
    private readonly DateTime _shownAt = DateTime.UtcNow;
    private Bitmap? _sample;
    private bool _hiding;

    private SplashOverlay(Activity activity, FrameLayout layer)
    {
        _activity = activity;
        _layer = layer;
    }

    public static SplashOverlay Show(Activity activity)
    {
        var layer = new FrameLayout(activity);
        layer.SetBackgroundResource(Resource.Color.splash_background);
        layer.Clickable = true;

        var icon = new ImageView(activity);
        icon.SetImageResource(Resource.Mipmap.icon);
        var size = (int)(IconSizeDp * activity.Resources!.DisplayMetrics!.Density);
        var iconParams = new FrameLayout.LayoutParams(size, size) { Gravity = GravityFlags.Center };
        layer.AddView(icon, iconParams);

        var decor = activity.Window?.DecorView as ViewGroup;
        decor?.AddView(layer, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));
        var overlay = new SplashOverlay(activity, layer);
        overlay.ScheduleCheck();
        return overlay;
    }

    private void ScheduleCheck()
    {
        _handler.PostDelayed(Check, (long)CheckInterval.TotalMilliseconds);
    }

    private void Check()
    {
        if (_hiding || _activity.IsFinishing)
        {
            return;
        }

        if (DateTime.UtcNow - _shownAt > MaximumWait)
        {
            Hide();
            return;
        }

        var surface = FindSurface(_activity.Window?.DecorView);
        if (surface is null || surface.Holder?.Surface is null || !surface.Holder.Surface.IsValid || surface.Width == 0 || surface.Height == 0)
        {
            ScheduleCheck();
            return;
        }

        _sample ??= Bitmap.CreateBitmap(8, 8, Bitmap.Config.Argb8888!)!;
        try
        {
            PixelCopy.Request(surface, _sample, this, _handler);
        }
        catch (Java.Lang.IllegalArgumentException)
        {
            ScheduleCheck();
        }
    }

    public void OnPixelCopyFinished(int copyResult)
    {
        if (copyResult == (int)PixelCopyResult.Success && HasContent(_sample!))
        {
            Hide();
            return;
        }

        ScheduleCheck();
    }

    /// <remarks>Vor dem ersten Bild ist die Zeichenflaeche reines Schwarz; die App selbst hat auch dunkel nie reines Schwarz.</remarks>
    private static bool HasContent(Bitmap sample)
    {
        for (var x = 0; x < sample.Width; x++)
        {
            for (var y = 0; y < sample.Height; y++)
            {
                if ((sample.GetPixel(x, y) & 0x00FFFFFF) != 0)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void Hide()
    {
        if (_hiding)
        {
            return;
        }

        _hiding = true;
        _layer.Animate()?.Alpha(0f).SetDuration((long)FadeDuration.TotalMilliseconds).WithEndAction(new Java.Lang.Runnable(Remove))?.Start();
    }

    private void Remove()
    {
        var parent = _layer.Parent as ViewGroup;
        parent?.RemoveView(_layer);
        _sample?.Recycle();
        _sample = null;
    }

    private static SurfaceView? FindSurface(View? view)
    {
        if (view is SurfaceView surface)
        {
            return surface;
        }

        if (view is not ViewGroup group)
        {
            return null;
        }

        for (var index = 0; index < group.ChildCount; index++)
        {
            var found = FindSurface(group.GetChildAt(index));
            if (found is not null)
            {
                return found;
            }
        }

        return null;
    }
}
