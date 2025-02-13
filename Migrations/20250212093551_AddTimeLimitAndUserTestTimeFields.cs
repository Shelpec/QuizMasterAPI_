using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizMasterAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddTimeLimitAndUserTestTimeFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FinishTime",
                table: "UserTests",
                newName: "ExpireTime");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartTime",
                table: "UserTests",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<int>(
                name: "TimeSpentSeconds",
                table: "UserTests",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeSpentSeconds",
                table: "UserTests");

            migrationBuilder.RenameColumn(
                name: "ExpireTime",
                table: "UserTests",
                newName: "FinishTime");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartTime",
                table: "UserTests",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);
        }
    }
}
