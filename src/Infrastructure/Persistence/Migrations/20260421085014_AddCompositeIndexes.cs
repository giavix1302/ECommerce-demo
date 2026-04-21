using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCompositeIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_refresh_tokens_UserId",
                table: "refresh_tokens");

            migrationBuilder.DropIndex(
                name: "IX_product_variants_ProductId",
                table: "product_variants");

            migrationBuilder.DropIndex(
                name: "IX_orders_UserId",
                table: "orders");

            migrationBuilder.DropIndex(
                name: "IX_coupon_usages_UserId",
                table: "coupon_usages");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "orders",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "PENDING",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldDefaultValue: "PENDING");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_UserId_IsRevoked",
                table: "refresh_tokens",
                columns: new[] { "UserId", "IsRevoked" });

            migrationBuilder.CreateIndex(
                name: "IX_product_variants_ProductId_IsDeleted",
                table: "product_variants",
                columns: new[] { "ProductId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_orders_UserId_Status",
                table: "orders",
                columns: new[] { "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_coupon_usages_UserId_CouponId",
                table: "coupon_usages",
                columns: new[] { "UserId", "CouponId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_refresh_tokens_UserId_IsRevoked",
                table: "refresh_tokens");

            migrationBuilder.DropIndex(
                name: "IX_product_variants_ProductId_IsDeleted",
                table: "product_variants");

            migrationBuilder.DropIndex(
                name: "IX_orders_UserId_Status",
                table: "orders");

            migrationBuilder.DropIndex(
                name: "IX_coupon_usages_UserId_CouponId",
                table: "coupon_usages");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "PENDING",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldDefaultValue: "PENDING");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_UserId",
                table: "refresh_tokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_product_variants_ProductId",
                table: "product_variants",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_orders_UserId",
                table: "orders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_coupon_usages_UserId",
                table: "coupon_usages",
                column: "UserId");
        }
    }
}
