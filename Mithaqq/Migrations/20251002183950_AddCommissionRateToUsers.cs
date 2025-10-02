using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mithaqq.Migrations
{
    /// <inheritdoc />
    public partial class AddCommissionRateToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PackageCount",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "PackagePriceForMarketer",
                table: "Products");

            migrationBuilder.AddColumn<decimal>(
                name: "CommissionRate",
                table: "AspNetUsers",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CommissionRate",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<int>(
                name: "PackageCount",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PackagePriceForMarketer",
                table: "Products",
                type: "decimal(18,2)",
                nullable: true);
        }
    }
}
