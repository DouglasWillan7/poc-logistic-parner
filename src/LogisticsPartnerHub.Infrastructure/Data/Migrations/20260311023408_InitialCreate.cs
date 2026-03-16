using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogisticsPartnerHub.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "partners",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    base_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    auth_type = table.Column<int>(type: "integer", nullable: false),
                    auth_config = table.Column<string>(type: "jsonb", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_partners", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "field_mappings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    partner_id = table.Column<Guid>(type: "uuid", nullable: false),
                    direction = table.Column<int>(type: "integer", nullable: false),
                    source_field = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    target_field = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    service_type = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_field_mappings", x => x.id);
                    table.ForeignKey(
                        name: "FK_field_mappings_partners_partner_id",
                        column: x => x.partner_id,
                        principalTable: "partners",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "partner_endpoints",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    partner_id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_type = table.Column<int>(type: "integer", nullable: false),
                    http_method = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_partner_endpoints", x => x.id);
                    table.ForeignKey(
                        name: "FK_partner_endpoints_partners_partner_id",
                        column: x => x.partner_id,
                        principalTable: "partners",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "service_orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    partner_id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_type = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    canonical_payload = table.Column<string>(type: "jsonb", nullable: false),
                    partner_payload = table.Column<string>(type: "jsonb", nullable: true),
                    partner_external_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_service_orders", x => x.id);
                    table.ForeignKey(
                        name: "FK_service_orders_partners_partner_id",
                        column: x => x.partner_id,
                        principalTable: "partners",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "service_order_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    direction = table.Column<int>(type: "integer", nullable: false),
                    request_payload = table.Column<string>(type: "jsonb", nullable: false),
                    response_payload = table.Column<string>(type: "jsonb", nullable: true),
                    http_status_code = table.Column<int>(type: "integer", nullable: true),
                    attempt_number = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_service_order_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_service_order_logs_service_orders_service_order_id",
                        column: x => x.service_order_id,
                        principalTable: "service_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_field_mappings_partner_id_service_type_direction",
                table: "field_mappings",
                columns: new[] { "partner_id", "service_type", "direction" });

            migrationBuilder.CreateIndex(
                name: "IX_partner_endpoints_partner_id_service_type",
                table: "partner_endpoints",
                columns: new[] { "partner_id", "service_type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_service_order_logs_service_order_id",
                table: "service_order_logs",
                column: "service_order_id");

            migrationBuilder.CreateIndex(
                name: "IX_service_orders_external_id",
                table: "service_orders",
                column: "external_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_service_orders_partner_id",
                table: "service_orders",
                column: "partner_id");

            migrationBuilder.CreateIndex(
                name: "IX_service_orders_status",
                table: "service_orders",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "field_mappings");

            migrationBuilder.DropTable(
                name: "partner_endpoints");

            migrationBuilder.DropTable(
                name: "service_order_logs");

            migrationBuilder.DropTable(
                name: "service_orders");

            migrationBuilder.DropTable(
                name: "partners");
        }
    }
}
