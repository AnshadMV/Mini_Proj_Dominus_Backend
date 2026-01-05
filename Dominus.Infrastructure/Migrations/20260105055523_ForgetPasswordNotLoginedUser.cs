using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dominus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ForgetPasswordNotLoginedUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PasswordOtp",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordOtpExpiry",
                table: "Users",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordOtp",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PasswordOtpExpiry",
                table: "Users");
        }
    }
}
