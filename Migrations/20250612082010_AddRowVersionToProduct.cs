using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddRowVersionToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RowVersion",
                table: "Products",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

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

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-0000-1111-111111111111"),
                column: "FirebaseUid",
                value: "test-user-1");

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-0000-2222-222222222222"),
                column: "FirebaseUid",
                value: "test-user-2");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Products");

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-0000-1111-111111111111"),
                column: "FirebaseUid",
                value: "manager");

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-0000-2222-222222222222"),
                column: "FirebaseUid",
                value: "employee");
        }
    }
}
