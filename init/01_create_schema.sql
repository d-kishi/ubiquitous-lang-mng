-- ===============================================
-- ユビキタス言語管理システム データベーススキーマ
-- 
-- 作成日: 2025-06-29
-- 最終更新: 2025-07-16
-- 対象DB: PostgreSQL専用（全環境統一）
-- 認証: ASP.NET Core Identity統合
-- 最適化: TIMESTAMPTZ、JSONB、GINインデックス等PostgreSQL固有機能活用
-- ===============================================

-- ===============================================
-- 1. ASP.NET Core Identity テーブル群
-- ===============================================

-- AspNetUsers: システム利用者の認証・権限情報管理
CREATE TABLE "AspNetUsers" (
    "Id" VARCHAR(450) PRIMARY KEY,                     -- ユーザーID（主キー）
    "UserName" VARCHAR(256),                           -- ユーザー名
    "NormalizedUserName" VARCHAR(256),                 -- 正規化ユーザー名
    "Email" VARCHAR(256),                              -- メールアドレス
    "NormalizedEmail" VARCHAR(256),                    -- 正規化メールアドレス
    "EmailConfirmed" BOOLEAN NOT NULL DEFAULT false,   -- メール確認済みフラグ
    "PasswordHash" TEXT,                               -- パスワードハッシュ値
    "SecurityStamp" TEXT,                              -- セキュリティスタンプ
    "ConcurrencyStamp" TEXT,                           -- 同時実行制御スタンプ
    "PhoneNumber" TEXT,                                -- 電話番号
    "PhoneNumberConfirmed" BOOLEAN NOT NULL DEFAULT false, -- 電話番号確認済みフラグ
    "TwoFactorEnabled" BOOLEAN NOT NULL DEFAULT false, -- 二要素認証有効フラグ
    "LockoutEnd" TIMESTAMPTZ,                          -- ロックアウト終了時間
    "LockoutEnabled" BOOLEAN NOT NULL DEFAULT false,   -- ロックアウト有効フラグ
    "AccessFailedCount" INTEGER NOT NULL DEFAULT 0,    -- アクセス失敗回数
    -- カスタムフィールド
    "Name" VARCHAR(50) NOT NULL,                       -- ユーザー氏名
    "IsFirstLogin" BOOLEAN NOT NULL DEFAULT true,      -- 初回ログインフラグ
    "InitialPassword" VARCHAR(100),                    -- 初期パスワード（平文一時保存）
    "PasswordResetToken" TEXT,                         -- パスワードリセットトークン（Phase A3必須）
    "PasswordResetExpiry" TIMESTAMPTZ,                 -- リセットトークン有効期限（Phase A3必須）
    "UpdatedBy" VARCHAR(450) NOT NULL,                 -- 最終更新者ID
    "UpdatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),    -- 最終更新日時
    "IsDeleted" BOOLEAN NOT NULL DEFAULT false         -- 論理削除フラグ
);

-- AspNetRoles: ロール管理
CREATE TABLE "AspNetRoles" (
    "Id" VARCHAR(450) PRIMARY KEY,                     -- ロールID（主キー）
    "Name" VARCHAR(256),                               -- ロール名
    "NormalizedName" VARCHAR(256),                     -- 正規化ロール名
    "ConcurrencyStamp" TEXT                            -- 同時実行制御スタンプ
);

-- AspNetUserRoles: ユーザー・ロール関連
CREATE TABLE "AspNetUserRoles" (
    "UserId" VARCHAR(450) NOT NULL,                    -- ユーザーID
    "RoleId" VARCHAR(450) NOT NULL,                    -- ロールID
    PRIMARY KEY ("UserId", "RoleId"),
    FOREIGN KEY ("UserId") REFERENCES "AspNetUsers"("Id") ON DELETE CASCADE,
    FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles"("Id") ON DELETE CASCADE
);

-- AspNetUserClaims: ユーザー別クレーム管理
CREATE TABLE "AspNetUserClaims" (
    "Id" BIGSERIAL PRIMARY KEY,                        -- クレームID（主キー）
    "UserId" VARCHAR(450) NOT NULL,                    -- ユーザーID
    "ClaimType" TEXT,                                  -- クレーム種別
    "ClaimValue" TEXT,                                 -- クレーム値
    FOREIGN KEY ("UserId") REFERENCES "AspNetUsers"("Id") ON DELETE CASCADE
);

-- AspNetRoleClaims: ロール別クレーム管理
CREATE TABLE "AspNetRoleClaims" (
    "Id" BIGSERIAL PRIMARY KEY,                        -- クレームID（主キー）
    "RoleId" VARCHAR(450) NOT NULL,                    -- ロールID
    "ClaimType" TEXT,                                  -- クレーム種別
    "ClaimValue" TEXT,                                 -- クレーム値
    FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles"("Id") ON DELETE CASCADE
);

