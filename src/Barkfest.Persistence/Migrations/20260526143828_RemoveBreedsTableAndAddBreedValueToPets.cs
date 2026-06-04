using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Barkfest.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBreedsTableAndAddBreedValueToPets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Add Breed to Pets with a temporary default so existing rows satisfy NOT NULL.
            migrationBuilder.AddColumn<int>(
                name: "Breed",
                table: "Pets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // 2. Copy breed values from Breeds into Pets before the source table is dropped.
            migrationBuilder.Sql(
                """
                UPDATE p
                SET p.Breed = b.BreedValue
                FROM Pets p
                INNER JOIN Breeds b ON b.PetId = p.PetId
                WHERE b.BreedValue IS NOT NULL
                """);

            // 3. Drop the Breeds table now that data has been migrated.
            migrationBuilder.DropTable(
                name: "Breeds");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Breed",
                table: "Pets");

            migrationBuilder.CreateTable(
                name: "Breeds",
                columns: table => new
                {
                    BreedId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "newsequentialid()"),
                    PetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BreedType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BreedValue = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Breeds", x => x.BreedId);
                    table.ForeignKey(
                        name: "FK_Breeds_Pets_PetId",
                        column: x => x.PetId,
                        principalTable: "Pets",
                        principalColumn: "PetId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Breeds_PetId",
                table: "Breeds",
                column: "PetId",
                unique: true);
        }
    }
}
