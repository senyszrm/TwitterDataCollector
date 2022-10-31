using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SampledStreamServer.Migrations
{
    public partial class InitialCreation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Hashtags",
                columns: table => new
                {
                    HashtagId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hashtags", x => x.HashtagId);
                });

            migrationBuilder.CreateTable(
                name: "SummaryDatas",
                columns: table => new
                {
                    SummaryDataId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TotalTweets = table.Column<uint>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SummaryDatas", x => x.SummaryDataId);
                });

            migrationBuilder.CreateTable(
                name: "HashtagOccurrence",
                columns: table => new
                {
                    HashtagOccurrenceId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TimeOfOccurrence = table.Column<DateTime>(type: "TEXT", nullable: false),
                    HashtagId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HashtagOccurrence", x => x.HashtagOccurrenceId);
                    table.ForeignKey(
                        name: "FK_HashtagOccurrence_Hashtags_HashtagId",
                        column: x => x.HashtagId,
                        principalTable: "Hashtags",
                        principalColumn: "HashtagId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HashtagOccurrence_HashtagId",
                table: "HashtagOccurrence",
                column: "HashtagId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HashtagOccurrence");

            migrationBuilder.DropTable(
                name: "SummaryDatas");

            migrationBuilder.DropTable(
                name: "Hashtags");
        }
    }
}
