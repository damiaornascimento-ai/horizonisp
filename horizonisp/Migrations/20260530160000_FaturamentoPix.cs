using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace horizonisp.Migrations
{
    /// <inheritdoc />
    public partial class FaturamentoPix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AvisoAtrasoEnviadoEm",
                table: "Faturas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LembreteVencimentoEnviadoEm",
                table: "Faturas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PixCopiaCola",
                table: "Faturas",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PixTxId",
                table: "Faturas",
                type: "nvarchar(25)",
                maxLength: 25,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "AvisoAtrasoEnviadoEm", table: "Faturas");
            migrationBuilder.DropColumn(name: "LembreteVencimentoEnviadoEm", table: "Faturas");
            migrationBuilder.DropColumn(name: "PixCopiaCola", table: "Faturas");
            migrationBuilder.DropColumn(name: "PixTxId", table: "Faturas");
        }
    }
}
