using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniJira.Migrations
{
    /// <inheritdoc />
    public partial class MakeInProgressNonSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Update the "In Progress" column to be non-system
            var inProgressColumnId = "22222222-2222-2222-2222-222222222222";

            migrationBuilder.Sql($@"
                UPDATE Columns
                SET IsSystem = 0
                WHERE Id = '{inProgressColumnId}';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert "In Progress" back to system column
            var inProgressColumnId = "22222222-2222-2222-2222-222222222222";

            migrationBuilder.Sql($@"
                UPDATE Columns
                SET IsSystem = 1
                WHERE Id = '{inProgressColumnId}';
            ");
        }
    }
}
