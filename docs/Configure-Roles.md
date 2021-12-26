## Configuring Your Roles and Users

The Authorization.Core package makes use of ASP.NET Core Identity's `IdentityRoleClaim` class to
map a set of user defined claims to an `IdentityRole`. The `IdentityUserClaim` class is used to 
map one or more of these `IdentityRole` entities to an `IdentityUser`.

### Define the Claims for your Roles

At startup, the package's AuthorizationManager scans the assemblies loaded into the execution context of 
of the current domain for classes that implement the `IDefinesClaims` interface. It uses this interface 
to load the claims defined by the application.

If there are any claims that must not be allowed against a pre-installed entity, the `RestrictedClaim` 
attribute can be used to indicate this. In the example below, users should not be allowed to delete any 
pre-installed Calendar entities. The `RestrictedClaim` attribute is added to the `Calendar.Delete` 
definition to indicate this. 

_**Note:**_ The AuthorizationManager has no understanding of what "Delete" means. All it knows is that it 
must ensure the current user has been assigned a role containing the "Calendar.Delete" claim when it receives 
an authorization request requiring that Claim. 

For discoverability, it is best to group related claims together in a class, and have these classes 
defined within a containing class as shown below.

```csharp
/// <summary>
/// Defines the Claims used by the application.
/// </summary>
public class AppClaims
{
    /// <summary>
    /// Defines the claims required to manipulate Calendar events.
    /// </summary>
    public class Calendar : IDefinesClaims
    {
        /// <summary>
        /// The user can list Calendar entities.
        /// </summary>
        public const string List = "Calendar.List";

        /// <summary>
        /// The user can create Calendar entities.
        /// </summary>
        public const string Create = "Calendar.Create";

        /// <summary>
        /// The user can read Calendar entities.
        /// </summary>
        public const string Read = "Calendar.Read";

        /// <summary>
        /// The user can update Calendar entities.
        /// </summary>
        public const string Update = "Calendar.Update";

        /// <summary>
        /// The user can delete Calendar entities.
        /// </summary>
        [RestrictedClaim]
        public const string Delete = "Calendar.Delete";

        /// <summary>
        /// Returns a list of all claims defined for Calendar events.
        /// </summary>
        public static readonly List<string> DefinedClaims = new List<string>
        {
            Create, Delete, Read, Update, List
        };

        ///<inheritdoc/>
        List<string> IDefinesClaims.DefinedClaims => DefinedClaims;
    }

    /// <summary>
    /// Defines the claims required to manipulate Documents.
    /// </summary>
    public class Document : IDefinesClaims ...
}
```

### Define the Guids of any pre-installed Users and Roles

The AuthorizationManager also scans the application's assemblies for classes that implement the `IDefinesGuids` 
interface. It uses this interface to load the Guids of any pre-installed Users and Roles.

Once again, it is best to group related Guids together in a class, and have these classes defined 
within a containing class as shown below.

```csharp
/// <summary>
/// Defines the Guids used by the application.
/// </summary>
public class AppGuids
{
    /// <summary>
    /// The Guids of the application's default roles.
    /// </summary>
    public class Role : IDefinesGuids
    {
        public const string CalendarManager = "fee71d38-9ed2-4962-8f43-8cd48678c65e";
        public const string DocumentManager = "45d88a4c-a4d9-4c2a-8cbf-38c883ff6130";

        /// <summary>
        /// Returns a list of all GUIDs defined for role entities.
        /// </summary>
        public static readonly List<string> DefinedGuids = new List<string>
        {
            CalendarManager, DocumentManager
        };

        ///<inheritdoc/>
        List<string> IDefinesGuids.DefinedGuids => DefinedGuids;
    }

    /// <summary>
    /// The Guids of the application's default users.
    /// </summary>
    public class User : IDefinesGuids ...
}
```

### Associate Claims with the appropriate Roles in the Identity database:

Now that the Claims and Guids are defined, use ASP.NET Core Identity's RoleManager to create and persist an 
`AuthRole` for each Role as shown below.

```csharp
var role = new AuthRole()
{
    Id = AppGuids.Role.CalendarManager,
    Name = nameof(AppGuids.Role.CalendarManager)
}.SetClaims(AppClaims.Calendar.DefinedClaims);

var result = await roleManager.CreateAsync(role);
if (!result.Succeeded)
{
    HandleError(role, result.Errors);
}
```

### Associate Roles with the appropriate Users in the Identity database:

Continuing with the example above, use ASP.NET Core Identity's UserManager to create and persist an 
`AuthUser` for each User as shown below.

```csharp
var user = new AuthUser()
{
    Id = AppGuids.User.CalendarGuy,
    Email = "CalendarGuy@company.com",
    UserName = "CalendarGuy"
}.SetClaims(new[] { role.Name });

result = await userManager.CreateAsync(user, "SuperSecretPassword!");
if (!result.Succeeded)
{
    HandleError(user, result.Errors);
}
```
