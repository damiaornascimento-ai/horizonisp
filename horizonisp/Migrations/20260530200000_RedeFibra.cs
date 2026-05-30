using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace horizonisp.Migrations
{
    /// <inheritdoc />
    public partial class RedeFibra : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Olts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Host = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Fabricante = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Localizacao = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_Olts", x => x.Id));

            migrationBuilder.CreateTable(
                name: "Onus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OltId = table.Column<int>(type: "int", nullable: false),
                    AssinaturaId = table.Column<int>(type: "int", nullable: true),
                    Serial = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Mac = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PonPorta = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SinalDbm = table.Column<int>(type: "int", nullable: true),
                    UltimaAtualizacao = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Onus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Onus_Assinaturas_AssinaturaId",
                        column: x => x.AssinaturaId,
                        principalTable: "Assinaturas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Onus_Olts_OltId",
                        column: x => x.OltId,
                        principalTable: "Olts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(name: "IX_Onus_AssinaturaId", table: "Onus", column: "AssinaturaId");
            migrationBuilder.CreateIndex(name: "IX_Onus_OltId", table: "Onus", column: "OltId");
            migrationBuilder.CreateIndex(name: "IX_Onus_Serial", table: "Onus", column: "Serial", unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Onus");
            migrationBuilder.DropTable(name: "Olts");
        }
    }
}
