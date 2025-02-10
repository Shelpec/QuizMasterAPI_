using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizMasterAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddIsSurveyTopicToTopic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSurveyTopic",
                table: "Topics",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSurveyTopic",
                table: "Topics");
        }
    }
}
