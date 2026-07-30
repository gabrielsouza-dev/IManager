using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IManager.Web.Migrations
{
    /// <inheritdoc />
    public partial class IncludePayslipProps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UnhealthyPay",
                table: "Payslips",
                newName: "UnhealthyAdditionals");

            migrationBuilder.RenameColumn(
                name: "TotalEarnings",
                table: "Payslips",
                newName: "TotalExtraEarnings");

            migrationBuilder.RenameColumn(
                name: "HazardPay",
                table: "Payslips",
                newName: "HazardAdditionals");

            migrationBuilder.AddColumn<decimal>(
                name: "NightShiftAdditionals",
                table: "Payslips",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "NightShiftHours",
                table: "Payslips",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "OvertimeHours",
                table: "Payslips",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "RegularHours",
                table: "Payslips",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<decimal>(
                name: "RegularSalary",
                table: "Payslips",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsTimeBank",
                table: "JobTitles",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NightShiftAdditionals",
                table: "Payslips");

            migrationBuilder.DropColumn(
                name: "NightShiftHours",
                table: "Payslips");

            migrationBuilder.DropColumn(
                name: "OvertimeHours",
                table: "Payslips");

            migrationBuilder.DropColumn(
                name: "RegularHours",
                table: "Payslips");

            migrationBuilder.DropColumn(
                name: "RegularSalary",
                table: "Payslips");

            migrationBuilder.DropColumn(
                name: "IsTimeBank",
                table: "JobTitles");

            migrationBuilder.RenameColumn(
                name: "UnhealthyAdditionals",
                table: "Payslips",
                newName: "UnhealthyPay");

            migrationBuilder.RenameColumn(
                name: "TotalExtraEarnings",
                table: "Payslips",
                newName: "TotalEarnings");

            migrationBuilder.RenameColumn(
                name: "HazardAdditionals",
                table: "Payslips",
                newName: "HazardPay");
        }
    }
}
