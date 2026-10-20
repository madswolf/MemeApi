using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MemeApi.Migrations
{
    /// <inheritdoc />
    public partial class AddMemeOfTheDayTimeZone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TimeZoneId",
                table: "MemeOfTheDaySchedules",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "Europe/Copenhagen");

            migrationBuilder.UpdateData(
                table: "MemeOfTheDaySchedules",
                keyColumn: "Id",
                keyValue: 1,
                column: "TimeZoneId",
                value: "Europe/Copenhagen");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeZoneId",
                table: "MemeOfTheDaySchedules");
        }
    }
}
