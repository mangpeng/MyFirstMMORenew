using Microsoft.EntityFrameworkCore.Migrations;

namespace Server.Migrations
{
    public partial class addDamageOptions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "Critical",
                table: "Player",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<int>(
                name: "CriticalRatio",
                table: "Player",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DamageRange",
                table: "Player",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Critical",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "CriticalRatio",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "DamageRange",
                table: "Player");
        }
    }
}
