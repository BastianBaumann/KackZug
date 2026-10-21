using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KackZug.Core.Bahn;
using KackZug.Core.Import;
using KackZug.Core.Models;

namespace KackZug.ViewModels;

/// <summary>Feld fuer einen Bahnhof: Namen tippen, suchen, Treffer antippen</summary>
/// <remarks>Gesucht wird erst auf Knopfdruck, damit bahn.de nicht bei jedem Buchstaben gefragt wird.</remarks>
public partial class StationPickerViewModel : ViewModelBase
{
    private readonly Func<IStationSearch> _search;

    public StationPickerViewModel(string label, Func<IStationSearch> search)
    {
        Label = label;
        _search = search;
    }

    public string Label { get; }

    public ObservableCollection<StationResult> Results { get; } = new ObservableCollection<StationResult>();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SearchCommand))]
    public partial string Query { get; set; } = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelection))]
    [NotifyPropertyChangedFor(nameof(IsSearchVisible))]
    public partial Station? Selected { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SearchCommand))]
    public partial bool IsSearching { get; set; }

    [ObservableProperty]
    public partial string? Message { get; set; }

    public bool HasSelection
    {
        get { return Selected is not null; }
    }

    public bool IsSearchVisible
    {
        get { return Selected is null; }
    }

    public void Choose(Station station)
    {
        Selected = station;
        Query = station.Name;
        Results.Clear();
        Message = null;
    }

    [RelayCommand(CanExecute = nameof(CanSearch))]
    public async Task SearchAsync()
    {
        IsSearching = true;
        Message = null;
        Results.Clear();
        try
        {
            var stations = await _search().SearchAsync(Query.Trim(), CancellationToken.None);
            foreach (var station in stations)
            {
                Results.Add(new StationResult(station, Choose));
            }

            if (Results.Count == 0)
            {
                Message = "Nichts gefunden. Anders schreiben?";
            }
        }
        catch (BahnApiException exception) when (exception.Kind == BahnApiErrorKind.NotFound)
        {
            Message = "Nichts gefunden. Anders schreiben?";
        }
        catch (BahnApiException exception)
        {
            Message = exception.UserMessage;
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            Message = "Suche ging gerade nicht: " + exception.Message;
        }
        finally
        {
            IsSearching = false;
        }
    }

    [RelayCommand]
    private void Change()
    {
        Selected = null;
    }

    private bool CanSearch()
    {
        return !IsSearching && Query.Trim().Length >= 2;
    }
}
