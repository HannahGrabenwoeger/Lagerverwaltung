using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddUnitAndPerformedBy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("65af311e-d8a3-43ad-b1b4-c6fc06889387"));

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("83ab39e4-0d0c-4600-9861-8eaf0f221ec4"));

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "User",
                table: "Movements",
                newName: "PerformedBy");

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "Products",
                type: "TEXT",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "MinimumStock", "Unit" },
                values: new object[] { 10, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "MinimumStock", "Unit" },
                values: new object[] { 5, null });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "Id", "FirebaseUid", "Role" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-0000-1111-111111111111"), "manager", "Manager" },
                    { new Guid("22222222-2222-0000-2222-222222222222"), "employee", "Employee" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-0000-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-0000-2222-222222222222"));

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "PerformedBy",
                table: "Movements",
                newName: "User");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Products",
                type: "BLOB",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "MinimumStock",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "MinimumStock",
                value: 0);

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "Id", "FirebaseUid", "Role" },
                values: new object[,]
                {
                    { new Guid("65af311e-d8a3-43ad-b1b4-c6fc06889387"), "employee", "Employee" },
                    { new Guid("83ab39e4-0d0c-4600-9861-8eaf0f221ec4"), "manager", "Manager" }
                });
        }
    }
}
