using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MicroERP.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class SeedInitialData : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.InsertData(
            schema: "Products",
            table: "Products",
            columns: new[] { "Id", "CreatedAt", "Description", "IsActive", "Name", "Price", "Sku", "UpdatedAt" },
            values: new object[,]
            {
                { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "PLA Filament 1.75mm - Jet Black (1kg)", 19.99m, "FIL-PLA-BLK-1KG", null },
                { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "PLA Filament 1.75mm - Signal White (1kg)", 19.99m, "FIL-PLA-WHT-1KG", null },
                { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "PETG Filament 1.75mm - Jet Black (1kg)", 22.49m, "FIL-PETG-BLK1K", null },
                { 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "Hardened Steel Nozzle 0.4mm", 9.99m, "PAR-NOZ-HS04", null },
                { 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "Textured PEI Build Plate 257x257mm", 29.99m, "ACC-PEI-257", null },
                { 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "3D Print Finishing Tool Set", 14.50m, "TOO-POST-SET", null },
                { 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "Filament Dry Box Container", 42.99m, "ACC-DRY-BOX1", null }
            });

        migrationBuilder.InsertData(
            schema: "Settings",
            table: "Warehouses",
            columns: new[] { "Id", "CreatedAt", "IsActive", "Location", "Name", "UpdatedAt" },
            values: new object[,]
            {
                { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Cracow", "Main Warehouse", null },
                { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Warsaw", "Regional Warehouse", null },
                { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Prague", "External Warehouse", null }
            });

        migrationBuilder.InsertData(
            schema: "Products",
            table: "StockLevels",
            columns: new[] { "Id", "CreatedAt", "ProductId", "Quantity", "UpdatedAt", "WarehouseId" },
            values: new object[,]
            {
                { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 150, null, 1 },
                { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 60, null, 2 },
                { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 45, null, 1 },
                { 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 20, null, 3 },
                { 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, 30, null, 1 },
                { 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 12, null, 1 },
                { 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 8, null, 2 },
                { 8, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 5, 75, null, 1 },
                { 9, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 6, 200, null, 2 },
                { 10, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 7, 5, null, 3 }
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DeleteData(
            schema: "Products",
            table: "StockLevels",
            keyColumn: "Id",
            keyValue: 1);

        migrationBuilder.DeleteData(
            schema: "Products",
            table: "StockLevels",
            keyColumn: "Id",
            keyValue: 2);

        migrationBuilder.DeleteData(
            schema: "Products",
            table: "StockLevels",
            keyColumn: "Id",
            keyValue: 3);

        migrationBuilder.DeleteData(
            schema: "Products",
            table: "StockLevels",
            keyColumn: "Id",
            keyValue: 4);

        migrationBuilder.DeleteData(
            schema: "Products",
            table: "StockLevels",
            keyColumn: "Id",
            keyValue: 5);

        migrationBuilder.DeleteData(
            schema: "Products",
            table: "StockLevels",
            keyColumn: "Id",
            keyValue: 6);

        migrationBuilder.DeleteData(
            schema: "Products",
            table: "StockLevels",
            keyColumn: "Id",
            keyValue: 7);

        migrationBuilder.DeleteData(
            schema: "Products",
            table: "StockLevels",
            keyColumn: "Id",
            keyValue: 8);

        migrationBuilder.DeleteData(
            schema: "Products",
            table: "StockLevels",
            keyColumn: "Id",
            keyValue: 9);

        migrationBuilder.DeleteData(
            schema: "Products",
            table: "StockLevels",
            keyColumn: "Id",
            keyValue: 10);

        migrationBuilder.DeleteData(
            schema: "Products",
            table: "Products",
            keyColumn: "Id",
            keyValue: 1);

        migrationBuilder.DeleteData(
            schema: "Products",
            table: "Products",
            keyColumn: "Id",
            keyValue: 2);

        migrationBuilder.DeleteData(
            schema: "Products",
            table: "Products",
            keyColumn: "Id",
            keyValue: 3);

        migrationBuilder.DeleteData(
            schema: "Products",
            table: "Products",
            keyColumn: "Id",
            keyValue: 4);

        migrationBuilder.DeleteData(
            schema: "Products",
            table: "Products",
            keyColumn: "Id",
            keyValue: 5);

        migrationBuilder.DeleteData(
            schema: "Products",
            table: "Products",
            keyColumn: "Id",
            keyValue: 6);

        migrationBuilder.DeleteData(
            schema: "Products",
            table: "Products",
            keyColumn: "Id",
            keyValue: 7);

        migrationBuilder.DeleteData(
            schema: "Settings",
            table: "Warehouses",
            keyColumn: "Id",
            keyValue: 1);

        migrationBuilder.DeleteData(
            schema: "Settings",
            table: "Warehouses",
            keyColumn: "Id",
            keyValue: 2);

        migrationBuilder.DeleteData(
            schema: "Settings",
            table: "Warehouses",
            keyColumn: "Id",
            keyValue: 3);
    }
}
