using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Barkfest.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemovePetProfileImageColumnsAndAddIsFeaturedImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfileImageBlobName",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "ProfileImageContentType",
                table: "Pets");

            migrationBuilder.AddColumn<bool>(
                name: "IsFeaturedImage",
                table: "PetImages",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFeaturedImage",
                table: "PetImages");

            migrationBuilder.AddColumn<string>(
                name: "ProfileImageBlobName",
                table: "Pets",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfileImageContentType",
                table: "Pets",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
