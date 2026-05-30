using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace horizonisp.Migrations
{
    /// <inheritdoc />
    public partial class OrdemServicoBoleto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BoletoCodigoBarras",
                table: "Faturas",
                type: "nvarchar(44)",
                maxLength: 44,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BoletoLinhaDigitavel",
                table: "Faturas",
                type: "nvarchar(54)",
                maxLength: 54,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BoletoNossoNumero",
                table: "Faturas",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OrdensServico",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    AssinaturaId = table.Column<int>(type: "int", nullable: true),
                    ChamadoId = table.Column<int>(type: "int", nullable: true),
                    Titulo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TecnicoResponsavel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Endereco = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    DataAgendada = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataConclusao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ObservacaoConclusao = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DataAbertura = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdensServico", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrdensServico_Assinaturas_AssinaturaId",
                        column: x => x.AssinaturaId,
                        principalTable: "Assinaturas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_OrdensServico_Chamados_ChamadoId",
                        column: x => x.ChamadoId,
                        principalTable: "Chamados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_OrdensServico_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(name: "IX_OrdensServico_AssinaturaId", table: "OrdensServico", column: "AssinaturaId");
            migrationBuilder.CreateIndex(name: "IX_OrdensServico_ChamadoId", table: "OrdensServico", column: "ChamadoId");
            migrationBuilder.CreateIndex(name: "IX_OrdensServico_ClienteId", table: "OrdensServico", column: "ClienteId");
            migrationBuilder.CreateIndex(name: "IX_OrdensServico_Status", table: "OrdensServico", column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "OrdensServico");
            migrationBuilder.DropColumn(name: "BoletoCodigoBarras", table: "Faturas");
            migrationBuilder.DropColumn(name: "BoletoLinhaDigitavel", table: "Faturas");
            migrationBuilder.DropColumn(name: "BoletoNossoNumero", table: "Faturas");
        }
    }
}
