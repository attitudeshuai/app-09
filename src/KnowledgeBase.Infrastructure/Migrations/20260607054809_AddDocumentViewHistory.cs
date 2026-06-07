using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KnowledgeBase.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentViewHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DocumentViewHistories",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    DocumentId = table.Column<long>(type: "bigint", nullable: false),
                    ViewedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentViewHistories", x => new { x.UserId, x.DocumentId });
                    table.ForeignKey(
                        name: "FK_DocumentViewHistories_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocumentViewHistories_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentViewHistories_DocumentId",
                table: "DocumentViewHistories",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentViewHistories_UserId",
                table: "DocumentViewHistories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentViewHistories_ViewedAt",
                table: "DocumentViewHistories",
                column: "ViewedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentViewHistories");
        }
    }
}
