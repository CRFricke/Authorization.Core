using CRFricke.Test.Support.Infrastructure;

namespace Authorization.Core.UI.Tests.Playwright.Infrastructure;

/// <summary>
/// Collection fixture for sharing Playwright instance across tests.
/// </summary>
[CollectionDefinition("Playwright")]
public class PlaywrightCollection : ICollectionFixture<PlaywrightFixture>
{
    // This class is just a marker for xUnit
}
