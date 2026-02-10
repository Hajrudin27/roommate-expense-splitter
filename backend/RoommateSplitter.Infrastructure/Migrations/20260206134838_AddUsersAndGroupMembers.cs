using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoommateSplitter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUsersAndGroupMembers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExpenseShares_Expenses_ExpenseId",
                table: "ExpenseShares");

            migrationBuilder.DropIndex(
                name: "IX_ExpenseShares_ExpenseId",
                table: "ExpenseShares");

            migrationBuilder.AddColumn<Guid>(
                name: "ExpenseRowId",
                table: "ExpenseShares",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseShares_ExpenseRowId",
                table: "ExpenseShares",
                column: "ExpenseRowId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExpenseShares_Expenses_ExpenseRowId",
                table: "ExpenseShares",
                column: "ExpenseRowId",
                principalTable: "Expenses",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExpenseShares_Expenses_ExpenseRowId",
                table: "ExpenseShares");

            migrationBuilder.DropIndex(
                name: "IX_ExpenseShares_ExpenseRowId",
                table: "ExpenseShares");

            migrationBuilder.DropColumn(
                name: "ExpenseRowId",
                table: "ExpenseShares");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseShares_ExpenseId",
                table: "ExpenseShares",
                column: "ExpenseId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExpenseShares_Expenses_ExpenseId",
                table: "ExpenseShares",
                column: "ExpenseId",
                principalTable: "Expenses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
