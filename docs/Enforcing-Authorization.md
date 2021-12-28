## Testing and Enforcing Authorization

The simplest way to enforce authorization is to add the `RequiresClaims` attribute to the `PageModel` 
(or `Controller`) class of any page that requires authorization:

```csharp
[RequiresClaims(AppClaims.Calendar.Update)]
public class EditModel : PageModel
{
    ...
}
```

It is also possible to check authorization programically:

```csharp
var result = await authorizationManager.AuthorizeAsync(
    User, calendar, new AppClaimRequirement(AppClaims.Calendar.Update)
    );
if (!result.Succeeded)
{
    HandleError(user, result.Errors);
}
```

Or in a Razor .cshtml file:

```csharp
@inject IAuthorizationManager authManager

<ul class="navbar-nav flex-grow-1">
    @if (await authManager.IsAuthorizedAsync(User, AppClaims.Calendar.List))
    {
        <li class="nav-item">
            <a class="nav-link" asp-area="Admin" asp-page="/Calendar">Calendar Management</a>
        </li>
    }
</ul>
```