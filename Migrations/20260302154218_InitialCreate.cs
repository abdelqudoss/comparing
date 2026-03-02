using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExcelCompare.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UploadBatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalRows = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UploadBatches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReceivedRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UploadBatchId = table.Column<int>(type: "int", nullable: false),
                    Mem = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Sn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Nid = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RegDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BatchNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MemberRank = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RefSn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceivedRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReceivedRecords_UploadBatches_UploadBatchId",
                        column: x => x.UploadBatchId,
                        principalTable: "UploadBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SentRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UploadBatchId = table.Column<int>(type: "int", nullable: false),
                    Mem = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Sn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Nid = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RegDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BatchNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MemberRank = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RefSn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SentRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SentRecords_UploadBatches_UploadBatchId",
                        column: x => x.UploadBatchId,
                        principalTable: "UploadBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReceivedRecords_Mem_Nid",
                table: "ReceivedRecords",
                columns: new[] { "Mem", "Nid" });

            migrationBuilder.CreateIndex(
                name: "IX_ReceivedRecords_UploadBatchId",
                table: "ReceivedRecords",
                column: "UploadBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_SentRecords_Mem_Nid",
                table: "SentRecords",
                columns: new[] { "Mem", "Nid" });

            migrationBuilder.CreateIndex(
                name: "IX_SentRecords_UploadBatchId",
                table: "SentRecords",
                column: "UploadBatchId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReceivedRecords");

            migrationBuilder.DropTable(
                name: "SentRecords");

            migrationBuilder.DropTable(
                name: "UploadBatches");
        }
    }
}
