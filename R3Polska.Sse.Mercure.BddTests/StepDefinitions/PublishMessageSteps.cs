using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using R3Polska.Sse.Mercure.BddTests.Support;
using R3Polska.Sse.Mercure.Message;
using Reqnroll;
using RichardSzalay.MockHttp;
using Shouldly;

namespace R3Polska.Sse.Mercure.BddTests.StepDefinitions;

[Binding]
public class PublishMessageSteps
{
    private readonly TestContext _context;

    public PublishMessageSteps(TestContext context)
    {
        _context = context;
    }

    [Given(@"a Mercure hub is configured at ""(.*)""")]
    public void GivenAMercureHubIsConfiguredAt(string host)
    {
        _context.Host = host;
    }

    [Given(@"the JWT token is ""(.*)""")]
    public void GivenTheJwtTokenIs(string token)
    {
        _context.Token = token;
    }

    [Given(@"I have a message with topic ""(.*)""")]
    public void GivenIHaveAMessageWithTopic(string topic)
    {
        _context.Message = new MercureMessage
        {
            Topic = topic,
            Payload = _context.Payload
        };
    }

    [Given(@"the message ID is ""(.*)""")]
    public void GivenTheMessageIdIs(string id)
    {
        _context.Message!.Id = id;
    }

    [Given(@"the payload contains:")]
    public void GivenThePayloadContains(DataTable table)
    {
        foreach (var row in table.Rows)
        {
            _context.Payload.Fields[row["Field"]] = row["Value"];
        }
    }

    [When(@"I publish the message")]
    public async Task WhenIPublishTheMessage()
    {
        SetupMockHttp(HttpStatusCode.OK, string.Empty);
        await ExecutePublish();
    }

    [Then(@"the message should be sent successfully")]
    public void ThenTheMessageShouldBeSentSuccessfully()
    {
        _context.PublishSucceeded.ShouldBeTrue();
        _context.ThrownException.ShouldBeNull();
    }

    [Then(@"the request should be sent to ""(.*)""")]
    public void ThenTheRequestShouldBeSentTo(string expectedUrl)
    {
        _context.CapturedRequest.ShouldNotBeNull();
        _context.CapturedRequest.RequestUri!.ToString().ShouldBe(expectedUrl);
    }

    [Then(@"the Authorization header should be ""(.*)""")]
    public void ThenTheAuthorizationHeaderShouldBe(string expectedHeader)
    {
        _context.CapturedRequest.ShouldNotBeNull();
        _context.CapturedRequest.Headers.Authorization.ShouldNotBeNull();
        _context.CapturedRequest.Headers.Authorization.ToString().ShouldBe(expectedHeader);
    }

    [Then(@"the request body should contain ""(.*)""")]
    public void ThenTheRequestBodyShouldContain(string expected)
    {
        _context.CapturedRequestBody.ShouldNotBeNull();
        _context.CapturedRequestBody.ShouldContain(expected);
    }

    [Then(@"the request body should not contain ""(.*)""")]
    public void ThenTheRequestBodyShouldNotContain(string notExpected)
    {
        _context.CapturedRequestBody.ShouldNotBeNull();
        _context.CapturedRequestBody.ShouldNotContain(notExpected);
    }

    [Then(@"the request body should contain the JSON payload")]
    public void ThenTheRequestBodyShouldContainTheJsonPayload()
    {
        _context.CapturedRequestBody.ShouldNotBeNull();
        // URL decode the body - note that + is used for spaces in form encoding
        var decodedBody = Uri.UnescapeDataString(_context.CapturedRequestBody.Replace("+", " "));

        // The TestPayload serializes with a "Fields" wrapper containing key-value pairs
        decodedBody.ShouldContain("\"Fields\":");
        foreach (var field in _context.Payload.Fields)
        {
            // Values are stored as strings in the dictionary
            decodedBody.ShouldContain($"\"{field.Key}\":\"{field.Value}\"");
        }
    }

    private void SetupMockHttp(HttpStatusCode statusCode, string content)
    {
        _context.MockHttp.Expect(HttpMethod.Post, $"{_context.Host}/.well-known/mercure")
            .Respond(req =>
            {
                _context.CapturedRequest = req;
                _context.CapturedRequestBody = req.Content!.ReadAsStringAsync().Result;
                return new HttpResponseMessage(statusCode) { Content = new StringContent(content) };
            });
    }

    private async Task ExecutePublish()
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
            await publisher.Publish(_context.Message!);
            _context.PublishSucceeded = true;
        }
        catch (Exception ex)
        {
            _context.ThrownException = ex;
            _context.PublishSucceeded = false;
        }
    }
}
