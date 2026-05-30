using System.Net;
using System.Text;

namespace Services.Tests.TestHelpers;

/// <summary>
/// A fake <see cref="HttpMessageHandler"/> that returns a canned response and records the last
/// request (and its body) so tests can assert on the payload the service sent. Used to drive
/// <c>PredictionService</c> without a real Python prediction endpoint.
/// </summary>
public sealed class StubHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _statusCode;
    private readonly string _responseBody;

    public HttpRequestMessage? LastRequest { get; private set; }
    public string? LastRequestBody { get; private set; }
    public int CallCount { get; private set; }

    public StubHttpMessageHandler(HttpStatusCode statusCode, string responseBody)
    {
        _statusCode = statusCode;
        _responseBody = responseBody;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        CallCount++;
        LastRequest = request;
        if (request.Content is not null)
        {
            LastRequestBody = await request.Content.ReadAsStringAsync(cancellationToken);
        }

        return new HttpResponseMessage(_statusCode)
        {
            Content = new StringContent(_responseBody, Encoding.UTF8, "application/json")
        };
    }
}
