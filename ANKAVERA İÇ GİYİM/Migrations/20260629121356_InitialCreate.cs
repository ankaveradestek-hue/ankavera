using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ANKAVERA_SİTESİ.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "RegisteredAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CategoryName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsNew = table.Column<bool>(type: "bit", nullable: false),
                    IsBestseller = table.Column<bool>(type: "bit", nullable: false),
                    Badge = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    StockCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Badge", "CategoryName", "CreatedAt", "Description", "ImageUrl", "IsActive", "IsBestseller", "IsNew", "Name", "Price", "StockCount" },
                values: new object[,]
                {
                    { 1, "Yeni", "Saten", new DateTime(2026, 6, 29, 15, 13, 55, 806, DateTimeKind.Local).AddTicks(3272), "Pudra pembe, ipeksi saten kumaş, dantelli kenar.", "https://images.unsplash.com/photo-1617922001439-4a2e6562f328?w=600&q=80", true, false, true, "Pembe Saten Takım", 849m, 99 },
                    { 2, "Çok Satan", "Dantel", new DateTime(2026, 6, 29, 15, 13, 55, 806, DateTimeKind.Local).AddTicks(3276), "Fransız danteli, sofistike tasarım.", "https://images.unsplash.com/photo-1604671368394-2240d0b1bb6c?w=600&q=80", true, true, false, "Ekru Dantel Bralet", 650m, 99 },
                    { 3, "", "Saten", new DateTime(2026, 6, 29, 15, 13, 55, 806, DateTimeKind.Local).AddTicks(3279), "Uzun kesim, V yaka, ince askılı.", "https://images.unsplash.com/photo-1631947430066-48c30d57b943?w=600&q=80", true, false, false, "Gül Kurusu Gecelik", 1190m, 99 },
                    { 4, "Özel", "Bridal", new DateTime(2026, 6, 29, 15, 13, 55, 806, DateTimeKind.Local).AddTicks(3281), "Düğün gecesi 3'lü lüks set.", "https://images.unsplash.com/photo-1616627547584-bf28cee262db?w=600&q=80", true, false, true, "Şampanya Bridal Set", 2250m, 99 },
                    { 5, "Çok Satan", "Dantel", new DateTime(2026, 6, 29, 15, 13, 55, 806, DateTimeKind.Local).AddTicks(3284), "Klasik korse, straplez kullanım.", "https://images.unsplash.com/photo-1618354691373-d851c5c3a990?w=600&q=80", true, true, false, "Derin Pembe Korse", 975m, 99 },
                    { 6, "Yeni", "Saten", new DateTime(2026, 6, 29, 15, 13, 55, 806, DateTimeKind.Local).AddTicks(3286), "Diz altı, kemer detaylı saten.", "https://images.unsplash.com/photo-1609081219090-a6d81d3085bf?w=600&q=80", true, false, true, "Lila Saten Sabahlık", 1450m, 99 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "RegisteredAt",
                table: "AspNetUsers");
        }
    }
}
