## Configuring Your Roles and Users

The Authorization.Core package makes use of ASP.NET Core Identity's `IdentityRoleClaim` class which
maps a set of user defined claims to an `IdentityRole`. The `IdentityUserClaim` class is used to 
map one or more of these `IdentityRole` entities to an `IdentityUser`.

### Define the claims associated with your entities

At startup, the package's AuthorizationManager scans the application's assemblies for classes that implement 
the `IDefinesClaims` interface. It uses this interface to load the claims defined by the application.

The Authorization.Core package provides a set of claims to control access to both Users and Roles. The claims 
defined for Role entities are shown below (the claims defined for User entities correspond one-for-one with 
those for Role entities):

```csharp
/// <summary>
/// Defines the Claims used by the Authorization system.
/// </summary>
public class SysClaims
{
    /// <summary>
    /// Defines the claims associated with the Role entity.
    /// </summary>
    public class Role : IDefinesClaims
    {
        /// <summary>
        /// The user can create Role entities.
        /// </summary>
        public const string Create = "Role.Create";

        /// <summary>
        /// The user can read Role entities.
        /// </summary>
        public const string Read = "Role.Read";

        /// <summary>
        /// The user can update Role entities.
        /// </summary>
        public const string Update = "Role.Update";

        /// <summary>
        /// The user can delete Role entities.
        /// </summary>
        [RestrictedClaim]
        public const string Delete = "Role.Delete";

        /// <summary>
        /// The user can list Role entities.
        /// </summary>
        public const string List = "Role.List";

        /// <summary>
        /// The user can update the claims collection of Role entities.
        /// </summary>
        [RestrictedClaim]
        public const string UpdateClaims = "Role.UpdateClaims";

        /// <summary>
        /// Returns a list of all Claims defined for Role entities.
        /// </summary>
        public static readonly List<string> DefinedClaims = new List<string>
        {
            Create, Delete, Read, Update, List, UpdateClaims
        };

        ///<inheritdoc/>
        List<string> IDefinesClaims.DefinedClaims => DefinedClaims;
    }

    /// <summary>
    /// Defines the claims associated with the User entity.
    /// </summary>
    public class User : IDefinesClaims ...
}
```

Sometimes the operation associated with a claim must be prevented when dealing with pre-installed entities. For 
example, a user assigned the `Role.Delete` claim must not be allowed to delete the `Administrator` Role. 
The `RestrictedClaim` attribute is used to identify such claims (as can be seen on the "Delete" and 
"UpdateClaims" definitions above).

**_Note:_** The AuthorizationManager has no understanding of what "Delete" means. All it knows is that it 
must ensure the current user has been assigned the `Role.Delete` claim when it receives an authorization 
request requiring that Claim. It is up to you to ensure your claim associations make sense (e.g. "Customer.Delete" 
is associated with your application's logic to delete a Customer object; "Customer.Update" is associated with your 
application's logic to update a Customer object, etc.).

Define the claims required to control access to the entities in your application. For discoverability, it is best 
to group related claims together in a class, and have these classes defined within a containing class. Use the 
example above as a guide.

### Define the Guids of your pre-installed entities

The AuthorizationManager also scans the application's assemblies for classes that implement the `IDefinesGuids` 
interface. It uses this interface to load the Guids of any entities pre-installed by the application.

The Authorization.Core package provides definitions for the Guids associated with the `Administrator` Role 
and User in the `SysGuids` class.

Define the Guids associated with any entities that are pre-installed by your application. Use the example 
below as a guide.

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

### Add SeedDatabaseAsync method override to your application's DbContext (optional)

The `AuthDbContext` contained in the Authorization.Core package contains a virtual `SeedDatabaseAsync` 
method that is called during database initialization to ensure the `Administrator` Role and User account 
are installed. If either are accidently deleted, they are reinstalled the next time the application is restarted.

_**Note:**_ The initial value for the Administrator's password is `Administrat0r!`. 
_**Always be sure to change the Administrator's initial password in a production environment.**_

_**Warning:**_ If the Administrator account is reinstalled during an application restart, 
_**the Administrator's password will revert to its initial value.**_

The `SeedDatabaseAsync` method can be overriden in your application's DbContext (the one you derived from 
`AuthDbContext`) to provide similar functionality for your application:

```csharp
/// <inheritdoc/>
public override async Task SeedDatabaseAsync(IServiceProvider serviceProvider)
{
    await base.SeedDatabaseAsync(serviceProvider);

    var normalizer = serviceProvider.GetRequiredService<ILookupNormalizer>();
    var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<ApplicationDbContext>();

    var role = await Roles.FindAsync(AppGuids.Role.CalendarManager);
    if (role == null)
    {
        role = new ApplicationRole
        {
            Id = AppGuids.Role.CalendarManager,
            Name = nameof(AppGuids.Role.CalendarManager),
            NormalizedName = normalizer.NormalizeName(nameof(AppGuids.Role.CalendarManager))
        }.SetClaims(AppClaims.Calendar.DefinedClaims);

        await Roles.AddAsync(role);
        logger.LogInformation($"{nameof(ApplicationRole)} '{role.Name}' has been created.");
    }

    var user = await Users.FindAsync(AppGuids.User.CalendarGuy);
    if (user == null)
    {
        var email = "CalendarGuy@company.com";

        user = new ApplicationUser
        {
            Id = AppGuids.User.CalendarGuy,
            Email = email,
            EmailConfirmed = true,
            LockoutEnabled = false,
            NormalizedEmail = normalizer.NormalizeEmail(email),
            NormalizedUserName = normalizer.NormalizeName(email),
            PasswordHash = "<hash-of-calendar-guys-password>",
            UserName = email
        }.SetClaims(new[] { role.Name });

        await Users.AddAsync(user);
        logger.LogInformation($"{nameof(ApplicationUser)} '{user.Email}' has been created.");
    }

    try
    {
        await SaveChangesAsync();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "SaveChangesAsync() method failed.");
    }
}
```


At this point you should still be able build your project and log in.

Now that your Roles and Users are configured, it's time to 
[add authorization checks to your application code](Enforcing-Authorization.md).