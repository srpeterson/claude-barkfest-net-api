using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Barkfest.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerDisplayNameNormalized : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DisplayNameNormalized",
                table: "Owners",
                type: "nvarchar(25)",
                maxLength: 25,
                nullable: true);

            // Backfill existing rows so the filtered unique index below can be created.
            // Mirrors Owner.Normalize (strip spaces, lowercase). This will fail loudly if
            // existing data contains normalized collisions - that data must be resolved
            // before applying this migration (verified clean on production prior to ship).
            migrationBuilder.Sql(
                "UPDATE [Owners] " +
                "SET [DisplayNameNormalized] = LOWER(REPLACE([DisplayName], ' ', '')) " +
                "WHERE [DisplayName] IS NOT NULL;");

            migrationBuilder.CreateIndex(
                name: "IX_Owners_DisplayNameNormalized",
                table: "Owners",
                column: "DisplayNameNormalized",
                unique: true,
                filter: "[DisplayNameNormalized] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Owners_DisplayNameNormalized",
                table: "Owners");

            migrationBuilder.DropColumn(
                name: "DisplayNameNormalized",
                table: "Owners");
        }
    }
}
