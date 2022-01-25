## Installing the Authorization.Core.UI package

This section assumes that ASP.NET Core Identity is already installed, and that you can sucessfully 
log in. To create a new ASP.NET Core Web Application with authentication, see 
[Create a Web app with authentication](https://docs.microsoft.com/en-gb/aspnet/core/security/authentication/identity?view=aspnetcore-6.0&tabs=visual-studio#create-a-web-app-with-authentication).
To add authentication to an existing ASP.NET Core Application, see 
[Scaffold Identity in ASP.NET Core projects](https://docs.microsoft.com/en-gb/aspnet/core/security/authentication/scaffold-identity?view=aspnetcore-6.0&tabs=visual-studio#scaffold-identity-into-a-razor-project-without-existing-authorization).

The following sections show how to install and configure the Authorization.Core.UI package.

_**Note:**_ If the Authorization.Core package is installed, it should be removed before installing 
the UI package (the code changes can remain).

### Install the package

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

**_Note:_** The following example shows the form to use when both the `AuthUiUser` and `AuthUiRole` classes 
have been extended (by ApplicationUser and ApplicationRole). 

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
must be `AuthUiUser` (or the name of your derived class; in this case, ApplicationUser).

```csharp
@using CRFricke.Authorization.Core.Data
@using Microsoft.AspNetCore.Identity
@inject SignInManager<ApplicationUser> SignInManager
@inject UserManager<ApplicationUser> UserManager
```

### Add a RenderSectionAsync statement to _Layout.cshtml

The razor pages exposed by the UI package need to add stylesheet links in the `<head>` section of 
_Layout.cshtml. Add the `RenderSectionAsync` statement, as shown below, to the end of the section:

```html
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>@ViewData["Title"] - Authorization.Core.UI.Test.Web</title>
    <link rel="stylesheet" href="~/lib/bootstrap/dist/css/bootstrap.min.css" />
    <link rel="stylesheet" href="~/css/site.css" />
    @await RenderSectionAsync("css", required: false)
</head>
    ⁝
```

### Update Startup.cs (Program.cs in .Net 6.0)

- Chain an `AddAccessRightBasedAuthorization` clause to the `AddDefaultIdentity` statement.
- Chain an `AddAuthorizationCoreUI` clause to the AddAccessRightBasedAuthorization clause.
    - You can specify an alternate area name for the pages exposed by the UI package by specifying the 
    desired name in the `AuthCoreUIOptions.FriendlyAreaName` property (shown below). If a value is 
    not specified, the default is "Authorization".
- Change any `IdentityUser` class references to `AuthUiUser` (or the name of your derived class).

```csharp
   services.AddDbContext<ApplicationDbContext>(options =>
      options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"))
      );
   services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
      .AddEntityFrameworkStores<ApplicationDbContext>()
      .AddAccessRightBasedAuthorization<ApplicationDbContext>()
      .AddAuthorizationCoreUI(options => options.FriendlyAreaName = "Admin");
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
