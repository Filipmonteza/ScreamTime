using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScreenTime.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FilterActiveAuditoriumNameIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Auditoriums_Name",
                table: "Auditoriums");

            migrationBuilder.CreateIndex(
                name: "IX_Auditoriums_Name",
                table: "Auditoriums",
                column: "Name",
                unique: true,
                filter: "[IsActive] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Auditoriums_Name",
                table: "Auditoriums");

            migrationBuilder.CreateIndex(
                name: "IX_Auditoriums_Name",
                table: "Auditoriums",
                column: "Name",
                unique: true);
        }
    }
}
