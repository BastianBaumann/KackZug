#if DEBUG
using KackZug.Core.Models;
using KackZug.Core.Sharing;

namespace KackZug.Desktop;

/// <summary>Erfundener Teilen-Link fuer die Screenshot-Tour; ohne Link verhaelt er sich wie eine nicht gefundene Verbindung</summary>
internal sealed class DemoShareLinks : IShareLinkCreator
{
    public string? Link { get; set; }

    public List<DateOnly> RequestedDays { get; } = new List<DateOnly>();

    public Task<string?> CreateAsync(SavedConnection connection, DateOnly travelDate, CancellationToken cancellationToken)
    {
        RequestedDays.Add(travelDate);
        return Task.FromResult(Link);
    }
}
#endif
