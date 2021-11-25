## Authorization.Core
A library to implement granular claims-based authorization on top of Microsoft's Identity Framework.

### Getting Started
Define the claims required to manipulate the objects in your application.
Create a class that implements the `IDefinesClaims` interface for each application object:
```c#
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
        public const string List = "Calendar.List";
        public const string Create = "Calendar.Create";
        public const string Read = "Calendar.Read";
        public const string Update = "Calendar.Update";
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

Define the GUIDs associated with any pre-defined application roles and objects.
Create a class that implements the `IDefinesGuids` interface for each:
```c#
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
Associate the claims with the appropriate roles in the Identity database:
```c#
var role = new AppRole()
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
Associate the role with the appropriate user(s):
```c#
var cgEmail = "CalendarGuy@company.com";
var cgPassword = "SuperSecretPassword";

var user = new AppUser()
{
    Id = AppGuids.User.CalendarGuy,
    Email = cgEmail,
    UserName = cgEmail
}.SetClaims(new[] { role.Name });

result = await userManager.CreateAsync(user, cgPassword);
if (!result.Succeeded)
{
    HandleError(user, result.Errors);
}
```
And finally, add the `RequiresClaims` attribute to the `PageModel` (or Controller) class of the pages that require authorization:
```c#
[RequiresClaims(AppClaims.Calendar.Update)]
public class EditModel : PageModel
{
    ...
}
```
It is also possible to check authorization programically:
```c#
var result = await authorizationManager.AuthorizeAsync(User, role, new AppClaimRequirement(AppClaims.Role.UpdateClaims));
if (!result.Succeeded)
{
    HandleError(user, result.Errors);
}
```