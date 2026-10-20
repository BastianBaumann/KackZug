using System.Net;
using System.Text;

namespace KackZug.Core.Tests;

/// <summary>Beantwortet HTTP-Anfragen aus vorbereiteten Antworten und merkt sich die Anfragen</summary>
public sealed class FakeHttpHandler : HttpMessageHandler
{
    private readonly List<(string PathStart, HttpStatusCode Status, string Body)> _answers = new();

    public List<string> RequestedPaths { get; } = new List<string>();

    public List<string> RequestBodies { get; } = new List<string>();

    public void Answer(string pathStart, HttpStatusCode status, string body)
    {
        _answers.Add((pathStart, status, body));
    }

    public void AnswerWithFixture(string pathStart, string fixtureName)
    {
        Answer(pathStart, HttpStatusCode.OK, Fixtures.Read(fixtureName));
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var path = request.RequestUri!.PathAndQuery;
        RequestedPaths.Add(path);
        if (request.Content is not null)
        {
            RequestBodies.Add(await request.Content.ReadAsStringAsync(cancellationToken));
        }

        foreach (var answer in _answers)
        {
            if (path.StartsWith("/web/api/" + answer.PathStart, StringComparison.Ordinal))
            {
                return new HttpResponseMessage(answer.Status)
                {
                    Content = new StringContent(answer.Body, Encoding.UTF8, "application/json")
                };
            }
        }

        throw new HttpRequestException("Keine Antwort vorbereitet fuer " + path);
    }

    public Bahn.BahnWebClient CreateBahnClient()
    {
        return new Bahn.BahnWebClient(CreateClient()) { MinimumGap = TimeSpan.Zero };
    }

    public HttpClient CreateClient()
    {
        return new HttpClient(this) { BaseAddress = new Uri(Bahn.BahnWebClient.BaseAddress) };
    }
}
