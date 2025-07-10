using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiFinanceira.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Alterando_tabla_transacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transacoes_Contas_ContaId1",
                table: "Transacoes");

            migrationBuilder.DropIndex(
                name: "IX_Transacoes_ContaId1",
                table: "Transacoes");

            migrationBuilder.DropColumn(
                name: "ContaId1",
                table: "Transacoes");

            migrationBuilder.DropColumn(
                name: "RelatedTransactionId",
                table: "Transacoes");

            migrationBuilder.DropColumn(
                name: "Telefone",
                table: "Pessoas");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ContaId1",
                table: "Transacoes",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RelatedTransactionId",
                table: "Transacoes",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Telefone",
                table: "Pessoas",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transacoes_ContaId1",
                table: "Transacoes",
                column: "ContaId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Transacoes_Contas_ContaId1",
                table: "Transacoes",
                column: "ContaId1",
                principalTable: "Contas",
                principalColumn: "Id");
        }
    }
}
