using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace horizonisp.Migrations
{
    /// <inheritdoc />
    public partial class RecuperacaoSenhaPortal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "RecuperacaoSenhaExpiraEm",
                table: "Clientes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecuperacaoSenhaToken",
                table: "Clientes",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "RecuperacaoSenhaExpiraEm", table: "Clientes");
            migrationBuilder.DropColumn(name: "RecuperacaoSenhaToken", table: "Clientes");
        }
    }
}
