using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonalFinance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInstallmentFieldsToJournalEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "installment_number",
                table: "journal_entries",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "installment_plan_id",
                table: "journal_entries",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "installment_total_count",
                table: "journal_entries",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_journal_entries_installment_plan_id",
                table: "journal_entries",
                column: "installment_plan_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_journal_entries_installment_plan_id",
                table: "journal_entries");

            migrationBuilder.DropColumn(
                name: "installment_number",
                table: "journal_entries");

            migrationBuilder.DropColumn(
                name: "installment_plan_id",
                table: "journal_entries");

            migrationBuilder.DropColumn(
                name: "installment_total_count",
                table: "journal_entries");
        }
    }
}
