using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoommateSplitter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GroupMemberIdDefault : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_GroupMembers",
                table: "GroupMembers");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "GroupMembers",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()",
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.Sql("""
                UPDATE "GroupMembers"
                SET "Id" = gen_random_uuid()
                WHERE "Id" IS NULL;
                """);

            migrationBuilder.AddPrimaryKey(
                name: "PK_GroupMembers",
                table: "GroupMembers",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_GroupMembers_GroupId_UserId",
                table: "GroupMembers",
                columns: new[] { "GroupId", "UserId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_GroupMembers",
                table: "GroupMembers");

            migrationBuilder.DropIndex(
                name: "IX_GroupMembers_GroupId_UserId",
                table: "GroupMembers");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "GroupMembers",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "gen_random_uuid()");
                
            migrationBuilder.AddPrimaryKey(
                name: "PK_GroupMembers",
                table: "GroupMembers",
                columns: new[] { "GroupId", "UserId" });
        }
            }
        }