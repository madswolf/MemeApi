using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MemeApi.Migrations
{
    /// <inheritdoc />
    public partial class ExpandDubloonEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.CreateTable(
                name: "DailyVotes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    VoteId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyVotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyVotes_DubloonEvents_Id",
                        column: x => x.Id,
                        principalTable: "DubloonEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DailyVotes_Votes_VoteId",
                        column: x => x.VoteId,
                        principalTable: "Votes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            var query = "insert into \"DailyVotes\"(\"Id\", \"VoteId\")\r\nselect d.\"Id\", d.\"ReferenceEntityId\" from \"DubloonEvents\" as d join \"Votes\" as v on d.\"ReferenceEntityId\" = v.\"Id\";";
            migrationBuilder.Sql(query);
            
            migrationBuilder.DropColumn(
                name: "ReferenceEntityId",
                table: "DubloonEvents");

            migrationBuilder.DropColumn(
                name: "eventType",
                table: "DubloonEvents");

            migrationBuilder.CreateIndex(
                name: "IX_DailyVotes_VoteId",
                table: "DailyVotes",
                column: "VoteId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReferenceEntityId",
                table: "DubloonEvents",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "eventType",
                table: "DubloonEvents",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            var query = "update \"DubloonEvents\" as d \r\nset \"ReferenceEntityId\" = dv.\"VoteId\", \"eventType\" = 1\r\nfrom \"DailyVotes\" as dv\r\nwhere d.\"Id\" = dv.\"Id\";";
            migrationBuilder.Sql(query);

            migrationBuilder.DropTable(
                name: "DailyVotes");

        }
    }
}
