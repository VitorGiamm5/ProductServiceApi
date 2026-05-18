using ProductServiceApp.EdgeTests.Support;

namespace ProductServiceApp.EdgeTests;

public sealed class AuthenticationEdgeTests(PlaywrightFixture fixture)
    : IClassFixture<PlaywrightFixture>
{
    [Fact]
    public async Task Login_page_starts_keycloak_flow()
    {
        var context = await fixture.Browser.NewContextAsync();
        var page = await context.NewPageAsync();

        await page.GotoAsync($"{fixture.Settings.WebBaseUrl}/auth/login");
        await page.WaitForURLAsync("**/realms/productservice/**", new PageWaitForURLOptions
        {
            Timeout = 30_000
        });

        page.Url.Should().Contain("/realms/productservice/");

        await context.DisposeAsync();
    }
}
