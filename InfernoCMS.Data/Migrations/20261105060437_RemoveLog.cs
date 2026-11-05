using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfernoCMS.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Log",
                schema: "inferno");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Log",
                schema: "inferno",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ErrorClass = table.Column<string>(type: "varchar(512)", unicode: false, maxLength: 512, nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorMethod = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    ErrorSource = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    EventDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EventLevel = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: false),
                    EventMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InnerErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MachineName = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    TenantId = table.Column<int>(type: "int", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Log", x => x.Id);
                });
        }
    }
}
