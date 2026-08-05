using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CineFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCinemaHallEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CinemaBranch",
                table: "Schedules");

            migrationBuilder.DropColumn(
                name: "HallName",
                table: "Schedules");

            migrationBuilder.RenameColumn(
                name: "ShowTime",
                table: "Schedules",
                newName: "Showtime");

            migrationBuilder.AddColumn<Guid>(
                name: "CinemaHallId",
                table: "Schedules",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "CinemaHalls",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    HallName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TotalCapacity = table.Column<int>(type: "integer", nullable: false),
                    SeatMapMatrixJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CinemaHalls", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_CinemaHallId",
                table: "Schedules",
                column: "CinemaHallId");

            migrationBuilder.AddForeignKey(
                name: "FK_Schedules_CinemaHalls_CinemaHallId",
                table: "Schedules",
                column: "CinemaHallId",
                principalTable: "CinemaHalls",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Schedules_CinemaHalls_CinemaHallId",
                table: "Schedules");

            migrationBuilder.DropTable(
                name: "CinemaHalls");

            migrationBuilder.DropIndex(
                name: "IX_Schedules_CinemaHallId",
                table: "Schedules");

            migrationBuilder.DropColumn(
                name: "CinemaHallId",
                table: "Schedules");

            migrationBuilder.RenameColumn(
                name: "Showtime",
                table: "Schedules",
                newName: "ShowTime");

            migrationBuilder.AddColumn<string>(
                name: "CinemaBranch",
                table: "Schedules",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HallName",
                table: "Schedules",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
