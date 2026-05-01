namespace Authorization.Core.UI.Test.Web;

internal static partial class LoggerExtensions
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "{RoleType} '{RoleName}' (ID: {RoleId}) has been created.")]
    public static partial void LogRoleCreated(
        this ILogger logger,
        string roleType,
        string roleName,
        string roleId);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Information,
        Message = "{UserType} '{UserEmail}' (ID: {UserId}) has been created.")]
    public static partial void LogUserCreated(
        this ILogger logger,
        string userType,
        string userEmail,
        string userId);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Error,
        Message = "SaveChangesAsync() method failed.")]
    public static partial void LogSaveChangesFailed(
        this ILogger logger,
        Exception exception);
}
