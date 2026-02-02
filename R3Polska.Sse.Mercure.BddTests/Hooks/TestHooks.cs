using R3Polska.Sse.Mercure.BddTests.Support;
using Reqnroll;
using Reqnroll.BoDi;

namespace R3Polska.Sse.Mercure.BddTests.Hooks;

[Binding]
public class TestHooks
{
    [BeforeScenario]
    public void BeforeScenario(IObjectContainer container)
    {
        container.RegisterInstanceAs(new TestContext());
    }
}
