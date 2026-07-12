using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IManager.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddAudictBaseEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LastModifierId",
                table: "UserProfiles",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifierId",
                table: "TimeEntries",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifierId",
                table: "TimeChecks",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifierId",
                table: "Payslips",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifierId",
                table: "Payrolls",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifierId",
                table: "JobTitles",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifierId",
                table: "Departments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifierId",
                table: "Companies",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_LastModifierId",
                table: "UserProfiles",
                column: "LastModifierId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeEntries_LastModifierId",
                table: "TimeEntries",
                column: "LastModifierId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeChecks_LastModifierId",
                table: "TimeChecks",
                column: "LastModifierId");

            migrationBuilder.CreateIndex(
                name: "IX_Payslips_LastModifierId",
                table: "Payslips",
                column: "LastModifierId");

            migrationBuilder.CreateIndex(
                name: "IX_Payrolls_LastModifierId",
                table: "Payrolls",
                column: "LastModifierId");

            migrationBuilder.CreateIndex(
                name: "IX_JobTitles_LastModifierId",
                table: "JobTitles",
                column: "LastModifierId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_LastModifierId",
                table: "Departments",
                column: "LastModifierId");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_LastModifierId",
                table: "Companies",
                column: "LastModifierId");

            migrationBuilder.AddForeignKey(
                name: "FK_Companies_AspNetUsers_LastModifierId",
                table: "Companies",
                column: "LastModifierId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_AspNetUsers_LastModifierId",
                table: "Departments",
                column: "LastModifierId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_JobTitles_AspNetUsers_LastModifierId",
                table: "JobTitles",
                column: "LastModifierId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payrolls_AspNetUsers_LastModifierId",
                table: "Payrolls",
                column: "LastModifierId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payslips_AspNetUsers_LastModifierId",
                table: "Payslips",
                column: "LastModifierId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TimeChecks_AspNetUsers_LastModifierId",
                table: "TimeChecks",
                column: "LastModifierId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TimeEntries_AspNetUsers_LastModifierId",
                table: "TimeEntries",
                column: "LastModifierId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserProfiles_AspNetUsers_LastModifierId",
                table: "UserProfiles",
                column: "LastModifierId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Companies_AspNetUsers_LastModifierId",
                table: "Companies");

            migrationBuilder.DropForeignKey(
                name: "FK_Departments_AspNetUsers_LastModifierId",
                table: "Departments");

            migrationBuilder.DropForeignKey(
                name: "FK_JobTitles_AspNetUsers_LastModifierId",
                table: "JobTitles");

            migrationBuilder.DropForeignKey(
                name: "FK_Payrolls_AspNetUsers_LastModifierId",
                table: "Payrolls");

            migrationBuilder.DropForeignKey(
                name: "FK_Payslips_AspNetUsers_LastModifierId",
                table: "Payslips");

            migrationBuilder.DropForeignKey(
                name: "FK_TimeChecks_AspNetUsers_LastModifierId",
                table: "TimeChecks");

            migrationBuilder.DropForeignKey(
                name: "FK_TimeEntries_AspNetUsers_LastModifierId",
                table: "TimeEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_UserProfiles_AspNetUsers_LastModifierId",
                table: "UserProfiles");

            migrationBuilder.DropIndex(
                name: "IX_UserProfiles_LastModifierId",
                table: "UserProfiles");

            migrationBuilder.DropIndex(
                name: "IX_TimeEntries_LastModifierId",
                table: "TimeEntries");

            migrationBuilder.DropIndex(
                name: "IX_TimeChecks_LastModifierId",
                table: "TimeChecks");

            migrationBuilder.DropIndex(
                name: "IX_Payslips_LastModifierId",
                table: "Payslips");

            migrationBuilder.DropIndex(
                name: "IX_Payrolls_LastModifierId",
                table: "Payrolls");

            migrationBuilder.DropIndex(
                name: "IX_JobTitles_LastModifierId",
                table: "JobTitles");

            migrationBuilder.DropIndex(
                name: "IX_Departments_LastModifierId",
                table: "Departments");

            migrationBuilder.DropIndex(
                name: "IX_Companies_LastModifierId",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "LastModifierId",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "LastModifierId",
                table: "TimeEntries");

            migrationBuilder.DropColumn(
                name: "LastModifierId",
                table: "TimeChecks");

            migrationBuilder.DropColumn(
                name: "LastModifierId",
                table: "Payslips");

            migrationBuilder.DropColumn(
                name: "LastModifierId",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "LastModifierId",
                table: "JobTitles");

            migrationBuilder.DropColumn(
                name: "LastModifierId",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "LastModifierId",
                table: "Companies");
        }
    }
}
