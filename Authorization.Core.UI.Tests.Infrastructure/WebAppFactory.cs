using Authorization.Core.UI.Test.Web.Data;
using CRFricke.Test.Support.Infrastructure;

namespace Authorization.Core.UI.Tests.Infrastructure;

public class WebAppFactory : WebApplicationFactory<Program, ApplicationDbContext>
{
    public override string HostingEnvironment => "IntegrationTests";

    public override string HostUrl => "http://AuthCoreUi.dev.localhost";
}
