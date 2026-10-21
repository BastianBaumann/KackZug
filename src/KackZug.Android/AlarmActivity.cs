using Android.Animation;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Window;
using KackZug.Core.Alarms;
using KackZug.Core.Localization;

namespace KackZug.Android;

/// <summary>Vollbild-Alarm ueber dem Sperrbildschirm, geht nur mit "Gesehen" weg (Aussehen laut Design System: AlarmScreen)</summary>
[Activity(
    Label = "KackZug Alarm",
    Theme = "@android:style/Theme.DeviceDefault.NoActionBar",
    Exported = false,
    LaunchMode = LaunchMode.SingleInstance,
    ExcludeFromRecents = true,
    TaskAffinity = "io.github.bastianbaumann.kackzug.alarm",
    ScreenOrientation = ScreenOrientation.Portrait)]
public class AlarmActivity : Activity
{
    private const int GroundColor = unchecked((int)0xFF0B0D10);
    private const int BlockColor = unchecked((int)0xFFD1001A);
    private const int MutedColor = unchecked((int)0xFFB9C0C8);

    private static AlarmActivity? _current;

    private int _notificationId;
    private bool _closing;
    private BlockedBackCallback? _backCallback;
    private ObjectAnimator? _pulse;

    public static void CloseIfShowing(int notificationId)
    {
        var current = _current;
        if (current is not null && current._notificationId == notificationId)
        {
            current._closing = true;
            current.RunOnUiThread(() => current.Finish());
        }
    }

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        BlockBack();
        ShowOverLockScreen();
        Show(Intent);
    }

    protected override void OnNewIntent(Intent? intent)
    {
        base.OnNewIntent(intent);
        Show(intent);
    }

    protected override void OnDestroy()
    {
        if (_current == this)
        {
            _current = null;
        }

        _pulse?.Cancel();
        base.OnDestroy();
    }

    /// <remarks>Wird der Alarm ohne "Gesehen" verlassen (Startknopf, App-Wechsel), kommt er mit Ton und Vollbild zurueck.
    /// Bei ausgeschaltetem Bildschirm nicht, sonst liesse er sich nie ausschalten; dann holt ihn die Wiederholung zurueck.</remarks>
    protected override void OnStop()
    {
        base.OnStop();
        if (_closing || IsFinishing || _notificationId == 0)
        {
            return;
        }

        var power = (PowerManager?)GetSystemService(PowerService);
        if (power is null || !power.IsInteractive)
        {
            return;
        }

        AlarmFlow.Repost(this, _notificationId);
    }

    public override void OnBackPressed()
    {
    }

    private void BlockBack()
    {
        if (!OperatingSystem.IsAndroidVersionAtLeast(33))
        {
            return;
        }

        _backCallback = new BlockedBackCallback();
        OnBackInvokedDispatcher.RegisterOnBackInvokedCallback(IOnBackInvokedDispatcher.PriorityOverlay, _backCallback);
    }

    public override bool OnKeyDown(Keycode keyCode, KeyEvent? e)
    {
        if (keyCode == Keycode.Back)
        {
            return true;
        }

        return base.OnKeyDown(keyCode, e);
    }

    private void ShowOverLockScreen()
    {
        if (OperatingSystem.IsAndroidVersionAtLeast(27))
        {
            SetShowWhenLocked(true);
            SetTurnScreenOn(true);
        }
        else
        {
            Window?.AddFlags(WindowManagerFlags.ShowWhenLocked | WindowManagerFlags.TurnScreenOn);
        }

        Window?.AddFlags(WindowManagerFlags.KeepScreenOn);
        Window?.SetStatusBarColor(new Color(GroundColor));
        Window?.SetNavigationBarColor(new Color(GroundColor));
    }

    private void Show(Intent? intent)
    {
        _notificationId = 0;
        if (intent is not null)
        {
            _notificationId = intent.GetIntExtra(AlarmFlow.ExtraNotificationId, 0);
        }

        var alarm = AlarmFlow.Store(this).Find(_notificationId);
        if (alarm is null)
        {
            Finish();
            return;
        }

        _current = this;
        AlarmFlow.ApplyLanguage(this);
        var frame = new FrameLayout(this);
        frame.SetBackgroundColor(new Color(GroundColor));
        frame.SetFitsSystemWindows(true);
        frame.AddView(BuildLayout(alarm));
        SetContentView(frame);
    }

    private View BuildLayout(PendingAlarm alarm)
    {
        var root = new LinearLayout(this) { Orientation = Orientation.Vertical };
        root.SetBackgroundColor(new Color(GroundColor));
        root.SetPadding(Dp(24), Dp(48), Dp(24), Dp(32));

        var kicker = Text(alarm.Kicker, 15, MutedColor, TypefaceStyle.Bold);
        kicker.LetterSpacing = 0.06f;
        root.AddView(kicker);

        var block = new LinearLayout(this) { Orientation = Orientation.Vertical };
        var blockShape = new GradientDrawable();
        blockShape.SetColor(BlockColor);
        blockShape.SetCornerRadius(Dp(16));
        block.Background = blockShape;
        block.SetPadding(Dp(24), Dp(24), Dp(24), Dp(24));
        block.AddView(Text(alarm.Headline, 36, unchecked((int)0xFFFFFFFF), TypefaceStyle.Bold));
        block.AddView(Text(alarm.TripLine, 20, unchecked((int)0xFFFFFFFF), TypefaceStyle.Bold));
        var blockParams = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
        blockParams.TopMargin = Dp(24);
        root.AddView(block, blockParams);
        StartPulse(block);

        var detail = Text(alarm.Text, 17, MutedColor, TypefaceStyle.Normal);
        var detailParams = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, 0, 1f);
        detailParams.TopMargin = Dp(24);
        var scroll = new ScrollView(this);
        scroll.AddView(detail);
        root.AddView(scroll, detailParams);

        var seen = new Button(this) { Text = Strings.Android_ActionSeen };
        seen.SetAllCaps(false);
        seen.SetTextSize(ComplexUnitType.Sp, 20);
        seen.SetTypeface(Typeface.DefaultBold, TypefaceStyle.Bold);
        seen.SetTextColor(new Color(GroundColor));
        seen.Background = Rounded(unchecked((int)0xFFFFFFFF), 0);
        seen.Click += (_, _) =>
        {
            AlarmFlow.Acknowledge(this, _notificationId);
            Finish();
        };
        var seenParams = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, Dp(72));
        seenParams.TopMargin = Dp(24);
        root.AddView(seen, seenParams);

        var navigator = new Button(this) { Text = Strings.Android_ActionOpenNavigator };
        navigator.SetAllCaps(false);
        navigator.SetTextSize(ComplexUnitType.Sp, 17);
        navigator.SetTextColor(new Color(unchecked((int)0xFFFFFFFF)));
        navigator.Background = Rounded(GroundColor, MutedColor);
        navigator.Click += (_, _) =>
        {
            AlarmFlow.Acknowledge(this, _notificationId);
            AlarmFlow.OpenNavigator(this);
            Finish();
        };
        var navigatorParams = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, Dp(52));
        navigatorParams.TopMargin = Dp(12);
        root.AddView(navigator, navigatorParams);

        return root;
    }

    private void StartPulse(View block)
    {
        var animationScale = global::Android.Provider.Settings.Global.GetFloat(ContentResolver, global::Android.Provider.Settings.Global.AnimatorDurationScale, 1f);
        if (animationScale == 0f)
        {
            return;
        }

        _pulse = ObjectAnimator.OfFloat(block, "alpha", 1f, 0.82f);
        _pulse!.SetDuration(800);
        _pulse.RepeatMode = ValueAnimatorRepeatMode.Reverse;
        _pulse.RepeatCount = ValueAnimator.Infinite;
        _pulse.Start();
    }

    private TextView Text(string text, float sizeSp, int color, TypefaceStyle style)
    {
        var view = new TextView(this) { Text = text };
        view.SetTextSize(ComplexUnitType.Sp, sizeSp);
        view.SetTextColor(new Color(color));
        view.SetTypeface(Typeface.Default, style);
        return view;
    }

    private GradientDrawable Rounded(int fill, int stroke)
    {
        var shape = new GradientDrawable();
        shape.SetColor(fill);
        shape.SetCornerRadius(Dp(8));
        if (stroke != 0)
        {
            shape.SetStroke(Dp(2), new Color(stroke));
        }

        return shape;
    }

    private int Dp(int value)
    {
        return (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, value, Resources!.DisplayMetrics);
    }
}
