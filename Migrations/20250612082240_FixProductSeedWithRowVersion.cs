using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class FixProductSeedWithRowVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "RowVersion",
                value: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1"));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "RowVersion",
                value: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "RowVersion",
                value: new Guid("e56da15c-f5ee-44f1-96e8-009c513c0d6f"));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "RowVersion",
                value: new Guid("007e1ca7-cff2-4abe-af1f-ff10e7fba172"));
        }
    }
}
