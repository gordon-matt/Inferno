using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfernoCMS.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveEntityTypeContentBlocks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM [localize].[LocalizableProperties]
                WHERE [EntityType] = 'Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Entities.EntityTypeContentBlock'
                """);

            migrationBuilder.DropTable(
                name: "EntityTypeContentBlocks",
                schema: "inferno");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EntityTypeContentBlocks",
                schema: "inferno",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BlockName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    BlockType = table.Column<string>(type: "varchar(1024)", unicode: false, maxLength: 1024, nullable: false),
                    BlockValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomTemplatePath = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    EntityId = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    EntityType = table.Column<string>(type: "varchar(512)", unicode: false, maxLength: 512, nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ZoneId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityTypeContentBlocks", x => x.Id);
                });
        }
    }
}
