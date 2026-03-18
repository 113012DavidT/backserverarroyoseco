using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace arroyoSeco.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgreSQL : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notificaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioId = table.Column<string>(type: "text", nullable: false),
                    Titulo = table.Column<string>(type: "text", nullable: false),
                    Mensaje = table.Column<string>(type: "text", nullable: false),
                    Tipo = table.Column<string>(type: "text", nullable: false),
                    Leida = table.Column<bool>(type: "boolean", nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UrlAccion = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notificaciones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Oferentes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Estado = table.Column<string>(type: "text", nullable: false),
                    NumeroAlojamientos = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Oferentes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesOferente",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NombreSolicitante = table.Column<string>(type: "text", nullable: false),
                    Telefono = table.Column<string>(type: "text", nullable: false),
                    NombreNegocio = table.Column<string>(type: "text", nullable: false),
                    Correo = table.Column<string>(type: "text", nullable: false),
                    Mensaje = table.Column<string>(type: "text", nullable: true),
                    TipoSolicitado = table.Column<int>(type: "integer", nullable: false),
                    Estatus = table.Column<string>(type: "text", nullable: false),
                    FechaSolicitud = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaRespuesta = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AdminId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesOferente", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Alojamientos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OferenteId = table.Column<string>(type: "text", nullable: false),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Ubicacion = table.Column<string>(type: "text", nullable: false),
                    MaxHuespedes = table.Column<int>(type: "integer", nullable: false),
                    Habitaciones = table.Column<int>(type: "integer", nullable: false),
                    Banos = table.Column<int>(type: "integer", nullable: false),
                    PrecioPorNoche = table.Column<decimal>(type: "numeric", nullable: false),
                    FotoPrincipal = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alojamientos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Alojamientos_Oferentes_OferenteId",
                        column: x => x.OferenteId,
                        principalTable: "Oferentes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Establecimientos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OferenteId = table.Column<string>(type: "text", nullable: false),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Ubicacion = table.Column<string>(type: "text", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: true),
                    FotoPrincipal = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Establecimientos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Establecimientos_Oferentes_OferenteId",
                        column: x => x.OferenteId,
                        principalTable: "Oferentes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FotosAlojamiento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AlojamientoId = table.Column<int>(type: "integer", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false),
                    Orden = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FotosAlojamiento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FotosAlojamiento_Alojamientos_AlojamientoId",
                        column: x => x.AlojamientoId,
                        principalTable: "Alojamientos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reservas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Folio = table.Column<string>(type: "text", nullable: false),
                    AlojamientoId = table.Column<int>(type: "integer", nullable: false),
                    ClienteId = table.Column<string>(type: "text", nullable: false),
                    FechaEntrada = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaSalida = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Total = table.Column<decimal>(type: "numeric(65,30)", nullable: false),
                    Estado = table.Column<string>(type: "text", nullable: false),
                    FechaReserva = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ComprobanteUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reservas_Alojamientos_AlojamientoId",
                        column: x => x.AlojamientoId,
                        principalTable: "Alojamientos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Menus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EstablecimientoId = table.Column<int>(type: "integer", nullable: false),
                    Nombre = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Menus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Menus_Establecimientos_EstablecimientoId",
                        column: x => x.EstablecimientoId,
                        principalTable: "Establecimientos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Mesas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EstablecimientoId = table.Column<int>(type: "integer", nullable: false),
                    Numero = table.Column<int>(type: "integer", nullable: false),
                    Capacidad = table.Column<int>(type: "integer", nullable: false),
                    Disponible = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mesas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Mesas_Establecimientos_EstablecimientoId",
                        column: x => x.EstablecimientoId,
                        principalTable: "Establecimientos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MenuItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MenuId = table.Column<int>(type: "integer", nullable: false),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: true),
                    Precio = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MenuItems_Menus_MenuId",
                        column: x => x.MenuId,
                        principalTable: "Menus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReservasGastronomia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioId = table.Column<string>(type: "text", nullable: false),
                    EstablecimientoId = table.Column<int>(type: "integer", nullable: false),
                    MesaId = table.Column<int>(type: "integer", nullable: true),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Estado = table.Column<string>(type: "text", nullable: false),
                    NumeroPersonas = table.Column<int>(type: "integer", nullable: false),
                    Total = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservasGastronomia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReservasGastronomia_Establecimientos_EstablecimientoId",
                        column: x => x.EstablecimientoId,
                        principalTable: "Establecimientos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReservasGastronomia_Mesas_MesaId",
                        column: x => x.MesaId,
                        principalTable: "Mesas",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Alojamientos_OferenteId",
                table: "Alojamientos",
                column: "OferenteId");

            migrationBuilder.CreateIndex(
                name: "IX_Establecimientos_OferenteId",
                table: "Establecimientos",
                column: "OferenteId");

            migrationBuilder.CreateIndex(
                name: "IX_FotosAlojamiento_AlojamientoId",
                table: "FotosAlojamiento",
                column: "AlojamientoId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_MenuId",
                table: "MenuItems",
                column: "MenuId");

            migrationBuilder.CreateIndex(
                name: "IX_Menus_EstablecimientoId",
                table: "Menus",
                column: "EstablecimientoId");

            migrationBuilder.CreateIndex(
                name: "IX_Mesas_EstablecimientoId",
                table: "Mesas",
                column: "EstablecimientoId");

            migrationBuilder.CreateIndex(
                name: "IX_Notificaciones_UsuarioId",
                table: "Notificaciones",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_AlojamientoId",
                table: "Reservas",
                column: "AlojamientoId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_Folio",
                table: "Reservas",
                column: "Folio",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReservasGastronomia_EstablecimientoId",
                table: "ReservasGastronomia",
                column: "EstablecimientoId");

            migrationBuilder.CreateIndex(
                name: "IX_ReservasGastronomia_MesaId",
                table: "ReservasGastronomia",
                column: "MesaId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesOferente_Estatus",
                table: "SolicitudesOferente",
                column: "Estatus");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FotosAlojamiento");

            migrationBuilder.DropTable(
                name: "MenuItems");

            migrationBuilder.DropTable(
                name: "Notificaciones");

            migrationBuilder.DropTable(
                name: "Reservas");

            migrationBuilder.DropTable(
                name: "ReservasGastronomia");

            migrationBuilder.DropTable(
                name: "SolicitudesOferente");

            migrationBuilder.DropTable(
                name: "Menus");

            migrationBuilder.DropTable(
                name: "Alojamientos");

            migrationBuilder.DropTable(
                name: "Mesas");

            migrationBuilder.DropTable(
                name: "Establecimientos");

            migrationBuilder.DropTable(
                name: "Oferentes");
        }
    }
}
