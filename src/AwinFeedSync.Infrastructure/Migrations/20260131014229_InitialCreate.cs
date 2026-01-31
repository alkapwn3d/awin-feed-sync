using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AwinFeedSync.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "advertisers",
                columns: table => new
                {
                    advertiser_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    default_commission_text = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_advertisers", x => x.advertiser_id);
                });

            migrationBuilder.CreateTable(
                name: "sync_runs",
                columns: table => new
                {
                    run_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    finished_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    error_text = table.Column<string>(type: "text", nullable: true),
                    advertisers_processed = table.Column<int>(type: "integer", nullable: false),
                    products_seen = table.Column<int>(type: "integer", nullable: false),
                    products_changed = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sync_runs", x => x.run_id);
                });

            migrationBuilder.CreateTable(
                name: "products",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    advertiser_id = table.Column<int>(type: "integer", nullable: false),
                    product_key = table.Column<string>(type: "text", nullable: false),
                    feed_product_id = table.Column<string>(type: "text", nullable: true),
                    sku = table.Column<string>(type: "text", nullable: true),
                    product_name = table.Column<string>(type: "text", nullable: true),
                    product_url = table.Column<string>(type: "text", nullable: true),
                    image_url = table.Column<string>(type: "text", nullable: true),
                    price = table.Column<decimal>(type: "numeric", nullable: true),
                    currency = table.Column<string>(type: "text", nullable: true),
                    category = table.Column<string>(type: "text", nullable: true),
                    subcategory = table.Column<string>(type: "text", nullable: true),
                    commission_text = table.Column<string>(type: "text", nullable: true),
                    commission_rate = table.Column<decimal>(type: "numeric", nullable: true),
                    tracking_url = table.Column<string>(type: "text", nullable: true),
                    tracking_url_source = table.Column<string>(type: "text", nullable: true),
                    extra = table.Column<string>(type: "jsonb", nullable: true),
                    content_hash = table.Column<string>(type: "text", nullable: false),
                    last_seen_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_changed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    inactive_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ai_summary = table.Column<string>(type: "text", nullable: true),
                    ai_summary_status = table.Column<string>(type: "text", nullable: true),
                    ai_summary_updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_products", x => x.id);
                    table.ForeignKey(
                        name: "FK_products_advertisers_advertiser_id",
                        column: x => x.advertiser_id,
                        principalTable: "advertisers",
                        principalColumn: "advertiser_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_products_advertiser_id",
                table: "products",
                column: "advertiser_id");

            migrationBuilder.CreateIndex(
                name: "IX_products_advertiser_id_product_key",
                table: "products",
                columns: new[] { "advertiser_id", "product_key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_products_last_changed_at",
                table: "products",
                column: "last_changed_at");

            migrationBuilder.CreateIndex(
                name: "IX_products_last_seen_at",
                table: "products",
                column: "last_seen_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "products");

            migrationBuilder.DropTable(
                name: "sync_runs");

            migrationBuilder.DropTable(
                name: "advertisers");
        }
    }
}
