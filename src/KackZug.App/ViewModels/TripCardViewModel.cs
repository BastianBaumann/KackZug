using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KackZug.Core.Bahn;
using KackZug.Core.Checking;
using KackZug.Core.Models;
using KackZug.Services;

namespace KackZug.ViewModels;

/// <summary>Eine Fahrt (Hin oder Rueck) in der Detailansicht: grosse Zeit, Status und Teilstrecken</summary>
public partial class TripCardViewModel : ViewModelBase
{
    private readonly IPlatform _platform;

    public TripCardViewModel(IPlatform platform, DateOnly date, Shift shift, TripDirection direction, SavedConnection connection, CheckState? state)
    {
        _platform = platform;
        NavigatorUrl = NavigatorLink.ForTrip(connection, date);
        Shift = shift;
        Direction = direction;
        Connection = connection;
        Title = AlertComposer.DirectionName(direction);
        BigTime = connection.FirstRide!.PlannedDeparture.ToString("HH:mm");
        Show(null, state);
    }

    public Shift Shift { get; }

    public TripDirection Direction { get; }

    public SavedConnection Connection { get; }

    public string Title { get; }

    public string BigTime { get; }

    public string? NavigatorUrl { get; }

    public ObservableCollection<LegDetailLine> Legs { get; } = new ObservableCollection<LegDetailLine>();

    public ObservableCollection<string> Notes { get; } = new ObservableCollection<string>();

    [ObservableProperty]
    public partial StatusBadge? Badge { get; set; }

    [ObservableProperty]
    public partial string NowText { get; set; } = "";

    [ObservableProperty]
    public partial bool IsCancelled { get; set; }

    [ObservableProperty]
    public partial string CheckedText { get; set; } = "";

    [RelayCommand]
    private void OpenInNavigator()
    {
        if (NavigatorUrl is not null)
        {
            _platform.OpenInNavigator(NavigatorUrl);
        }
    }

    public void Show(TripStatus? status, CheckState? state)
    {
        Legs.Clear();
        Notes.Clear();
        for (var index = 0; index < Connection.Legs.Count; index++)
        {
            LegResult? result = null;
            if (status is not null && index < status.Legs.Count)
            {
                result = status.Legs[index];
            }

            Legs.Add(new LegDetailLine(Connection.Legs[index], result));
        }

        NowText = "";
        IsCancelled = false;
        if (status is not null)
        {
            Badge = new StatusBadge(status.Overall, AlertComposer.BadgeText(status), "");
            IsCancelled = status.Overall == LegStatus.Cancelled;
            var firstRide = status.Legs.FirstOrDefault(result => !result.Leg.IsWalk);
            var observation = firstRide?.Observation;
            if (observation?.LiveDeparture is not null && firstRide!.DelayMinutes > 0)
            {
                NowText = "jetzt " + observation.LiveDeparture.Value.ToString("HH:mm");
            }

            foreach (var transfer in status.Transfers)
            {
                var station = status.Legs[transfer.ArrivingLegIndex].Leg.To.Name;
                var spare = Math.Max(0, transfer.SpareMinutes);
                var text = "Umstieg in " + station + ": " + spare + " Min. Puffer";
                if (transfer.AtRisk)
                {
                    text += " – Anschluss gefährdet";
                }

                Notes.Add(text);
            }

            if (status.Overall == LegStatus.PlannedOnly)
            {
                Notes.Add("Echtzeitdaten gibt es erst kurz vor der Abfahrt.");
            }

            if (status.Overall == LegStatus.Unknown)
            {
                Notes.Add("bahn.de war gerade nicht erreichbar oder hatte noch keine Daten.");
            }

            CheckedText = "Gerade geprüft";
            return;
        }

        if (state is null)
        {
            Badge = new StatusBadge(null, "—", "");
            CheckedText = "Noch nicht geprüft";
            return;
        }

        var badgeText = state.LastBadgeText;
        if (string.IsNullOrEmpty(badgeText))
        {
            badgeText = StatusBadge.ShortText(state.LastStatus);
        }

        Badge = new StatusBadge(state.LastStatus, badgeText, "");
        IsCancelled = state.LastStatus == LegStatus.Cancelled;
        CheckedText = "Zuletzt geprüft " + Core.Planning.BerlinTime.ToWallClock(state.LastCheckedAt, Core.Planning.BerlinTime.Zone).ToString("dd.MM. HH:mm");
    }
}
