using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace horizonisp.Migrations
{
    /// <inheritdoc />
    public partial class ChamadosPix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Chamados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    Assunto = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Categoria = table.Column<int>(type: "int", nullable: false),
                    Prioridade = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DataAbertura = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chamados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chamados_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChamadoMensagens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChamadoId = table.Column<int>(type: "int", nullable: false),
                    AutorTipo = table.Column<int>(type: "int", nullable: false),
                    AutorNome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Conteudo = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    DataEnvio = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChamadoMensagens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChamadoMensagens_Chamados_ChamadoId",
                        column: x => x.ChamadoId,
                        principalTable: "Chamados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PagamentosPix",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FaturaId = table.Column<int>(type: "int", nullable: false),
                    TxId = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    EndToEndId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Valor = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Origem = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RecebidoEm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PagamentosPix", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PagamentosPix_Faturas_FaturaId",
                        column: x => x.FaturaId,
                        principalTable: "Faturas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(name: "IX_Chamados_ClienteId", table: "Chamados", column: "ClienteId");
            migrationBuilder.CreateIndex(name: "IX_Chamados_Status", table: "Chamados", column: "Status");
            migrationBuilder.CreateIndex(name: "IX_ChamadoMensagens_ChamadoId", table: "ChamadoMensagens", column: "ChamadoId");
            migrationBuilder.CreateIndex(name: "IX_Faturas_PixTxId", table: "Faturas", column: "PixTxId");
            migrationBuilder.CreateIndex(name: "IX_PagamentosPix_EndToEndId", table: "PagamentosPix", column: "EndToEndId");
            migrationBuilder.CreateIndex(name: "IX_PagamentosPix_FaturaId", table: "PagamentosPix", column: "FaturaId");
            migrationBuilder.CreateIndex(name: "IX_PagamentosPix_TxId", table: "PagamentosPix", column: "TxId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ChamadoMensagens");
            migrationBuilder.DropTable(name: "PagamentosPix");
            migrationBuilder.DropTable(name: "Chamados");
            migrationBuilder.DropIndex(name: "IX_Faturas_PixTxId", table: "Faturas");
        }
    }
}
