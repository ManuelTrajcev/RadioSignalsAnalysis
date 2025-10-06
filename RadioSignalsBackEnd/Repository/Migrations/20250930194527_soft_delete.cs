using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class soft_delete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Settlements",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "Settlements",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "ReferenceThresholds",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "ReferenceThresholds",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Municipalities",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "Municipalities",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Measurements",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "Measurements",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "GeoCoordinates",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "GeoCoordinates",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "ElectricFieldStrengths",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "ElectricFieldStrengths",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "ChannelFrequencies",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "ChannelFrequencies",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Settlements");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Settlements");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ReferenceThresholds");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "ReferenceThresholds");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Municipalities");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Municipalities");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Measurements");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Measurements");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "GeoCoordinates");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "GeoCoordinates");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ElectricFieldStrengths");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "ElectricFieldStrengths");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ChannelFrequencies");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "ChannelFrequencies");
        }
    }
}
