using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using R3Polska.Sse.Mercure.BddTests.Support;
using Reqnroll;
using RichardSzalay.MockHttp;
using Shouldly;

namespace R3Polska.Sse.Mercure.BddTests.StepDefinitions;

[Binding]
public class ErrorHandlingSteps
{
    private readonly TestContext _context;
    private CancellationTokenSource? _cts;

    public ErrorHandlingSteps(TestContext context)
    {
        _context = context;
    }

    [Given(@"the Mercure hub will respond with status (\d+) and message ""(.*)""")]
    public void GivenTheMercureHubWillRespondWithStatusAndMessage(int statusCode, string errorMessage)
    {
        _context.MockStatusCode = (HttpStatusCode)statusCode;
        _context.MockErrorMessage = errorMessage;
    }

    [Given(@"the Mercure hub is unreachable")]
    public void GivenTheMercureHubIsUnreachable()
    {
        _context.SimulateNetworkError = true;
    }

    [Given(@"the request will be cancelled")]
    public void GivenTheRequestWillBeCancelled()
    {
        _context.SimulateCancellation = true;
    }

    [When(@"I attempt to publish the message")]
    public async Task WhenIAttemptToPublishTheMessage()
    {
        if (_context.SimulateNetworkError)
        {
            _context.MockHttp.Expect(HttpMethod.Post, $"{_context.Host}/.well-known/mercure")
                .Throw(new HttpRequestException("Network error"));
        }
        else if (_context.SimulateCancellation)
        {
            _cts = new CancellationTokenSource();
            _cts.Cancel();
            _context.MockHttp.Expect(HttpMethod.Post, $"{_context.Host}/.well-known/mercure")
                .Respond(HttpStatusCode.OK);
        }
        else if (_context.MockStatusCode.HasValue)
        {
            _context.MockHttp.Expect(HttpMethod.Post, $"{_context.Host}/.well-known/mercure")
                .Respond(_context.MockStatusCode.Value, "text/plain", _context.MockErrorMessage ?? string.Empty);
        }

        await ExecutePublishWithErrorHandling();
    }

    [Then(@"a MercurePublisherException should be thrown")]
    public void ThenAMercurePublisherExceptionShouldBeThrown()
    {
        _context.ThrownException.ShouldNotBeNull();
        _context.ThrownException.ShouldBeOfType<MercurePublisherException>();
    }

    [Then(@"the exception message should contain ""(.*)""")]
    public void ThenTheExceptionMessageShouldContain(string expected)
    {
        _context.ThrownException.ShouldNotBeNull();
        _context.ThrownException.Message.ShouldContain(expected);
    }

    [Then(@"the exception should have an inner exception of type ""(.*)""")]
    public void ThenTheExceptionShouldHaveAnInnerExceptionOfType(string exceptionTypeName)
    {
        _context.ThrownException.ShouldNotBeNull();
        _context.ThrownException.InnerException.ShouldNotBeNull();
        _context.ThrownException.InnerException.GetType().Name.ShouldBe(exceptionTypeName);
    }

    private async Task ExecutePublishWithErrorHandling()
    {
        var options = Options.Create(new MercurePublisherOptions
        {
            Host = _context.Host!,
            Token = _context.Token!
        });

        var httpClient = _context.MockHttp.ToHttpClient();
        var publisher = new MercurePublisher(NullLogger<MercurePublisher>.Instance, options, httpClient);

        try
        {
            var ct = _cts?.Token ?? CancellationToken.None;
            await publisher.Publish(_context.Message!, ct);
            _context.PublishSucceeded = true;
        }
        catch (Exception ex)
        {
            _context.ThrownException = ex;
            _context.PublishSucceeded = false;
        }
    }
}
