using Android.Content;
using KackZug.Core.Models;
using KackZug.Core.Planning;

namespace KackZug.Android;

/// <summary>Packt eine geplante Pruefung in einen Intent und wieder heraus</summary>
public static class CheckIntent
{
    public const string Action = "io.github.bastianbaumann.kackzug.CHECK";

    private const string ExtraKind = "kind";
    private const string ExtraDate = "date";
    private const string ExtraShift = "shift";
    private const string ExtraDirection = "direction";
    private const string ExtraLead = "lead";
    private const string ExtraRetry = "retry";

    public static Intent Create(Context context, PlannedCheck check, bool isRetry)
    {
        var intent = new Intent(context, typeof(CheckAlarmReceiver));
        intent.SetAction(Action);
        intent.PutExtra(ExtraKind, check.Kind.ToString());
        intent.PutExtra(ExtraDate, check.TravelDate.ToString("yyyy-MM-dd"));
        intent.PutExtra(ExtraShift, check.ShiftId.ToString());
        intent.PutExtra(ExtraDirection, check.Direction.ToString());
        intent.PutExtra(ExtraLead, check.LeadMinutes);
        intent.PutExtra(ExtraRetry, isRetry);
        return intent;
    }

    public static PlannedCheck? Read(Intent intent, out bool isRetry)
    {
        isRetry = intent.GetBooleanExtra(ExtraRetry, false);
        var kindText = intent.GetStringExtra(ExtraKind);
        var dateText = intent.GetStringExtra(ExtraDate);
        var shiftText = intent.GetStringExtra(ExtraShift);
        var directionText = intent.GetStringExtra(ExtraDirection);
        if (!Enum.TryParse<CheckKind>(kindText, out var kind) || !DateOnly.TryParse(dateText, out var date))
        {
            return null;
        }

        Guid.TryParse(shiftText, out var shiftId);
        Enum.TryParse<TripDirection>(directionText, out var direction);
        var check = new PlannedCheck
        {
            Kind = kind,
            TravelDate = date,
            ShiftId = shiftId,
            Direction = direction,
            LeadMinutes = intent.GetIntExtra(ExtraLead, 0)
        };
        check.RequestCode = StableHash.Of(check.Key);
        return check;
    }
}
