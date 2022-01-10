using Microsoft.EntityFrameworkCore.Migrations;

namespace Authorization.Core.UI.Test.Web.Data.Migrations
{
    public partial class AuthCoreUiSchema : Migration
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
			migrationBuilder.Sql(
				@"PRAGMA foreign_keys = 0;
                DROP TABLE IF EXISTS AspNetUsers_temp_table;
				CREATE TABLE AspNetUsers_temp_table AS SELECT * FROM AspNetUsers;
				DROP TABLE AspNetUsers;
				CREATE TABLE AspNetUsers (
					Id                   TEXT    NOT NULL CONSTRAINT PK_AspNetUsers PRIMARY KEY,
					UserName             TEXT,
					NormalizedUserName   TEXT,
					Email                TEXT,
					NormalizedEmail      TEXT,
					EmailConfirmed       INTEGER NOT NULL,
					PasswordHash         TEXT,
					SecurityStamp        TEXT,
					ConcurrencyStamp     TEXT,
					PhoneNumber          TEXT,
					PhoneNumberConfirmed INTEGER NOT NULL,
					TwoFactorEnabled     INTEGER NOT NULL,
					LockoutEnd           TEXT,
					LockoutEnabled       INTEGER NOT NULL,
					AccessFailedCount    INTEGER NOT NULL
				);
				INSERT INTO AspNetUsers (
					Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed,
					PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed,
					TwoFactorEnabled, LockoutEnd,LockoutEnabled, AccessFailedCount
				)
				SELECT 
					Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed,
					PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed,
					TwoFactorEnabled, LockoutEnd,LockoutEnabled, AccessFailedCount
				  FROM AspNetUsers_temp_table;
				DROP TABLE AspNetUsers_temp_table;
				CREATE INDEX EmailIndex ON AspNetUsers (NormalizedEmail);
				CREATE UNIQUE INDEX UserNameIndex ON AspNetUsers(NormalizedUserName);
				PRAGMA foreign_keys = 1;"
				);

			migrationBuilder.Sql(
				@"PRAGMA foreign_keys = 0;
                DROP TABLE IF EXISTS AspNetRoles_temp_table;
				CREATE TABLE AspNetRoles_temp_table AS SELECT * FROM AspNetRoles;
				DROP TABLE AspNetRoles;
				CREATE TABLE AspNetRoles (
					Id               TEXT NOT NULL CONSTRAINT PK_AspNetRoles PRIMARY KEY,
					Name             TEXT,
					NormalizedName   TEXT,
					ConcurrencyStamp TEXT
				);
				INSERT INTO AspNetRoles (
					Id, Name, NormalizedName, ConcurrencyStamp
					)
				SELECT
					Id, Name, NormalizedName, ConcurrencyStamp
				  FROM AspNetRoles_temp_table;
				DROP TABLE AspNetRoles_temp_table;
				CREATE UNIQUE INDEX RoleNameIndex ON AspNetRoles (NormalizedName);
				PRAGMA foreign_keys = 1;"
				);
        }
	}
}
