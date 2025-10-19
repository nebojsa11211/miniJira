using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniJira.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkOrderTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkOrderHeaders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TaskId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Customer = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Object = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Product = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    WorkOrderNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    OrderReference = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    DeliveryReference = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    OrderDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeliveryDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PageNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Material = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Surface = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Cut = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Processing = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Packing = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrderHeaders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrderHeaders_Tasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrderRows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    WorkOrderHeaderId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RowNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    PositionPZ = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    PositionP1 = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    PositionR = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    PositionO = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    PositionP2 = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    Length = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Width = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Thickness = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Pieces = table.Column<int>(type: "INTEGER", nullable: true),
                    FromPieces = table.Column<int>(type: "INTEGER", nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ProcessingNotes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    SquareMeters = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    SquareMetersTot = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    LinearMeters = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    CubicMetersTot = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    WeightKg = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    MaterialDensity = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrderRows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrderRows_WorkOrderHeaders_WorkOrderHeaderId",
                        column: x => x.WorkOrderHeaderId,
                        principalTable: "WorkOrderHeaders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderHeaders_CreatedAt",
                table: "WorkOrderHeaders",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderHeaders_TaskId",
                table: "WorkOrderHeaders",
                column: "TaskId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderHeaders_UpdatedAt",
                table: "WorkOrderHeaders",
                column: "UpdatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderHeaders_WorkOrderNumber",
                table: "WorkOrderHeaders",
                column: "WorkOrderNumber");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderRows_WorkOrderHeaderId",
                table: "WorkOrderRows",
                column: "WorkOrderHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderRows_WorkOrderHeaderId_RowNumber",
                table: "WorkOrderRows",
                columns: new[] { "WorkOrderHeaderId", "RowNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkOrderRows");

            migrationBuilder.DropTable(
                name: "WorkOrderHeaders");
        }
    }
}
