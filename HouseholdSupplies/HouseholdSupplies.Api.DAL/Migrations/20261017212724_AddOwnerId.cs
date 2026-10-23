using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HouseholdSupplies.Api.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "ShoppingLists",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "ShoppingListItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "Places",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "Items",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "ShoppingLists");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "ShoppingListItems");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Places");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Items");
        }
    }
}