-- AspNetUsers テーブルコメント
COMMENT ON TABLE "AspNetUsers" IS 'ASP.NET Core Identity ユーザー情報とカスタムプロフィール';
COMMENT ON COLUMN "AspNetUsers"."Id" IS 'ユーザーID（主キー、GUID形式）';
COMMENT ON COLUMN "AspNetUsers"."UserName" IS 'ユーザー名（ログイン用）';
COMMENT ON COLUMN "AspNetUsers"."NormalizedUserName" IS '正規化ユーザー名（検索用）';
COMMENT ON COLUMN "AspNetUsers"."Email" IS 'メールアドレス';
COMMENT ON COLUMN "AspNetUsers"."NormalizedEmail" IS '正規化メールアドレス（検索用）';
COMMENT ON COLUMN "AspNetUsers"."EmailConfirmed" IS 'メール確認済みフラグ';
COMMENT ON COLUMN "AspNetUsers"."PasswordHash" IS 'パスワードハッシュ値（Identity管理）';
COMMENT ON COLUMN "AspNetUsers"."SecurityStamp" IS 'セキュリティスタンプ（パスワード変更時更新）';
COMMENT ON COLUMN "AspNetUsers"."ConcurrencyStamp" IS '同時実行制御スタンプ';
COMMENT ON COLUMN "AspNetUsers"."PhoneNumber" IS '電話番号';
COMMENT ON COLUMN "AspNetUsers"."PhoneNumberConfirmed" IS '電話番号確認済みフラグ';
COMMENT ON COLUMN "AspNetUsers"."TwoFactorEnabled" IS '二要素認証有効フラグ';
COMMENT ON COLUMN "AspNetUsers"."LockoutEnd" IS 'ロックアウト終了時間';
COMMENT ON COLUMN "AspNetUsers"."LockoutEnabled" IS 'ロックアウト有効フラグ';
COMMENT ON COLUMN "AspNetUsers"."AccessFailedCount" IS 'アクセス失敗回数';
COMMENT ON COLUMN "AspNetUsers"."Name" IS 'ユーザー氏名（カスタムフィールド）';
COMMENT ON COLUMN "AspNetUsers"."IsFirstLogin" IS '初回ログインフラグ（カスタムフィールド）';
COMMENT ON COLUMN "AspNetUsers"."InitialPassword" IS '初期パスワード（初回ログイン時まで保持）';
COMMENT ON COLUMN "AspNetUsers"."PasswordResetToken" IS 'パスワードリセットトークン（Phase A3機能）';
COMMENT ON COLUMN "AspNetUsers"."PasswordResetExpiry" IS 'リセットトークン有効期限（Phase A3機能）';
COMMENT ON COLUMN "AspNetUsers"."UpdatedBy" IS '最終更新者ID';
COMMENT ON COLUMN "AspNetUsers"."UpdatedAt" IS '最終更新日時（タイムゾーン付き）';
COMMENT ON COLUMN "AspNetUsers"."IsDeleted" IS '論理削除フラグ（false:有効、true:削除済み）';

-- AspNetRoles テーブルコメント
COMMENT ON TABLE "AspNetRoles" IS 'ASP.NET Core Identity ロール管理';
COMMENT ON COLUMN "AspNetRoles"."Id" IS 'ロールID（主キー、GUID形式）';
COMMENT ON COLUMN "AspNetRoles"."Name" IS 'ロール名';
COMMENT ON COLUMN "AspNetRoles"."NormalizedName" IS '正規化ロール名（検索用）';
COMMENT ON COLUMN "AspNetRoles"."ConcurrencyStamp" IS '同時実行制御スタンプ';

-- AspNetUserRoles テーブルコメント
COMMENT ON TABLE "AspNetUserRoles" IS 'ASP.NET Core Identity ユーザー・ロール関連';
COMMENT ON COLUMN "AspNetUserRoles"."UserId" IS 'ユーザーID（外部キー）';
COMMENT ON COLUMN "AspNetUserRoles"."RoleId" IS 'ロールID（外部キー）';

-- AspNetUserClaims テーブルコメント
COMMENT ON TABLE "AspNetUserClaims" IS 'ASP.NET Core Identity ユーザー別クレーム管理';
COMMENT ON COLUMN "AspNetUserClaims"."Id" IS 'クレームID（主キー）';
COMMENT ON COLUMN "AspNetUserClaims"."UserId" IS 'ユーザーID（外部キー）';
COMMENT ON COLUMN "AspNetUserClaims"."ClaimType" IS 'クレーム種別';
COMMENT ON COLUMN "AspNetUserClaims"."ClaimValue" IS 'クレーム値';

-- AspNetRoleClaims テーブルコメント
COMMENT ON TABLE "AspNetRoleClaims" IS 'ASP.NET Core Identity ロール別クレーム管理';
COMMENT ON COLUMN "AspNetRoleClaims"."Id" IS 'クレームID（主キー）';
COMMENT ON COLUMN "AspNetRoleClaims"."RoleId" IS 'ロールID（外部キー）';
COMMENT ON COLUMN "AspNetRoleClaims"."ClaimType" IS 'クレーム種別';
COMMENT ON COLUMN "AspNetRoleClaims"."ClaimValue" IS 'クレーム値';

