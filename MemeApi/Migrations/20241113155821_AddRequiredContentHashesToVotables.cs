using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MemeApi.Migrations
{
    /// <inheritdoc />
    public partial class AddRequiredContentHashesToVotables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE ""Votes""
                SET ""ElementId"" = '8496497b-ad24-497a-b29d-8717286afde1'
                WHERE ""ElementId"" IN (
                    '5444bf7b-2fa4-4574-96c7-3278d509a01c',
                    '87ee4982-71aa-4b68-8347-df036262ae2f',
                    '894eb1d1-9751-42cb-bd46-77f93b0817b7',
                    '8ea72a7e-59fe-4d49-b398-bedee8bf65c2',
                    'd5495973-d3ce-40f1-b0f0-ae558d6d6bfa',
                    'dd52ae08-307d-4f1c-b2d9-3e2f22d2fb52'
                    );"
                );
            
            migrationBuilder.Sql("DELETE FROM \"Votables\" WHERE \"ContentHash\" IS NULL;");
            
            migrationBuilder.AlterColumn<string>(
                name: "ContentHash",
                table: "Votables",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ContentHash",
                table: "Votables",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
