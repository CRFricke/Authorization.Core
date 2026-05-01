using Microsoft.Extensions.Logging;

namespace CRFricke.Authorization.Core;

internal static partial class LoggerExtensions
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Debug,
        Message = "{ClassName} for \"{AppClaimRequirement}\" not met for user '{UserName}' - user ID is null.")]
    internal static partial void LogClaimRequirementNotMetNoUserId(
        this ILogger logger,
        string className,
        AppClaimRequirement appClaimRequirement,
        string? userName);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Information,
        Message = "{ClassName} of \"{Claim}\" for {ResourceType} '{ObjectName}' not met by '{UserName}' - restricted operation on system User or Role.")]
    internal static partial void LogClaimRequirementNotMetSystemObject(
        this ILogger logger,
        string className,
        string claim,
        string resourceType,
        string objectName,
        string? userName);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Debug,
        Message = "{ClassName} for \"{AppClaimRequirement}\" met for user '{UserName}' via {RoleName} role.")]
    internal static partial void LogClaimRequirementMetViaAdministrator(
        this ILogger logger,
        string className,
        AppClaimRequirement appClaimRequirement,
        string? userName,
        string roleName);

    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Debug,
        Message = "{ClassName} for \"{AppClaimRequirement}\" met for user '{UserName}'.")]
    internal static partial void LogClaimRequirementMet(
        this ILogger logger,
        string className,
        AppClaimRequirement appClaimRequirement,
        string? userName);

    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Information,
        Message = "{RoleType} '{RoleName}' (ID: {RoleId}) has been created.")]
    internal static partial void LogRoleCreated(
        this ILogger logger,
        string roleType,
        string roleName,
        string roleId);

    [LoggerMessage(
        EventId = 6,
        Level = LogLevel.Information,
        Message = "{UserType} '{UserEmail}' (ID: {UserId}) has been created.")]
    internal static partial void LogUserCreated(
        this ILogger logger,
        string userType,
        string userEmail,
        string userId);

    [LoggerMessage(
        EventId = 7,
        Level = LogLevel.Error,
        Message = "SaveChangesAsync() method failed.")]
    internal static partial void LogSaveChangesFailed(
        this ILogger logger,
        Exception ex);

    [LoggerMessage(
        EventId = 8,
        Level = LogLevel.Information,
        Message = "Fixup successful for {UpdateCount} {UpdateEntity}.")]
    internal static partial void LogFixupSuccessful(
        this ILogger logger,
        int updateCount,
        string updateEntity);

    [LoggerMessage(
        EventId = 9,
        Level = LogLevel.Error,
        Message = "FixupUserClaimValuesAsync() method failed.")]
    internal static partial void LogFixupUserClaimValuesFailed(
        this ILogger logger,
        Exception ex);
}
