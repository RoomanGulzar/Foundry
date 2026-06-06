using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChangeRequestAnalyzer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Init_1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DocumentContent",
                table: "ChangeRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentFileName",
                table: "ChangeRequests",
                type: "nvarchar(260)",
                maxLength: 260,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocumentContent",
                table: "ChangeRequests");

            migrationBuilder.DropColumn(
                name: "DocumentFileName",
                table: "ChangeRequests");
        }
    }
}