-- ===============================================
-- 2. プロジェクト管理テーブル
-- ===============================================

-- Projects: プロジェクト情報の管理
CREATE TABLE "Projects" (
    "ProjectId" BIGSERIAL PRIMARY KEY,                 -- プロジェクトID（主キー）
    "ProjectName" VARCHAR(50) NOT NULL UNIQUE,         -- プロジェクト名（システム内一意）
    "Description" TEXT,                                -- プロジェクト説明
    "UpdatedBy" VARCHAR(450) NOT NULL,                 -- 最終更新者ID
    "UpdatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),    -- 最終更新日時
    "IsDeleted" BOOLEAN NOT NULL DEFAULT false,        -- 論理削除フラグ
    FOREIGN KEY ("UpdatedBy") REFERENCES "AspNetUsers"("Id")
);

-- Projectsテーブルコメント
COMMENT ON TABLE "Projects" IS 'プロジェクト情報の管理とユーザー・ドメインとの関連制御';
COMMENT ON COLUMN "Projects"."ProjectId" IS 'プロジェクトID（主キー）';
COMMENT ON COLUMN "Projects"."ProjectName" IS 'プロジェクト名（システム内一意）';
COMMENT ON COLUMN "Projects"."Description" IS 'プロジェクト説明';
COMMENT ON COLUMN "Projects"."UpdatedBy" IS '最終更新者ユーザーID';
COMMENT ON COLUMN "Projects"."UpdatedAt" IS '最終更新日時（タイムゾーン付き）';
COMMENT ON COLUMN "Projects"."IsDeleted" IS '論理削除フラグ（false:有効、true:削除済み）';

-- ===============================================
-- 3. ユーザー・プロジェクト関連テーブル
-- ===============================================

-- UserProjects: ユーザーとプロジェクトの多対多関連管理
CREATE TABLE "UserProjects" (
    "UserProjectId" BIGSERIAL PRIMARY KEY,             -- ユーザープロジェクトID（主キー）
    "UserId" VARCHAR(450) NOT NULL,                    -- ユーザーID
    "ProjectId" BIGINT NOT NULL,                       -- プロジェクトID
    "UpdatedBy" VARCHAR(450) NOT NULL,                 -- 最終更新者ID
    "UpdatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),    -- 最終更新日時
    FOREIGN KEY ("UserId") REFERENCES "AspNetUsers"("Id"),
    FOREIGN KEY ("ProjectId") REFERENCES "Projects"("ProjectId"),
    FOREIGN KEY ("UpdatedBy") REFERENCES "AspNetUsers"("Id"),
    UNIQUE ("UserId", "ProjectId")                       -- ユーザー・プロジェクト組み合わせ一意
);

-- UserProjectsテーブルコメント
COMMENT ON TABLE "UserProjects" IS 'ユーザーとプロジェクトの多対多関連を管理、権限制御の基盤';
COMMENT ON COLUMN "UserProjects"."UserProjectId" IS 'ユーザープロジェクトID（主キー）';
COMMENT ON COLUMN "UserProjects"."UserId" IS 'ユーザーID（外部キー）';
COMMENT ON COLUMN "UserProjects"."ProjectId" IS 'プロジェクトID（外部キー）';
COMMENT ON COLUMN "UserProjects"."UpdatedBy" IS '最終更新者ユーザーID';
COMMENT ON COLUMN "UserProjects"."UpdatedAt" IS '最終更新日時（タイムゾーン付き）';

-- ===============================================
-- 4. ドメイン管理テーブル
-- ===============================================

-- Domains: プロジェクト内ドメイン分類の管理
CREATE TABLE "Domains" (
    "DomainId" BIGSERIAL PRIMARY KEY,                  -- ドメインID（主キー）
    "ProjectId" BIGINT NOT NULL,                       -- 所属プロジェクトID
    "DomainName" VARCHAR(30) NOT NULL,                 -- ドメイン名
    "Description" TEXT,                                -- ドメイン説明
    "UpdatedBy" VARCHAR(450) NOT NULL,                 -- 最終更新者ID
    "UpdatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),    -- 最終更新日時
    "IsDeleted" BOOLEAN NOT NULL DEFAULT false,        -- 論理削除フラグ
    FOREIGN KEY ("ProjectId") REFERENCES "Projects"("ProjectId"),
    FOREIGN KEY ("UpdatedBy") REFERENCES "AspNetUsers"("Id"),
    UNIQUE ("ProjectId", "DomainName")                   -- プロジェクト内ドメイン名一意制約
);

