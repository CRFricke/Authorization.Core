## Installing the Authorization.Core package
1. Install the CRFricke.Authorization.Core package:

    - Via dotnet CLI:

        `dotnet add PROJECT package CRFricke.Authorization.Core --version 1.0.0`

    - Via NuGet Package Manager:

       [Visual Studio](https://docs.microsoft.com/en-us/nuget/consume-packages/install-use-packages-visual-studio#find-and-install-a-package)

       [Visual Studio for Mac](https://docs.microsoft.com/en-us/visualstudio/mac/nuget-walkthrough?toc=%2Fnuget%2Ftoc.json&view=vsmac-2019#find-and-install-a-package)

2. (Optional) Extend the `AuthDbContext`, `AuthRole`, and/or `AuthUser` classes.

     The Authorization.Core package extends the Microsoft Identity classes `IdentityDbContext`, 
     `IdentityRole`, and `IdentityUser`. If you extended any of these classes (or need to), 
     you must derive your classes from the associated Authorization.Core classes.

3. Verify the class name specified in `_LoginPartial.cshtml`.

    The name specified in the `@inject` statements for `SignInManager` and `UserManager` must 
    be `AuthUser` (or the name of your derived class).

4. Update Startup.cs (Program.cs in \.Net 6.0):

    Chain an `AddAuthorizationCore` statement to `AddDefaultIdentity`:

```c#
    services.AddDefaultIdentity<AuthUser>(options => options.SignIn.RequireConfirmedAccount = true)
      .AddRoles<AppRole>()
      .AddEntityFrameworkStores<AuthDbContext>()
      .AddAuthorizationCore<AuthDbContext>();
``` 

    **_Note:_** In the example above, `.AddRoles<AppRole>()` is specified because AppRole extends `AuthRole`. 
    If `AuthRole` is not extended, the clause can be omitted.

