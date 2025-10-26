using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UbiquitousLanguageManager.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // DraftUbiquitousLang.Status CHECK制約追加
            migrationBuilder.Sql(
                @"ALTER TABLE ""DraftUbiquitousLang""
                  ADD CONSTRAINT ""CK_DraftUbiquitousLang_Status""
                  CHECK (""Status"" IN ('Draft', 'PendingApproval'))");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // DraftUbiquitousLang.Status CHECK制約削除
            migrationBuilder.Sql(
                @"ALTER TABLE ""DraftUbiquitousLang""
                  DROP CONSTRAINT IF EXISTS ""CK_DraftUbiquitousLang_Status""");
        }
    }
}
