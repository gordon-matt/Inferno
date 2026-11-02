using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfernoCMS.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemovePageTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PageTypes",
                schema: "inferno");

            migrationBuilder.DropColumn(
                name: "PageTypeId",
                schema: "inferno",
                table: "Pages");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PageTypeId",
                schema: "inferno",
                table: "Pages",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "PageTypes",
                schema: "inferno",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LayoutPath = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageTypes", x => x.Id);
                });
        }
    }
}
