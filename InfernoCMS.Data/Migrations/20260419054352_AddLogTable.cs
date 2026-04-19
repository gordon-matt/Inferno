using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfernoCMS.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLogTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Log",
                schema: "inferno",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EventLevel = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    MachineName = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    EventMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorSource = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ErrorClass = table.Column<string>(type: "varchar(512)", unicode: false, maxLength: 512, nullable: true),
                    ErrorMethod = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InnerErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenantId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Log", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Log",
                schema: "inferno");
        }
    }
}
