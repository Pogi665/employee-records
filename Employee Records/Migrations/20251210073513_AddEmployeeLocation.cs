using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Employee_Records.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Employees",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Employees",
                type: "double",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Employees",
                type: "double",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Employees");
        }
    }
}
