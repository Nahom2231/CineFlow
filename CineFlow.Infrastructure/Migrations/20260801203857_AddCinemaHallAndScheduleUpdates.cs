using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CineFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCinemaHallAndScheduleUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Movies_Director_DirectorId1",
                table: "Movies");

            migrationBuilder.DropIndex(
                name: "IX_Movies_DirectorId1",
                table: "Movies");

            migrationBuilder.DropColumn(
                name: "DirectorId1",
                table: "Movies");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Tickets",
                type: "text",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<bool>(
                name: "IsUsed",
                table: "Tickets",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UsedAt",
                table: "Tickets",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsUsed",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "UsedAt",
                table: "Tickets");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Tickets",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Guid>(
                name: "DirectorId1",
                table: "Movies",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Movies_DirectorId1",
                table: "Movies",
                column: "DirectorId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Movies_Director_DirectorId1",
                table: "Movies",
                column: "DirectorId1",
                principalTable: "Director",
                principalColumn: "Id");
        }
    }
}
