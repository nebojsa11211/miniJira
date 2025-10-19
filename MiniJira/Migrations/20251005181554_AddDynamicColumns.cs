using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniJira.Migrations
{
    /// <inheritdoc />
    public partial class AddDynamicColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ColumnId",
                table: "Tasks",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Columns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    Color = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false, defaultValue: "#DFE1E6"),
                    IsSystem = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Columns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ColumnTranslations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ColumnId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Culture = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ColumnTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ColumnTranslations_Columns_ColumnId",
                        column: x => x.ColumnId,
                        principalTable: "Columns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_ColumnId",
                table: "Tasks",
                column: "ColumnId");

            migrationBuilder.CreateIndex(
                name: "IX_Columns_IsActive",
                table: "Columns",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Columns_Order",
                table: "Columns",
                column: "Order");

            migrationBuilder.CreateIndex(
                name: "IX_ColumnTranslations_ColumnId_Culture",
                table: "ColumnTranslations",
                columns: new[] { "ColumnId", "Culture" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Columns_ColumnId",
                table: "Tasks",
                column: "ColumnId",
                principalTable: "Columns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // Seed default columns
            var now = DateTime.UtcNow;
            var toDoColumnId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var inProgressColumnId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var doneColumnId = Guid.Parse("33333333-3333-3333-3333-333333333333");

            // Insert default columns
            migrationBuilder.InsertData(
                table: "Columns",
                columns: new[] { "Id", "Order", "Color", "IsSystem", "IsActive", "CreatedAt", "UpdatedAt" },
                values: new object[,]
                {
                    { toDoColumnId, 0, "#DFE1E6", true, true, now, now },
                    { inProgressColumnId, 1, "#0052CC", true, true, now, now },
                    { doneColumnId, 2, "#00875A", true, true, now, now }
                });

            // Insert column translations for To Do
            migrationBuilder.InsertData(
                table: "ColumnTranslations",
                columns: new[] { "Id", "ColumnId", "Culture", "Name", "Description" },
                values: new object[,]
                {
                    { Guid.NewGuid(), toDoColumnId, "en-US", "To Do", "Tasks to be started" },
                    { Guid.NewGuid(), toDoColumnId, "hr-HR", "Za uraditi", "Zadaci za početak" },
                    { Guid.NewGuid(), inProgressColumnId, "en-US", "In Progress", "Currently working on" },
                    { Guid.NewGuid(), inProgressColumnId, "hr-HR", "U tijeku", "Trenutno se radi" },
                    { Guid.NewGuid(), doneColumnId, "en-US", "Done", "Completed tasks" },
                    { Guid.NewGuid(), doneColumnId, "hr-HR", "Gotovo", "Završeni zadaci" }
                });

            // Migrate existing tasks to use ColumnId based on their Status enum value
            migrationBuilder.Sql($@"
                UPDATE Tasks
                SET ColumnId = CASE
                    WHEN Status = 0 THEN '{toDoColumnId}'
                    WHEN Status = 1 THEN '{inProgressColumnId}'
                    WHEN Status = 2 THEN '{doneColumnId}'
                    ELSE '{toDoColumnId}'
                END
                WHERE ColumnId IS NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Clear ColumnId from tasks before dropping the foreign key
            migrationBuilder.Sql("UPDATE Tasks SET ColumnId = NULL WHERE ColumnId IS NOT NULL;");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Columns_ColumnId",
                table: "Tasks");

            migrationBuilder.DropTable(
                name: "ColumnTranslations");

            migrationBuilder.DropTable(
                name: "Columns");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_ColumnId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "ColumnId",
                table: "Tasks");
        }
    }
}
