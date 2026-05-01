using Microsoft.Extensions.Logging;

namespace CRFricke.Authorization.Core.UI;

internal static partial class LoggerExtensions
{
    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Warning,
        Message = "'{principalEmail}' attempted to create {roleType} with elevated privileges.")]
    public static partial void LogAttemptedElevatedPrivilegeRoleCreation(
        this ILogger logger,
        string? principalEmail,
        string roleType);

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Error,
        Message = "'{principalEmail}' could not create {roleType} '{roleName}' (ID: {roleId}).")]
    public static partial void LogRoleCreationFailed(
        this ILogger logger,
        Exception exception,
        string? principalEmail,
        string roleType,
        string? roleName,
        string roleId);

    [LoggerMessage(
        EventId = 1003,
        Level = LogLevel.Information,
        Message = "'{principalEmail}' created {roleType} '{roleName}' (ID: {roleId}).")]
    public static partial void LogRoleCreated(
        this ILogger logger,
        string? principalEmail,
        string roleType,
        string? roleName,
        string roleId);

    [LoggerMessage(
        EventId = 1004,
        Level = LogLevel.Warning,
        Message = "'{principalEmail}' attempted to delete system {roleType} '{roleName}' (ID: {roleId}).")]
    public static partial void LogAttemptedSystemRoleDeletion(
        this ILogger logger,
        string? principalEmail,
        string roleType,
        string? roleName,
        string roleId);

    [LoggerMessage(
        EventId = 1005,
        Level = LogLevel.Error,
        Message = "'{principalEmail}' could not delete {roleType} '{roleName}' (ID: {roleId}).")]
    public static partial void LogRoleDeletionFailed(
        this ILogger logger,
        Exception exception,
        string? principalEmail,
        string roleType,
        string? roleName,
        string roleId);

    [LoggerMessage(
        EventId = 1006,
        Level = LogLevel.Information,
        Message = "'{principalEmail}' deleted {roleType} '{roleName}' (ID: {roleId}).")]
    public static partial void LogRoleDeleted(
        this ILogger logger,
        string? principalEmail,
        string roleType,
        string? roleName,
        string roleId);

    [LoggerMessage(
        EventId = 1007,
        Level = LogLevel.Warning,
        Message = "'{principalEmail}' attempted to update the claims of system {roleType} '{roleName}' (ID: {roleId}).")]
    public static partial void LogAttemptedSystemRoleClaimsUpdate(
        this ILogger logger,
        string? principalEmail,
        string roleType,
        string? roleName,
        string roleId);

    [LoggerMessage(
        EventId = 1008,
        Level = LogLevel.Warning,
        Message = "'{principalEmail}' attempted to give {roleType} '{roleName}' (ID: {roleId}) elevated privileges.")]
    public static partial void LogAttemptedElevatedPrivilegeRoleUpdate(
        this ILogger logger,
        string? principalEmail,
        string roleType,
        string? roleName,
        string roleId);

    [LoggerMessage(
        EventId = 1009,
        Level = LogLevel.Error,
        Message = "'{principalEmail}' could not update {roleType} '{roleName}' (ID: {roleId}).")]
    public static partial void LogRoleUpdateFailed(
        this ILogger logger,
        Exception exception,
        string? principalEmail,
        string roleType,
        string? roleName,
        string roleId);

    [LoggerMessage(
        EventId = 1010,
        Level = LogLevel.Information,
        Message = "'{principalEmail}' updated {roleType} '{roleName}' (ID: {roleId}).")]
    public static partial void LogRoleUpdated(
        this ILogger logger,
        string? principalEmail,
        string roleType,
        string? roleName,
        string roleId);

    [LoggerMessage(
        EventId = 1011,
        Level = LogLevel.Warning,
        Message = "'{principalEmail}' attempted to create {userType} with elevated privileges.")]
    public static partial void LogAttemptedElevatedPrivilegeUserCreation(
        this ILogger logger,
        string? principalEmail,
        string userType);

    [LoggerMessage(
        EventId = 1012,
        Level = LogLevel.Error,
        Message = "'{principalEmail}' could not create {userType} '{userEmail}' (ID '{userId}').")]
    public static partial void LogUserCreationFailed(
        this ILogger logger,
        Exception exception,
        string? principalEmail,
        string userType,
        string? userEmail,
        string userId);

    [LoggerMessage(
        EventId = 1013,
        Level = LogLevel.Information,
        Message = "'{principalEmail}' created {userType} '{userEmail}' (ID '{userId}').")]
    public static partial void LogUserCreated(
        this ILogger logger,
        string? principalEmail,
        string userType,
        string? userEmail,
        string userId);

    [LoggerMessage(
        EventId = 1014,
        Level = LogLevel.Debug,
        Message = "User password validation failed: {errors}.")]
    public static partial void LogPasswordValidationFailed(
        this ILogger logger,
        string errors);

    [LoggerMessage(
        EventId = 1015,
        Level = LogLevel.Warning,
        Message = "'{principalEmail}' attempted to delete system {userType} '{userEmail}' (ID '{userId}').")]
    public static partial void LogAttemptedSystemUserDeletion(
        this ILogger logger,
        string? principalEmail,
        string userType,
        string? userEmail,
        string userId);

    [LoggerMessage(
        EventId = 1016,
        Level = LogLevel.Error,
        Message = "'{principalEmail}' could not delete {userType} '{userEmail}' (ID '{userId}').")]
    public static partial void LogUserDeletionFailed(
        this ILogger logger,
        Exception exception,
        string? principalEmail,
        string userType,
        string? userEmail,
        string userId);

    [LoggerMessage(
        EventId = 1017,
        Level = LogLevel.Information,
        Message = "'{principalEmail}' deleted {userType} '{userEmail}' (ID '{userId}').")]
    public static partial void LogUserDeleted(
        this ILogger logger,
        string? principalEmail,
        string userType,
        string? userEmail,
        string userId);

    [LoggerMessage(
        EventId = 1018,
        Level = LogLevel.Warning,
        Message = "'{principalEmail}' attempted to update the Roles of system {userType} '{userEmail}' (ID '{userId}')")]
    public static partial void LogAttemptedSystemUserRolesUpdate(
        this ILogger logger,
        string? principalEmail,
        string userType,
        string? userEmail,
        string userId);

    [LoggerMessage(
        EventId = 1019,
        Level = LogLevel.Warning,
        Message = "'{principalEmail}' attempted to give {userType} '{userEmail}' (ID '{userId}') elevated privileges.")]
    public static partial void LogAttemptedElevatedPrivilegeUserUpdate(
        this ILogger logger,
        string? principalEmail,
        string userType,
        string? userEmail,
        string userId);

    [LoggerMessage(
        EventId = 1020,
        Level = LogLevel.Warning,
        Message = "'{principalEmail}' attempted to elevate their own privileges.")]
    public static partial void LogAttemptedSelfPrivilegeElevation(
        this ILogger logger,
        string? principalEmail);

    [LoggerMessage(
        EventId = 1021,
        Level = LogLevel.Error,
        Message = "'{principalEmail}' could not update {userType} '{userEmail}' (ID '{userId}').")]
    public static partial void LogUserUpdateFailed(
        this ILogger logger,
        Exception exception,
        string? principalEmail,
        string userType,
        string? userEmail,
        string userId);

    [LoggerMessage(
        EventId = 1022,
        Level = LogLevel.Information,
        Message = "'{principalEmail}' updated {userType} '{userEmail}' (ID '{userId}').")]
    public static partial void LogUserUpdated(
        this ILogger logger,
        string? principalEmail,
        string userType,
        string? userEmail,
        string userId);

    [LoggerMessage(
        EventId = 1023,
        Level = LogLevel.Information,
        Message = "{roleType} '{roleName}' (ID: {roleId}) has been updated.")]
    public static partial void LogRoleSeeded(
        this ILogger logger,
        string roleType,
        string? roleName,
        string roleId);

    [LoggerMessage(
        EventId = 1024,
        Level = LogLevel.Information,
        Message = "{roleType} '{roleName}' (ID: {roleId}) has been created.")]
    public static partial void LogRoleCreatedDuringSeed(
        this ILogger logger,
        string roleType,
        string? roleName,
        string roleId);

    [LoggerMessage(
        EventId = 1025,
        Level = LogLevel.Error,
        Message = "SaveChangesAsync() method failed.")]
    public static partial void LogSaveChangesFailed(
        this ILogger logger,
        Exception exception);
}
