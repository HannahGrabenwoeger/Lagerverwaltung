using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 

namespace backend.Migrations
{
    public partial class AddLowStockNotificationFlag : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Warehouses",
                keyColumn: "Id",
                keyValue: new Guid("0dfdc6cf-30fa-4a81-b8e7-dd6fb69b7c31"));

            migrationBuilder.DeleteData(
                table: "Warehouses",
                keyColumn: "Id",
                keyValue: new Guid("3039c739-404d-4942-96e6-85608920fb5e"));

            migrationBuilder.DropColumn(
                name: "MovementType",
                table: "Movements");

            migrationBuilder.AddColumn<bool>(
                name: "HasSentLowStockNotification",
                table: "Products",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "HasSentLowStockNotification", "WarehouseId" },
                values: new object[] { false, new Guid("ffb47036-0657-43dc-80d1-2e3010ce7d49") });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "HasSentLowStockNotification", "WarehouseId" },
                values: new object[] { false, new Guid("2225f63f-84fe-4440-b992-579b38e03269") });

            // Füge neue Warehouse-Daten ein – diese IDs müssen mit den UpdateData-Befehlen übereinstimmen, falls du sie ändern möchtest.
            migrationBuilder.InsertData(
                table: "Warehouses",
                columns: new[] { "Id", "Location", "Name" },
                values: new object[,]
                {
                    { new Guid("74d8099d-cc60-47d2-a221-9c84af78a7df"), "Standort A", "Lager A" },
                    { new Guid("83f8f3e6-649a-46b7-8960-08707f94648b"), "Standort B", "Lager B" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Warehouses",
                keyColumn: "Id",
                keyValue: new Guid("74d8099d-cc60-47d2-a221-9c84af78a7df"));

            migrationBuilder.DeleteData(
                table: "Warehouses",
                keyColumn: "Id",
                keyValue: new Guid("83f8f3e6-649a-46b7-8960-08707f94648b"));

            migrationBuilder.DropColumn(
                name: "HasSentLowStockNotification",
                table: "Products");

            migrationBuilder.AddColumn<string>(
                name: "MovementType",
                table: "Movements",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "WarehouseId",
                value: new Guid("05e1ed1c-5617-48cf-bffc-c9922b9a9d1b"));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "WarehouseId",
                value: new Guid("74bfb85e-4a54-4743-a692-bf36d16a5ccb"));

            migrationBuilder.InsertData(
                table: "Warehouses",
                columns: new[] { "Id", "Location", "Name" },
                values: new object[,]
                {
                    { new Guid("0dfdc6cf-30fa-4a81-b8e7-dd6fb69b7c31"), "Standort A", "Lager A" },
                    { new Guid("3039c739-404d-4942-96e6-85608920fb5e"), "Standort B", "Lager B" }
                });
        }
    }
}