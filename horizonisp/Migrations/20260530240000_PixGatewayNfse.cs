using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace horizonisp.Migrations
{
    /// <inheritdoc />
    public partial class PixGatewayNfse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PixGatewayRef",
                table: "Faturas",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PixExpiracaoEm",
                table: "Faturas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "NotasFiscaisServico",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FaturaId = table.Column<int>(type: "int", nullable: false),
                    Numero = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CodigoVerificacao = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DataEmissao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Discriminacao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    MensagemErro = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LinkPdf = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotasFiscaisServico", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotasFiscaisServico_Faturas_FaturaId",
                        column: x => x.FaturaId,
                        principalTable: "Faturas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NotasFiscaisServico_FaturaId",
                table: "NotasFiscaisServico",
                column: "FaturaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotasFiscaisServico_Status",
                table: "NotasFiscaisServico",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "NotasFiscaisServico");
            migrationBuilder.DropColumn(name: "PixGatewayRef", table: "Faturas");
            migrationBuilder.DropColumn(name: "PixExpiracaoEm", table: "Faturas");
        }
    }
}
