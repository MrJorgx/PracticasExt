using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Ejercicios.Backend.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Dni = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: false),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Apellidos = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TipoCliente = table.Column<string>(type: "text", nullable: false),
                    CuotaMaxima = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    FechaAlta = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Dni);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NombreUsuario = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    NombreCompleto = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorCode = table.Column<string>(type: "text", nullable: true),
                    TwoFactorCodeExpiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FailedTwoFactorAttempts = table.Column<int>(type: "integer", nullable: false),
                    PasswordResetToken = table.Column<string>(type: "text", nullable: true),
                    PasswordResetTokenExpiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Recibos",
                columns: table => new
                {
                    NumeroRecibo = table.Column<string>(type: "text", nullable: false),
                    DniCliente = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: false),
                    Importe = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    FechaEmision = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recibos", x => x.NumeroRecibo);
                    table.ForeignKey(
                        name: "FK_Recibos_Clientes_DniCliente",
                        column: x => x.DniCliente,
                        principalTable: "Clientes",
                        principalColumn: "Dni",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Recibos_DniCliente",
                table: "Recibos",
                column: "DniCliente");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_NombreUsuario",
                table: "Usuarios",
                column: "NombreUsuario",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Recibos");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Clientes");
        }
    }
}