-- Domainsテーブルコメント
COMMENT ON TABLE "Domains" IS 'プロジェクト内ドメイン分類と承認権限の管理単位';
COMMENT ON COLUMN "Domains"."DomainId" IS 'ドメインID（主キー）';
COMMENT ON COLUMN "Domains"."ProjectId" IS '所属プロジェクトID';
COMMENT ON COLUMN "Domains"."DomainName" IS 'ドメイン名（プロジェクト内一意）';
COMMENT ON COLUMN "Domains"."Description" IS 'ドメイン説明';
COMMENT ON COLUMN "Domains"."UpdatedBy" IS '最終更新者ユーザーID';
COMMENT ON COLUMN "Domains"."UpdatedAt" IS '最終更新日時（タイムゾーン付き）';
COMMENT ON COLUMN "Domains"."IsDeleted" IS '論理削除フラグ（false:有効、true:削除済み）';

-- ===============================================
-- 5. ドメイン承認者管理テーブル
-- ===============================================

-- DomainApprovers: ドメイン承認権限の管理
CREATE TABLE "DomainApprovers" (
    "DomainApproverId" BIGSERIAL PRIMARY KEY,          -- ドメイン承認者ID（主キー）
    "DomainId" BIGINT NOT NULL,                        -- ドメインID
    "ApproverId" VARCHAR(450) NOT NULL,                -- 承認者ユーザーID
    "UpdatedBy" VARCHAR(450) NOT NULL,                 -- 最終更新者ID
    "UpdatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),    -- 最終更新日時
    FOREIGN KEY ("DomainId") REFERENCES "Domains"("DomainId"),
    FOREIGN KEY ("ApproverId") REFERENCES "AspNetUsers"("Id"),
    FOREIGN KEY ("UpdatedBy") REFERENCES "AspNetUsers"("Id"),
    UNIQUE ("DomainId", "ApproverId")                    -- ドメイン・承認者組み合わせ一意
);

-- DomainApproversテーブルコメント
COMMENT ON TABLE "DomainApprovers" IS 'ドメイン別承認権限の管理、承認者とドメインの多対多関連';
COMMENT ON COLUMN "DomainApprovers"."DomainApproverId" IS 'ドメイン承認者ID（主キー）';
COMMENT ON COLUMN "DomainApprovers"."DomainId" IS 'ドメインID（外部キー）';
COMMENT ON COLUMN "DomainApprovers"."ApproverId" IS '承認者ユーザーID（外部キー）';
COMMENT ON COLUMN "DomainApprovers"."UpdatedBy" IS '最終更新者ユーザーID';
COMMENT ON COLUMN "DomainApprovers"."UpdatedAt" IS '最終更新日時（タイムゾーン付き）';

-- ===============================================
-- 6. 正式ユビキタス言語管理テーブル
-- ===============================================

-- FormalUbiquitousLang: 承認済み正式ユビキタス言語管理
CREATE TABLE "FormalUbiquitousLang" (
    "FormalUbiquitousLangId" BIGSERIAL PRIMARY KEY,    -- 正式ユビキタス言語ID（主キー）
    "DomainId" BIGINT NOT NULL,                        -- 所属ドメインID
    "JapaneseName" VARCHAR(30) NOT NULL,               -- 和名
    "EnglishName" VARCHAR(50) NOT NULL,                -- 英名
    "Description" TEXT NOT NULL,                       -- 意味・説明（改行可能）
    "OccurrenceContext" VARCHAR(50),                   -- 発生機会
    "Remarks" TEXT,                                    -- 備考（改行可能）
    "UpdatedBy" VARCHAR(450) NOT NULL,                 -- 最終更新者ID
    "UpdatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),    -- 最終更新日時
    "IsDeleted" BOOLEAN NOT NULL DEFAULT false,        -- 論理削除フラグ
    FOREIGN KEY ("DomainId") REFERENCES "Domains"("DomainId"),
    FOREIGN KEY ("UpdatedBy") REFERENCES "AspNetUsers"("Id"),
    UNIQUE ("DomainId", "JapaneseName")                  -- ドメイン内和名一意制約
);

-- FormalUbiquitousLangテーブルコメント
COMMENT ON TABLE "FormalUbiquitousLang" IS '承認済み正式ユビキタス言語の管理、Claude Code出力対象データ';
COMMENT ON COLUMN "FormalUbiquitousLang"."FormalUbiquitousLangId" IS '正式ユビキタス言語ID（主キー）';
COMMENT ON COLUMN "FormalUbiquitousLang"."DomainId" IS '所属ドメインID（外部キー）';
COMMENT ON COLUMN "FormalUbiquitousLang"."JapaneseName" IS '和名（ドメイン内一意）';
COMMENT ON COLUMN "FormalUbiquitousLang"."EnglishName" IS '英名';
COMMENT ON COLUMN "FormalUbiquitousLang"."Description" IS '意味・説明（改行可能）';
COMMENT ON COLUMN "FormalUbiquitousLang"."OccurrenceContext" IS '発生機会';
COMMENT ON COLUMN "FormalUbiquitousLang"."Remarks" IS '備考（改行可能）';
COMMENT ON COLUMN "FormalUbiquitousLang"."UpdatedBy" IS '最終更新者ユーザーID';
COMMENT ON COLUMN "FormalUbiquitousLang"."UpdatedAt" IS '最終更新日時（タイムゾーン付き）';
COMMENT ON COLUMN "FormalUbiquitousLang"."IsDeleted" IS '論理削除フラグ（false:有効、true:削除済み）';

