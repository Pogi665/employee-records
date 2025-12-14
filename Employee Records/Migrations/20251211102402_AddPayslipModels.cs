using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Employee_Records.Migrations
{
    /// <inheritdoc />
    public partial class AddPayslipModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PayslipRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    PayPeriodMonth = table.Column<int>(type: "int", nullable: false),
                    PayPeriodYear = table.Column<int>(type: "int", nullable: false),
                    PayPeriodHalf = table.Column<int>(type: "int", nullable: false),
                    RequestDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ProcessedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProcessedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    RejectionReason = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GeneratedPayslipId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayslipRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayslipRequests_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Payslips",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    PayPeriodStart = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    PayPeriodEnd = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    BasicPay = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    HolidayPay = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OvertimePay = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Allowances = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SSS = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PhilHealth = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PagIBIG = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WithholdingTax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherDeductions = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GrossPay = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalDeductions = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetPay = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GeneratedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ApprovedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ApprovalDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DaysWorked = table.Column<int>(type: "int", nullable: false),
                    RegularHolidaysWorked = table.Column<int>(type: "int", nullable: false),
                    SpecialHolidaysWorked = table.Column<int>(type: "int", nullable: false),
                    OvertimeHours = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Remarks = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PayslipRequestId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payslips", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payslips_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Payslips_PayslipRequests_PayslipRequestId",
                        column: x => x.PayslipRequestId,
                        principalTable: "PayslipRequests",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_PayslipRequests_EmployeeId_Status",
                table: "PayslipRequests",
                columns: new[] { "EmployeeId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PayslipRequests_GeneratedPayslipId",
                table: "PayslipRequests",
                column: "GeneratedPayslipId");

            migrationBuilder.CreateIndex(
                name: "IX_Payslips_EmployeeId_PayPeriodStart_PayPeriodEnd",
                table: "Payslips",
                columns: new[] { "EmployeeId", "PayPeriodStart", "PayPeriodEnd" });

            migrationBuilder.CreateIndex(
                name: "IX_Payslips_PayslipRequestId",
                table: "Payslips",
                column: "PayslipRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_PayslipRequests_Payslips_GeneratedPayslipId",
                table: "PayslipRequests",
                column: "GeneratedPayslipId",
                principalTable: "Payslips",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PayslipRequests_Payslips_GeneratedPayslipId",
                table: "PayslipRequests");

            migrationBuilder.DropTable(
                name: "Payslips");

            migrationBuilder.DropTable(
                name: "PayslipRequests");
        }
    }
}
