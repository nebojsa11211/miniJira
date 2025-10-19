using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniJira.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerToTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Customer",
                table: "Tasks",
                type: "TEXT",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Customer",
                table: "Tasks");
        }
    }
}