-- ===============================================
-- 7. ドラフトユビキタス言語管理テーブル
-- ===============================================

-- DraftUbiquitousLang: 編集中・承認申請中のユビキタス言語管理
CREATE TABLE "DraftUbiquitousLang" (
    "DraftUbiquitousLangId" BIGSERIAL PRIMARY KEY,     -- ドラフトユビキタス言語ID（主キー）
    "DomainId" BIGINT NOT NULL,                        -- 所属ドメインID
    "JapaneseName" VARCHAR(30) NOT NULL,               -- 和名
    "EnglishName" VARCHAR(50),                         -- 英名
    "Description" TEXT,                                -- 意味・説明（改行可能）
    "OccurrenceContext" VARCHAR(50),                   -- 発生機会
    "Remarks" TEXT,                                    -- 備考（改行可能）
    "Status" VARCHAR(20) NOT NULL DEFAULT 'Draft',     -- ステータス
    CHECK ("Status" IN ('Draft', 'PendingApproval')),
    "ApplicantId" VARCHAR(450),                        -- 申請者ID
    "ApplicationDate" TIMESTAMPTZ,                     -- 申請日時
    "RejectionReason" TEXT,                            -- 却下理由
    "SourceFormalUbiquitousLangId" BIGINT,             -- 編集元正式ユビキタス言語ID
    "UpdatedBy" VARCHAR(450) NOT NULL,                 -- 最終更新者ID
    "UpdatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),    -- 最終更新日時
    FOREIGN KEY ("DomainId") REFERENCES "Domains"("DomainId"),
    FOREIGN KEY ("ApplicantId") REFERENCES "AspNetUsers"("Id"),
    FOREIGN KEY ("UpdatedBy") REFERENCES "AspNetUsers"("Id"),
    FOREIGN KEY ("SourceFormalUbiquitousLangId") REFERENCES "FormalUbiquitousLang"("FormalUbiquitousLangId")
);

-- DraftUbiquitousLangテーブルコメント
COMMENT ON TABLE "DraftUbiquitousLang" IS '編集中・承認申請中のドラフトユビキタス言語管理';
COMMENT ON COLUMN "DraftUbiquitousLang"."DraftUbiquitousLangId" IS 'ドラフトユビキタス言語ID（主キー）';
COMMENT ON COLUMN "DraftUbiquitousLang"."DomainId" IS '所属ドメインID（外部キー）';
COMMENT ON COLUMN "DraftUbiquitousLang"."JapaneseName" IS '和名';
COMMENT ON COLUMN "DraftUbiquitousLang"."EnglishName" IS '英名';
COMMENT ON COLUMN "DraftUbiquitousLang"."Description" IS '意味・説明（改行可能）';
COMMENT ON COLUMN "DraftUbiquitousLang"."OccurrenceContext" IS '発生機会';
COMMENT ON COLUMN "DraftUbiquitousLang"."Remarks" IS '備考（改行可能）';
COMMENT ON COLUMN "DraftUbiquitousLang"."Status" IS 'ステータス（Draft/PendingApproval）';
COMMENT ON COLUMN "DraftUbiquitousLang"."ApplicantId" IS '申請者ユーザーID';
COMMENT ON COLUMN "DraftUbiquitousLang"."ApplicationDate" IS '申請日時';
COMMENT ON COLUMN "DraftUbiquitousLang"."RejectionReason" IS '却下理由';
COMMENT ON COLUMN "DraftUbiquitousLang"."SourceFormalUbiquitousLangId" IS '編集元正式ユビキタス言語ID';
COMMENT ON COLUMN "DraftUbiquitousLang"."UpdatedBy" IS '最終更新者ユーザーID';
COMMENT ON COLUMN "DraftUbiquitousLang"."UpdatedAt" IS '最終更新日時（タイムゾーン付き）';

-- ===============================================
-- 8. 関連ユビキタス言語管理テーブル
-- ===============================================

-- RelatedUbiquitousLang: ユビキタス言語間の関連性管理
CREATE TABLE "RelatedUbiquitousLang" (
    "RelatedUbiquitousLangId" BIGSERIAL PRIMARY KEY,   -- 関連ユビキタス言語ID（主キー）
    "SourceUbiquitousLangId" BIGINT NOT NULL,          -- 関連元ユビキタス言語ID
    "TargetUbiquitousLangId" BIGINT NOT NULL,          -- 関連先ユビキタス言語ID
    "UpdatedBy" VARCHAR(450) NOT NULL,                 -- 最終更新者ID
    "UpdatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),    -- 最終更新日時
    FOREIGN KEY ("SourceUbiquitousLangId") REFERENCES "FormalUbiquitousLang"("FormalUbiquitousLangId"),
    FOREIGN KEY ("TargetUbiquitousLangId") REFERENCES "FormalUbiquitousLang"("FormalUbiquitousLangId"),
    FOREIGN KEY ("UpdatedBy") REFERENCES "AspNetUsers"("Id"),
    UNIQUE ("SourceUbiquitousLangId", "TargetUbiquitousLangId")
);

