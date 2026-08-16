using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonalFinance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    kind = table.Column<string>(type: "text", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_system_defined = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_categories", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_accounts_category_id",
                table: "accounts",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_accounts_ledger_id_category_id",
                table: "accounts",
                columns: new[] { "ledger_id", "category_id" },
                unique: true,
                filter: "category_id IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "fk_accounts_categories_category_id",
                table: "accounts",
                column: "category_id",
                principalTable: "categories",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_accounts_categories_category_id",
                table: "accounts");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropIndex(
                name: "ix_accounts_category_id",
                table: "accounts");

            migrationBuilder.DropIndex(
                name: "ix_accounts_ledger_id_category_id",
                table: "accounts");
        }
    }
}
