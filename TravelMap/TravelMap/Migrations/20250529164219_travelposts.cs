using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelMap.Migrations
{
    /// <inheritdoc />
    public partial class travelposts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TravelImage_travelposts_TravelPostId",
                table: "TravelImage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TravelImage",
                table: "TravelImage");

            migrationBuilder.RenameTable(
                name: "TravelImage",
                newName: "TravelImages");

            migrationBuilder.RenameIndex(
                name: "IX_TravelImage_TravelPostId",
                table: "TravelImages",
                newName: "IX_TravelImages_TravelPostId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TravelImages",
                table: "TravelImages",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TravelImages_travelposts_TravelPostId",
                table: "TravelImages",
                column: "TravelPostId",
                principalTable: "travelposts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TravelImages_travelposts_TravelPostId",
                table: "TravelImages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TravelImages",
                table: "TravelImages");

            migrationBuilder.RenameTable(
                name: "TravelImages",
                newName: "TravelImage");

            migrationBuilder.RenameIndex(
                name: "IX_TravelImages_TravelPostId",
                table: "TravelImage",
                newName: "IX_TravelImage_TravelPostId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TravelImage",
                table: "TravelImage",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TravelImage_travelposts_TravelPostId",
                table: "TravelImage",
                column: "TravelPostId",
                principalTable: "travelposts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