-- RelatedUbiquitousLangテーブルコメント
COMMENT ON TABLE "RelatedUbiquitousLang" IS 'ユビキタス言語間の関連性管理、多対多関連';
COMMENT ON COLUMN "RelatedUbiquitousLang"."RelatedUbiquitousLangId" IS '関連ユビキタス言語ID（主キー）';
COMMENT ON COLUMN "RelatedUbiquitousLang"."SourceUbiquitousLangId" IS '関連元ユビキタス言語ID';
COMMENT ON COLUMN "RelatedUbiquitousLang"."TargetUbiquitousLangId" IS '関連先ユビキタス言語ID';
COMMENT ON COLUMN "RelatedUbiquitousLang"."UpdatedBy" IS '最終更新者ユーザーID';
COMMENT ON COLUMN "RelatedUbiquitousLang"."UpdatedAt" IS '最終更新日時（タイムゾーン付き）';

-- ===============================================
-- 9. ドラフトユビキタス言語関連管理テーブル
-- ===============================================

-- DraftUbiquitousLangRelations: ドラフト段階での関連性管理
CREATE TABLE "DraftUbiquitousLangRelations" (
    "DraftUbiquitousLangRelationId" BIGSERIAL PRIMARY KEY, -- ドラフト関連ID（主キー）
    "DraftUbiquitousLangId" BIGINT NOT NULL,           -- ドラフトユビキタス言語ID
    "FormalUbiquitousLangId" BIGINT NOT NULL,          -- 関連正式ユビキタス言語ID
    "UpdatedBy" VARCHAR(450) NOT NULL,                 -- 最終更新者ID
    "UpdatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),    -- 最終更新日時
    FOREIGN KEY ("DraftUbiquitousLangId") REFERENCES "DraftUbiquitousLang"("DraftUbiquitousLangId"),
    FOREIGN KEY ("FormalUbiquitousLangId") REFERENCES "FormalUbiquitousLang"("FormalUbiquitousLangId"),
    FOREIGN KEY ("UpdatedBy") REFERENCES "AspNetUsers"("Id"),
    UNIQUE ("DraftUbiquitousLangId", "FormalUbiquitousLangId")
);

-- DraftUbiquitousLangRelationsテーブルコメント
COMMENT ON TABLE "DraftUbiquitousLangRelations" IS 'ドラフトユビキタス言語と正式ユビキタス言語間の関連性管理';
COMMENT ON COLUMN "DraftUbiquitousLangRelations"."DraftUbiquitousLangRelationId" IS 'ドラフト関連ID（主キー）';
COMMENT ON COLUMN "DraftUbiquitousLangRelations"."DraftUbiquitousLangId" IS 'ドラフトユビキタス言語ID';
COMMENT ON COLUMN "DraftUbiquitousLangRelations"."FormalUbiquitousLangId" IS '関連正式ユビキタス言語ID';
COMMENT ON COLUMN "DraftUbiquitousLangRelations"."UpdatedBy" IS '最終更新者ユーザーID';
COMMENT ON COLUMN "DraftUbiquitousLangRelations"."UpdatedAt" IS '最終更新日時（タイムゾーン付き）';

-- ===============================================
-- 10. 正式ユビキタス言語履歴管理テーブル
-- ===============================================

-- FormalUbiquitousLangHistory: 正式ユビキタス言語の変更履歴管理
CREATE TABLE "FormalUbiquitousLangHistory" (
    "HistoryId" BIGSERIAL PRIMARY KEY,                 -- 履歴ID（主キー）
    "FormalUbiquitousLangId" BIGINT NOT NULL,          -- 元の正式ユビキタス言語ID
    "DomainId" BIGINT NOT NULL,                        -- 所属ドメインID
    "JapaneseName" VARCHAR(30) NOT NULL,               -- 和名
    "EnglishName" VARCHAR(50) NOT NULL,                -- 英名
    "Description" TEXT NOT NULL,                       -- 意味・説明（改行可能）
    "OccurrenceContext" VARCHAR(50),                   -- 発生機会
    "Remarks" TEXT,                                    -- 備考（改行可能）
    "RelatedUbiquitousLangSnapshot" JSONB,             -- 関連ユビキタス言語スナップショット（PostgreSQL最適化）
    "UpdatedBy" VARCHAR(450) NOT NULL,                 -- 最終更新者ID
    "UpdatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),    -- 最終更新日時
    "IsDeleted" BOOLEAN NOT NULL DEFAULT false,        -- 論理削除フラグ
    FOREIGN KEY ("DomainId") REFERENCES "Domains"("DomainId"),
    FOREIGN KEY ("UpdatedBy") REFERENCES "AspNetUsers"("Id")
);

