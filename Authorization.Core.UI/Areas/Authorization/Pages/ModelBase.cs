using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

#pragma warning disable IDE0130 // Namespace does not match folder structure

namespace CRFricke.Authorization.Core.UI.Pages;

public class ModelBase : PageModel
{
    /// <summary>
    /// Sends a notification message of the specified <see cref="Severity"/> to the specified page.
    /// </summary>
    /// <param name="toType">The <see cref="Type"/> of the page to receive the message.</param>
    /// <param name="severity">The severity of the message.</param>
    /// <param name="message">The message to be sent.</param>
    internal void SendNotification(Type toType, Severity severity, string message)
    {
        var notifications = TempData.GetNotifications(toType.FullName!) ?? [];
        notifications.Add(new Notification { Severity = severity, Message = message });

        TempData.SetNotifications(toType.FullName!, notifications);
    }

    ///<inheritdoc/>
    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var pageType = GetType();
        if (pageType.IsGenericType)
        {
            pageType = pageType.BaseType!;
        }

        Notifications = TempData.GetNotifications(pageType.FullName!);

        base.OnPageHandlerExecuting(context);
    }

    public IList<Notification>? Notifications { get; private set; }
}
