using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class edit_flags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastEditedAt",
                table: "Settlements",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastEditedBy",
                table: "Settlements",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastEditedAt",
                table: "ReferenceThresholds",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastEditedBy",
                table: "ReferenceThresholds",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastEditedAt",
                table: "Municipalities",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastEditedBy",
                table: "Municipalities",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastEditedAt",
                table: "Measurements",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastEditedBy",
                table: "Measurements",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastEditedAt",
                table: "GeoCoordinates",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastEditedBy",
                table: "GeoCoordinates",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastEditedAt",
                table: "ElectricFieldStrengths",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastEditedBy",
                table: "ElectricFieldStrengths",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastEditedAt",
                table: "ChannelFrequencies",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastEditedBy",
                table: "ChannelFrequencies",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastEditedAt",
                table: "Settlements");

            migrationBuilder.DropColumn(
                name: "LastEditedBy",
                table: "Settlements");

            migrationBuilder.DropColumn(
                name: "LastEditedAt",
                table: "ReferenceThresholds");

            migrationBuilder.DropColumn(
                name: "LastEditedBy",
                table: "ReferenceThresholds");

            migrationBuilder.DropColumn(
                name: "LastEditedAt",
                table: "Municipalities");

            migrationBuilder.DropColumn(
                name: "LastEditedBy",
                table: "Municipalities");

            migrationBuilder.DropColumn(
                name: "LastEditedAt",
                table: "Measurements");

            migrationBuilder.DropColumn(
                name: "LastEditedBy",
                table: "Measurements");

            migrationBuilder.DropColumn(
                name: "LastEditedAt",
                table: "GeoCoordinates");

            migrationBuilder.DropColumn(
                name: "LastEditedBy",
                table: "GeoCoordinates");

            migrationBuilder.DropColumn(
                name: "LastEditedAt",
                table: "ElectricFieldStrengths");

            migrationBuilder.DropColumn(
                name: "LastEditedBy",
                table: "ElectricFieldStrengths");

            migrationBuilder.DropColumn(
                name: "LastEditedAt",
                table: "ChannelFrequencies");

            migrationBuilder.DropColumn(
                name: "LastEditedBy",
                table: "ChannelFrequencies");
        }
    }
}
