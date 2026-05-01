using CRFricke.Authorization.Core;
using CRFricke.Authorization.Core.Data;

#pragma warning disable CA1515 // Consider making public types internal

namespace Authorization.Core.Tests.Data;

/// <summary>
///  Describes an application role entity.
/// </summary>
public class AppRole : AuthRole, IRequiresAuthorization
{ }
