using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParkingLotManagement.Migrations
{
    /// <inheritdoc />
    public partial class UpdateIndentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "GithubId",
                table: "AspNetUsers",
                newName: "Avatar");

            migrationBuilder.RenameColumn(
                name: "FacebookId",
                table: "AspNetUsers",
                newName: "Address");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Avatar",
                table: "AspNetUsers",
                newName: "GithubId");

            migrationBuilder.RenameColumn(
                name: "Address",
                table: "AspNetUsers",
                newName: "FacebookId");
        }
    }
}
