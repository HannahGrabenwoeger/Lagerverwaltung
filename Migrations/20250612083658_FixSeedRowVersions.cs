using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class FixSeedRowVersions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: Guid.Parse("33333333-3333-3333-3333-333333333333"),
                column: "RowVersion",
                value: Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1")
            );

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: Guid.Parse("44444444-4444-4444-4444-444444444444"),
                column: "RowVersion",
                value: Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2")
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
