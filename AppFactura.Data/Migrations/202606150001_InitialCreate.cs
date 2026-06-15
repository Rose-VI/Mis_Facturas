using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace AppFactura.Data.Migrations;

[DbContext(typeof(InvoiceDbContext))]
[Migration("202606150001_InitialCreate")]
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Invoices",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Title = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                IssuedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                Memorandum = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                IsFavorite = table.Column<bool>(type: "INTEGER", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Invoices", x => x.Id));

        migrationBuilder.CreateTable(
            name: "InvoiceEvidences",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                InvoiceId = table.Column<int>(type: "INTEGER", nullable: false),
                LocalPath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                FileName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                SortOrder = table.Column<int>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_InvoiceEvidences", x => x.Id);
                table.ForeignKey(
                    name: "FK_InvoiceEvidences_Invoices_InvoiceId",
                    column: x => x.InvoiceId,
                    principalTable: "Invoices",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_InvoiceEvidences_InvoiceId_SortOrder",
            table: "InvoiceEvidences",
            columns: new[] { "InvoiceId", "SortOrder" });
        migrationBuilder.CreateIndex(name: "IX_Invoices_IsFavorite", table: "Invoices", column: "IsFavorite");
        migrationBuilder.CreateIndex(name: "IX_Invoices_IssuedAt", table: "Invoices", column: "IssuedAt");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "InvoiceEvidences");
        migrationBuilder.DropTable(name: "Invoices");
    }
}
