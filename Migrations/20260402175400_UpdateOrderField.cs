using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAnMonLTWeb.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOrderField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.DropColumn(
            //     name: "Address",
            //     table: "Orders");

            // migrationBuilder.RenameColumn(
            //     name: "Note",
            //     table: "Orders",
            //     newName: "ShippingAddress");

            // migrationBuilder.AddColumn<string>(
            //     name: "Notes",
            //     table: "Orders",
            //     type: "nvarchar(max)",
            //     nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmount",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            // migrationBuilder.AddColumn<string>(
            //     name: "UserId",
            //     table: "Orders",
            //     type: "nvarchar(450)",
            //     nullable: true);

            // migrationBuilder.CreateIndex(
            //     name: "IX_Orders_UserId",
            //     table: "Orders",
            //     column: "UserId");

            // migrationBuilder.AddForeignKey(
            //     name: "FK_Orders_AspNetUsers_UserId",
            //     table: "Orders",
            //     column: "UserId",
            //     principalTable: "AspNetUsers",
            //     principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_AspNetUsers_UserId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_UserId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "TotalAmount",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "ShippingAddress",
                table: "Orders",
                newName: "Note");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
