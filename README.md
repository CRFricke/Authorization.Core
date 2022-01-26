# Authorization.Core

The Authorization.Core package provides an efficient implementation of a rights-based authorization 
framework that sits on top of Microsoft's ASP.NET Core Identity Framework. The package can be used 
on its own or with the Authorization.Core.UI package to provide a rich GUI for managing your 
application's Users and Roles.

## The Authorization.Core Package

The Core package uses `Claims` to represent the access rights defined by the application. These Claims are 
assigned to Roles using the ASP.NET Core Identity Framework's `IdentityRoleClaim` class. Roles are assigned 
to Users via the Identity Framework's `IdentityUserClaim` class.

The package exposes the `AuthUser` and `AuthRole` classes which extend the Identity Framework's `IdentityUser` 
and `IdentityRole` classes. 

The Core package also exposes the `AuthDbContext` and a generic `AuthDbContext<TUser, TRole>` class. 
These classes extend the Identity Framework's `IdentityDbContext<TUser, TRole, string>` class. They 
provide a virtual `SeedDatabase` method which is used to ensure that the Administrator Role and User 
account exist in the database during application startup. The method can be overriden in a derived class to 
provide similar functionality for your application.

If you will be installing the Authorization.Core.UI package, you can skip ahead to the next section. 
Otherwise, you can use the following steps to install and configure the Core package.

1. [Install the Authorization.Core package](docs/Install-Core-Package.md)
2. [Configure your Roles and Users](docs/Configure-Core-Roles-and-Users.md)
3. [Testing and Enforcing Authorization](docs/Enforcing-Authorization.md)

## The Authorization.Core.UI Package

The UI package provides the GUI that is used to manage your Users and Roles. It exposes the `AuthUiRole` 
class which extends the Core package's `AuthRole` class and adds a Description property. The UI package 
also exposes the `AuthUiUser` class which extends `AuthUser` and  adds the `GivenName` and `Surname` 
properties. And finally, the UI package exposes the `AuthUiContext` and `AuthUiContext<TUser, TRole>` 
classes that derive from `AuthDbContext` and `AuthDbContext<TUser, TRole>` respectively.

The following steps can be used to install and configure the package.

1. [Installing the Authorization.Core.UI package](docs/Install-UI-Package.md)
2. [Configure your Roles and Users](docs/Configure-UI-Roles-and-Users.md)
3. [Testing and Enforcing Authorization](docs/Enforcing-Authorization.md)

The following sections describe the UI package's management pages.

- [Role Management Pages](docs/UI-Role-Management-Page.md)
- [User Management Pages](docs/UI-User-Management-Page.md)