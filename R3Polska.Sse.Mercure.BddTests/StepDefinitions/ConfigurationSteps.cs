using System.ComponentModel.DataAnnotations;
using R3Polska.Sse.Mercure.BddTests.Support;
using Reqnroll;
using Shouldly;

namespace R3Polska.Sse.Mercure.BddTests.StepDefinitions;

[Binding]
public class ConfigurationSteps
{
    private readonly TestContext _context;

    public ConfigurationSteps(TestContext context)
    {
        _context = context;
    }

    [Given(@"I configure the publisher with:")]
    public void GivenIConfigureThePublisherWith(DataTable table)
    {
        _context.Options = new MercurePublisherOptions
        {
            Host = table.Rows.First(r => r["Option"] == "Host")["Value"],
            Token = table.Rows.First(r => r["Option"] == "Token")["Value"]
        };
    }

    [When(@"I validate the options")]
    public void WhenIValidateTheOptions()
    {
        var validationContext = new ValidationContext(_context.Options!);
        Validator.TryValidateObject(
            _context.Options!,
            validationContext,
            _context.ValidationResults,
            validateAllProperties: true);
    }

    [Then(@"the validation should pass")]
    public void ThenTheValidationShouldPass()
    {
        _context.ValidationResults.ShouldBeEmpty();
    }

    [Then(@"the validation should fail")]
    public void ThenTheValidationShouldFail()
    {
        _context.ValidationResults.ShouldNotBeEmpty();
    }

    [Then(@"the validation error should mention ""(.*)""")]
    public void ThenTheValidationErrorShouldMention(string fieldName)
    {
        _context.ValidationResults
            .ShouldContain(r => r.MemberNames.Contains(fieldName));
    }
}