-- FormalUbiquitousLangHistoryテーブルコメント
COMMENT ON TABLE "FormalUbiquitousLangHistory" IS '正式ユビキタス言語の変更履歴管理、JSONB活用でスナップショット保存';
COMMENT ON COLUMN "FormalUbiquitousLangHistory"."HistoryId" IS '履歴ID（主キー）';
COMMENT ON COLUMN "FormalUbiquitousLangHistory"."FormalUbiquitousLangId" IS '元の正式ユビキタス言語ID';
COMMENT ON COLUMN "FormalUbiquitousLangHistory"."DomainId" IS '所属ドメインID（外部キー）';
COMMENT ON COLUMN "FormalUbiquitousLangHistory"."JapaneseName" IS '和名';
COMMENT ON COLUMN "FormalUbiquitousLangHistory"."EnglishName" IS '英名';
COMMENT ON COLUMN "FormalUbiquitousLangHistory"."Description" IS '意味・説明（改行可能）';
COMMENT ON COLUMN "FormalUbiquitousLangHistory"."OccurrenceContext" IS '発生機会';
COMMENT ON COLUMN "FormalUbiquitousLangHistory"."Remarks" IS '備考（改行可能）';
COMMENT ON COLUMN "FormalUbiquitousLangHistory"."RelatedUbiquitousLangSnapshot" IS '関連ユビキタス言語スナップショット（JSONB、GINインデックス対応）';
COMMENT ON COLUMN "FormalUbiquitousLangHistory"."UpdatedBy" IS '最終更新者ユーザーID';
COMMENT ON COLUMN "FormalUbiquitousLangHistory"."UpdatedAt" IS '最終更新日時（タイムゾーン付き）';
COMMENT ON COLUMN "FormalUbiquitousLangHistory"."IsDeleted" IS '論理削除フラグ（false:有効、true:削除済み）';

-- ===============================================
-- PostgreSQL専用インデックス作成
-- ===============================================

-- AspNetUsers インデックス
CREATE UNIQUE INDEX IX_AspNetUsers_NormalizedUserName ON "AspNetUsers" ("NormalizedUserName");
CREATE INDEX IX_AspNetUsers_NormalizedEmail ON "AspNetUsers" ("NormalizedEmail");
CREATE INDEX IX_AspNetUsers_IsDeleted ON "AspNetUsers" ("IsDeleted");
CREATE INDEX IX_AspNetUsers_IsFirstLogin ON "AspNetUsers" ("IsFirstLogin");

-- AspNetRoles インデックス
CREATE UNIQUE INDEX IX_AspNetRoles_NormalizedName ON "AspNetRoles" ("NormalizedName");

-- AspNetUserRoles インデックス
CREATE INDEX IX_AspNetUserRoles_RoleId ON "AspNetUserRoles" ("RoleId");

-- AspNetUserClaims インデックス
CREATE INDEX IX_AspNetUserClaims_UserId ON "AspNetUserClaims" ("UserId");

-- AspNetRoleClaims インデックス
CREATE INDEX IX_AspNetRoleClaims_RoleId ON "AspNetRoleClaims" ("RoleId");

-- Projects インデックス
CREATE INDEX IX_Projects_ProjectName ON "Projects" ("ProjectName") WHERE IsDeleted = false;
CREATE INDEX IX_Projects_IsDeleted ON "Projects" ("IsDeleted");

-- UserProjects インデックス
CREATE INDEX IX_UserProjects_UserId ON "UserProjects" ("UserId");
CREATE INDEX IX_UserProjects_ProjectId ON "UserProjects" ("ProjectId");

-- Domains インデックス
CREATE INDEX IX_Domains_ProjectId ON "Domains" ("ProjectId") WHERE IsDeleted = false;
CREATE INDEX IX_Domains_IsDeleted ON "Domains" ("IsDeleted");

-- DomainApprovers インデックス
CREATE INDEX IX_DomainApprovers_DomainId ON "DomainApprovers" ("DomainId");
CREATE INDEX IX_DomainApprovers_ApproverId ON "DomainApprovers" ("ApproverId");

-- FormalUbiquitousLang インデックス
CREATE INDEX IX_FormalUbiquitousLang_DomainId ON "FormalUbiquitousLang" ("DomainId") WHERE IsDeleted = false;
CREATE INDEX IX_FormalUbiquitousLang_JapaneseName ON "FormalUbiquitousLang" ("JapaneseName") WHERE IsDeleted = false;
CREATE INDEX IX_FormalUbiquitousLang_UpdatedAt ON "FormalUbiquitousLang" (UpdatedAt DESC) WHERE IsDeleted = false;

-- DraftUbiquitousLang インデックス
CREATE INDEX IX_DraftUbiquitousLang_DomainId ON "DraftUbiquitousLang" ("DomainId");
CREATE INDEX IX_DraftUbiquitousLang_Status ON "DraftUbiquitousLang" ("Status");
CREATE INDEX IX_DraftUbiquitousLang_UpdatedAt ON "DraftUbiquitousLang" (UpdatedAt DESC);
CREATE INDEX IX_DraftUbiquitousLang_ApplicantId ON "DraftUbiquitousLang" ("ApplicantId");

