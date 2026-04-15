using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DomainRestructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_cart_items_products_ProductId",
                table: "cart_items");

            migrationBuilder.DropForeignKey(
                name: "FK_order_items_products_ProductId",
                table: "order_items");

            migrationBuilder.DropIndex(
                name: "IX_products_Sku",
                table: "products");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "products");

            migrationBuilder.DropColumn(
                name: "Sku",
                table: "products");

            migrationBuilder.DropColumn(
                name: "StockQuantity",
                table: "products");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "order_items",
                newName: "VariantId");

            migrationBuilder.RenameIndex(
                name: "IX_order_items_ProductId",
                table: "order_items",
                newName: "IX_order_items_VariantId");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "cart_items",
                newName: "VariantId");

            migrationBuilder.RenameIndex(
                name: "IX_cart_items_ProductId",
                table: "cart_items",
                newName: "IX_cart_items_VariantId");

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentExpiredAt",
                table: "orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "COD");

            migrationBuilder.AddColumn<string>(
                name: "PaymentStatus",
                table: "orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "UNPAID");

            migrationBuilder.AddColumn<decimal>(
                name: "PromotionDiscountAmount",
                table: "orders",
                type: "decimal(15,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<long>(
                name: "PromotionRuleId",
                table: "orders",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ShippingFee",
                table: "orders",
                type: "decimal(15,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<long>(
                name: "PromotionRuleId",
                table: "order_items",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "payment_transactions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    PayOSOrderCode = table.Column<long>(type: "bigint", nullable: false),
                    PaymentLinkId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    AccountNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Reference = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TransactionDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    CounterAccountBankName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CounterAccountName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CounterAccountNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_payment_transactions_orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "product_attributes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_attributes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "product_variants",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    Sku = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Price = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    StockQuantity = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_variants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_product_variants_products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "promotion_rules",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    AllowCoupon = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_promotion_rules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "shipments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    AhamoveOrderId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ServiceId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TotalFee = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    Distance = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    CodAmount = table.Column<decimal>(type: "decimal(15,2)", nullable: false, defaultValue: 0m),
                    SharedLink = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PickupAddress = table.Column<string>(type: "text", nullable: false),
                    DeliveryAddress = table.Column<string>(type: "text", nullable: false),
                    SupplierId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SupplierName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    AhamoveCreateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shipments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_shipments_orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "variant_attribute_values",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VariantId = table.Column<long>(type: "bigint", nullable: false),
                    AttributeId = table.Column<long>(type: "bigint", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_variant_attribute_values", x => x.Id);
                    table.ForeignKey(
                        name: "FK_variant_attribute_values_product_attributes_AttributeId",
                        column: x => x.AttributeId,
                        principalTable: "product_attributes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_variant_attribute_values_product_variants_VariantId",
                        column: x => x.VariantId,
                        principalTable: "product_variants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "promotion_actions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PromotionRuleId = table.Column<long>(type: "bigint", nullable: false),
                    ActionType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DiscountValue = table.Column<decimal>(type: "decimal(15,2)", nullable: true),
                    GiftProductId = table.Column<long>(type: "bigint", nullable: true),
                    GiftQuantity = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_promotion_actions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_promotion_actions_products_GiftProductId",
                        column: x => x.GiftProductId,
                        principalTable: "products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_promotion_actions_promotion_rules_PromotionRuleId",
                        column: x => x.PromotionRuleId,
                        principalTable: "promotion_rules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "promotion_conditions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PromotionRuleId = table.Column<long>(type: "bigint", nullable: false),
                    ConditionType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TargetId = table.Column<long>(type: "bigint", nullable: true),
                    Value = table.Column<decimal>(type: "decimal(15,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_promotion_conditions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_promotion_conditions_promotion_rules_PromotionRuleId",
                        column: x => x.PromotionRuleId,
                        principalTable: "promotion_rules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_orders_PromotionRuleId",
                table: "orders",
                column: "PromotionRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_order_items_PromotionRuleId",
                table: "order_items",
                column: "PromotionRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_payment_transactions_OrderId",
                table: "payment_transactions",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_payment_transactions_PayOSOrderCode",
                table: "payment_transactions",
                column: "PayOSOrderCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_product_attributes_Name",
                table: "product_attributes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_product_variants_ProductId",
                table: "product_variants",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_product_variants_Sku",
                table: "product_variants",
                column: "Sku",
                unique: true,
                filter: "[sku] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_promotion_actions_GiftProductId",
                table: "promotion_actions",
                column: "GiftProductId");

            migrationBuilder.CreateIndex(
                name: "IX_promotion_actions_PromotionRuleId",
                table: "promotion_actions",
                column: "PromotionRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_promotion_conditions_PromotionRuleId",
                table: "promotion_conditions",
                column: "PromotionRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_shipments_AhamoveOrderId",
                table: "shipments",
                column: "AhamoveOrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_shipments_OrderId",
                table: "shipments",
                column: "OrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_variant_attribute_values_AttributeId",
                table: "variant_attribute_values",
                column: "AttributeId");

            migrationBuilder.CreateIndex(
                name: "IX_variant_attribute_values_VariantId",
                table: "variant_attribute_values",
                column: "VariantId");

            migrationBuilder.AddForeignKey(
                name: "FK_cart_items_product_variants_VariantId",
                table: "cart_items",
                column: "VariantId",
                principalTable: "product_variants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_order_items_product_variants_VariantId",
                table: "order_items",
                column: "VariantId",
                principalTable: "product_variants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_order_items_promotion_rules_PromotionRuleId",
                table: "order_items",
                column: "PromotionRuleId",
                principalTable: "promotion_rules",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_orders_promotion_rules_PromotionRuleId",
                table: "orders",
                column: "PromotionRuleId",
                principalTable: "promotion_rules",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_cart_items_product_variants_VariantId",
                table: "cart_items");

            migrationBuilder.DropForeignKey(
                name: "FK_order_items_product_variants_VariantId",
                table: "order_items");

            migrationBuilder.DropForeignKey(
                name: "FK_order_items_promotion_rules_PromotionRuleId",
                table: "order_items");

            migrationBuilder.DropForeignKey(
                name: "FK_orders_promotion_rules_PromotionRuleId",
                table: "orders");

            migrationBuilder.DropTable(
                name: "payment_transactions");

            migrationBuilder.DropTable(
                name: "promotion_actions");

            migrationBuilder.DropTable(
                name: "promotion_conditions");

            migrationBuilder.DropTable(
                name: "shipments");

            migrationBuilder.DropTable(
                name: "variant_attribute_values");

            migrationBuilder.DropTable(
                name: "promotion_rules");

            migrationBuilder.DropTable(
                name: "product_attributes");

            migrationBuilder.DropTable(
                name: "product_variants");

            migrationBuilder.DropIndex(
                name: "IX_orders_PromotionRuleId",
                table: "orders");

            migrationBuilder.DropIndex(
                name: "IX_order_items_PromotionRuleId",
                table: "order_items");

            migrationBuilder.DropColumn(
                name: "PaymentExpiredAt",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "PromotionDiscountAmount",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "PromotionRuleId",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "ShippingFee",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "PromotionRuleId",
                table: "order_items");

            migrationBuilder.RenameColumn(
                name: "VariantId",
                table: "order_items",
                newName: "ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_order_items_VariantId",
                table: "order_items",
                newName: "IX_order_items_ProductId");

            migrationBuilder.RenameColumn(
                name: "VariantId",
                table: "cart_items",
                newName: "ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_cart_items_VariantId",
                table: "cart_items",
                newName: "IX_cart_items_ProductId");

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "products",
                type: "decimal(15,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Sku",
                table: "products",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StockQuantity",
                table: "products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_products_Sku",
                table: "products",
                column: "Sku",
                unique: true,
                filter: "[sku] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_cart_items_products_ProductId",
                table: "cart_items",
                column: "ProductId",
                principalTable: "products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_order_items_products_ProductId",
                table: "order_items",
                column: "ProductId",
                principalTable: "products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
