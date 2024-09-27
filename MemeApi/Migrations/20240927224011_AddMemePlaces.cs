using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MemeApi.Migrations
{
    /// <inheritdoc />
    public partial class AddMemePlaces : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.CreateTable(
                name: "MemePlaces",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Width = table.Column<int>(type: "integer", nullable: false),
                    Height = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemePlaces", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlaceSubmissions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    OwnerId = table.Column<string>(type: "text", nullable: false),
                    PlaceId = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    PixelSubmissions = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaceSubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlaceSubmissions_MemePlaces_PlaceId",
                        column: x => x.PlaceId,
                        principalTable: "MemePlaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlaceSubmissions_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlacePixelPurchase",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    SubmissionId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlacePixelPurchase", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlacePixelPurchase_DubloonEvents_Id",
                        column: x => x.Id,
                        principalTable: "DubloonEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlacePixelPurchase_PlaceSubmissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalTable: "PlaceSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                table: "MemePlaces",
                columns: new[] { "Id", "Name", "CreatedAt", "Width", "Height" },
                values: new object[] { "c6a058f9-8ce4-40e0-bd55-8f98d249f7aa", "Bean-canvas", DateTime.UtcNow, 1920, 1080 });

            migrationBuilder.CreateIndex(
                name: "IX_PlacePixelPurchase_SubmissionId",
                table: "PlacePixelPurchase",
                column: "SubmissionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlaceSubmissions_OwnerId",
                table: "PlaceSubmissions",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaceSubmissions_PlaceId",
                table: "PlaceSubmissions",
                column: "PlaceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlacePixelPurchase");

            migrationBuilder.DropTable(
                name: "PlaceSubmissions");

            migrationBuilder.DropTable(
                name: "MemePlaces");
        }
    }
}
