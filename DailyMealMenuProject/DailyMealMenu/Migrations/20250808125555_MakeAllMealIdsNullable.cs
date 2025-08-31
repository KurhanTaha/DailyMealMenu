using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DailyMealMenu.Migrations
{
    /// <inheritdoc />
    public partial class MakeAllMealIdsNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Desserts_Meals_MealId",
                table: "Desserts");

            migrationBuilder.DropForeignKey(
                name: "FK_MainDishes_Meals_MealId",
                table: "MainDishes");

            migrationBuilder.DropForeignKey(
                name: "FK_Others_Meals_MealId",
                table: "Others");

            migrationBuilder.DropForeignKey(
                name: "FK_Soups_Meals_MealId",
                table: "Soups");

            migrationBuilder.DropForeignKey(
                name: "FK_Starters_Meals_MealId",
                table: "Starters");

            migrationBuilder.AlterColumn<int>(
                name: "MealId",
                table: "Starters",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "MealId",
                table: "Soups",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "MealId",
                table: "Others",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "MealId",
                table: "MainDishes",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "MealId",
                table: "Desserts",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Desserts_Meals_MealId",
                table: "Desserts",
                column: "MealId",
                principalTable: "Meals",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MainDishes_Meals_MealId",
                table: "MainDishes",
                column: "MealId",
                principalTable: "Meals",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Others_Meals_MealId",
                table: "Others",
                column: "MealId",
                principalTable: "Meals",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Soups_Meals_MealId",
                table: "Soups",
                column: "MealId",
                principalTable: "Meals",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Starters_Meals_MealId",
                table: "Starters",
                column: "MealId",
                principalTable: "Meals",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Desserts_Meals_MealId",
                table: "Desserts");

            migrationBuilder.DropForeignKey(
                name: "FK_MainDishes_Meals_MealId",
                table: "MainDishes");

            migrationBuilder.DropForeignKey(
                name: "FK_Others_Meals_MealId",
                table: "Others");

            migrationBuilder.DropForeignKey(
                name: "FK_Soups_Meals_MealId",
                table: "Soups");

            migrationBuilder.DropForeignKey(
                name: "FK_Starters_Meals_MealId",
                table: "Starters");

            migrationBuilder.AlterColumn<int>(
                name: "MealId",
                table: "Starters",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MealId",
                table: "Soups",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MealId",
                table: "Others",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MealId",
                table: "MainDishes",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MealId",
                table: "Desserts",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Desserts_Meals_MealId",
                table: "Desserts",
                column: "MealId",
                principalTable: "Meals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MainDishes_Meals_MealId",
                table: "MainDishes",
                column: "MealId",
                principalTable: "Meals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Others_Meals_MealId",
                table: "Others",
                column: "MealId",
                principalTable: "Meals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Soups_Meals_MealId",
                table: "Soups",
                column: "MealId",
                principalTable: "Meals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Starters_Meals_MealId",
                table: "Starters",
                column: "MealId",
                principalTable: "Meals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
