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

It is also possible to check authorization programically in a Razor .cshtml file:

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

Or in the Razor model (or MVC Controller):

```csharp
public class EditModel : PageModel
{
    private readonly IAuthorizationManager _authManager;
    private readonly IRepository<Calendar> _calendarRepo;
    private readonly ILogger<EditModel> _logger;

    public EditModel(IAuthorizationManager authManager, IRepository<Calendar> calendarRepo, ILogger<EditModel> logger)
    {
        _authManager = authManager;
        _calendarRepo = calendarRepo;
        _logger = logger;
    }

    public async Task OnGetAsync(string id)
    {
        var calendarEvent = _calendarRepo.Find(id);
        if (calendarEvent == null)
        {
            return NotFound();
        }

        var authResult = await _authManager.AuthorizeAsync(
            User, calendarEvent, new AppClaimRequirement(AppClaims.Calendar.Update)
            );
        if (!authResult.Succeeded)
        {
            HandleError(user, authResult.Errors);
        }

        ...

    }
}
```

Or using Microsoft's authorization service:

```csharp
public class EditModel : PageModel
{
    private readonly IAuthorizationService _authService;
    private readonly IRepository<Calendar> _calendarRepo;
    private readonly ILogger<EditModel> _logger;

    public EditModel(IAuthorizationService authService, IRepository<Calendar> calendarRepo, ILogger<EditModel> logger)
    {
        _authService = authService;
        _calendarRepo = calendarRepo;
        _logger = logger;
    }

    public async Task OnGetAsync(string id)
    {
        var calendarEvent = _calendarRepo.Find(id);
        if (calendarEvent == null)
        {
            return NotFound();
        }

        var requirements = new[] { new AppClaimRequirement(AppClaims.Calendar.Update) } ;
        var authResult = await _authService.AuthorizeAsync(
            User, calendarEvent, new AuthorizationPolicy(requirements, Array.Empty<string>())
            );
        if (!authResult.Succeeded)
        {
            HandleError(calendarEvent, authResult.Failure);
        }

        ...

    }
}
```

If your application updates the claims associated with a Role, it also needs to refresh the role cache to 
pick up the change:

```csharp
    if (RoleModel.ClaimsUpdated)
    {
        _authManager.RefreshRole(role.Id);
    }
```

Similarly, if your application updates the roles associated with a User, it also needs to refresh the user 
cache to pick up the change:

```csharp
    if (UserModel.ClaimsUpdated)
    {
        _authManager.RefreshUser(user.Id);
    }
```
