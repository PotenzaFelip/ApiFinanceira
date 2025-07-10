using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiFinanceira.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial_commit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Pessoas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Documento = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    SenhaHash = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pessoas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Contas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PessoaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Branch = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Account = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: false),
                    Saldo = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Limite = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contas_Pessoas_PessoaId",
                        column: x => x.PessoaId,
                        principalTable: "Pessoas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Cartoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Number = table.Column<string>(type: "character varying(19)", maxLength: 19, nullable: false),
                    Cvv = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    ContaId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cartoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cartoes_Contas_ContaId",
                        column: x => x.ContaId,
                        principalTable: "Contas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Transacoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<decimal>(type: "numeric", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    IsReverted = table.Column<bool>(type: "boolean", nullable: false),
                    OriginalTransactionId = table.Column<Guid>(type: "uuid", nullable: true),
                    RelatedTransactionId = table.Column<Guid>(type: "uuid", nullable: true),
                    ContaId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transacoes_Contas_ContaId",
                        column: x => x.ContaId,
                        principalTable: "Contas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Transacoes_Contas_ContaId1",
                        column: x => x.ContaId1,
                        principalTable: "Contas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Transacoes_Transacoes_OriginalTransactionId",
                        column: x => x.OriginalTransactionId,
                        principalTable: "Transacoes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cartoes_ContaId",
                table: "Cartoes",
                column: "ContaId");

            migrationBuilder.CreateIndex(
                name: "IX_Cartoes_Number",
                table: "Cartoes",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contas_Account",
                table: "Contas",
                column: "Account",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contas_PessoaId",
                table: "Contas",
                column: "PessoaId");

            migrationBuilder.CreateIndex(
                name: "IX_Pessoas_Documento",
                table: "Pessoas",
                column: "Documento",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transacoes_ContaId",
                table: "Transacoes",
                column: "ContaId");

            migrationBuilder.CreateIndex(
                name: "IX_Transacoes_ContaId1",
                table: "Transacoes",
                column: "ContaId1");

            migrationBuilder.CreateIndex(
                name: "IX_Transacoes_OriginalTransactionId",
                table: "Transacoes",
                column: "OriginalTransactionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cartoes");

            migrationBuilder.DropTable(
                name: "Transacoes");

            migrationBuilder.DropTable(
                name: "Contas");

            migrationBuilder.DropTable(
                name: "Pessoas");
        }
    }
}
