using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfernoCMS.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNewSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Inferno_Tenants",
                table: "Inferno_Tenants");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Inferno_Settings",
                table: "Inferno_Settings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Inferno_ScheduledTasks",
                table: "Inferno_ScheduledTasks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Inferno_LocalizableStrings",
                table: "Inferno_LocalizableStrings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Inferno_LocalizableProperties",
                table: "Inferno_LocalizableProperties");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Inferno_Languages",
                table: "Inferno_Languages");

            migrationBuilder.EnsureSchema(
                name: "inferno");

            migrationBuilder.RenameTable(
                name: "Inferno_Tenants",
                newName: "Tenants",
                newSchema: "inferno");

            migrationBuilder.RenameTable(
                name: "Inferno_Settings",
                newName: "Settings",
                newSchema: "inferno");

            migrationBuilder.RenameTable(
                name: "Inferno_ScheduledTasks",
                newName: "ScheduledTasks",
                newSchema: "inferno");

            migrationBuilder.RenameTable(
                name: "Inferno_LocalizableStrings",
                newName: "LocalizableStrings",
                newSchema: "inferno");

            migrationBuilder.RenameTable(
                name: "Inferno_LocalizableProperties",
                newName: "LocalizableProperties",
                newSchema: "inferno");

            migrationBuilder.RenameTable(
                name: "Inferno_Languages",
                newName: "Languages",
                newSchema: "inferno");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetUserTokens",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserTokens",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "ProviderKey",
                table: "AspNetUserLogins",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserLogins",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "Hosts",
                schema: "inferno",
                table: "Tenants",
                type: "nvarchar(1024)",
                maxLength: 1024,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1024)",
                oldMaxLength: 1024);

            migrationBuilder.AlterColumn<string>(
                name: "Value",
                schema: "inferno",
                table: "Settings",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "TextValue",
                schema: "inferno",
                table: "LocalizableStrings",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CultureCode",
                schema: "inferno",
                table: "LocalizableStrings",
                type: "varchar(10)",
                unicode: false,
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldUnicode: false,
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "Value",
                schema: "inferno",
                table: "LocalizableProperties",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CultureCode",
                schema: "inferno",
                table: "LocalizableProperties",
                type: "varchar(10)",
                unicode: false,
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldUnicode: false,
                oldMaxLength: 10);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tenants",
                schema: "inferno",
                table: "Tenants",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Settings",
                schema: "inferno",
                table: "Settings",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ScheduledTasks",
                schema: "inferno",
                table: "ScheduledTasks",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LocalizableStrings",
                schema: "inferno",
                table: "LocalizableStrings",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LocalizableProperties",
                schema: "inferno",
                table: "LocalizableProperties",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Languages",
                schema: "inferno",
                table: "Languages",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Tenants",
                schema: "inferno",
                table: "Tenants");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Settings",
                schema: "inferno",
                table: "Settings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ScheduledTasks",
                schema: "inferno",
                table: "ScheduledTasks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LocalizableStrings",
                schema: "inferno",
                table: "LocalizableStrings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LocalizableProperties",
                schema: "inferno",
                table: "LocalizableProperties");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Languages",
                schema: "inferno",
                table: "Languages");

            migrationBuilder.RenameTable(
                name: "Tenants",
                schema: "inferno",
                newName: "Inferno_Tenants");

            migrationBuilder.RenameTable(
                name: "Settings",
                schema: "inferno",
                newName: "Inferno_Settings");

            migrationBuilder.RenameTable(
                name: "ScheduledTasks",
                schema: "inferno",
                newName: "Inferno_ScheduledTasks");

            migrationBuilder.RenameTable(
                name: "LocalizableStrings",
                schema: "inferno",
                newName: "Inferno_LocalizableStrings");

            migrationBuilder.RenameTable(
                name: "LocalizableProperties",
                schema: "inferno",
                newName: "Inferno_LocalizableProperties");

            migrationBuilder.RenameTable(
                name: "Languages",
                schema: "inferno",
                newName: "Inferno_Languages");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetUserTokens",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserTokens",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "ProviderKey",
                table: "AspNetUserLogins",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserLogins",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Hosts",
                table: "Inferno_Tenants",
                type: "nvarchar(1024)",
                maxLength: 1024,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(1024)",
                oldMaxLength: 1024,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "Inferno_Settings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TextValue",
                table: "Inferno_LocalizableStrings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CultureCode",
                table: "Inferno_LocalizableStrings",
                type: "varchar(10)",
                unicode: false,
                maxLength: 10,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldUnicode: false,
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "Inferno_LocalizableProperties",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CultureCode",
                table: "Inferno_LocalizableProperties",
                type: "varchar(10)",
                unicode: false,
                maxLength: 10,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldUnicode: false,
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Inferno_Tenants",
                table: "Inferno_Tenants",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Inferno_Settings",
                table: "Inferno_Settings",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Inferno_ScheduledTasks",
                table: "Inferno_ScheduledTasks",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Inferno_LocalizableStrings",
                table: "Inferno_LocalizableStrings",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Inferno_LocalizableProperties",
                table: "Inferno_LocalizableProperties",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Inferno_Languages",
                table: "Inferno_Languages",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    PermissionId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => new { x.PermissionId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_RolePermissions_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleId",
                table: "RolePermissions",
                column: "RoleId");
        }
    }
}
