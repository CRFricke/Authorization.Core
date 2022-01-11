## Configuring Your Roles and Users

The Authorization.Core package makes use of ASP.NET Core Identity's `IdentityRoleClaim` class which
maps a set of user defined claims to an `IdentityRole`. The `IdentityUserClaim` class is used to 
map one or more of these `IdentityRole` entities to an `IdentityUser`.

### Define the claims associated with your entities

At startup, the package's AuthorizationManager scans the assemblies loaded into the execution context of 
of the current domain for classes that implement the `IDefinesClaims` interface. It uses this interface 
to load the claims defined by the application.

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
request requiring that Claim. 

Define the claims required to control access to the entities in your application. For discoverability, it is best 
to group related claims together in a class, and have these classes defined within a containing class. Use the 
example above as a guide.

### Define the Guids of your pre-installed entities

The AuthorizationManager also scans the application's assemblies for classes that implement the `IDefinesGuids` 
interface. It uses this interface to load the Guids of any entities pre-installed by the application.

The Authorization.Core package provides definitions for the Guids associated with the `Administrator` Role 
and User in the `SysGuids` class.

Define the Guids associated any entities that are pre-installed by your application. Use the example below as 
a guide.

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

### Add database initialization to the application's startup logic

The Authorization.Core package contains logic that can be called during database initialization to ensure 
the `Administrator` Role and User are installed. If either are accidently deleted, they will be reinstalled 
the next time the application is restarted.

The following steps show how to set this up.

#### Add SeedDatabase override to your application's dbcontext (optional)

If you have entities that you want to ensure are installed during database initialization, you can override the 
`AuthDbContext.SeedDatabase` method in your DbContext (the one you derived from `AuthDbContext`):

```csharp
/// <inheritdoc/>
public override void SeedDatabase()
{
    base.SeedDatabase();

    var normalizer = new UpperInvariantLookupNormalizer();

    var role = Roles.Find(AppGuids.Role.CalendarManager);
    if (role == null)
    {
        role = new ApplicationRole
        {
            Id = AppGuids.Role.CalendarManager,
            Name = nameof(AppGuids.Role.CalendarManager),
            NormalizedName = normalizer.NormalizeName(nameof(AppGuids.Role.CalendarManager))
        }.SetClaims(AppClaims.Calendar.DefinedClaims);

        Roles.Add(role);
    }

    var user = Users.Find(AppGuids.User.CalendarGuy);
    if (user == null)
    {
        var email = "CalendarGuy@company.com";

        user = new ApplicationUser
        {
            Id = AppGuids.User.CalendarGuy,
            Email = email,
            EmailConfirmed = true,
            LockoutEnabled = true,
            NormalizedEmail = normalizer.NormalizeEmail(email),
            NormalizedUserName = normalizer.NormalizeName(email),
            PasswordHash = "<hash-of-calendar-guys-password>",
            UserName = email
        }.SetClaims(new[] { role.Name });

        Users.Add(user);
    }

    SaveChanges();
}
```

#### Add database initializer class

This sample class ensures the database is created and all migrations have been run. It then calls the 
`SeedDatabase` method of the DbContext:

```csharp
public static class DbInitializer
{
    public static void InitializeDatabase(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(DbInitializer));
        var dbContext = services.GetRequiredService<ApplicationDbContext>();

        try
        {
            dbContext.Database.Migrate();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database.Migrate() failed.");
            throw;
        }

        try
        {
            dbContext.SeedDatabase();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "SeedDatabase() failed.");
            throw;
        }
    }
}
```

#### Call database initializer class during application startup

Replace the code in the Main method of Program.cs with the code shown below (change the names in the `try` block 
to match the class you created in the previous step as necessary):

```csharp
public class Program
{
    public static void Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();

        using (var scope = host.Services.CreateScope())
        {
            var services = scope.ServiceProvider;

            try
            {
                DbInitializer.InitializeDatabase(services);
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred initializing the DB.");
            }
        }

        host.Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}
```

In the case of .Net 6.0, the code goes after the `builder.Build()` statement in Program.cs:

```csharp
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        DbInitializer.InitializeDatabase(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred initializing the DB.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
```

At this point you should still be able build your project and log in.

_**Note:**_ The first time your application is run, the database will be seeded with the `Administrator` Role 
and User account. The initial value for the Administrator's password is **"Administrat0r!"**. If the Administrator 
User account is deleted, it will be restored the next time the application is restarted. 
_**When this occurs, the Administrator's password will revert to its initial value.**_

_**Always be sure to change the Administrator's initial password in a production environment.**_

Now that your Roles and Users are configured, it's time to 
[add authorization checks to your application code](Enforcing-Authorization.md).