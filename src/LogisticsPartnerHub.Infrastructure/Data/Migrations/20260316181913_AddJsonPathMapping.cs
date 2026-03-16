using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogisticsPartnerHub.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddJsonPathMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "source_field",
                table: "field_mappings");

            migrationBuilder.DropColumn(
                name: "target_field",
                table: "field_mappings");

            migrationBuilder.AddColumn<string>(
                name: "default_value",
                table: "field_mappings",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "order",
                table: "field_mappings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "source_path",
                table: "field_mappings",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "target_path",
                table: "field_mappings",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "default_value",
                table: "field_mappings");

            migrationBuilder.DropColumn(
                name: "order",
                table: "field_mappings");

            migrationBuilder.DropColumn(
                name: "source_path",
                table: "field_mappings");

            migrationBuilder.DropColumn(
                name: "target_path",
                table: "field_mappings");

            migrationBuilder.AddColumn<string>(
                name: "source_field",
                table: "field_mappings",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "target_field",
                table: "field_mappings",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }
    }
}
