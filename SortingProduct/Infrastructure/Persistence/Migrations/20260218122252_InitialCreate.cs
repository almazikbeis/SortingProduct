using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SortingProduct.Infrastructure.Persistence.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "product_batches",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    unit_price_eur = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    initial_quantity = table.Column<int>(type: "integer", nullable: false),
                    remaining_quantity = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() at time zone 'utc'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_batches", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "product_groups",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    total_price_eur = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() at time zone 'utc'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_groups", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "product_group_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_batch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    line_total_eur = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_group_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_product_group_items_product_batches_product_batch_id",
                        column: x => x.product_batch_id,
                        principalTable: "product_batches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_product_group_items_product_groups_product_group_id",
                        column: x => x.product_group_id,
                        principalTable: "product_groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_product_batches_status",
                table: "product_batches",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_group_items_group_batch",
                table: "product_group_items",
                columns: new[] { "product_group_id", "product_batch_id" });

            migrationBuilder.CreateIndex(
                name: "IX_product_group_items_product_batch_id",
                table: "product_group_items",
                column: "product_batch_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_groups_created_at",
                table: "product_groups",
                column: "created_at");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "product_group_items");

            migrationBuilder.DropTable(
                name: "product_batches");

            migrationBuilder.DropTable(
                name: "product_groups");
        }
    }
}
