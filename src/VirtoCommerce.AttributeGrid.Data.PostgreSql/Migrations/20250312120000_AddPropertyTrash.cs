using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.AttributeGrid.Data.PostgreSql.Migrations;

public partial class AddPropertyTrash : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "AttributeGrid_Trash",
            columns: table => new
            {
                Id = table.Column<string>(maxLength: 128, nullable: false),
                PropertyId = table.Column<string>(maxLength: 128, nullable: false),
                PropertyName = table.Column<string>(maxLength: 256, nullable: false),
                PropertyCode = table.Column<string>(maxLength: 128, nullable: false),
                CatalogId = table.Column<string>(maxLength: 128, nullable: true),
                CategoryId = table.Column<string>(maxLength: 128, nullable: true),
                PropertyDataJson = table.Column<string>(nullable: false),
                DeletedBy = table.Column<string>(maxLength: 128, nullable: false),
                DeletedDate = table.Column<DateTime>(nullable: false),
                ExpirationDate = table.Column<DateTime>(nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AttributeGrid_Trash", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AttributeGrid_Trash_DeletedDate",
            table: "AttributeGrid_Trash",
            column: "DeletedDate");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "AttributeGrid_Trash");
    }
}
