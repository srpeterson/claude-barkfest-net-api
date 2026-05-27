using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Barkfest.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPetLikes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Likes",
                table: "Pets",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Likes",
                table: "Pets");
        }
    }
}
