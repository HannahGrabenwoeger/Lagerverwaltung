using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class CreateUserRolesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create the UserRoles table
            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FirebaseUid = table.Column<string>(type: "TEXT", nullable: false),
                    Role = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => x.Id);
                });

            // Update existing data in other tables
            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "Name",
                value: "Product 1");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "Name",
                value: "Product 2");

            // Insert seed data into UserRoles table using fixed GUIDs
            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "Id", "FirebaseUid", "Role" },
                values: new object[,]
                {
                    { new Guid("9f147bac-bc26-45bc-a8fc-f4e2815e9fac"), "test-user-2", "Employee" },
                    { new Guid("e9be03b3-a6cb-483c-93a7-cb3d18ce4c43"), "test-user-1", "Manager" }
                });

            migrationBuilder.UpdateData(
                table: "Warehouses",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "Location", "Name" },
                values: new object[] { "Location A", "Warehouse A" });

            migrationBuilder.UpdateData(
                table: "Warehouses",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "Location", "Name" },
                values: new object[] { "Location B", "Warehouse B" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("9f147bac-bc26-45bc-a8fc-f4e2815e9fac"));

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("e9be03b3-a6cb-483c-93a7-cb3d18ce4c43"));

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "Name",
                value: "Produkt 1");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "Name",
                value: "Produkt 2");

            migrationBuilder.UpdateData(
                table: "Warehouses",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "Location", "Name" },
                values: new object[] { "Standort A", "Lager A" });

            migrationBuilder.UpdateData(
                table: "Warehouses",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "Location", "Name" },
                values: new object[] { "Standort B", "Lager B" });
        }
    }
}
