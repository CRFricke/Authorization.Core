using Microsoft.EntityFrameworkCore.Migrations;

namespace Authorization.Core.UI.Test.Web.Data.Migrations
{
    public partial class AuthCoreUI : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GivenName",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Surname",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "AspNetRoles",
                nullable: true);

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Description", "Name", "NormalizedName" },
                values: new object[] { "3f1dfcb9-7088-4877-8352-7a6e43063650", "2dbdb73d-9092-444a-ab0d-4f7b13df08c8", "Administrators have access to all portions of the application.", "Administrator", "ADMINISTRATOR" });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Description", "Name", "NormalizedName" },
                values: new object[] { "5e79c59c-b0c1-4857-8f3a-d99dbd1e099f", "527a0a87-6896-43bc-a9b5-1ba15078fef0", "RoleManagers are responsible for managing the application's Roles.", "RoleManager", "ROLEMANAGER" });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Description", "Name", "NormalizedName" },
                values: new object[] { "d29ad18a-eaae-407c-8398-92a99182148a", "e7479557-cd9e-4a7d-939d-a49b0189e25c", "UserManagers are responsible for managing the application's Users.", "UserManager", "USERMANAGER" });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "GivenName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "Surname", "TwoFactorEnabled", "UserName" },
                values: new object[] { "8156bb9b-f56e-4f83-8a11-b0418b843e9b", 0, "0294b69d-de95-4e35-a621-88774d594574", "Admin@company.com", true, null, false, null, "ADMIN@COMPANY.COM", "ADMIN@COMPANY.COM", "AQAAAAEAACcQAAAAEPPGh+zIZ8PSo5IQ1IjPnVqUph0c0utc5Kd37NmA8U1Fhe+MEu3gbxP81sPcxkJaMQ==", null, false, "38dcc602-75e9-487d-8dd2-d4fd25b2770d", null, false, "Admin@company.com" });

            migrationBuilder.InsertData(
                table: "AspNetRoleClaims",
                columns: new[] { "Id", "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { 1, "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/authorizationdecision", "Role.Create", "5e79c59c-b0c1-4857-8f3a-d99dbd1e099f" });

            migrationBuilder.InsertData(
                table: "AspNetRoleClaims",
                columns: new[] { "Id", "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { 2, "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/authorizationdecision", "Role.Delete", "5e79c59c-b0c1-4857-8f3a-d99dbd1e099f" });

            migrationBuilder.InsertData(
                table: "AspNetRoleClaims",
                columns: new[] { "Id", "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { 3, "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/authorizationdecision", "Role.List", "5e79c59c-b0c1-4857-8f3a-d99dbd1e099f" });

            migrationBuilder.InsertData(
                table: "AspNetRoleClaims",
                columns: new[] { "Id", "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { 4, "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/authorizationdecision", "Role.Read", "5e79c59c-b0c1-4857-8f3a-d99dbd1e099f" });

            migrationBuilder.InsertData(
                table: "AspNetRoleClaims",
                columns: new[] { "Id", "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { 5, "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/authorizationdecision", "Role.Update", "5e79c59c-b0c1-4857-8f3a-d99dbd1e099f" });

            migrationBuilder.InsertData(
                table: "AspNetRoleClaims",
                columns: new[] { "Id", "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { 6, "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/authorizationdecision", "Role.UpdateClaims", "5e79c59c-b0c1-4857-8f3a-d99dbd1e099f" });

            migrationBuilder.InsertData(
                table: "AspNetRoleClaims",
                columns: new[] { "Id", "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { 7, "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/authorizationdecision", "User.Create", "d29ad18a-eaae-407c-8398-92a99182148a" });

            migrationBuilder.InsertData(
                table: "AspNetRoleClaims",
                columns: new[] { "Id", "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { 8, "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/authorizationdecision", "User.Delete", "d29ad18a-eaae-407c-8398-92a99182148a" });

            migrationBuilder.InsertData(
                table: "AspNetRoleClaims",
                columns: new[] { "Id", "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { 9, "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/authorizationdecision", "User.List", "d29ad18a-eaae-407c-8398-92a99182148a" });

            migrationBuilder.InsertData(
                table: "AspNetRoleClaims",
                columns: new[] { "Id", "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { 10, "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/authorizationdecision", "User.Read", "d29ad18a-eaae-407c-8398-92a99182148a" });

            migrationBuilder.InsertData(
                table: "AspNetRoleClaims",
                columns: new[] { "Id", "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { 11, "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/authorizationdecision", "User.Update", "d29ad18a-eaae-407c-8398-92a99182148a" });

            migrationBuilder.InsertData(
                table: "AspNetRoleClaims",
                columns: new[] { "Id", "ClaimType", "ClaimValue", "RoleId" },
                values: new object[] { 12, "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/authorizationdecision", "User.UpdateClaims", "d29ad18a-eaae-407c-8398-92a99182148a" });

            migrationBuilder.InsertData(
                table: "AspNetUserClaims",
                columns: new[] { "Id", "ClaimType", "ClaimValue", "UserId" },
                values: new object[] { 1, "http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Administrator", "8156bb9b-f56e-4f83-8a11-b0418b843e9b" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "3f1dfcb9-7088-4877-8352-7a6e43063650");

            migrationBuilder.DeleteData(
                table: "AspNetUserClaims",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "5e79c59c-b0c1-4857-8f3a-d99dbd1e099f");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d29ad18a-eaae-407c-8398-92a99182148a");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "8156bb9b-f56e-4f83-8a11-b0418b843e9b");

            migrationBuilder.DropColumn(
                name: "GivenName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Surname",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "AspNetRoles");
        }
    }
}
