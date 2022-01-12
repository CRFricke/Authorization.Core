# Authorization.Core

The Authorization.Core package provides an efficient implementation of a claims-based authorization 
framework that sits on top of Microsoft's ASP.NET Core Identity Framework. The package can be used 
on its own or with the Authorization.Core.UI package to provide a rich GUI for managing your 
application's Users and Roles.

## The Authorization.Core Package

The Core package exposes the `AuthUser` and `AuthRole` classes. These classes extend the 
`IdentityUser` and `IdentityRole` classes provided by the ASP.NET Core Identity Framework. 
The package uses the Identity Framework's `IdentityRoleClaim` class to map user defined claims 
to Roles. It uses the `IdentityUserClaim` class to map Roles to Users.

The Core package also exposes the `AuthDbContext` and a generic `AuthDbContext<TUser, TRole>` 
class. These classes extend the Identity Framework's `IdentityDbContext<TUser, TRole, string>` 
class. They provide a virtual `SeedDatabase` method which is used to ensure that the Administrator 
Role and User account exist in the database during application startup.

If you will be installing the Authorization.Core.UI package, you can skip ahead to the next section. 
Otherwise, you can use the following steps to install and configure the Core package.

1.   [Install the Authorization.Core package](docs/Install-Core-Package.md)
2.   [Configure your Roles and Users](docs/Configure-Roles.md)
3.   [Testing and Enforcing Authorization](docs/Enforcing-Authorization.md)

## The Authorization.Core.UI Package

The Authorization.Core.UI package provides the UI that is used to manage your Users and Roles.

The following steps can be used to install and configure the package.

1.   [Installing the Authorization.Core.UI package](docs/Install-Core.UI-Package.md)
