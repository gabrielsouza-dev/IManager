using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IManager.Web.Migrations
{
    /// <inheritdoc />
    public partial class FixParentReference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_TimeEntries_ParentId",
                table: "TimeEntries",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_TimeEntries_TimeEntries_ParentId",
                table: "TimeEntries",
                column: "ParentId",
                principalTable: "TimeEntries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TimeEntries_TimeEntries_ParentId",
                table: "TimeEntries");

            migrationBuilder.DropIndex(
                name: "IX_TimeEntries_ParentId",
                table: "TimeEntries");
        }
    }
}
