using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Order.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InboxMessages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    event_type = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    received_on_utc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    processed_on_utc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    attempt_count = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    max_attempts = table.Column<int>(type: "int", nullable: false, defaultValue: 3),
                    next_attempt_on_utc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedOnUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedOnUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InboxMessages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    cancel_reason = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    refund_reason = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    customer_email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    customer_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    customer_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    customer_phone_number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    coupon_code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    discount_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true, defaultValue: 0m),
                    order_no = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    shipping_address_line = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    shipping_city = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    shipping_country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    shipping_postal_code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    shipping_state_or_province = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    shipping_subdivision = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    created_on_utc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    last_modified_on_utc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    last_modified_by = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orders", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    event_type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    occurred_on_utc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    processed_on_utc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    last_error_message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    attempt_count = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    max_attempts = table.Column<int>(type: "int", nullable: false, defaultValue: 3),
                    next_attempt_on_utc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedOnUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedOnUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "order_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    order_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    product_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    product_image_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    product_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    product_price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    created_on_utc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    last_modified_on_utc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    last_modified_by = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_order_items_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessages_event_type",
                table: "InboxMessages",
                column: "event_type");

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessages_next_attempt_on_utc_processed_on_utc_attempt_count",
                table: "InboxMessages",
                columns: new[] { "next_attempt_on_utc", "processed_on_utc", "attempt_count" });

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessages_processed_on_utc",
                table: "InboxMessages",
                column: "processed_on_utc");

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessages_processed_on_utc_attempt_count_max_attempts",
                table: "InboxMessages",
                columns: new[] { "processed_on_utc", "attempt_count", "max_attempts" });

            migrationBuilder.CreateIndex(
                name: "IX_order_items_order_id",
                table: "order_items",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_processed_on_utc",
                table: "OutboxMessages",
                column: "processed_on_utc");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_processed_on_utc_attempt_count_max_attempts",
                table: "OutboxMessages",
                columns: new[] { "processed_on_utc", "attempt_count", "max_attempts" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InboxMessages");

            migrationBuilder.DropTable(
                name: "order_items");

            migrationBuilder.DropTable(
                name: "OutboxMessages");

            migrationBuilder.DropTable(
                name: "orders");
        }
    }
}
