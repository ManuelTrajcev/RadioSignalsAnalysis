using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class soft_delete_2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Settlements_DeletedAt",
                table: "Settlements",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ReferenceThresholds_DeletedAt",
                table: "ReferenceThresholds",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Municipalities_DeletedAt",
                table: "Municipalities",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Measurements_DeletedAt",
                table: "Measurements",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_GeoCoordinates_DeletedAt",
                table: "GeoCoordinates",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ElectricFieldStrengths_DeletedAt",
                table: "ElectricFieldStrengths",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelFrequencies_DeletedAt",
                table: "ChannelFrequencies",
                column: "DeletedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Settlements_DeletedAt",
                table: "Settlements");

            migrationBuilder.DropIndex(
                name: "IX_ReferenceThresholds_DeletedAt",
                table: "ReferenceThresholds");

            migrationBuilder.DropIndex(
                name: "IX_Municipalities_DeletedAt",
                table: "Municipalities");

            migrationBuilder.DropIndex(
                name: "IX_Measurements_DeletedAt",
                table: "Measurements");

            migrationBuilder.DropIndex(
                name: "IX_GeoCoordinates_DeletedAt",
                table: "GeoCoordinates");

            migrationBuilder.DropIndex(
                name: "IX_ElectricFieldStrengths_DeletedAt",
                table: "ElectricFieldStrengths");

            migrationBuilder.DropIndex(
                name: "IX_ChannelFrequencies_DeletedAt",
                table: "ChannelFrequencies");
        }
    }
}
