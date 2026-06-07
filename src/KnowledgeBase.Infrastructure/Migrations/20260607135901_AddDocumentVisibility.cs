using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KnowledgeBase.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentVisibility : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AllowedRoles",
                table: "Documents",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "Visibility",
                table: "Documents",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_Documents_Visibility",
                table: "Documents",
                column: "Visibility");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Documents_Visibility",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "AllowedRoles",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "Visibility",
                table: "Documents");
        }
    }
}
