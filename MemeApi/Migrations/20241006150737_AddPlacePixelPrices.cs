using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MemeApi.Migrations
{
    /// <inheritdoc />
    public partial class AddPlacePixelPrices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlacePixelPrices",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    PlaceId = table.Column<string>(type: "text", nullable: false),
                    PricePerPixel = table.Column<double>(type: "double precision", nullable: false),
                    PriceChangeTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlacePixelPrices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlacePixelPrices_MemePlaces_PlaceId",
                        column: x => x.PlaceId,
                        principalTable: "MemePlaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql(@"
                INSERT INTO ""PlacePixelPrices"" (""Id"", ""PlaceId"", ""PricePerPixel"", ""PriceChangeTime"")
                SELECT 
                    gen_random_uuid(),   -- Generate a new GUID (UUID) for the Id column
                    mp.""Id"",           -- Use the Id of the existing MemePlace
                    0.01,                -- Set PricePerPixel to 0.01
                    NOW()                -- Set the current time for PriceChangeTime
                FROM 
                    ""MemePlaces"" mp;
            ");

            migrationBuilder.CreateIndex(
                name: "IX_PlacePixelPrices_PlaceId",
                table: "PlacePixelPrices",
                column: "PlaceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlacePixelPrices");
        }
    }
}
