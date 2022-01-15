## Installing the Authorization.Core.UI package

This section assumes that ASP.NET Core Identity is already installed, and that you can sucessfully 
log in. To create a new ASP.NET Core Web Application with authentication, see 
[Create a Web app with authentication](https://docs.microsoft.com/en-gb/aspnet/core/security/authentication/identity?view=aspnetcore-6.0&tabs=visual-studio#create-a-web-app-with-authentication).
To add authentication to an existing ASP.NET Core Application, see 
[Scaffold Identity in ASP.NET Core projects](https://docs.microsoft.com/en-gb/aspnet/core/security/authentication/scaffold-identity?view=aspnetcore-6.0&tabs=visual-studio#scaffold-identity-into-a-razor-project-without-existing-authorization).

The following sections show how to install and configure the Authorization.Core.UI package.

### Install the package

_**Note:**_ If the Authorization.Core package is installed, it should be removed before installing 
the UI package (the code changes can remain).

- Via dotnet CLI:

```
  dotnet add <project> package CRFricke.Authorization.Core.UI
```

- Via NuGet Package Manager:

    Using [Visual Studio](https://docs.microsoft.com/en-us/nuget/consume-packages/install-use-packages-visual-studio#find-and-install-a-package)

    Using [Visual Studio for Mac](https://docs.microsoft.com/en-us/visualstudio/mac/nuget-walkthrough?toc=%2Fnuget%2Ftoc.json&view=vsmac-2019#find-and-install-a-package)

### Extend the AuthUiRole and/or AuthUiUser classes (optional)

The Authorization.Core.UI package extends the Microsoft Identity classes `IdentityRole` and 
`IdentityUser`. If you extended any of these classes (or need to), you must derive your 
classes from the associated Authorization.Core.UI classes (`AuthUiRole` and `AuthUiUser`).

### Update the ApplicationDbContext class to derive from AuthUiContext

**_Note:_** The following example shows the form to use when both the `AuthUiRole` and `AuthUiUser` classes 
have been extended. 

```csharp
using CRFricke.Authorization.Core.UI.Data;
using Microsoft.EntityFrameworkCore;

namespace WebApplication.Data
{
    public class ApplicationDbContext : AuthUiContext<ApplicationUser, ApplicationRole>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}
```

### Update the class name specified in _LoginPartial.cshtml

The class name specified in the `@inject` statements for `SignInManager` and `UserManager` 
must be `AuthUiUser` (or the name of your derived class; in this case, `ApplicationUser`).

```csharp
@using CRFricke.Authorization.Core.Data
@using Microsoft.AspNetCore.Identity
@inject SignInManager<ApplicationUser> SignInManager
@inject UserManager<ApplicationUser> UserManager
```

### Update Startup.cs (Program.cs in .Net 6.0)

- Chain an `AddAccessRightBasedAuthorization` clause to the `AddDefaultIdentity` statement.
- Chain an `AddAuthorizationCoreUI` clause to the AddAccessRightBasedAuthorization clause.
- Change any `IdentityUser` class references to `AuthUiUser` (or the name of your derived class).

```csharp
   services.AddDbContext<ApplicationDbContext>(options =>
      options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"))
      );
   services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
      .AddEntityFrameworkStores<ApplicationDbContext>()
      .AddAccessRightBasedAuthorization<ApplicationDbContext>()
      .AddAuthorizationCoreUI();
``` 

### Add a migration to pick up schema changes added by the UI package

The UI package adds the following properties:

- AuthUiRole adds the `Description` property.
- AuthUiUser adds the following properties: `GivenName` and `Surname`.

Using the Package Manager Console run the following commands, where \<migration-name\> is the name you choose 
for the migration (eg. "AuthorizationCoreUISchema"):

```
Add-Migration <migration-name>
Update-Database
```

At this point you should be able build your project and still be able to log in.

Now that the Authorization.Core.UI package is installed, it is time to 
[configure your roles and users](Configure-UI-Roles-and-Users.md)
