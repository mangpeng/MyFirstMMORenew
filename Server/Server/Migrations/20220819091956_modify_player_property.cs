using Microsoft.EntityFrameworkCore.Migrations;

namespace Server.Migrations
{
    public partial class modify_player_property : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CriticalRatio",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "Speed",
                table: "Player");

            migrationBuilder.AlterColumn<int>(
                name: "Critical",
                table: "Player",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AddColumn<int>(
                name: "CriticalDamage",
                table: "Player",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MoveSpeed",
                table: "Player",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CriticalDamage",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "MoveSpeed",
                table: "Player");

            migrationBuilder.AlterColumn<float>(
                name: "Critical",
                table: "Player",
                type: "real",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<int>(
                name: "CriticalRatio",
                table: "Player",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<float>(
                name: "Speed",
                table: "Player",
                type: "real",
                nullable: false,
                defaultValue: 0f);
        }
    }
}
