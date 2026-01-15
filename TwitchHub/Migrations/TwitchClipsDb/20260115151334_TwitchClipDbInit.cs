using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TwitchHub.Migrations.TwitchClipsDb
{
    /// <inheritdoc />
    public partial class TwitchClipDbInit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TwitchClips",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClipId = table.Column<string>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    ChannelId = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TwitchClips", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TwitchClips_ClipId",
                table: "TwitchClips",
                column: "ClipId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TwitchClips");
        }
    }
}
