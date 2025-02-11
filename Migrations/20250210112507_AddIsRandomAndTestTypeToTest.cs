using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizMasterAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddIsRandomAndTestTypeToTest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsSurvey",
                table: "Tests",
                newName: "IsRandom");

            migrationBuilder.AddColumn<string>(
                name: "TestType",
                table: "Tests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TestType",
                table: "Tests");

            migrationBuilder.RenameColumn(
                name: "IsRandom",
                table: "Tests",
                newName: "IsSurvey");
        }
    }
}
