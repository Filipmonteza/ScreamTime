using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScreenTime.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReleaseTicketsOnCancel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tickets_ScreeningId_SeatId",
                table: "Tickets");

            migrationBuilder.AddColumn<DateTime>(
                name: "ReleasedAt",
                table: "Tickets",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_ScreeningId_SeatId",
                table: "Tickets",
                columns: new[] { "ScreeningId", "SeatId" },
                unique: true,
                filter: "[ReleasedAt] IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tickets_ScreeningId_SeatId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "ReleasedAt",
                table: "Tickets");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_ScreeningId_SeatId",
                table: "Tickets",
                columns: new[] { "ScreeningId", "SeatId" },
                unique: true);
        }
    }
}
