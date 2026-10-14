using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Migrations;
using PMS.Domain.Entities;

#nullable disable

namespace PMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedPMSRolesAndAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Generate a valid ASP.NET Core Identity password hash.
            var adminUser = new ApplicationUser
            {
                UserName = "admin",
                Email = "admin@pms.local"
            };

            var passwordHasher = new PasswordHasher<ApplicationUser>();
            var passwordHash = passwordHasher.HashPassword(adminUser, "Admin@123");

            // 1. Insert roles if they do not exist.
            migrationBuilder.Sql("""
        IF NOT EXISTS (SELECT 1 FROM dbo.AspNetRoles WHERE NormalizedName = 'ADMIN')
            INSERT INTO dbo.AspNetRoles (Name, NormalizedName, ConcurrencyStamp)
            VALUES ('Admin', 'ADMIN', CONVERT(nvarchar(36), NEWID()));

        IF NOT EXISTS (SELECT 1 FROM dbo.AspNetRoles WHERE NormalizedName = 'PROJECTMANAGER')
            INSERT INTO dbo.AspNetRoles (Name, NormalizedName, ConcurrencyStamp)
            VALUES ('ProjectManager', 'PROJECTMANAGER', CONVERT(nvarchar(36), NEWID()));

        IF NOT EXISTS (SELECT 1 FROM dbo.AspNetRoles WHERE NormalizedName = 'MEMBER')
            INSERT INTO dbo.AspNetRoles (Name, NormalizedName, ConcurrencyStamp)
            VALUES ('Member', 'MEMBER', CONVERT(nvarchar(36), NEWID()));

        IF NOT EXISTS (SELECT 1 FROM dbo.AspNetRoles WHERE NormalizedName = 'USER')
            INSERT INTO dbo.AspNetRoles (Name, NormalizedName, ConcurrencyStamp)
            VALUES ('User', 'USER', CONVERT(nvarchar(36), NEWID()));
        """);

            // 2. Insert admin user if it does not exist.
            migrationBuilder.Sql($"""
        IF NOT EXISTS (
            SELECT 1 FROM dbo.AspNetUsers
            WHERE NormalizedUserName = 'ADMIN'
        )
        BEGIN
            INSERT INTO dbo.AspNetUsers
            (
                UserName,
                NormalizedUserName,
                Email,
                NormalizedEmail,
                EmailConfirmed,
                PasswordHash,
                SecurityStamp,
                ConcurrencyStamp,
                PhoneNumberConfirmed,
                TwoFactorEnabled,
                LockoutEnabled,
                AccessFailedCount,
                DisplayName,
                IsActive,
                CreatedDate,
                CreatedBy
            )
            VALUES
            (
                N'admin',
                N'ADMIN',
                N'admin@pms.local',
                N'ADMIN@PMS.LOCAL',
                1,
                N'{passwordHash}',
                CONVERT(nvarchar(36), NEWID()),
                CONVERT(nvarchar(36), NEWID()),
                0,
                0,
                1,
                0,
                N'Jalal Hussain',
                1,
                SYSUTCDATETIME(),
                0
            );
        END;
        """);

            // 3. Assign admin user to Admin role.
            migrationBuilder.Sql("""
        INSERT INTO dbo.AspNetUserRoles (UserId, RoleId)
        SELECT u.Id, r.Id
        FROM dbo.AspNetUsers u
        CROSS JOIN dbo.AspNetRoles r
        WHERE u.NormalizedUserName = 'ADMIN'
          AND r.NormalizedName = 'ADMIN'
          AND NOT EXISTS (
              SELECT 1
              FROM dbo.AspNetUserRoles ur
              WHERE ur.UserId = u.Id
                AND ur.RoleId = r.Id
          );
        """);

            // 4. Insert role permissions.
            var permissions = new Dictionary<string, string[]>
    {
        {
            "Admin",
            new[]
            {
                "Projects.View",
                "Projects.Create",
                "Projects.Edit",
                "Projects.Delete",
                "Projects.AssignTeam",
                "Tasks.View",
                "Tasks.Create",
                "Tasks.Edit",
                "Users.Manage",
                "User.View",
                "User.Create",
                "User.Edit",
                "User.Delete",
                "Role.View",
                "Role.Create",
                "Role.Edit",
                "Role.Delete",
            }
        },
        {
            "ProjectManager",
            new[]
            {
                "Projects.View",
                "Projects.Create",
                "Projects.Edit",
                "Tasks.View",
                "Tasks.Create",
                "Tasks.Edit",
                "Projects.AssignTeam",
                "Tasks.Delete",
            }
        },
        {
            "Member",
            new[]
            {
                "Projects.View",
                "Projects.Edit",
                "Tasks.View",
                "Tasks.Create",
                "Tasks.Edit"
            }
        },
        {
            "User",
            new[]
            {
                "Projects.View"
            }
        }
    };

            // IMPORTANT: Replace "permission" with the actual value of
            // PermissionConstants.ClaimType in your application.
            const string claimType = "permission";

            foreach (var role in permissions)
            {
                foreach (var permission in role.Value)
                {
                    migrationBuilder.Sql($"""
                IF NOT EXISTS
                (
                    SELECT 1
                    FROM dbo.AspNetRoleClaims rc
                    INNER JOIN dbo.AspNetRoles r ON r.Id = rc.RoleId
                    WHERE r.NormalizedName = N'{role.Key.ToUpperInvariant()}'
                      AND rc.ClaimType = N'{claimType}'
                      AND rc.ClaimValue = N'{permission}'
                )
                BEGIN
                    INSERT INTO dbo.AspNetRoleClaims
                        (RoleId, ClaimType, ClaimValue)
                    SELECT Id, N'{claimType}', N'{permission}'
                    FROM dbo.AspNetRoles
                    WHERE NormalizedName = N'{role.Key.ToUpperInvariant()}';
                END;
                """);
                }
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove the seeded admin user's role assignment and account.
            migrationBuilder.Sql("""
        DELETE ur
        FROM dbo.AspNetUserRoles ur
        INNER JOIN dbo.AspNetUsers u ON u.Id = ur.UserId
        WHERE u.NormalizedUserName = 'ADMIN'
          AND EXISTS (
              SELECT 1
              FROM dbo.AspNetRoles r
              WHERE r.Id = ur.RoleId
                AND r.NormalizedName = 'ADMIN'
          );

        DELETE FROM dbo.AspNetUsers
        WHERE NormalizedUserName = 'ADMIN';
        """);

            // Remove the seeded permissions.
            const string claimType = "permission";

            var permissions = new Dictionary<string, string[]>
    {
        {
            "Admin",
            new[]
            {
                "Projects.View",
                "Projects.Create",
                "Projects.Edit",
                "Projects.Delete",
                "Projects.AssignTeam",
                "Users.Manage"
            }
        },
        {
            "ProjectManager",
            new[]
            {
                "Projects.View",
                "Projects.Create",
                "Projects.Edit",
                "Tasks.View",
                "Tasks.Create",
                "Tasks.Edit",
                "Projects.AssignTeam"
            }
        },
        {
            "Member",
            new[]
            {
                "Projects.View",
                "Tasks.View",
                "Tasks.Create",
                "Tasks.Edit"
            }
        },
        {
            "User",
            new[] { "Projects.View" }
        }
    };

            foreach (var role in permissions)
            {
                foreach (var permission in role.Value)
                {
                    migrationBuilder.Sql($"""
                DELETE rc
                FROM dbo.AspNetRoleClaims rc
                INNER JOIN dbo.AspNetRoles r ON r.Id = rc.RoleId
                WHERE r.NormalizedName = N'{role.Key.ToUpperInvariant()}'
                  AND rc.ClaimType = N'{claimType}'
                  AND rc.ClaimValue = N'{permission}';
                """);
                }
            }

            // Remove seeded roles if they are no longer assigned to users.
            migrationBuilder.Sql("""
        DELETE r
        FROM dbo.AspNetRoles r
        WHERE r.NormalizedName IN
            ('ADMIN', 'PROJECTMANAGER', 'MEMBER', 'USER')
          AND NOT EXISTS (
              SELECT 1
              FROM dbo.AspNetUserRoles ur
              WHERE ur.RoleId = r.Id
          );
        """);
        }
    }
}