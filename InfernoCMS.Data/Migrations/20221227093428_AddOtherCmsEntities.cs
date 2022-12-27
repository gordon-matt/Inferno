using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfernoCMS.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOtherCmsEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BlogCategories",
                schema: "inferno",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    UrlSlug = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BlogTags",
                schema: "inferno",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    UrlSlug = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogTags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContentBlocks",
                schema: "inferno",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BlockName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    BlockType = table.Column<string>(type: "varchar(1024)", unicode: false, maxLength: 1024, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ZoneId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    BlockValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomTemplatePath = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentBlocks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EntityTypeContentBlocks",
                schema: "inferno",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntityType = table.Column<string>(type: "varchar(512)", unicode: false, maxLength: 512, nullable: false),
                    EntityId = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    BlockName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    BlockType = table.Column<string>(type: "varchar(1024)", unicode: false, maxLength: 1024, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ZoneId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    BlockValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomTemplatePath = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityTypeContentBlocks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MenuItems",
                schema: "inferno",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MenuId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Url = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CssClass = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: true),
                    Position = table.Column<int>(type: "int", nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Enabled = table.Column<bool>(type: "bit", nullable: false),
                    IsExternalUrl = table.Column<bool>(type: "bit", nullable: false),
                    RefId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Menus",
                schema: "inferno",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    UrlFilter = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TenantId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Menus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SitemapConfig",
                schema: "inferno",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChangeFrequency = table.Column<byte>(type: "tinyint", nullable: false),
                    Priority = table.Column<float>(type: "real", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SitemapConfig", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Zones",
                schema: "inferno",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Zones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BlogPosts",
                schema: "inferno",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    DateCreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    Headline = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    TeaserImageUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ShortDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FullDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UseExternalLink = table.Column<bool>(type: "bit", nullable: false),
                    ExternalLink = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    MetaKeywords = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    MetaDescription = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TenantId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogPosts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlogPosts_BlogCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalSchema: "inferno",
                        principalTable: "BlogCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BlogPostTags",
                schema: "inferno",
                columns: table => new
                {
                    PostId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TagId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogPostTags", x => new { x.PostId, x.TagId });
                    table.ForeignKey(
                        name: "FK_BlogPostTags_BlogPosts_PostId",
                        column: x => x.PostId,
                        principalSchema: "inferno",
                        principalTable: "BlogPosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BlogPostTags_BlogTags_TagId",
                        column: x => x.TagId,
                        principalSchema: "inferno",
                        principalTable: "BlogTags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_CategoryId",
                schema: "inferno",
                table: "BlogPosts",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_BlogPostTags_TagId",
                schema: "inferno",
                table: "BlogPostTags",
                column: "TagId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlogPostTags",
                schema: "inferno");

            migrationBuilder.DropTable(
                name: "ContentBlocks",
                schema: "inferno");

            migrationBuilder.DropTable(
                name: "EntityTypeContentBlocks",
                schema: "inferno");

            migrationBuilder.DropTable(
                name: "MenuItems",
                schema: "inferno");

            migrationBuilder.DropTable(
                name: "Menus",
                schema: "inferno");

            migrationBuilder.DropTable(
                name: "SitemapConfig",
                schema: "inferno");

            migrationBuilder.DropTable(
                name: "Zones",
                schema: "inferno");

            migrationBuilder.DropTable(
                name: "BlogPosts",
                schema: "inferno");

            migrationBuilder.DropTable(
                name: "BlogTags",
                schema: "inferno");

            migrationBuilder.DropTable(
                name: "BlogCategories",
                schema: "inferno");
        }
    }
}
