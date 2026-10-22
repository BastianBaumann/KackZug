using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KackZug.Core.Checking;
using KackZug.Core.Models;
using KackZug.Localization;

namespace KackZug.ViewModels;

/// <summary>Eine Fahrt (Hin, Rueck oder Einzelfahrt) in der Detailansicht: grosse Zeit, Status und Teilstrecken</summary>
public partial class TripCardViewModel : ViewModelBase
{
    private readonly MainViewModel _main;
    private readonly DateOnly _date;
    private string? _shareLink;

    public TripCardViewModel(MainViewModel main, DateOnly date, Guid ownerId, string title, TripDirection direction, SavedConnection connection, CheckState? state)
    {
        _main = main;
        _date = date;
        OwnerId = ownerId;
        Direction = direction;
        Connection = connection;
        Title = title;
        BigTime = connection.FirstRide!.PlannedDeparture.ToString("HH:mm");
        Show(null, state);
    }

    /// <summary>Id der Schicht oder der Einzelfahrt</summary>
    public Guid OwnerId { get; }

    public TripDirection Direction { get; }

    public SavedConnection Connection { get; }

    public string Title { get; }

    public string BigTime { get; }

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

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(OpenInNavigatorCommand))]
    public partial bool IsPreparingNavigator { get; set; }

    [RelayCommand(CanExecute = nameof(CanOpenInNavigator))]
    public async Task OpenInNavigatorAsync()
    {
        if (_shareLink is null)
        {
            IsPreparingNavigator = true;
            try
            {
                _shareLink = await _main.Services.ShareLinks.CreateAsync(Connection, _date, CancellationToken.None);
            }
            catch (Exception exception) when (exception is not OutOfMemoryException)
            {
                _shareLink = null;
            }
            finally
            {
                IsPreparingNavigator = false;
            }
        }

        var openedInNavigator = _main.Services.Platform.OpenInNavigator(_shareLink);
        if (!openedInNavigator && _shareLink is null)
        {
            _main.ShowToast(UiStrings.Trip_NavigatorMissingNoLink, true);
        }
        else if (!openedInNavigator)
        {
            _main.ShowToast(UiStrings.Trip_NavigatorMissing, false);
        }
        else if (_shareLink is null)
        {
            _main.ShowToast(UiStrings.Trip_NoLink, true);
        }
    }

    private bool CanOpenInNavigator()
    {
        return !IsPreparingNavigator;
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
                NowText = UiLanguage.Format(UiStrings.Trip_Now, observation.LiveDeparture.Value.ToString("HH:mm"));
            }

            foreach (var transfer in status.Transfers)
            {
                var station = status.Legs[transfer.ArrivingLegIndex].Leg.To.Name;
                var spare = Math.Max(0, transfer.SpareMinutes);
                var format = UiStrings.Trip_Transfer;
                if (transfer.AtRisk)
                {
                    format = UiStrings.Trip_TransferAtRisk;
                }

                var text = UiLanguage.Format(format, station, spare);

                Notes.Add(text);
            }

            if (status.Overall == LegStatus.PlannedOnly)
            {
                Notes.Add(UiStrings.Trip_PlannedOnlyNote);
            }

            if (status.Overall == LegStatus.Unknown || status.Overall == LegStatus.PartlyCancelled)
            {
                var reasons = status.Legs
                    .Where(result => (result.Status == LegStatus.Unknown || result.Status == LegStatus.PartlyCancelled) && !string.IsNullOrEmpty(result.Observation?.Problem))
                    .Select(result => UiLanguage.Format(UiStrings.Trip_LegReason, result.Leg.From.Name, result.Leg.To.Name, result.Observation!.Problem!))
                    .ToList();
                if (reasons.Count == 0 && status.Overall == LegStatus.Unknown)
                {
                    Notes.Add(UiStrings.Trip_UnknownNote);
                }

                foreach (var reason in reasons)
                {
                    Notes.Add(reason);
                }
            }

            CheckedText = UiStrings.Trip_JustChecked;
            return;
        }

        if (state is null)
        {
            Badge = new StatusBadge(null, "—", "");
            CheckedText = UiStrings.Trip_NotChecked;
            return;
        }

        var badgeText = StatusBadge.TextFor(state);

        Badge = new StatusBadge(state.LastStatus, badgeText, "");
        IsCancelled = state.LastStatus == LegStatus.Cancelled;
        CheckedText = UiLanguage.Format(UiStrings.Trip_LastChecked, Core.Planning.BerlinTime.ToWallClock(state.LastCheckedAt, Core.Planning.BerlinTime.Zone));
    }
}
