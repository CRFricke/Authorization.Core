## Installing the Authorization.Core.UI package

This section assumes that ASP.NET Core Identity is already installed and you can sucessfully 
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
have been extended (by "ApplicationUser" and "ApplicationRole"). 

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
must be `AuthUiUser` (or the name of your derived class; in this case, "ApplicationUser").

```csharp
@using CRFricke.Authorization.Core.Data
@using Microsoft.AspNetCore.Identity
@inject SignInManager<ApplicationUser> SignInManager
@inject UserManager<ApplicationUser> UserManager
```

### Add DataTables package references to _Layout.cshtml

The razor pages exposed by the UI package make use of a client side package called DataTables.net. 
This package supports both Bootstrap 4 and 5. It requires references for the associated CSS and 
javascript files be added to the `_Layout.cshtml` file that exposes the UI package's razor pages.

**_Note:_** _If your application uses the DataTables.net client side package and already has references for it 
in `_Layout.cshtml`, there is no need to add the statements described below; you can skip this section._

The UI package provides the required CSS and javascript files in the 
`~/_content/CRFricke.Authorization.Core.UI/lib/datatables` folder.

Add a stylesheet link for `dataTables.min.css` to the `<head>` section.
Also add a script reference for `dataTables.min.js` to the end of the `<body>` section. 
Both statements are shown below:

```html
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>@ViewData["Title"] - Authorization.Core.UI.Test.Web</title>
    <link rel="stylesheet" type="text/css" href="~/lib/bootstrap/dist/css/bootstrap.min.css" />
    <link rel="stylesheet" type="text/css" href="_content/CRFricke.Authorization.Core.UI/lib/datatables/dataTables.min.css" />
    <link rel="stylesheet" type="text/css" href="~/css/site.css" />
</head>
<body>

    ⁝

    <script src="~/lib/jquery/dist/jquery.min.js"></script>
    <script src="~/lib/bootstrap/dist/js/bootstrap.bundle.min.js"></script>
    <script src="~/_content/CRFricke.Authorization.Core.UI/lib/datatables/dataTables.min.js"></script>
    <script src="~/js/site.js" asp-append-version="true"></script>

    @await RenderSectionAsync("Scripts", required: false)
</body>
</html>
```

### Update Startup.cs (Program.cs in .Net 6.0)

- If the `AddDefaultIdentity` clause references the `IdentityUser` class, change it to `AuthUiUser` 
  (or the name of your derived class; "ApplicationUser" below).
- If you extended the `AuthUiRole` class, chain an `AddRoles` clause with the name of the new class 
  to the `AddDefaultIdentity` statement ("ApplicationRole", in this case).
- Chain an `AddAccessRightBasedAuthorization` clause to the `AddEntityFrameworkStores` clause.
  - AddAccessRightBasedAuthorization accepts an optional parameter, `DbInitializationOption`, to specify 
    how the Microsoft Identity database (which the Authorization.Core package uses) is to be initialized. 
    The default is `DbInitializationOption.Migrate`. which will apply any outstanding database migrations 
    during startup..
- Chain an `AddAuthorizationCoreUI` clause to the AddAccessRightBasedAuthorization clause.
    - You can specify an alternate area name for the pages exposed by the UI package by specifying the 
    desired name in the `AuthCoreUIOptions.FriendlyAreaName` property (shown below). If a value is 
    not specified, the default is "Authorization".

```csharp
   services.AddDbContext<ApplicationDbContext>(options =>
      options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"))
      );
   services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
      .AddRoles<ApplicationRole>()
      .AddEntityFrameworkStores<ApplicationDbContext>()
      .AddAccessRightBasedAuthorization()
      .AddAuthorizationCoreUI(options => options.FriendlyAreaName = "Admin");
``` 

### Add a migration to pick up schema changes added by the UI package

The UI package adds the following properties:

- AuthUiRole adds the `Description` property.
- AuthUiUser adds the following properties: `GivenName` and `Surname`.

Using the Package Manager Console run the following commands, where \<migration-name\> is the name you chose 
for the migration (eg. "AuthorizationCoreUISchema"):

```
Add-Migration <migration-name>
Update-Database
```

At this point you should be able build your project and still be able to log in.

Now that the Authorization.Core.UI package is installed, it is time to 
[configure your roles and users](Configure-UI-Roles-and-Users.md)