-- RelatedUbiquitousLang インデックス
CREATE INDEX IX_RelatedUbiquitousLang_SourceUbiquitousLangId ON "RelatedUbiquitousLang" ("SourceUbiquitousLangId");
CREATE INDEX IX_RelatedUbiquitousLang_TargetUbiquitousLangId ON "RelatedUbiquitousLang" ("TargetUbiquitousLangId");

-- DraftUbiquitousLangRelations インデックス
CREATE INDEX IX_DraftUbiquitousLangRelations_DraftUbiquitousLangId ON "DraftUbiquitousLangRelations" ("DraftUbiquitousLangId");
CREATE INDEX IX_DraftUbiquitousLangRelations_FormalUbiquitousLangId ON "DraftUbiquitousLangRelations" ("FormalUbiquitousLangId");

-- FormalUbiquitousLangHistory インデックス
CREATE INDEX IX_FormalUbiquitousLangHistory_FormalUbiquitousLangId ON "FormalUbiquitousLangHistory" ("FormalUbiquitousLangId");
CREATE INDEX IX_FormalUbiquitousLangHistory_UpdatedAt ON "FormalUbiquitousLangHistory" (UpdatedAt DESC);
CREATE INDEX IX_FormalUbiquitousLangHistory_DomainId ON "FormalUbiquitousLangHistory" ("DomainId");

-- PostgreSQL専用GINインデックス（JSONB検索最適化）
CREATE INDEX IX_FormalUbiquitousLangHistory_RelatedSnapshot_GIN 
    ON "FormalUbiquitousLangHistory" USING GIN ("RelatedUbiquitousLangSnapshot");

-- ===============================================
-- 基本ロールデータ挿入
-- ===============================================

-- 基本ロール作成
INSERT INTO "AspNetRoles" ("Id", "Name", "NormalizedName", "ConcurrencyStamp")
VALUES 
    ('super-user', 'SuperUser', 'SUPERUSER', RANDOM()::TEXT),
    ('project-manager', 'ProjectManager', 'PROJECTMANAGER', RANDOM()::TEXT),
    ('domain-approver', 'DomainApprover', 'DOMAINAPPROVER', RANDOM()::TEXT),
    ('general-user', 'GeneralUser', 'GENERALUSER', RANDOM()::TEXT);

-- ===============================================
-- PostgreSQL専用関数・トリガー（将来拡張用）
-- ===============================================

-- UpdatedAt自動更新関数
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.UpdatedAt = NOW();
    RETURN NEW;
END;
$$ language 'plpgsql';

-- 各テーブルにUpdatedAt自動更新トリガーを追加（将来の保守性向上）
-- 現在はDEFAULT NOW()を使用しているため、必要に応じて有効化

-- ===============================================
-- 設計メモ・PostgreSQL最適化ポイント
-- ===============================================

/*
ASP.NET Core Identity統合 + PostgreSQL専用最適化の採用内容:

1. 認証システム:
   - AspNetUsers: Identity標準フィールド + カスタムフィールド
   - AspNetRoles: 4つの基本ロール（SuperUser/ProjectManager/DomainApprover/GeneralUser）
   - AspNetUserRoles: ユーザー・ロール関連
   - InitialPassword: 初回ログイン時の一時パスワード保存（US-005対応）

2. データ型最適化:
   - VARCHAR(450): Identity標準ID型（PostgreSQL対応）
   - BIGSERIAL: 自動増分主キー（IDENTITY不要）
   - TIMESTAMPTZ: タイムゾーン対応日時型
   - BOOLEAN: true/false（BIT不要）
   - TEXT: 可変長文字列（長い内容用）
   - VARCHAR(n): 固定長制限文字列（短い内容用）
   - JSONB: バイナリJSON（高速検索・インデックス対応）

3. 外部キー整合性:
   - 全UpdatedBy列: AspNetUsers.Id参照
   - 全ユーザー関連外部キー: AspNetUsers.Id参照
   - CASCADE DELETE: Identity標準動作

4. インデックス戦略:
   - Identity標準インデックス: NormalizedUserName, NormalizedEmail等
   - 部分インデックス: WHERE IsDeleted = false
   - GINインデックス: JSONB検索最適化
   - 複合インデックス: 頻繁な検索パターンに対応

5. 制約・機能:
   - CHECK制約: Status値の検証
   - 外部キー制約: 参照整合性保証
   - UNIQUE制約: 一意性保証
   - トリガー関数: 自動メンテナンス機能

6. パフォーマンス:
   - NOW()関数: デフォルト値による自動設定
   - 論理削除対応インデックス: 削除済みデータの除外
   - JSONB活用: 構造化データの効率的格納・検索

7. 運用・保守性:
   - コメント完備: 全テーブル・カラムの説明
   - 一貫した命名規則: Identity + PostgreSQL慣習に準拠
   - 将来拡張対応: トリガー関数等の基盤整備
*/