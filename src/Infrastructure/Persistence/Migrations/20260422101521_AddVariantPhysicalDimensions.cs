using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVariantPhysicalDimensions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "HeightCm",
                table: "product_variants",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "LengthCm",
                table: "product_variants",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "WeightKg",
                table: "product_variants",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "WidthCm",
                table: "product_variants",
                type: "real",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HeightCm",
                table: "product_variants");

            migrationBuilder.DropColumn(
                name: "LengthCm",
                table: "product_variants");

            migrationBuilder.DropColumn(
                name: "WeightKg",
                table: "product_variants");

            migrationBuilder.DropColumn(
                name: "WidthCm",
                table: "product_variants");
        }
    }
}
