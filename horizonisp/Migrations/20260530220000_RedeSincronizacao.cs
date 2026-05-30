using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace horizonisp.Migrations
{
    /// <inheritdoc />
    public partial class RedeSincronizacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PortaApi",
                table: "Olts",
                type: "int",
                nullable: false,
                defaultValue: 80);

            migrationBuilder.AddColumn<string>(
                name: "SenhaApi",
                table: "Olts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UltimaSincronizacao",
                table: "Olts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UsuarioApi",
                table: "Olts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "PortaApi", table: "Olts");
            migrationBuilder.DropColumn(name: "SenhaApi", table: "Olts");
            migrationBuilder.DropColumn(name: "UltimaSincronizacao", table: "Olts");
            migrationBuilder.DropColumn(name: "UsuarioApi", table: "Olts");
        }
    }
}
