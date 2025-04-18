﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MemeApi.Migrations
{
    /// <inheritdoc />
    public partial class AddLotteries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Lotteries",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    TicketCost = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lotteries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LotteryBrackets",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    LotteryId = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ProbabilityWeight = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LotteryBrackets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LotteryBrackets_Lotteries_LotteryId",
                        column: x => x.LotteryId,
                        principalTable: "Lotteries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LotteryItems",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    BracketId = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ItemCount = table.Column<int>(type: "integer", nullable: false),
                    ThumbNailFileName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LotteryItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LotteryItems_LotteryBrackets_BracketId",
                        column: x => x.BracketId,
                        principalTable: "LotteryBrackets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LotteryTickets",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ItemId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LotteryTickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LotteryTickets_DubloonEvents_Id",
                        column: x => x.Id,
                        principalTable: "DubloonEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LotteryTickets_LotteryItems_ItemId",
                        column: x => x.ItemId,
                        principalTable: "LotteryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LotteryBrackets_LotteryId",
                table: "LotteryBrackets",
                column: "LotteryId");

            migrationBuilder.CreateIndex(
                name: "IX_LotteryItems_BracketId",
                table: "LotteryItems",
                column: "BracketId");

            migrationBuilder.CreateIndex(
                name: "IX_LotteryTickets_ItemId",
                table: "LotteryTickets",
                column: "ItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LotteryTickets");

            migrationBuilder.DropTable(
                name: "LotteryItems");

            migrationBuilder.DropTable(
                name: "LotteryBrackets");

            migrationBuilder.DropTable(
                name: "Lotteries");
        }
    }
}
