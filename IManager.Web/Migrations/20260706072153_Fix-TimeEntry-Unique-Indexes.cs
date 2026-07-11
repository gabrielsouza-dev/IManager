using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IManager.Web.Migrations
{
    /// <inheritdoc />
    public partial class FixTimeEntryUniqueIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TimeEntries_EmployeeId_Date",
                table: "TimeEntries");

            migrationBuilder.CreateIndex(
                name: "IX_TimeEntries_EmployeeId_Date",
                table: "TimeEntries",
                columns: new[] { "EmployeeId", "Date" },
                unique: true,
                filter: "\"IsCurrent\" = true");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TimeEntries_EmployeeId_Date",
                table: "TimeEntries");

            migrationBuilder.CreateIndex(
                name: "IX_TimeEntries_EmployeeId_Date",
                table: "TimeEntries",
                columns: new[] { "EmployeeId", "Date" },
                unique: true,
                filter: "[IsCurrent] = 1");
        }
    }
}
