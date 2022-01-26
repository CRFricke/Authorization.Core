# Role Management Pages

The Authorization.Core.UI package provides a user interface for managing your application's Roles.
Each Role defines a collection of access right claims that control what a particular group of users 
are allowed to do.

## Role Management Page

The Role Management page is the landing page for all Role management functions.

The default Area for the page is `Authorization`, however it can be changed by specifying a different 
value for the `options.FriendlyAreaName` property of the `AddAuthorizationCoreUI` clause 
as described in [Update Startup.cs](Install-UI-Package.md#update-startupcs-programcs-in-net-60).

Our sample web application specifies "Admin" for FriendlyAreaName. As a result, the application's Role 
Management page is located at `https://localhost:5001/Admin/Role` (as seen in the screen capture 
below).

The following links are available on this page:

  |**Link**|**Action**|
  |---|---|
  |Create New| Displays the Create Role page which is used to configure a new Role.|
  |Edit|Displays the Edit Role page which is used to edit the associated Role.|
  |Details|Displays the Role Details page which is used to view the details of the associated Role.|
  |Delete|Displays the Delete Role page which can be used to delete the associated Role.|

&nbsp;
![Role Management Page](../assets/Role-Management-Page.png)

## Create Role Page

The Create Role page is used to configure a new Role.
You enter a name, an optional description, and select the claims for the Role.
The Search text box can be used to filter the displayed claims.

Click the `Create` button to save the Role, or `Back to List` to cancel the operation.

&nbsp;
![Create Role Page](../assets/Role-Create-Page.png)

## Edit Role Page

The Edit Role page is used to make changes to a Role. You can change the Role's name and description.
When editing a system Role (like "Administrator" or "RoleManager"), the list of claims will be disabled;
otherwise, the assigned claims can also be changed.

When your changes are complete, click `Save` to save the Role, or `Back to List` to cancel the operation.

&nbsp;
![Edit Role Page](../assets/Role-Edit-Page.png)

## Role Details Page

The Role Details page is used to view a Role's configuration.
The page displays the Role's Id, Name, Description, and its claim assignments.

When done, click `Edit` to go to the Edit Role page, or `Back to List` to return to the Role 
Management page.

&nbsp;
![Role Details Page](../assets/Role-Details-Page.png)

## Delete Role Page

The Delete Role page is used to delete the selected Role. In addition to the Role's Id, Name, and 
Description, the page lists any Users that are assigned to the Role.

The page will not allow a system Role to be deleted (a warning message is displayed and the Delete button 
is disabled).

Click `Delete` to delete the Role, or `Back to List` to cancel the operation.

&nbsp;  
![Delete Role Page](../assets/Role-Delete-Page.png)
