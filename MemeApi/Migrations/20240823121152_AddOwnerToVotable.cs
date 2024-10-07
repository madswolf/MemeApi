using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MemeApi.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerToVotable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "Votables",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Votables_OwnerId",
                table: "Votables",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Votables_Users_OwnerId",
                table: "Votables",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Votables_Users_OwnerId",
                table: "Votables");

            migrationBuilder.DropIndex(
                name: "IX_Votables_OwnerId",
                table: "Votables");


            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Votables");
        }
    }
}
