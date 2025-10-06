using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UbiquitousLanguageManager.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class PhaseB1_AddProjectAndDomainFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Projects",
                type: "timestamptz",
                nullable: true,
                comment: "最終更新日時（タイムゾーン付き）",
                oldClrType: typeof(DateTime),
                oldType: "timestamptz",
                oldComment: "最終更新日時（タイムゾーン付き）");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Projects",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");  // 既存レコードにはマイグレーション実行時刻を設定

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Projects",
                type: "boolean",
                nullable: false,
                defaultValue: true);  // 既存レコードはアクティブとして扱う

            migrationBuilder.AddColumn<long>(
                name: "OwnerId",
                table: "Projects",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Domains",
                type: "timestamptz",
                nullable: true,
                comment: "最終更新日時（タイムゾーン付き）",
                oldClrType: typeof(DateTime),
                oldType: "timestamptz",
                oldComment: "最終更新日時（タイムゾーン付き）");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Domains",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");  // 既存レコードにはマイグレーション実行時刻を設定

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Domains",
                type: "boolean",
                nullable: false,
                defaultValue: true);  // 既存レコードはアクティブとして扱う

            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                table: "Domains",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "OwnerId",
                table: "Domains",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Domains");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Domains");

            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "Domains");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Domains");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Projects",
                type: "timestamptz",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                comment: "最終更新日時（タイムゾーン付き）",
                oldClrType: typeof(DateTime),
                oldType: "timestamptz",
                oldNullable: true,
                oldComment: "最終更新日時（タイムゾーン付き）");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Domains",
                type: "timestamptz",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                comment: "最終更新日時（タイムゾーン付き）",
                oldClrType: typeof(DateTime),
                oldType: "timestamptz",
                oldNullable: true,
                oldComment: "最終更新日時（タイムゾーン付き）");
        }
    }
}
