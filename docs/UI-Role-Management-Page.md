# Role Management User Interface

The Authorization.Core.UI package provides a user interface for managing your application's Roles.
Each Role defines a collection of access right claims that control what a particular group of users 
are allowed to do.

## Role Management Page

The Role Management Page is the landing page for all Role management functions.

The default Area for the page is `Authorization`, however it can be changed by specifying a different 
value for the `options.FriendlyAreaName` property of the `AddAuthorizationCoreUI` clause 
as described in [Update Startup.cs](Install-UI-Package.md#update-startupcs-programcs-in-net-60).

Our sample web application specifies "Admin" for FriendlyAreaName. As a result, the Role Management 
Page is located at ```https://localhost:5001/Admin/Role``` (as seen in the screen capture below).

The following links are available on this page:

  |**Link**|**Action**|
  |---|---|
  |Create New| Displays Create Role page. Used to configure a new Role.|
  |Edit|Displays Edit Role page. Used to edit the associated Role.|
  |Details|Displays Role Details page. Used to view the details of the associated Role.|
  |Delete|Displays Delete Role page. Used to delete the associated Role.|

![Role Management Page](../assets/Role-Management-Page.png)

## Create Role Page

![Create Role Page](../assets/Role-Create-Page.png)

## Edit Role Page

![Edit Role Page](../assets/Role-Edit-Page.png)

## Role Details Page

![Role Details Page](../assets/Role-Details-Page.png)

## Delete Role Page

![Delete Role Page](../assets/Role-Delete-Page.png)
