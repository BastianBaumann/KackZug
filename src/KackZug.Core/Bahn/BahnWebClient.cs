using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace KackZug.Core.Bahn;

/// <summary>Spricht mit der nicht offiziell dokumentierten Web-Schnittstelle von bahn.de</summary>
/// <remarks>bahn.de lehnt eigene User-Agents mit 403 ab, deshalb meldet sich die App wie ein mobiler Browser.</remarks>
public sealed class BahnWebClient
{
    public const string BaseAddress = "https://www.bahn.de/web/api/";
    public const string BrowserUserAgent = "Mozilla/5.0 (Linux; Android 14) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/129.0 Mobile Safari/537.36";

    private readonly HttpClient _httpClient;

    public BahnWebClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public static HttpClient CreateHttpClient()
    {
        var handler = new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.All
        };
        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri(BaseAddress),
            Timeout = TimeSpan.FromSeconds(15)
        };
        return client;
    }

    public async Task<string> GetReconAsync(string vbid, CancellationToken cancellationToken)
    {
        using var document = await SendAsync(HttpMethod.Get, "angebote/verbindung/" + vbid, null, cancellationToken);
        var recon = BahnJson.GetString(document.RootElement, "hinfahrtRecon");
        if (string.IsNullOrEmpty(recon))
        {
            throw new BahnApiException(BahnApiErrorKind.Format, "hinfahrtRecon fehlt.");
        }

        return recon;
    }

    public Task<JsonDocument> PostReconAsync(string recon, CancellationToken cancellationToken)
    {
        var body = CreateReconBody(recon);
        return SendAsync(HttpMethod.Post, "angebote/recon", body, cancellationToken);
    }

    public Task<JsonDocument> GetDeparturesAsync(string evaNumber, DateTime time, CancellationToken cancellationToken)
    {
        var date = time.ToString("yyyy-MM-dd");
        var clock = time.ToString("HH:mm:ss");
        var path = "reiseloesung/abfahrten?ortExtId=" + Uri.EscapeDataString(evaNumber)
            + "&datum=" + date
            + "&zeit=" + Uri.EscapeDataString(clock);
        return SendAsync(HttpMethod.Get, path, null, cancellationToken);
    }

    public Task<JsonDocument> GetTripAsync(string journeyId, CancellationToken cancellationToken)
    {
        var path = "reiseloesung/fahrt?journeyId=" + Uri.EscapeDataString(journeyId);
        return SendAsync(HttpMethod.Get, path, null, cancellationToken);
    }

    private async Task<JsonDocument> SendAsync(HttpMethod method, string path, string? jsonBody, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, path);
        request.Headers.TryAddWithoutValidation("User-Agent", BrowserUserAgent);
        request.Headers.TryAddWithoutValidation("Accept", "application/json");
        request.Headers.TryAddWithoutValidation("Accept-Language", "de-DE");
        if (jsonBody is not null)
        {
            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
        }

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(request, cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            throw new BahnApiException(BahnApiErrorKind.Network, "Netzwerkfehler.", exception);
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            throw new BahnApiException(BahnApiErrorKind.Network, "Zeitueberschreitung.", exception);
        }

        using (response)
        {
            var text = await response.Content.ReadAsStringAsync(cancellationToken);
            ThrowOnError(response.StatusCode, text);
            try
            {
                return JsonDocument.Parse(text);
            }
            catch (JsonException exception)
            {
                throw new BahnApiException(BahnApiErrorKind.Format, "Antwort ist kein JSON.", exception);
            }
        }
    }

    private static void ThrowOnError(HttpStatusCode statusCode, string text)
    {
        if (text.Contains("OPS_BLOCKED", StringComparison.Ordinal))
        {
            throw new BahnApiException(BahnApiErrorKind.Blocked, "bahn.de hat die Anfrage gesperrt.");
        }

        var code = (int)statusCode;
        if (code == 403 || code == 429)
        {
            throw new BahnApiException(BahnApiErrorKind.Blocked, "bahn.de hat die Anfrage abgelehnt (" + code + ").");
        }

        if (code == 404 || code == 422 || code == 500)
        {
            throw new BahnApiException(BahnApiErrorKind.NotFound, "bahn.de kennt die Anfrage nicht (" + code + ").");
        }

        if (code < 200 || code > 299)
        {
            throw new BahnApiException(BahnApiErrorKind.Network, "bahn.de antwortet mit Fehler " + code + ".");
        }
    }

    private static string CreateReconBody(string recon)
    {
        var discount = new JsonObject
        {
            ["art"] = "KEINE_ERMAESSIGUNG",
            ["klasse"] = "KLASSENLOS"
        };
        var traveller = new JsonObject
        {
            ["typ"] = "ERWACHSENER",
            ["ermaessigungen"] = new JsonArray(discount),
            ["alter"] = new JsonArray(),
            ["anzahl"] = 1
        };
        var body = new JsonObject
        {
            ["klasse"] = "KLASSE_2",
            ["reisende"] = new JsonArray(traveller),
            ["ctxRecon"] = recon,
            ["deutschlandTicketVorhanden"] = false,
            ["nurDeutschlandTicketVerbindungen"] = false,
            ["reservierungsKontingenteVorhanden"] = false
        };
        return body.ToJsonString();
    }
}
