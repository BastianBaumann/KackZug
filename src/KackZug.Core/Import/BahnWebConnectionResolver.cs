using KackZug.Core.Bahn;
using KackZug.Core.Localization;

namespace KackZug.Core.Import;

/// <summary>Loest einen geteilten Link ueber bahn.de auf, notfalls nur aus dem Verbindungsschluessel</summary>
public sealed class BahnWebConnectionResolver : IConnectionResolver
{
    private readonly BahnWebClient _client;
    private readonly IClock _clock;

    public BahnWebConnectionResolver(BahnWebClient client, IClock clock)
    {
        _client = client;
        _clock = clock;
    }

    public async Task<ResolvedConnection> ResolveAsync(string sharedText, CancellationToken cancellationToken)
    {
        var vbid = ShareLinkParser.FindVbid(sharedText);
        if (vbid is null)
        {
            throw new ImportException(Strings.Import_ErrorNoLink);
        }

        string recon;
        try
        {
            recon = await _client.GetReconAsync(vbid, cancellationToken);
        }
        catch (BahnApiException exception)
        {
            throw new ImportException(exception.UserMessage, exception);
        }

        try
        {
            using var document = await _client.PostReconAsync(recon, cancellationToken);
            var connection = ReconResponseMapper.Map(document.RootElement, _clock.Now);
            return connection;
        }
        catch (BahnApiException)
        {
            return ReconStringParser.Parse(recon, _clock.Now);
        }
        catch (FormatException)
        {
            return ReconStringParser.Parse(recon, _clock.Now);
        }
    }
}
