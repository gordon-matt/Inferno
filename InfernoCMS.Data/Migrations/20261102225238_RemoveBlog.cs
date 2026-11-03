using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfernoCMS.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBlog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlogPostTags",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BlogCategories",
                schema: "inferno",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: true),
                    UrlSlug = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
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
                    TenantId = table.Column<int>(type: "int", nullable: true),
                    UrlSlug = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogTags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BlogPosts",
                schema: "inferno",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    DateCreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExternalLink = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    FullDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Headline = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    MetaDescription = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    MetaKeywords = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ShortDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    TeaserImageUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TenantId = table.Column<int>(type: "int", nullable: true),
                    UseExternalLink = table.Column<bool>(type: "bit", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false)
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
    }
}
