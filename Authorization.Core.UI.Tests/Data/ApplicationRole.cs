using CRFricke.Authorization.Core;
using CRFricke.Authorization.Core.UI.Data;

#pragma warning disable CA1515 // Consider making public types internal

namespace Authorization.Core.UI.Tests.Data;

public class ApplicationRole : AuthUiRole, IRequiresAuthorization
{
}
