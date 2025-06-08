using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class FixUserRoleSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("9f147bac-bc26-45bc-a8fc-f4e2815e9fac"));

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("e9be03b3-a6cb-483c-93a7-cb3d18ce4c43"));

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "Id", "FirebaseUid", "Role" },
                values: new object[,]
                {
                    { new Guid("65af311e-d8a3-43ad-b1b4-c6fc06889387"), "test-user-2", "Employee" },
                    { new Guid("83ab39e4-0d0c-4600-9861-8eaf0f221ec4"), "test-user-1", "Manager" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("65af311e-d8a3-43ad-b1b4-c6fc06889387"));

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("83ab39e4-0d0c-4600-9861-8eaf0f221ec4"));

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "Id", "FirebaseUid", "Role" },
                values: new object[,]
                {
                    { new Guid("9f147bac-bc26-45bc-a8fc-f4e2815e9fac"), "test-user-2", "Employee" },
                    { new Guid("e9be03b3-a6cb-483c-93a7-cb3d18ce4c43"), "test-user-1", "Manager" }
                });
        }
    }
}
