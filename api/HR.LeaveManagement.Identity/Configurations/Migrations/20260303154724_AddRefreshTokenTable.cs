using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR.LeaveManagement.Identity.Configurations.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshTokenTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JwtId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false),
                    AddedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "d04eb003-db0e-4eff-943d-aba0f84aa153",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "94c73e9f-dac6-44ec-aaf5-e71bf7447e20", "AQAAAAIAAYagAAAAECTLTW+j62eMkkiXIOr3ujpbIs28evYCN+RFVHv31jBwMrHl9joQyZC9xjGPCB9s+Q==", "43c8fca1-3d5c-4181-b5ae-c5170f3f28e5" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "f0493071-d633-4ec6-8215-051097bc8bef1",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "9f2847ce-a5d1-4e77-af70-dd17a10cb700", "AQAAAAIAAYagAAAAEGQyYlj2+/TOapox9h8cybIzjjG7T4NkVgcqZMn8txLhsmoaL9wh8o3Rie17+AGNcQ==", "3641367a-258d-4430-ad50-5df2284d4464" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "d04eb003-db0e-4eff-943d-aba0f84aa153",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "72d493d9-ed8d-414b-95e5-166e66bed661", "AQAAAAIAAYagAAAAENpCwy+v2DvYec/wAGEmEO3Ee+l1aUPIWnSWkj8FoBJxT7CJk93/yiND194Q3r7f8A==", "04029839-8a30-4d70-ba36-7e1944af1637" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "f0493071-d633-4ec6-8215-051097bc8bef1",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "cc91b249-abd5-44b7-9f9b-d16377b8cfde", "AQAAAAIAAYagAAAAEKXxWijKY482UuXjAitij+clgyAUVC6IAuGMLfiQNaPxNyteIbQ09sGFdbKUQhDRGQ==", "69011c06-e3f2-4ad8-9452-b833274cd8cc" });
        }
    }
}
