namespace Authorization.Core.UI.Tests.Playwright.Infrastructure;

public record Login(string Email, string Password);

internal static class Logins
{
    public static Login Administrator => new("admin@company.com", "Administrat0r!");

    public static Login RoleManager => new("role.guy@company.com", "R0le.Guy!");

    public static Login UserManager => new("user.guy@company.com", "U5er.Guy!");
}
