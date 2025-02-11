using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizMasterAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryIdToTopic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Сначала вручную создадим дефолтную категорию (если нужно)
            migrationBuilder.Sql(@"
        IF NOT EXISTS (SELECT 1 FROM Categories WHERE Id = 1)
        BEGIN
            INSERT INTO Categories (Id, Name)
            VALUES (1, 'Default Category')
        END
    ");

            // Назначим всем осиротевшим топикам CategoryId=1
            migrationBuilder.Sql(@"
        UPDATE Topics
        SET CategoryId = 1
        WHERE CategoryId IS NULL
    ");

            migrationBuilder.DropForeignKey(
                name: "FK_Topics_Categories_CategoryId",
                table: "Topics");

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "Topics",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Topics_Categories_CategoryId",
                table: "Topics",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Topics_Categories_CategoryId",
                table: "Topics");

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "Topics",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Topics_Categories_CategoryId",
                table: "Topics",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id");
        }
    }
}
