using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IManager.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddPayrollProps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payslips_Payrolls_PayrollId",
                table: "Payslips");

            migrationBuilder.DropIndex(
                name: "IX_Payrolls_CompanyId",
                table: "Payrolls");

            migrationBuilder.AddColumn<Guid>(
                name: "PayslipId",
                table: "TimeEntries",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId1",
                table: "Payrolls",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "Competence",
                table: "Payrolls",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.CreateIndex(
                name: "IX_TimeEntries_PayslipId",
                table: "TimeEntries",
                column: "PayslipId");

            migrationBuilder.CreateIndex(
                name: "IX_Payrolls_CompanyId_Competence",
                table: "Payrolls",
                columns: new[] { "CompanyId", "Competence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payrolls_CompanyId1",
                table: "Payrolls",
                column: "CompanyId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Payrolls_Companies_CompanyId1",
                table: "Payrolls",
                column: "CompanyId1",
                principalTable: "Companies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Payslips_Payrolls_PayrollId",
                table: "Payslips",
                column: "PayrollId",
                principalTable: "Payrolls",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TimeEntries_Payslips_PayslipId",
                table: "TimeEntries",
                column: "PayslipId",
                principalTable: "Payslips",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payrolls_Companies_CompanyId1",
                table: "Payrolls");

            migrationBuilder.DropForeignKey(
                name: "FK_Payslips_Payrolls_PayrollId",
                table: "Payslips");

            migrationBuilder.DropForeignKey(
                name: "FK_TimeEntries_Payslips_PayslipId",
                table: "TimeEntries");

            migrationBuilder.DropIndex(
                name: "IX_TimeEntries_PayslipId",
                table: "TimeEntries");

            migrationBuilder.DropIndex(
                name: "IX_Payrolls_CompanyId_Competence",
                table: "Payrolls");

            migrationBuilder.DropIndex(
                name: "IX_Payrolls_CompanyId1",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "PayslipId",
                table: "TimeEntries");

            migrationBuilder.DropColumn(
                name: "CompanyId1",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "Competence",
                table: "Payrolls");

            migrationBuilder.CreateIndex(
                name: "IX_Payrolls_CompanyId",
                table: "Payrolls",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payslips_Payrolls_PayrollId",
                table: "Payslips",
                column: "PayrollId",
                principalTable: "Payrolls",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
