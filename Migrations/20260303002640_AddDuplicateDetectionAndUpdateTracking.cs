using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExcelCompare.Migrations
{
    /// <inheritdoc />
    public partial class AddDuplicateDetectionAndUpdateTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsUpdate",
                table: "UploadBatches",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "OriginalBatchId",
                table: "UploadBatches",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UploadDuplicates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UploadBatchId = table.Column<int>(type: "int", nullable: false),
                    DuplicateType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Mem = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Nid = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ConflictingBatchId = table.Column<int>(type: "int", nullable: true),
                    DetectedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UploadDuplicates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UploadDuplicates_UploadBatches_ConflictingBatchId",
                        column: x => x.ConflictingBatchId,
                        principalTable: "UploadBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UploadDuplicates_UploadBatches_UploadBatchId",
                        column: x => x.UploadBatchId,
                        principalTable: "UploadBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UploadBatches_OriginalBatchId",
                table: "UploadBatches",
                column: "OriginalBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_SentRecords_Mem",
                table: "SentRecords",
                column: "Mem");

            migrationBuilder.CreateIndex(
                name: "IX_SentRecords_Nid",
                table: "SentRecords",
                column: "Nid");

            migrationBuilder.CreateIndex(
                name: "IX_ReceivedRecords_Mem",
                table: "ReceivedRecords",
                column: "Mem");

            migrationBuilder.CreateIndex(
                name: "IX_ReceivedRecords_Nid",
                table: "ReceivedRecords",
                column: "Nid");

            migrationBuilder.CreateIndex(
                name: "IX_UploadDuplicates_ConflictingBatchId",
                table: "UploadDuplicates",
                column: "ConflictingBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_UploadDuplicates_Mem_Nid",
                table: "UploadDuplicates",
                columns: new[] { "Mem", "Nid" });

            migrationBuilder.CreateIndex(
                name: "IX_UploadDuplicates_UploadBatchId",
                table: "UploadDuplicates",
                column: "UploadBatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_UploadBatches_UploadBatches_OriginalBatchId",
                table: "UploadBatches",
                column: "OriginalBatchId",
                principalTable: "UploadBatches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UploadBatches_UploadBatches_OriginalBatchId",
                table: "UploadBatches");

            migrationBuilder.DropTable(
                name: "UploadDuplicates");

            migrationBuilder.DropIndex(
                name: "IX_UploadBatches_OriginalBatchId",
                table: "UploadBatches");

            migrationBuilder.DropIndex(
                name: "IX_SentRecords_Mem",
                table: "SentRecords");

            migrationBuilder.DropIndex(
                name: "IX_SentRecords_Nid",
                table: "SentRecords");

            migrationBuilder.DropIndex(
                name: "IX_ReceivedRecords_Mem",
                table: "ReceivedRecords");

            migrationBuilder.DropIndex(
                name: "IX_ReceivedRecords_Nid",
                table: "ReceivedRecords");

            migrationBuilder.DropColumn(
                name: "IsUpdate",
                table: "UploadBatches");

            migrationBuilder.DropColumn(
                name: "OriginalBatchId",
                table: "UploadBatches");
        }
    }
}
