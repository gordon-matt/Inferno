using InfernoCMS.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfernoCMS.Data.Migrations
{
    /// <summary>
    /// Names are unique per tenant, and host roles stop being the role of every tenant.
    /// A tenant user who was a member of a host role is moved to a role of the same name in their own tenant.
    /// Profile rows take the tenant of their user.
    /// </summary>
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261122120000_ScopeIdentityToTenant")]
    public class ScopeIdentityToTenant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "RoleNameIndex", table: "AspNetRoles");
            migrationBuilder.DropIndex(name: "UserNameIndex", table: "AspNetUsers");
            migrationBuilder.DropIndex(name: "EmailIndex", table: "AspNetUsers");

            migrationBuilder.Sql("""
                INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp, TenantId)
                SELECT CONVERT(nvarchar(450), NEWID()), needed.Name, needed.NormalizedName, CONVERT(nvarchar(450), NEWID()), needed.TenantId
                FROM (
                    SELECT DISTINCT hostRole.Name, hostRole.NormalizedName, users.TenantId
                    FROM AspNetUserRoles AS link
                    INNER JOIN AspNetRoles AS hostRole ON hostRole.Id = link.RoleId
                    INNER JOIN AspNetUsers AS users ON users.Id = link.UserId
                    WHERE hostRole.TenantId IS NULL
                        AND users.TenantId IS NOT NULL
                        AND NOT EXISTS (
                            SELECT 1
                            FROM AspNetRoles AS existing
                            WHERE existing.TenantId = users.TenantId
                                AND existing.NormalizedName = hostRole.NormalizedName)
                ) AS needed;

                UPDATE link
                SET RoleId = tenantRole.Id
                FROM AspNetUserRoles AS link
                INNER JOIN AspNetRoles AS hostRole ON hostRole.Id = link.RoleId AND hostRole.TenantId IS NULL
                INNER JOIN AspNetUsers AS users ON users.Id = link.UserId AND users.TenantId IS NOT NULL
                INNER JOIN AspNetRoles AS tenantRole ON tenantRole.TenantId = users.TenantId AND tenantRole.NormalizedName = hostRole.NormalizedName
                WHERE link.RoleId <> tenantRole.Id;

                UPDATE profile
                SET TenantId = users.TenantId
                FROM UserProfiles AS profile
                INNER JOIN AspNetUsers AS users ON users.Id = profile.UserId
                WHERE profile.TenantId IS NULL;
                """);

            migrationBuilder.CreateIndex(
                name: "RoleNameHostIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[TenantId] IS NULL AND [NormalizedName] IS NOT NULL");
            migrationBuilder.CreateIndex(
                name: "RoleNameTenantIndex",
                table: "AspNetRoles",
                columns: ["TenantId", "NormalizedName"],
                unique: true,
                filter: "[TenantId] IS NOT NULL AND [NormalizedName] IS NOT NULL");
            migrationBuilder.CreateIndex(
                name: "UserNameHostIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[TenantId] IS NULL AND [NormalizedUserName] IS NOT NULL");
            migrationBuilder.CreateIndex(
                name: "UserNameTenantIndex",
                table: "AspNetUsers",
                columns: ["TenantId", "NormalizedUserName"],
                unique: true,
                filter: "[TenantId] IS NOT NULL AND [NormalizedUserName] IS NOT NULL");
            migrationBuilder.CreateIndex(
                name: "EmailHostIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail",
                unique: true,
                filter: "[TenantId] IS NULL AND [NormalizedEmail] IS NOT NULL");
            migrationBuilder.CreateIndex(
                name: "EmailTenantIndex",
                table: "AspNetUsers",
                columns: ["TenantId", "NormalizedEmail"],
                unique: true,
                filter: "[TenantId] IS NOT NULL AND [NormalizedEmail] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "RoleNameHostIndex", table: "AspNetRoles");
            migrationBuilder.DropIndex(name: "RoleNameTenantIndex", table: "AspNetRoles");
            migrationBuilder.DropIndex(name: "UserNameHostIndex", table: "AspNetUsers");
            migrationBuilder.DropIndex(name: "UserNameTenantIndex", table: "AspNetUsers");
            migrationBuilder.DropIndex(name: "EmailHostIndex", table: "AspNetUsers");
            migrationBuilder.DropIndex(name: "EmailTenantIndex", table: "AspNetUsers");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");
            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");
            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");
        }
    }
}
