using KackZug.Core;
using KackZug.Core.Bahn;
using KackZug.Core.Checking;
using KackZug.Core.Import;
using KackZug.Core.Models;
using KackZug.Core.Planning;
using KackZug.Core.Storage;

namespace KackZug.Services;

/// <summary>Haelt Daten, Speicher und Plattform zusammen und plant nach jeder Aenderung die Pruefungen neu</summary>
public sealed class AppServices
{
    private IConnectionResolver? _resolver;
    private IStationSearch? _stationSearch;
    private BahnWebClient? _bahnClient;

    private AppServices(IPlatform platform, AppDataStore store, AppData data, IClock clock)
    {
        Platform = platform;
        Store = store;
        Data = data;
        Clock = clock;
        Theme = new ThemeService();
    }

    public event EventHandler? DataChanged;

    public IPlatform Platform { get; }

    public AppDataStore Store { get; }

    public AppData Data { get; private set; }

    public IClock Clock { get; }

    public ThemeService Theme { get; }

    public IConnectionResolver Resolver
    {
        get
        {
            if (_resolver is null)
            {
                _resolver = new BahnWebConnectionResolver(BahnClient, Clock);
            }

            return _resolver;
        }
        set { _resolver = value; }
    }

    public IStationSearch StationSearch
    {
        get
        {
            if (_stationSearch is null)
            {
                _stationSearch = new BahnWebStationSearch(BahnClient);
            }

            return _stationSearch;
        }
        set { _stationSearch = value; }
    }

    public Func<ITripStatusSource>? StatusSourceFactory { get; set; }

    public ITripStatusSource CreateStatusSource()
    {
        if (StatusSourceFactory is not null)
        {
            return StatusSourceFactory();
        }

        return new BahnWebTripStatusSource(BahnClient);
    }

    public BahnWebClient BahnClient
    {
        get
        {
            if (_bahnClient is null)
            {
                _bahnClient = new BahnWebClient(BahnWebClient.CreateHttpClient());
            }

            return _bahnClient;
        }
    }

    public static AppServices Create(IPlatform platform)
    {
        var clock = new SystemClock();
        var store = new AppDataStore(platform.DataDirectory);
        var data = store.Load();
        var services = new AppServices(platform, store, data, clock);
        services.RunMaintenance();
        return services;
    }

    public void RunMaintenance()
    {
        var today = DateOnly.FromDateTime(BerlinTime.ToWallClock(Clock.Now, BerlinTime.Zone));
        var changed = DataMaintenance.Apply(Data, today);
        if (changed)
        {
            Store.Save(Data);
        }
    }

    public void Reload()
    {
        Data = Store.Load();
        RunMaintenance();
        DataChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SaveAndReschedule()
    {
        Store.Save(Data);
        Reschedule();
        DataChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Reschedule()
    {
        var checks = CheckPlanner.Plan(Data, Clock.Now, BerlinTime.Zone);
        Platform.ScheduleChecks(checks);
    }
}
