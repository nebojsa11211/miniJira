using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniJira.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskColumnHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaskColumnHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TaskId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FromColumnId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ToColumnId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ChangedBy = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskColumnHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskColumnHistories_Columns_FromColumnId",
                        column: x => x.FromColumnId,
                        principalTable: "Columns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskColumnHistories_Columns_ToColumnId",
                        column: x => x.ToColumnId,
                        principalTable: "Columns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskColumnHistories_Tasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskColumnHistories_ChangedAt",
                table: "TaskColumnHistories",
                column: "ChangedAt");

            migrationBuilder.CreateIndex(
                name: "IX_TaskColumnHistories_FromColumnId",
                table: "TaskColumnHistories",
                column: "FromColumnId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskColumnHistories_TaskId",
                table: "TaskColumnHistories",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskColumnHistories_ToColumnId",
                table: "TaskColumnHistories",
                column: "ToColumnId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskColumnHistories");
        }
    }
}
