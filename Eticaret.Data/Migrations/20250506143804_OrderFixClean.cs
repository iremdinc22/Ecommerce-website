using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eticaret.Data.Migrations
{
    public partial class OrderFixClean : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Addresses_Addresses_AddressId",
                table: "Addresses");

            //migrationBuilder.DropIndex(
            //    name: "IX_Addresses_AddressId",
            //    table: "Addresses");

            migrationBuilder.DropColumn(
                name: "AddressId",
                table: "Addresses");

            // Sadece yeni sütunu ekliyoruz
            migrationBuilder.AddColumn<string>(
                name: "OrderState",
                table: "Orders",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_AppUserId",
                table: "Addresses",
                column: "AppUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Addresses_AppUsers_AppUserId",
                table: "Addresses",
                column: "AppUserId",
                principalTable: "AppUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            // Seed güncellemeleri duruma göre kalabilir, ihtiyacın varsa bırakabilirsin
            migrationBuilder.UpdateData(
                table: "AppUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreateDate", "UserGuid" },
                values: new object[] { new DateTime(2025, 5, 6, 17, 38, 4, 235, DateTimeKind.Local).AddTicks(3200), new Guid("64e81fbe-2207-4224-b3ad-762d27dd54ec") });

            migrationBuilder.UpdateData(
                table: "Categorys",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreateDate",
                value: new DateTime(2025, 5, 6, 17, 38, 4, 235, DateTimeKind.Local).AddTicks(4880));

            migrationBuilder.UpdateData(
                table: "Categorys",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreateDate",
                value: new DateTime(2025, 5, 6, 17, 38, 4, 235, DateTimeKind.Local).AddTicks(4890));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Addresses_AppUsers_AppUserId",
                table: "Addresses");

            migrationBuilder.DropIndex(
                name: "IX_Addresses_AppUserId",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "OrderState",
                table: "Orders");

            migrationBuilder.AddColumn<int>(
                name: "AddressId",
                table: "Addresses",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AppUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreateDate", "UserGuid" },
                values: new object[] { new DateTime(2025, 4, 28, 10, 4, 17, 523, DateTimeKind.Local).AddTicks(4840), new Guid("b25f7a56-dc63-45cf-8e01-c8abd5bd256a") });

            migrationBuilder.UpdateData(
                table: "Categorys",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreateDate",
                value: new DateTime(2025, 4, 28, 10, 4, 17, 523, DateTimeKind.Local).AddTicks(5990));

            migrationBuilder.UpdateData(
                table: "Categorys",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreateDate",
                value: new DateTime(2025, 4, 28, 10, 4, 17, 523, DateTimeKind.Local).AddTicks(5990));

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_AddressId",
                table: "Addresses",
                column: "AddressId");

            migrationBuilder.AddForeignKey(
                name: "FK_Addresses_Addresses_AddressId",
                table: "Addresses",
                column: "AddressId",
                principalTable: "Addresses",
                principalColumn: "Id");
        }
    }
}
