using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GriForms.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FormVersionLifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FormVersions_TopicId",
                table: "FormVersions");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "FormVersions",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "FormVersions",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "PublishedAt",
                table: "FormVersions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublishedBy",
                table: "FormVersions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RetiredAt",
                table: "FormVersions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RetiredBy",
                table: "FormVersions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FormVersions_TopicId_VersionNo",
                table: "FormVersions",
                columns: new[] { "TopicId", "VersionNo" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FormVersions_TopicId_VersionNo",
                table: "FormVersions");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "FormVersions");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "FormVersions");

            migrationBuilder.DropColumn(
                name: "PublishedAt",
                table: "FormVersions");

            migrationBuilder.DropColumn(
                name: "PublishedBy",
                table: "FormVersions");

            migrationBuilder.DropColumn(
                name: "RetiredAt",
                table: "FormVersions");

            migrationBuilder.DropColumn(
                name: "RetiredBy",
                table: "FormVersions");

            migrationBuilder.CreateIndex(
                name: "IX_FormVersions_TopicId",
                table: "FormVersions",
                column: "TopicId");
        }
    }
}
