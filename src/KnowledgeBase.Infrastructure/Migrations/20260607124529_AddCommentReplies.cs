using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KnowledgeBase.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCommentReplies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ParentId",
                table: "DocumentComments",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ReplyToUserId",
                table: "DocumentComments",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OperationLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    ActionType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TargetType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TargetId = table.Column<long>(type: "bigint", nullable: true),
                    TargetName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Details = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IpAddress = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserAgent = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperationLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OperationLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentComments_ParentId",
                table: "DocumentComments",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentComments_ReplyToUserId",
                table: "DocumentComments",
                column: "ReplyToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OperationLogs_ActionType",
                table: "OperationLogs",
                column: "ActionType");

            migrationBuilder.CreateIndex(
                name: "IX_OperationLogs_CreatedAt",
                table: "OperationLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_OperationLogs_TargetType",
                table: "OperationLogs",
                column: "TargetType");

            migrationBuilder.CreateIndex(
                name: "IX_OperationLogs_UserId",
                table: "OperationLogs",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentComments_DocumentComments_ParentId",
                table: "DocumentComments",
                column: "ParentId",
                principalTable: "DocumentComments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentComments_Users_ReplyToUserId",
                table: "DocumentComments",
                column: "ReplyToUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentComments_DocumentComments_ParentId",
                table: "DocumentComments");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentComments_Users_ReplyToUserId",
                table: "DocumentComments");

            migrationBuilder.DropTable(
                name: "OperationLogs");

            migrationBuilder.DropIndex(
                name: "IX_DocumentComments_ParentId",
                table: "DocumentComments");

            migrationBuilder.DropIndex(
                name: "IX_DocumentComments_ReplyToUserId",
                table: "DocumentComments");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "DocumentComments");

            migrationBuilder.DropColumn(
                name: "ReplyToUserId",
                table: "DocumentComments");
        }
    }
}
