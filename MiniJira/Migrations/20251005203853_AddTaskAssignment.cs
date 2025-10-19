using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniJira.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AssignedUserId",
                table: "Tasks",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TaskOwnerHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TaskId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PreviousOwnerId = table.Column<Guid>(type: "TEXT", nullable: true),
                    NewOwnerId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ChangedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskOwnerHistories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskOwnerHistories_ChangedAt",
                table: "TaskOwnerHistories",
                column: "ChangedAt");

            migrationBuilder.CreateIndex(
                name: "IX_TaskOwnerHistories_TaskId",
                table: "TaskOwnerHistories",
                column: "TaskId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskOwnerHistories");

            migrationBuilder.DropColumn(
                name: "AssignedUserId",
                table: "Tasks");
        }
    }
}
