## Installing the Authorization.Core package

This section assumes that ASP.NET Core Identity is already installed, and that you can sucessfully 
log in. To create a new ASP.NET Core Web Application with authentication, see 
[Create a Web app with authentication](https://docs.microsoft.com/en-gb/aspnet/core/security/authentication/identity?view=aspnetcore-6.0&tabs=visual-studio#create-a-web-app-with-authentication).
To add authentication to an existing ASP.NET Core Application, see 
[Scaffold Identity in ASP.NET Core projects](https://docs.microsoft.com/en-gb/aspnet/core/security/authentication/scaffold-identity?view=aspnetcore-6.0&tabs=visual-studio#scaffold-identity-into-a-razor-project-without-existing-authorization).

The following steps show how to install the Authorization.Core package.

1.  Install the CRFricke.Authorization.Core package:

    - Via dotnet CLI:

        `dotnet add PROJECT package CRFricke.Authorization.Core --version 1.0.0`

    - Via NuGet Package Manager:

        Using [Visual Studio](https://docs.microsoft.com/en-us/nuget/consume-packages/install-use-packages-visual-studio#find-and-install-a-package)

        Using [Visual Studio for Mac](https://docs.microsoft.com/en-us/visualstudio/mac/nuget-walkthrough?toc=%2Fnuget%2Ftoc.json&view=vsmac-2019#find-and-install-a-package)

2.  (Optional) Extend the `AuthDbContext`, `AuthRole`, and/or `AuthUser` classes.

     The Authorization.Core package extends the Microsoft Identity classes `IdentityDbContext`, 
     `IdentityRole`, and `IdentityUser`. If you extended any of these classes (or need to), 
     you must derive your classes from the associated Authorization.Core classes.

3.  Verify the class name specified in `_LoginPartial.cshtml`.

    The class name specified in the `@inject` statements for `SignInManager` and `UserManager` 
    must be `AuthUser` (or the name of your derived class).

4.  Update Startup.cs (Program.cs in \.Net 6.0):

    - Change any `IdentityDbContext` class references to `AuthDbContext`.
    - Change any `IdentityUser` class references to `AuthUser`.
    - Chain an `AddAuthorizationCore` clause to the `AddDefaultIdentity` statement.

```csharp
     services.AddDbContext<AuthDbContext>(options =>
        options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"))
        );
     services.AddDefaultIdentity<AuthUser>(options => options.SignIn.RequireConfirmedAccount = true)
        .AddEntityFrameworkStores<AuthDbContext>()
        .AddAuthorizationCore<AuthDbContext>();
``` 

At this point you should be able build your project and still be able to log in.

Now that the Authorization.Core package is installed, it is time to 
[Configure your roles](Configure-Roles.md).
