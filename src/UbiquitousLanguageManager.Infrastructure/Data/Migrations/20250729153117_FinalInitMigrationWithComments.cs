using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UbiquitousLanguageManager.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class FinalInitMigrationWithComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                },
                comment: "ASP.NET Core Identity ロール管理");

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false, comment: "ユーザーID（主キー、GUID形式）"),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "ユーザー氏名（カスタムフィールド）"),
                    IsFirstLogin = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true, comment: "初回ログインフラグ（カスタムフィールド）"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP", comment: "最終更新日時（タイムゾーン付き）"),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false, comment: "論理削除フラグ（false:有効、true:削除済み）"),
                    InitialPassword = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "初期パスワード（初回ログイン時まで保持）"),
                    PasswordResetToken = table.Column<string>(type: "text", nullable: true, comment: "パスワードリセットトークン（Phase A3機能）"),
                    PasswordResetExpiry = table.Column<DateTime>(type: "timestamptz", nullable: true, comment: "リセットトークン有効期限（Phase A3機能）"),
                    UpdatedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false, comment: "最終更新者ID"),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true, comment: "ユーザー名（ログイン用）"),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true, comment: "正規化ユーザー名（検索用）"),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true, comment: "メールアドレス"),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true, comment: "正規化メールアドレス（検索用）"),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false, comment: "メール確認済みフラグ"),
                    PasswordHash = table.Column<string>(type: "text", nullable: true, comment: "パスワードハッシュ値（Identity管理）"),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true, comment: "セキュリティスタンプ（パスワード変更時更新）"),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true, comment: "同時実行制御スタンプ"),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true, comment: "電話番号"),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false, comment: "電話番号確認済みフラグ"),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false, comment: "二要素認証有効フラグ"),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "ロックアウト終了時間"),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false, comment: "ロックアウト有効フラグ"),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false, comment: "アクセス失敗回数")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                },
                comment: "ASP.NET Core Identity ユーザー情報とカスタムプロフィール");

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "ASP.NET Core Identity ユーザー・ロール関連");

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    ProjectId = table.Column<long>(type: "bigint", nullable: false, comment: "プロジェクトID（主キー）")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProjectName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "プロジェクト名（システム内一意）"),
                    Description = table.Column<string>(type: "text", nullable: true, comment: "プロジェクト説明"),
                    UpdatedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false, comment: "最終更新者ユーザーID"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false, comment: "最終更新日時（タイムゾーン付き）"),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false, comment: "論理削除フラグ（false:有効、true:削除済み）")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.ProjectId);
                    table.ForeignKey(
                        name: "FK_Projects_AspNetUsers_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "プロジェクト情報の管理とユーザー・ドメインとの関連制御");

            migrationBuilder.CreateTable(
                name: "Domains",
                columns: table => new
                {
                    DomainId = table.Column<long>(type: "bigint", nullable: false, comment: "ドメインID（主キー）")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProjectId = table.Column<long>(type: "bigint", nullable: false, comment: "所属プロジェクトID"),
                    DomainName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "ドメイン名（プロジェクト内一意）"),
                    Description = table.Column<string>(type: "text", nullable: true, comment: "ドメイン説明"),
                    UpdatedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false, comment: "最終更新者ユーザーID"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false, comment: "最終更新日時（タイムゾーン付き）"),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false, comment: "論理削除フラグ（false:有効、true:削除済み）")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Domains", x => x.DomainId);
                    table.ForeignKey(
                        name: "FK_Domains_AspNetUsers_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Domains_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "プロジェクト内ドメイン分類と承認権限の管理単位");

            migrationBuilder.CreateTable(
                name: "UserProjects",
                columns: table => new
                {
                    UserProjectId = table.Column<long>(type: "bigint", nullable: false, comment: "ユーザープロジェクトID（主キー）")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false, comment: "ユーザーID（外部キー）"),
                    ProjectId = table.Column<long>(type: "bigint", nullable: false, comment: "プロジェクトID（外部キー）"),
                    UpdatedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false, comment: "最終更新者ユーザーID"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false, comment: "最終更新日時（タイムゾーン付き）")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProjects", x => x.UserProjectId);
                    table.ForeignKey(
                        name: "FK_UserProjects_AspNetUsers_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserProjects_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserProjects_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "ユーザーとプロジェクトの多対多関連を管理、権限制御の基盤");

            migrationBuilder.CreateTable(
                name: "DomainApprovers",
                columns: table => new
                {
                    DomainApproverId = table.Column<long>(type: "bigint", nullable: false, comment: "ドメイン承認者ID（主キー）")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DomainId = table.Column<long>(type: "bigint", nullable: false, comment: "ドメインID（外部キー）"),
                    ApproverId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false, comment: "承認者ユーザーID（外部キー）"),
                    UpdatedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false, comment: "最終更新者ユーザーID"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false, comment: "最終更新日時（タイムゾーン付き）")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DomainApprovers", x => x.DomainApproverId);
                    table.ForeignKey(
                        name: "FK_DomainApprovers_AspNetUsers_ApproverId",
                        column: x => x.ApproverId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DomainApprovers_AspNetUsers_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DomainApprovers_Domains_DomainId",
                        column: x => x.DomainId,
                        principalTable: "Domains",
                        principalColumn: "DomainId",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "ドメイン別承認権限の管理、承認者とドメインの多対多関連");

            migrationBuilder.CreateTable(
                name: "FormalUbiquitousLang",
                columns: table => new
                {
                    FormalUbiquitousLangId = table.Column<long>(type: "bigint", nullable: false, comment: "正式ユビキタス言語ID（主キー）")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DomainId = table.Column<long>(type: "bigint", nullable: false, comment: "所属ドメインID（外部キー）"),
                    JapaneseName = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, comment: "和名（ドメイン内一意）"),
                    EnglishName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "英名"),
                    Description = table.Column<string>(type: "text", nullable: false, comment: "意味・説明（改行可能）"),
                    OccurrenceContext = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "発生機会"),
                    Remarks = table.Column<string>(type: "text", nullable: true, comment: "備考（改行可能）"),
                    UpdatedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false, comment: "最終更新者ユーザーID"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false, comment: "最終更新日時（タイムゾーン付き）"),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false, comment: "論理削除フラグ（false:有効、true:削除済み）")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormalUbiquitousLang", x => x.FormalUbiquitousLangId);
                    table.ForeignKey(
                        name: "FK_FormalUbiquitousLang_AspNetUsers_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FormalUbiquitousLang_Domains_DomainId",
                        column: x => x.DomainId,
                        principalTable: "Domains",
                        principalColumn: "DomainId",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "承認済み正式ユビキタス言語の管理、Claude Code出力対象データ");

            migrationBuilder.CreateTable(
                name: "DraftUbiquitousLang",
                columns: table => new
                {
                    DraftUbiquitousLangId = table.Column<long>(type: "bigint", nullable: false, comment: "ドラフトユビキタス言語ID（主キー）")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DomainId = table.Column<long>(type: "bigint", nullable: false, comment: "所属ドメインID（外部キー）"),
                    JapaneseName = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, comment: "和名"),
                    EnglishName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "英名"),
                    Description = table.Column<string>(type: "text", nullable: true, comment: "意味・説明（改行可能）"),
                    OccurrenceContext = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "発生機会"),
                    Remarks = table.Column<string>(type: "text", nullable: true, comment: "備考（改行可能）"),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Draft", comment: "ステータス（Draft/PendingApproval）"),
                    ApplicantId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true, comment: "申請者ユーザーID"),
                    ApplicationDate = table.Column<DateTime>(type: "timestamptz", nullable: true, comment: "申請日時"),
                    RejectionReason = table.Column<string>(type: "text", nullable: true, comment: "却下理由"),
                    SourceFormalUbiquitousLangId = table.Column<long>(type: "bigint", nullable: true, comment: "編集元正式ユビキタス言語ID"),
                    UpdatedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false, comment: "最終更新者ユーザーID"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false, comment: "最終更新日時（タイムゾーン付き）")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DraftUbiquitousLang", x => x.DraftUbiquitousLangId);
                    table.ForeignKey(
                        name: "FK_DraftUbiquitousLang_AspNetUsers_ApplicantId",
                        column: x => x.ApplicantId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DraftUbiquitousLang_AspNetUsers_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DraftUbiquitousLang_Domains_DomainId",
                        column: x => x.DomainId,
                        principalTable: "Domains",
                        principalColumn: "DomainId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DraftUbiquitousLang_FormalUbiquitousLang_SourceFormalUbiqui~",
                        column: x => x.SourceFormalUbiquitousLangId,
                        principalTable: "FormalUbiquitousLang",
                        principalColumn: "FormalUbiquitousLangId",
                        onDelete: ReferentialAction.SetNull);
                },
                comment: "編集中・承認申請中のドラフトユビキタス言語管理");

            migrationBuilder.CreateTable(
                name: "FormalUbiquitousLangHistory",
                columns: table => new
                {
                    HistoryId = table.Column<long>(type: "bigint", nullable: false, comment: "履歴ID（主キー）")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FormalUbiquitousLangId = table.Column<long>(type: "bigint", nullable: false, comment: "元の正式ユビキタス言語ID"),
                    DomainId = table.Column<long>(type: "bigint", nullable: false, comment: "所属ドメインID（外部キー）"),
                    JapaneseName = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, comment: "和名"),
                    EnglishName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "英名"),
                    Description = table.Column<string>(type: "text", nullable: false, comment: "意味・説明（改行可能）"),
                    OccurrenceContext = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "発生機会"),
                    Remarks = table.Column<string>(type: "text", nullable: true, comment: "備考（改行可能）"),
                    RelatedUbiquitousLangSnapshot = table.Column<string>(type: "jsonb", nullable: true, comment: "関連ユビキタス言語スナップショット（JSONB、GINインデックス対応）"),
                    UpdatedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false, comment: "最終更新者ユーザーID"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false, comment: "最終更新日時（タイムゾーン付き）"),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false, comment: "論理削除フラグ（false:有効、true:削除済み）")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormalUbiquitousLangHistory", x => x.HistoryId);
                    table.ForeignKey(
                        name: "FK_FormalUbiquitousLangHistory_AspNetUsers_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FormalUbiquitousLangHistory_Domains_DomainId",
                        column: x => x.DomainId,
                        principalTable: "Domains",
                        principalColumn: "DomainId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FormalUbiquitousLangHistory_FormalUbiquitousLang_FormalUbiq~",
                        column: x => x.FormalUbiquitousLangId,
                        principalTable: "FormalUbiquitousLang",
                        principalColumn: "FormalUbiquitousLangId",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "正式ユビキタス言語の変更履歴管理、JSONB活用でスナップショット保存");

            migrationBuilder.CreateTable(
                name: "RelatedUbiquitousLang",
                columns: table => new
                {
                    RelatedUbiquitousLangId = table.Column<long>(type: "bigint", nullable: false, comment: "関連ユビキタス言語ID（主キー）")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SourceUbiquitousLangId = table.Column<long>(type: "bigint", nullable: false, comment: "関連元ユビキタス言語ID"),
                    TargetUbiquitousLangId = table.Column<long>(type: "bigint", nullable: false, comment: "関連先ユビキタス言語ID"),
                    UpdatedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false, comment: "最終更新者ユーザーID"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false, comment: "最終更新日時（タイムゾーン付き）")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelatedUbiquitousLang", x => x.RelatedUbiquitousLangId);
                    table.ForeignKey(
                        name: "FK_RelatedUbiquitousLang_AspNetUsers_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RelatedUbiquitousLang_FormalUbiquitousLang_SourceUbiquitous~",
                        column: x => x.SourceUbiquitousLangId,
                        principalTable: "FormalUbiquitousLang",
                        principalColumn: "FormalUbiquitousLangId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RelatedUbiquitousLang_FormalUbiquitousLang_TargetUbiquitous~",
                        column: x => x.TargetUbiquitousLangId,
                        principalTable: "FormalUbiquitousLang",
                        principalColumn: "FormalUbiquitousLangId",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "ユビキタス言語間の関連性管理、多対多関連");

            migrationBuilder.CreateTable(
                name: "DraftUbiquitousLangRelations",
                columns: table => new
                {
                    DraftUbiquitousLangRelationId = table.Column<long>(type: "bigint", nullable: false, comment: "ドラフト関連ID（主キー）")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DraftUbiquitousLangId = table.Column<long>(type: "bigint", nullable: false, comment: "ドラフトユビキタス言語ID"),
                    FormalUbiquitousLangId = table.Column<long>(type: "bigint", nullable: false, comment: "関連正式ユビキタス言語ID"),
                    UpdatedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false, comment: "最終更新者ユーザーID"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false, comment: "最終更新日時（タイムゾーン付き）")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DraftUbiquitousLangRelations", x => x.DraftUbiquitousLangRelationId);
                    table.ForeignKey(
                        name: "FK_DraftUbiquitousLangRelations_AspNetUsers_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DraftUbiquitousLangRelations_DraftUbiquitousLang_DraftUbiqu~",
                        column: x => x.DraftUbiquitousLangId,
                        principalTable: "DraftUbiquitousLang",
                        principalColumn: "DraftUbiquitousLangId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DraftUbiquitousLangRelations_FormalUbiquitousLang_FormalUbi~",
                        column: x => x.FormalUbiquitousLangId,
                        principalTable: "FormalUbiquitousLang",
                        principalColumn: "FormalUbiquitousLangId",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "ドラフトユビキタス言語と正式ユビキタス言語間の関連性管理");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUsers_IsDeleted",
                table: "AspNetUsers",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUsers_PasswordResetExpiry",
                table: "AspNetUsers",
                column: "PasswordResetExpiry");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUsers_PasswordResetToken",
                table: "AspNetUsers",
                column: "PasswordResetToken");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUsers_UpdatedAt",
                table: "AspNetUsers",
                column: "UpdatedAt");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DomainApprovers_ApproverId",
                table: "DomainApprovers",
                column: "ApproverId");

            migrationBuilder.CreateIndex(
                name: "IX_DomainApprovers_DomainId_ApproverId_Unique",
                table: "DomainApprovers",
                columns: new[] { "DomainId", "ApproverId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DomainApprovers_UpdatedBy",
                table: "DomainApprovers",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Domains_DomainName",
                table: "Domains",
                column: "DomainName");

            migrationBuilder.CreateIndex(
                name: "IX_Domains_IsDeleted",
                table: "Domains",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Domains_ProjectId",
                table: "Domains",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Domains_UpdatedAt",
                table: "Domains",
                column: "UpdatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Domains_UpdatedBy",
                table: "Domains",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_DraftUbiquitousLang_ApplicantId",
                table: "DraftUbiquitousLang",
                column: "ApplicantId");

            migrationBuilder.CreateIndex(
                name: "IX_DraftUbiquitousLang_DomainId",
                table: "DraftUbiquitousLang",
                column: "DomainId");

            migrationBuilder.CreateIndex(
                name: "IX_DraftUbiquitousLang_JapaneseName",
                table: "DraftUbiquitousLang",
                column: "JapaneseName");

            migrationBuilder.CreateIndex(
                name: "IX_DraftUbiquitousLang_SourceFormalUbiquitousLangId",
                table: "DraftUbiquitousLang",
                column: "SourceFormalUbiquitousLangId");

            migrationBuilder.CreateIndex(
                name: "IX_DraftUbiquitousLang_Status",
                table: "DraftUbiquitousLang",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_DraftUbiquitousLang_UpdatedAt",
                table: "DraftUbiquitousLang",
                column: "UpdatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_DraftUbiquitousLang_UpdatedBy",
                table: "DraftUbiquitousLang",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_DraftUbiquitousLangRelation_Draft_Formal_Unique",
                table: "DraftUbiquitousLangRelations",
                columns: new[] { "DraftUbiquitousLangId", "FormalUbiquitousLangId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DraftUbiquitousLangRelations_FormalUbiquitousLangId",
                table: "DraftUbiquitousLangRelations",
                column: "FormalUbiquitousLangId");

            migrationBuilder.CreateIndex(
                name: "IX_DraftUbiquitousLangRelations_UpdatedBy",
                table: "DraftUbiquitousLangRelations",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_FormalUbiquitousLang_DomainId",
                table: "FormalUbiquitousLang",
                column: "DomainId");

            migrationBuilder.CreateIndex(
                name: "IX_FormalUbiquitousLang_EnglishName",
                table: "FormalUbiquitousLang",
                column: "EnglishName");

            migrationBuilder.CreateIndex(
                name: "IX_FormalUbiquitousLang_IsDeleted",
                table: "FormalUbiquitousLang",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_FormalUbiquitousLang_JapaneseName",
                table: "FormalUbiquitousLang",
                column: "JapaneseName");

            migrationBuilder.CreateIndex(
                name: "IX_FormalUbiquitousLang_UpdatedAt",
                table: "FormalUbiquitousLang",
                column: "UpdatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_FormalUbiquitousLang_UpdatedBy",
                table: "FormalUbiquitousLang",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_FormalUbiquitousLangHistory_DomainId",
                table: "FormalUbiquitousLangHistory",
                column: "DomainId");

            migrationBuilder.CreateIndex(
                name: "IX_FormalUbiquitousLangHistory_FormalUbiquitousLangId",
                table: "FormalUbiquitousLangHistory",
                column: "FormalUbiquitousLangId");

            migrationBuilder.CreateIndex(
                name: "IX_FormalUbiquitousLangHistory_IsDeleted",
                table: "FormalUbiquitousLangHistory",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_FormalUbiquitousLangHistory_UpdatedAt",
                table: "FormalUbiquitousLangHistory",
                column: "UpdatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_FormalUbiquitousLangHistory_UpdatedBy",
                table: "FormalUbiquitousLangHistory",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_IsDeleted",
                table: "Projects",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ProjectName",
                table: "Projects",
                column: "ProjectName");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_UpdatedAt",
                table: "Projects",
                column: "UpdatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_UpdatedBy",
                table: "Projects",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RelatedUbiquitousLang_Source_Target_Unique",
                table: "RelatedUbiquitousLang",
                columns: new[] { "SourceUbiquitousLangId", "TargetUbiquitousLangId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RelatedUbiquitousLang_TargetUbiquitousLangId",
                table: "RelatedUbiquitousLang",
                column: "TargetUbiquitousLangId");

            migrationBuilder.CreateIndex(
                name: "IX_RelatedUbiquitousLang_UpdatedBy",
                table: "RelatedUbiquitousLang",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_UserProjects_ProjectId",
                table: "UserProjects",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProjects_UpdatedBy",
                table: "UserProjects",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_UserProjects_UserId_ProjectId_Unique",
                table: "UserProjects",
                columns: new[] { "UserId", "ProjectId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "DomainApprovers");

            migrationBuilder.DropTable(
                name: "DraftUbiquitousLangRelations");

            migrationBuilder.DropTable(
                name: "FormalUbiquitousLangHistory");

            migrationBuilder.DropTable(
                name: "RelatedUbiquitousLang");

            migrationBuilder.DropTable(
                name: "UserProjects");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "DraftUbiquitousLang");

            migrationBuilder.DropTable(
                name: "FormalUbiquitousLang");

            migrationBuilder.DropTable(
                name: "Domains");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
