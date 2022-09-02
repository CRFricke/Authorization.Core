## Installing the Authorization.Core package

This section assumes that ASP.NET Core Identity is already installed, and that you can sucessfully 
log in. To create a new ASP.NET Core Web Application with authentication, see 
[Create a Web app with authentication](https://docs.microsoft.com/en-gb/aspnet/core/security/authentication/identity?view=aspnetcore-6.0&tabs=visual-studio#create-a-web-app-with-authentication).
To add authentication to an existing ASP.NET Core Application, see 
[Scaffold Identity in ASP.NET Core projects](https://docs.microsoft.com/en-gb/aspnet/core/security/authentication/scaffold-identity?view=aspnetcore-6.0&tabs=visual-studio#scaffold-identity-into-a-razor-project-without-existing-authorization).

The following sections show how to install and configure the Authorization.Core package.

### Install the package

- Via dotnet CLI:

```
  dotnet add <project> package CRFricke.Authorization.Core
```

- Via NuGet Package Manager:

    Using [Visual Studio](https://docs.microsoft.com/en-us/nuget/consume-packages/install-use-packages-visual-studio#find-and-install-a-package)

    Using [Visual Studio for Mac](https://docs.microsoft.com/en-us/visualstudio/mac/nuget-walkthrough?toc=%2Fnuget%2Ftoc.json&view=vsmac-2019#find-and-install-a-package)

### Extend the AuthRole and/or AuthUser classes (optional)

The Authorization.Core package extends the Microsoft Identity classes `IdentityRole` and 
`IdentityUser`. If you extended any of these classes (or need to), you must derive your 
classes from the associated Authorization.Core classes (`AuthRole` and `AuthUser`).

### Update the ApplicationDbContext class to derive from AuthDbContext

**_Note:_** The following example shows the form to use when both the `AuthRole` and `AuthUser` classes 
have been extended. 

```csharp
using CRFricke.Authorization.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace WebApplication.Data
{
    public class ApplicationDbContext : AuthDbContext<ApplicationUser, ApplicationRole>
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
must be `AuthUser` (or the name of your derived class; in this case, `ApplicationUser`).

```csharp
@using CRFricke.Authorization.Core.Data
@using Microsoft.AspNetCore.Identity
@inject SignInManager<ApplicationUser> SignInManager
@inject UserManager<ApplicationUser> UserManager
```

### Update Startup.cs (Program.cs in .Net 6.0)

- If you extended the `AuthRole` class, chain an `AddRoles` clause with the name of the new class 
  to the `AddDefaultIdentity` statement (`ApplicationRole`, in this case).
- Chain an `AddAccessRightBasedAuthorization` clause to the `AddEntityFrameworkStores` clause.
  - AddAccessRightBasedAuthorization accepts an optional parameter, `DbInitializationOption`, to specify 
    how the Microsoft Identity database (which the Authorization.Core package uses) is to be initialized. 
    The default is `DbInitializationOption.Migrate`. which will apply any outstanding database migrations 
    during startup..
- Change any `IdentityUser` class references to `AuthUser` (or the name of your derived class; 
  `ApplicationUser` below).

```csharp
   services.AddDbContext<ApplicationDbContext>(options =>
      options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"))
      );
   services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
      .AddRoles<ApplicationRole>()
      .AddEntityFrameworkStores<ApplicationDbContext>()
      .AddAccessRightBasedAuthorization();
``` 

At this point you should be able build your project and still be able to log in.

Now that the Authorization.Core package is installed, it is time to 
[configure your roles and users](Configure-Core-Roles-and-Users.md)