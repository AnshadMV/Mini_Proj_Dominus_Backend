using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dominus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedIntoRazorPay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UroPayOrderId",
                table: "Orders",
                newName: "RazorOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RazorOrderId",
                table: "Orders",
                newName: "UroPayOrderId");
        }
    }
}
