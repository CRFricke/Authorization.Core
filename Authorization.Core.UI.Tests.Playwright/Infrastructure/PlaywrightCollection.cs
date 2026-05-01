using CRFricke.Test.Support.Infrastructure;

#pragma warning disable CA1515 // Consider making public types internal
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix

namespace Authorization.Core.UI.Tests.Playwright.Infrastructure;

/// <summary>
/// Collection fixture for sharing Playwright instance across tests.
/// </summary>
[CollectionDefinition("Playwright")]
public class PlaywrightCollection : ICollectionFixture<PlaywrightFixture>
{
    // This class is just a marker for xUnit
}
