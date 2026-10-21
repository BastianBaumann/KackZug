using Android.App;
using Android.Content;
using KackZug.Core;
using KackZug.Core.Bahn;
using KackZug.Core.Checking;
using KackZug.Core.Localization;
using KackZug.Core.Storage;

namespace KackZug.Android;

/// <summary>Wird vom Alarm geweckt, prueft die Fahrt im Hintergrund und meldet Probleme, auch bei geschlossener App</summary>
[BroadcastReceiver(Enabled = true, Exported = false)]
public sealed class CheckAlarmReceiver : BroadcastReceiver
{
    private static readonly TimeSpan HardTimeout = TimeSpan.FromSeconds(9);

    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context is null || intent is null)
        {
            return;
        }

        var check = CheckIntent.Read(intent, out var isRetry);
        if (check is null)
        {
            return;
        }

        var pending = GoAsync();
        var appContext = context.ApplicationContext!;
        Task.Run(async () =>
        {
            try
            {
                await RunAsync(appContext, check, isRetry);
            }
            finally
            {
                pending?.Finish();
            }
        });
    }

    private static async Task RunAsync(Context context, Core.Planning.PlannedCheck check, bool isRetry)
    {
        var store = new AppDataStore(AndroidPaths.DataDirectory(context));
        using var timeout = new CancellationTokenSource(HardTimeout);
        try
        {
            var client = new BahnWebClient(BahnWebClient.CreateHttpClient()) { MinimumGap = TimeSpan.FromMilliseconds(300) };
            var source = new BahnWebTripStatusSource(client);
            var backgroundCheck = new BackgroundCheck(store, source, new SystemClock());
            var result = await backgroundCheck.RunAsync(check, isRetry, timeout.Token);

            if (result.AlertToShow is not null)
            {
                var settings = store.Load().Settings;
                Language.Apply(settings.Language);
                AlertNotifier.Show(context, result.AlertToShow, settings.InsistentAlerts);
            }

            AlarmScheduler.ScheduleAll(context, result.NextChecks);
            if (result.RetrySoon)
            {
                AlarmScheduler.ScheduleRetry(context, check);
            }
        }
        catch (OperationCanceledException)
        {
            if (!isRetry)
            {
                AlarmScheduler.ScheduleRetry(context, check);
            }
        }
        catch (Exception exception)
        {
            global::Android.Util.Log.Error("KackZug", "Pruefung fehlgeschlagen: " + exception);
            AlarmScheduler.ScheduleFromFile(context);
        }

        App.ReloadFromBackground();
    }
}
