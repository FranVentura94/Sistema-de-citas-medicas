using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MsClinical.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InicialClinical : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "Diagnosticos",
                schema: "dbo",
                columns: table => new
                {
                    DiagnosticoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Diagnosticos", x => x.DiagnosticoId);
                });

            migrationBuilder.CreateTable(
                name: "ExpedientesClinicos",
                schema: "dbo",
                columns: table => new
                {
                    ExpedienteClinicoId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PacienteId = table.Column<long>(type: "bigint", nullable: false),
                    PacienteNombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NumeroExpediente = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaApertura = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Alergias = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AntecedentesPersonales = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AntecedentesFamiliares = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpedientesClinicos", x => x.ExpedienteClinicoId);
                });

            migrationBuilder.CreateTable(
                name: "Medicamentos",
                schema: "dbo",
                columns: table => new
                {
                    MedicamentoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PrincipioActivo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Presentacion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Concentracion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medicamentos", x => x.MedicamentoId);
                });

            migrationBuilder.CreateTable(
                name: "Atenciones",
                schema: "dbo",
                columns: table => new
                {
                    AtencionId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExpedienteClinicoId = table.Column<long>(type: "bigint", nullable: false),
                    CitaId = table.Column<long>(type: "bigint", nullable: false),
                    PacienteId = table.Column<long>(type: "bigint", nullable: false),
                    PacienteNombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MedicoId = table.Column<int>(type: "int", nullable: false),
                    MedicoNombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    EspecialidadNombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MotivoConsulta = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Atenciones", x => x.AtencionId);
                    table.ForeignKey(
                        name: "FK_Atenciones_ExpedientesClinicos_ExpedienteClinicoId",
                        column: x => x.ExpedienteClinicoId,
                        principalSchema: "dbo",
                        principalTable: "ExpedientesClinicos",
                        principalColumn: "ExpedienteClinicoId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AtencionDiagnosticos",
                schema: "dbo",
                columns: table => new
                {
                    AtencionId = table.Column<long>(type: "bigint", nullable: false),
                    DiagnosticoId = table.Column<int>(type: "int", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Observacion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AtencionDiagnosticos", x => new { x.AtencionId, x.DiagnosticoId });
                    table.ForeignKey(
                        name: "FK_AtencionDiagnosticos_Atenciones_AtencionId",
                        column: x => x.AtencionId,
                        principalSchema: "dbo",
                        principalTable: "Atenciones",
                        principalColumn: "AtencionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AtencionDiagnosticos_Diagnosticos_DiagnosticoId",
                        column: x => x.DiagnosticoId,
                        principalSchema: "dbo",
                        principalTable: "Diagnosticos",
                        principalColumn: "DiagnosticoId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NotasMedicas",
                schema: "dbo",
                columns: table => new
                {
                    NotaMedicaId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AtencionId = table.Column<long>(type: "bigint", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Contenido = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Autor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotasMedicas", x => x.NotaMedicaId);
                    table.ForeignKey(
                        name: "FK_NotasMedicas_Atenciones_AtencionId",
                        column: x => x.AtencionId,
                        principalSchema: "dbo",
                        principalTable: "Atenciones",
                        principalColumn: "AtencionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Prescripciones",
                schema: "dbo",
                columns: table => new
                {
                    PrescripcionId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AtencionId = table.Column<long>(type: "bigint", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IndicacionesGenerales = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prescripciones", x => x.PrescripcionId);
                    table.ForeignKey(
                        name: "FK_Prescripciones_Atenciones_AtencionId",
                        column: x => x.AtencionId,
                        principalSchema: "dbo",
                        principalTable: "Atenciones",
                        principalColumn: "AtencionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SignosVitales",
                schema: "dbo",
                columns: table => new
                {
                    SignosVitalesId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AtencionId = table.Column<long>(type: "bigint", nullable: false),
                    PresionSistolica = table.Column<int>(type: "int", nullable: true),
                    PresionDiastolica = table.Column<int>(type: "int", nullable: true),
                    FrecuenciaCardiaca = table.Column<int>(type: "int", nullable: true),
                    FrecuenciaRespiratoria = table.Column<int>(type: "int", nullable: true),
                    Temperatura = table.Column<decimal>(type: "decimal(4,1)", precision: 4, scale: 1, nullable: true),
                    SaturacionOxigeno = table.Column<int>(type: "int", nullable: true),
                    Peso = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    Talla = table.Column<decimal>(type: "decimal(5,1)", precision: 5, scale: 1, nullable: true),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SignosVitales", x => x.SignosVitalesId);
                    table.ForeignKey(
                        name: "FK_SignosVitales_Atenciones_AtencionId",
                        column: x => x.AtencionId,
                        principalSchema: "dbo",
                        principalTable: "Atenciones",
                        principalColumn: "AtencionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PrescripcionDetalles",
                schema: "dbo",
                columns: table => new
                {
                    PrescripcionDetalleId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PrescripcionId = table.Column<long>(type: "bigint", nullable: false),
                    MedicamentoId = table.Column<int>(type: "int", nullable: false),
                    Dosis = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Frecuencia = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Duracion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Via = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Indicaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrescripcionDetalles", x => x.PrescripcionDetalleId);
                    table.ForeignKey(
                        name: "FK_PrescripcionDetalles_Medicamentos_MedicamentoId",
                        column: x => x.MedicamentoId,
                        principalSchema: "dbo",
                        principalTable: "Medicamentos",
                        principalColumn: "MedicamentoId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrescripcionDetalles_Prescripciones_PrescripcionId",
                        column: x => x.PrescripcionId,
                        principalSchema: "dbo",
                        principalTable: "Prescripciones",
                        principalColumn: "PrescripcionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "Diagnosticos",
                columns: new[] { "DiagnosticoId", "Codigo", "Descripcion" },
                values: new object[,]
                {
                    { 1, "J00", "Rinofaringitis aguda (resfriado común)" },
                    { 2, "J02.9", "Faringitis aguda, no especificada" },
                    { 3, "J03.9", "Amigdalitis aguda, no especificada" },
                    { 4, "J06.9", "Infección aguda de las vías respiratorias superiores" },
                    { 5, "J20.9", "Bronquitis aguda, no especificada" },
                    { 6, "A09", "Diarrea y gastroenteritis de presunto origen infeccioso" },
                    { 7, "K29.7", "Gastritis, no especificada" },
                    { 8, "K21.9", "Enfermedad del reflujo gastroesofágico" },
                    { 9, "N39.0", "Infección de vías urinarias, sitio no especificado" },
                    { 10, "I10", "Hipertensión esencial (primaria)" },
                    { 11, "E11.9", "Diabetes mellitus tipo 2 sin complicaciones" },
                    { 12, "E78.5", "Hiperlipidemia, no especificada" },
                    { 13, "E66.9", "Obesidad, no especificada" },
                    { 14, "R51", "Cefalea" },
                    { 15, "G43.9", "Migraña, no especificada" },
                    { 16, "M54.5", "Lumbago no especificado" },
                    { 17, "L20.9", "Dermatitis atópica, no especificada" },
                    { 18, "J45.9", "Asma, no especificada" },
                    { 19, "F41.1", "Trastorno de ansiedad generalizada" },
                    { 20, "Z00.0", "Examen médico general" }
                });

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "Medicamentos",
                columns: new[] { "MedicamentoId", "Concentracion", "Nombre", "Presentacion", "PrincipioActivo" },
                values: new object[,]
                {
                    { 1, "500 mg", "Acetaminofén", "Tableta", "Paracetamol" },
                    { 2, "400 mg", "Ibuprofeno", "Tableta", "Ibuprofeno" },
                    { 3, "500 mg", "Amoxicilina", "Cápsula", "Amoxicilina" },
                    { 4, "250 mg/5 ml", "Amoxicilina suspensión", "Suspensión", "Amoxicilina" },
                    { 5, "500 mg", "Azitromicina", "Tableta", "Azitromicina" },
                    { 6, "10 mg", "Loratadina", "Tableta", "Loratadina" },
                    { 7, "20 mg", "Omeprazol", "Cápsula", "Omeprazol" },
                    { 8, "850 mg", "Metformina", "Tableta", "Metformina" },
                    { 9, "50 mg", "Losartán", "Tableta", "Losartán potásico" },
                    { 10, "10 mg", "Enalapril", "Tableta", "Enalapril" },
                    { 11, "20 mg", "Atorvastatina", "Tableta", "Atorvastatina" },
                    { 12, "100 mcg/dosis", "Salbutamol inhalador", "Inhalador", "Salbutamol" },
                    { 13, "27.9 g", "Suero oral", "Sobre", "Sales de rehidratación oral" },
                    { 14, "100 mg", "Nitrofurantoína", "Cápsula", "Nitrofurantoína" },
                    { 15, "50 mg", "Diclofenaco", "Tableta", "Diclofenaco sódico" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AtencionDiagnosticos_DiagnosticoId",
                schema: "dbo",
                table: "AtencionDiagnosticos",
                column: "DiagnosticoId");

            migrationBuilder.CreateIndex(
                name: "IX_Atenciones_CitaId",
                schema: "dbo",
                table: "Atenciones",
                column: "CitaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Atenciones_ExpedienteClinicoId",
                schema: "dbo",
                table: "Atenciones",
                column: "ExpedienteClinicoId");

            migrationBuilder.CreateIndex(
                name: "IX_Atenciones_MedicoId_FechaInicio",
                schema: "dbo",
                table: "Atenciones",
                columns: new[] { "MedicoId", "FechaInicio" });

            migrationBuilder.CreateIndex(
                name: "IX_Diagnosticos_Codigo",
                schema: "dbo",
                table: "Diagnosticos",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExpedientesClinicos_NumeroExpediente",
                schema: "dbo",
                table: "ExpedientesClinicos",
                column: "NumeroExpediente",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExpedientesClinicos_PacienteId",
                schema: "dbo",
                table: "ExpedientesClinicos",
                column: "PacienteId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotasMedicas_AtencionId",
                schema: "dbo",
                table: "NotasMedicas",
                column: "AtencionId");

            migrationBuilder.CreateIndex(
                name: "IX_PrescripcionDetalles_MedicamentoId",
                schema: "dbo",
                table: "PrescripcionDetalles",
                column: "MedicamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_PrescripcionDetalles_PrescripcionId",
                schema: "dbo",
                table: "PrescripcionDetalles",
                column: "PrescripcionId");

            migrationBuilder.CreateIndex(
                name: "IX_Prescripciones_AtencionId",
                schema: "dbo",
                table: "Prescripciones",
                column: "AtencionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SignosVitales_AtencionId",
                schema: "dbo",
                table: "SignosVitales",
                column: "AtencionId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AtencionDiagnosticos",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "NotasMedicas",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "PrescripcionDetalles",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "SignosVitales",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Diagnosticos",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Medicamentos",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Prescripciones",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Atenciones",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ExpedientesClinicos",
                schema: "dbo");
        }
    }
}
