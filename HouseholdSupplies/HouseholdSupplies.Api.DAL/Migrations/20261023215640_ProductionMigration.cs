using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HouseholdSupplies.Api.DAL.Migrations
{
    /// <inheritdoc />
    public partial class ProductionMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The earlier production migrations now create SQL Server types directly.
            // Keep this migration ID for history compatibility; no conversion is needed.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // The preceding migration already targets the same SQL Server schema.
        }
    }
}
