#if DEBUG
using System.Net;
using System.Text;

namespace KackZug.Desktop;

/// <summary>Erfundene GitHub-Antwort fuer die Screenshot-Tour; ohne Tag antwortet sie wie ein privates Projekt mit 404</summary>
internal sealed class DemoUpdateHandler : HttpMessageHandler
{
    private readonly string? _tag;

    public DemoUpdateHandler(string? tag)
    {
        _tag = tag;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (_tag is null)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent("{\"message\":\"Not Found\"}") });
        }

        var body = "{\"tag_name\":\"" + _tag + "\",\"html_url\":\"https://github.com/BastianBaumann/KackZug/releases/tag/" + _tag + "\"}";
        var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(body, Encoding.UTF8, "application/json") };
        return Task.FromResult(response);
    }
}
#endif
