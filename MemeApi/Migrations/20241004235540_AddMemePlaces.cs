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
            migrationBuilder.DropForeignKey(
                name: "FK_PlacePixelPurchase_PlaceSubmissions_SubmissionId",
                table: "PlacePixelPurchase");

            migrationBuilder.DeleteData(
                table: "Topics",
                keyColumn: "Id",
                keyValue: "780e7e42-fc5c-4aa0-ae32-31472534affa");

            migrationBuilder.DeleteData(
                table: "Topics",
                keyColumn: "Id",
                keyValue: "8ec29ec9-4b01-4ac0-ab5b-41a892d231ca");

            migrationBuilder.DeleteData(
                table: "Topics",
                keyColumn: "Id",
                keyValue: "bc8acf9d-9d5c-4a2f-b65e-5be86c18e8af");

            migrationBuilder.DeleteData(
                table: "Topics",
                keyColumn: "Id",
                keyValue: "d0bf87b6-b004-4ad0-97b4-855a4bef137b");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "9fce75fc-ed08-404d-9d09-b0b0932f90b2");

            migrationBuilder.DropColumn(
                name: "PixelSubmissions",
                table: "PlaceSubmissions");

            migrationBuilder.AddColumn<int>(
                name: "PixelChangeCount",
                table: "PlaceSubmissions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "SubmissionId",
                table: "PlacePixelPurchase",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "LastLoginAt", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "ProfilePicFile", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "612296aa-c263-4750-ad44-81e84938a234", 0, "269096b7-2c31-490a-ba08-23a11618fc7e", "", false, new DateTime(2024, 10, 4, 23, 55, 39, 347, DateTimeKind.Utc).AddTicks(717), false, null, null, null, null, null, false, null, "04-10-2024 23:55:39", false, "" });

            migrationBuilder.InsertData(
                table: "Topics",
                columns: new[] { "Id", "Description", "HasRestrictedPosting", "Name", "OwnerId" },
                values: new object[,]
                {
                    { "046f3eb0-d3c6-435f-8fd5-0096c0838fd7", "Memes created 2020-2023", false, "Swu-legacy", "612296aa-c263-4750-ad44-81e84938a234" },
                    { "10d8202f-c7b7-4921-878e-6bcc150ec807", "Memes of the day", false, "MemeOfTheDay", "612296aa-c263-4750-ad44-81e84938a234" },
                    { "50e9f653-c5e8-4ad9-8590-1cccbce00f67", "Memes are back baby!", false, "Bean-den", "612296aa-c263-4750-ad44-81e84938a234" },
                    { "6ff4d869-2cc6-4077-9192-7288b4f77825", "Memes are back baby!", false, "Rotte-Grotte", "612296aa-c263-4750-ad44-81e84938a234" }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_PlacePixelPurchase_PlaceSubmissions_SubmissionId",
                table: "PlacePixelPurchase",
                column: "SubmissionId",
                principalTable: "PlaceSubmissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlacePixelPurchase_PlaceSubmissions_SubmissionId",
                table: "PlacePixelPurchase");

            migrationBuilder.DeleteData(
                table: "Topics",
                keyColumn: "Id",
                keyValue: "046f3eb0-d3c6-435f-8fd5-0096c0838fd7");

            migrationBuilder.DeleteData(
                table: "Topics",
                keyColumn: "Id",
                keyValue: "10d8202f-c7b7-4921-878e-6bcc150ec807");

            migrationBuilder.DeleteData(
                table: "Topics",
                keyColumn: "Id",
                keyValue: "50e9f653-c5e8-4ad9-8590-1cccbce00f67");

            migrationBuilder.DeleteData(
                table: "Topics",
                keyColumn: "Id",
                keyValue: "6ff4d869-2cc6-4077-9192-7288b4f77825");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "612296aa-c263-4750-ad44-81e84938a234");

            migrationBuilder.DropColumn(
                name: "PixelChangeCount",
                table: "PlaceSubmissions");

            migrationBuilder.AddColumn<string>(
                name: "PixelSubmissions",
                table: "PlaceSubmissions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "SubmissionId",
                table: "PlacePixelPurchase",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "LastLoginAt", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "ProfilePicFile", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "9fce75fc-ed08-404d-9d09-b0b0932f90b2", 0, "dc1fff0c-45da-4d68-b4bf-d76122610487", "", false, new DateTime(2024, 9, 23, 23, 9, 26, 611, DateTimeKind.Utc).AddTicks(2213), false, null, null, null, null, null, false, null, "23-09-2024 23:09:26", false, "" });

            migrationBuilder.InsertData(
                table: "Topics",
                columns: new[] { "Id", "Description", "HasRestrictedPosting", "Name", "OwnerId" },
                values: new object[,]
                {
                    { "780e7e42-fc5c-4aa0-ae32-31472534affa", "Memes are back baby!", false, "Rotte-Grotte", "9fce75fc-ed08-404d-9d09-b0b0932f90b2" },
                    { "8ec29ec9-4b01-4ac0-ab5b-41a892d231ca", "Memes of the day", false, "MemeOfTheDay", "9fce75fc-ed08-404d-9d09-b0b0932f90b2" },
                    { "bc8acf9d-9d5c-4a2f-b65e-5be86c18e8af", "Memes created 2020-2023", false, "Swu-legacy", "9fce75fc-ed08-404d-9d09-b0b0932f90b2" },
                    { "d0bf87b6-b004-4ad0-97b4-855a4bef137b", "Memes are back baby!", false, "Bean-den", "9fce75fc-ed08-404d-9d09-b0b0932f90b2" }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_PlacePixelPurchase_PlaceSubmissions_SubmissionId",
                table: "PlacePixelPurchase",
                column: "SubmissionId",
                principalTable: "PlaceSubmissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
