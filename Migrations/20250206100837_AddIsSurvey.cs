using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizMasterAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddIsSurvey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSurvey",
                table: "Tests",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSurvey",
                table: "Tests");
        }
    }
}
