using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Barkfest.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerDisplayName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "Owners",
                type: "nvarchar(25)",
                maxLength: 25,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "Owners");
        }
    }
}
