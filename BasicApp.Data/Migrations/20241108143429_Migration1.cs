using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BasicApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class Migration1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Password = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    PasswordRetryCount = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    CreatedById = table.Column<int>(type: "INTEGER", nullable: false),
                    InsertDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedById = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    UpdateDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
