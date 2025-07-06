-- ===============================================
-- ユビキタス言語管理システム データベーススキーマ
-- 
-- 作成日: 2025-06-29
-- 最終更新: 2025-07-02
-- 対象DB: PostgreSQL専用（全環境統一）
-- 最適化: TIMESTAMPTZ、JSONB、GINインデックス等PostgreSQL固有機能活用
-- ===============================================

-- ===============================================
-- 1. ユーザー管理テーブル
-- ===============================================

-- Users: システム利用者の認証・権限情報管理
CREATE TABLE Users (
    UserId BIGSERIAL PRIMARY KEY,                   -- ユーザーID（主キー）
    Email VARCHAR(254) NOT NULL UNIQUE,             -- メールアドレス（ログインID）
    PasswordHash TEXT NOT NULL,                     -- パスワードハッシュ値
    Name VARCHAR(50) NOT NULL,                      -- ユーザー氏名
    UserRole VARCHAR(20) NOT NULL                   -- ユーザーロール
        CHECK (UserRole IN ('SuperUser', 'ProjectManager', 'DomainApprover', 'GeneralUser')),
    IsActive BOOLEAN NOT NULL DEFAULT true,         -- アクティブフラグ
    IsFirstLogin BOOLEAN NOT NULL DEFAULT true,     -- 初回ログインフラグ
    PasswordResetToken TEXT,                        -- パスワードリセットトークン
    PasswordResetExpiry TIMESTAMPTZ,                -- リセットトークン有効期限
    UpdatedBy BIGINT NOT NULL,                      -- 最終更新者ID
    UpdatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),   -- 最終更新日時
    IsDeleted BOOLEAN NOT NULL DEFAULT false        -- 論理削除フラグ
);

-- Usersテーブルコメント
COMMENT ON TABLE Users IS 'システム利用者の認証・権限情報と基本プロフィールを管理';
COMMENT ON COLUMN Users.UserId IS 'ユーザーID（主キー）';
COMMENT ON COLUMN Users.Email IS 'メールアドレス（ログインID、システム内一意）';
COMMENT ON COLUMN Users.PasswordHash IS 'パスワードハッシュ値（bcrypt）';
COMMENT ON COLUMN Users.Name IS 'ユーザー氏名';
COMMENT ON COLUMN Users.UserRole IS 'ユーザーロール（SuperUser/ProjectManager/DomainApprover/GeneralUser）';
COMMENT ON COLUMN Users.IsActive IS 'アクティブフラグ（true:有効、false:無効）';
COMMENT ON COLUMN Users.IsFirstLogin IS '初回ログインフラグ（true:初回、false:変更済み）';
COMMENT ON COLUMN Users.PasswordResetToken IS 'パスワードリセット用トークン';
COMMENT ON COLUMN Users.PasswordResetExpiry IS 'リセットトークン有効期限（24時間）';
COMMENT ON COLUMN Users.UpdatedBy IS '最終更新者ユーザーID';
COMMENT ON COLUMN Users.UpdatedAt IS '最終更新日時（タイムゾーン付き）';
COMMENT ON COLUMN Users.IsDeleted IS '論理削除フラグ（false:有効、true:削除済み）';

-- ===============================================
-- 2. プロジェクト管理テーブル
-- ===============================================

-- Projects: プロジェクト情報の管理
CREATE TABLE Projects (
    ProjectId BIGSERIAL PRIMARY KEY,               -- プロジェクトID（主キー）
    ProjectName VARCHAR(100) NOT NULL,             -- プロジェクト名
    ProjectDescription TEXT,                       -- プロジェクト説明
    ProjectManager BIGINT NOT NULL,                -- プロジェクト管理者ID
    StartDate DATE,                                -- プロジェクト開始日
    EndDate DATE,                                  -- プロジェクト終了日
    ProjectStatus VARCHAR(20) NOT NULL DEFAULT 'Active' -- プロジェクト状態
        CHECK (ProjectStatus IN ('Active', 'Inactive', 'Archived')),
    UpdatedBy BIGINT NOT NULL,                     -- 最終更新者ID
    UpdatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),  -- 最終更新日時
    IsDeleted BOOLEAN NOT NULL DEFAULT false       -- 論理削除フラグ
);

-- Projectsテーブルコメント
COMMENT ON TABLE Projects IS 'プロジェクト情報と管理者権限を管理';
COMMENT ON COLUMN Projects.ProjectId IS 'プロジェクトID（主キー）';
COMMENT ON COLUMN Projects.ProjectName IS 'プロジェクト名（100文字以内）';
COMMENT ON COLUMN Projects.ProjectDescription IS 'プロジェクト説明（任意）';
COMMENT ON COLUMN Projects.ProjectManager IS 'プロジェクト管理者ユーザーID';
COMMENT ON COLUMN Projects.StartDate IS 'プロジェクト開始日（任意）';
COMMENT ON COLUMN Projects.EndDate IS 'プロジェクト終了日（任意）';
COMMENT ON COLUMN Projects.ProjectStatus IS 'プロジェクト状態（Active/Inactive/Archived）';
COMMENT ON COLUMN Projects.UpdatedBy IS '最終更新者ユーザーID';
COMMENT ON COLUMN Projects.UpdatedAt IS '最終更新日時（タイムゾーン付き）';
COMMENT ON COLUMN Projects.IsDeleted IS '論理削除フラグ（false:有効、true:削除済み）';

-- ===============================================
-- 3. ドメイン管理テーブル
-- ===============================================

-- Domains: ドメイン領域情報の管理
CREATE TABLE Domains (
    DomainId BIGSERIAL PRIMARY KEY,                -- ドメインID（主キー）
    ProjectId BIGINT NOT NULL,                     -- 所属プロジェクトID
    DomainName VARCHAR(100) NOT NULL,              -- ドメイン名
    DomainDescription TEXT,                        -- ドメイン説明
    DomainApprover BIGINT NOT NULL,                -- ドメイン承認者ID
    DomainStatus VARCHAR(20) NOT NULL DEFAULT 'Active' -- ドメイン状態
        CHECK (DomainStatus IN ('Active', 'Inactive', 'Archived')),
    UpdatedBy BIGINT NOT NULL,                     -- 最終更新者ID
    UpdatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),  -- 最終更新日時
    IsDeleted BOOLEAN NOT NULL DEFAULT false       -- 論理削除フラグ
);

-- Domainsテーブルコメント
COMMENT ON TABLE Domains IS 'ドメイン領域情報と承認者権限を管理';
COMMENT ON COLUMN Domains.DomainId IS 'ドメインID（主キー）';
COMMENT ON COLUMN Domains.ProjectId IS '所属プロジェクトID';
COMMENT ON COLUMN Domains.DomainName IS 'ドメイン名（100文字以内）';
COMMENT ON COLUMN Domains.DomainDescription IS 'ドメイン説明（任意）';
COMMENT ON COLUMN Domains.DomainApprover IS 'ドメイン承認者ユーザーID';
COMMENT ON COLUMN Domains.DomainStatus IS 'ドメイン状態（Active/Inactive/Archived）';
COMMENT ON COLUMN Domains.UpdatedBy IS '最終更新者ユーザーID';
COMMENT ON COLUMN Domains.UpdatedAt IS '最終更新日時（タイムゾーン付き）';
COMMENT ON COLUMN Domains.IsDeleted IS '論理削除フラグ（false:有効、true:削除済み）';

-- ===============================================
-- 4. ドラフトユビキタス言語管理テーブル
-- ===============================================

-- DraftUbiquitousLang: ドラフト段階のユビキタス言語管理
CREATE TABLE DraftUbiquitousLang (
    DraftUbiquitousLangId BIGSERIAL PRIMARY KEY,   -- ドラフトユビキタス言語ID（主キー）
    DomainId BIGINT NOT NULL,                      -- 所属ドメインID
    Term VARCHAR(100) NOT NULL,                    -- 用語名
    Definition TEXT NOT NULL,                      -- 定義文
    Context TEXT,                                  -- 利用文脈
    ExampleUsage TEXT,                             -- 利用例
    RelatedConcepts TEXT,                          -- 関連概念
    Notes TEXT,                                    -- 備考
    Status VARCHAR(20) NOT NULL DEFAULT 'Draft'    -- ステータス
        CHECK (Status IN ('Draft', 'Review', 'Approved', 'Rejected')),
    CreatedBy BIGINT NOT NULL,                     -- 作成者ID
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),  -- 作成日時
    UpdatedBy BIGINT NOT NULL,                     -- 最終更新者ID
    UpdatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),  -- 最終更新日時
    IsDeleted BOOLEAN NOT NULL DEFAULT false       -- 論理削除フラグ
);

-- DraftUbiquitousLangテーブルコメント
COMMENT ON TABLE DraftUbiquitousLang IS 'ドラフト段階のユビキタス言語と承認プロセスを管理';
COMMENT ON COLUMN DraftUbiquitousLang.DraftUbiquitousLangId IS 'ドラフトユビキタス言語ID（主キー）';
COMMENT ON COLUMN DraftUbiquitousLang.DomainId IS '所属ドメインID';
COMMENT ON COLUMN DraftUbiquitousLang.Term IS '用語名（100文字以内）';
COMMENT ON COLUMN DraftUbiquitousLang.Definition IS '定義文（必須）';
COMMENT ON COLUMN DraftUbiquitousLang.Context IS '利用文脈（任意）';
COMMENT ON COLUMN DraftUbiquitousLang.ExampleUsage IS '利用例（任意）';
COMMENT ON COLUMN DraftUbiquitousLang.RelatedConcepts IS '関連概念（任意）';
COMMENT ON COLUMN DraftUbiquitousLang.Notes IS '備考（任意）';
COMMENT ON COLUMN DraftUbiquitousLang.Status IS 'ステータス（Draft/Review/Approved/Rejected）';
COMMENT ON COLUMN DraftUbiquitousLang.CreatedBy IS '作成者ユーザーID';
COMMENT ON COLUMN DraftUbiquitousLang.CreatedAt IS '作成日時（タイムゾーン付き）';
COMMENT ON COLUMN DraftUbiquitousLang.UpdatedBy IS '最終更新者ユーザーID';
COMMENT ON COLUMN DraftUbiquitousLang.UpdatedAt IS '最終更新日時（タイムゾーン付き）';
COMMENT ON COLUMN DraftUbiquitousLang.IsDeleted IS '論理削除フラグ（false:有効、true:削除済み）';

-- ===============================================
-- 5. 正式ユビキタス言語管理テーブル
-- ===============================================

-- FormalUbiquitousLang: 承認済み正式ユビキタス言語管理
CREATE TABLE FormalUbiquitousLang (
    FormalUbiquitousLangId BIGSERIAL PRIMARY KEY,  -- 正式ユビキタス言語ID（主キー）
    DomainId BIGINT NOT NULL,                      -- 所属ドメインID
    Term VARCHAR(100) NOT NULL,                    -- 用語名
    Definition TEXT NOT NULL,                      -- 定義文
    Context TEXT,                                  -- 利用文脈
    ExampleUsage TEXT,                             -- 利用例
    RelatedConcepts TEXT,                          -- 関連概念
    Notes TEXT,                                    -- 備考
    ApprovedBy BIGINT NOT NULL,                    -- 承認者ID
    ApprovedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(), -- 承認日時
    SourceDraftId BIGINT,                          -- 元となったドラフトID
    UpdatedBy BIGINT NOT NULL,                     -- 最終更新者ID
    UpdatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),  -- 最終更新日時
    IsDeleted BOOLEAN NOT NULL DEFAULT false       -- 論理削除フラグ
);

-- FormalUbiquitousLangテーブルコメント
COMMENT ON TABLE FormalUbiquitousLang IS '承認済み正式ユビキタス言語と承認情報を管理';
COMMENT ON COLUMN FormalUbiquitousLang.FormalUbiquitousLangId IS '正式ユビキタス言語ID（主キー）';
COMMENT ON COLUMN FormalUbiquitousLang.DomainId IS '所属ドメインID';
COMMENT ON COLUMN FormalUbiquitousLang.Term IS '用語名（100文字以内）';
COMMENT ON COLUMN FormalUbiquitousLang.Definition IS '定義文（必須）';
COMMENT ON COLUMN FormalUbiquitousLang.Context IS '利用文脈（任意）';
COMMENT ON COLUMN FormalUbiquitousLang.ExampleUsage IS '利用例（任意）';
COMMENT ON COLUMN FormalUbiquitousLang.RelatedConcepts IS '関連概念（任意）';
COMMENT ON COLUMN FormalUbiquitousLang.Notes IS '備考（任意）';
COMMENT ON COLUMN FormalUbiquitousLang.ApprovedBy IS '承認者ユーザーID';
COMMENT ON COLUMN FormalUbiquitousLang.ApprovedAt IS '承認日時（タイムゾーン付き）';
COMMENT ON COLUMN FormalUbiquitousLang.SourceDraftId IS '元となったドラフトユビキタス言語ID（任意）';
COMMENT ON COLUMN FormalUbiquitousLang.UpdatedBy IS '最終更新者ユーザーID';
COMMENT ON COLUMN FormalUbiquitousLang.UpdatedAt IS '最終更新日時（タイムゾーン付き）';
COMMENT ON COLUMN FormalUbiquitousLang.IsDeleted IS '論理削除フラグ（false:有効、true:削除済み）';

-- ===============================================
-- 6. ユーザー・プロジェクト関連テーブル
-- ===============================================

-- UserProjects: ユーザーとプロジェクトの多対多関係管理
CREATE TABLE UserProjects (
    UserProjectId BIGSERIAL PRIMARY KEY,          -- ユーザープロジェクトID（主キー）
    UserId BIGINT NOT NULL,                       -- ユーザーID
    ProjectId BIGINT NOT NULL,                    -- プロジェクトID
    Role VARCHAR(20) NOT NULL                     -- プロジェクト内ロール
        CHECK (Role IN ('ProjectManager', 'DomainApprover', 'GeneralUser')),
    AssignedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(), -- 割り当て日時
    UpdatedBy BIGINT NOT NULL,                     -- 最終更新者ID
    UpdatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),  -- 最終更新日時
    IsDeleted BOOLEAN NOT NULL DEFAULT false       -- 論理削除フラグ
);

-- UserProjectsテーブルコメント
COMMENT ON TABLE UserProjects IS 'ユーザーとプロジェクトの多対多関係とプロジェクト内ロールを管理';
COMMENT ON COLUMN UserProjects.UserProjectId IS 'ユーザープロジェクトID（主キー）';
COMMENT ON COLUMN UserProjects.UserId IS 'ユーザーID';
COMMENT ON COLUMN UserProjects.ProjectId IS 'プロジェクトID';
COMMENT ON COLUMN UserProjects.Role IS 'プロジェクト内ロール（ProjectManager/DomainApprover/GeneralUser）';
COMMENT ON COLUMN UserProjects.AssignedAt IS '割り当て日時（タイムゾーン付き）';
COMMENT ON COLUMN UserProjects.UpdatedBy IS '最終更新者ユーザーID';
COMMENT ON COLUMN UserProjects.UpdatedAt IS '最終更新日時（タイムゾーン付き）';
COMMENT ON COLUMN UserProjects.IsDeleted IS '論理削除フラグ（false:有効、true:削除済み）';

-- ===============================================
-- 7. ドメイン承認者管理テーブル
-- ===============================================

-- DomainApprovers: ドメインごとの承認者管理
CREATE TABLE DomainApprovers (
    DomainApproverId BIGSERIAL PRIMARY KEY,       -- ドメイン承認者ID（主キー）
    DomainId BIGINT NOT NULL,                     -- ドメインID
    UserId BIGINT NOT NULL,                       -- 承認者ユーザーID
    AssignedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(), -- 割り当て日時
    UpdatedBy BIGINT NOT NULL,                     -- 最終更新者ID
    UpdatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),  -- 最終更新日時
    IsDeleted BOOLEAN NOT NULL DEFAULT false       -- 論理削除フラグ
);

-- DomainApproversテーブルコメント
COMMENT ON TABLE DomainApprovers IS 'ドメインごとの承認者割り当てを管理';
COMMENT ON COLUMN DomainApprovers.DomainApproverId IS 'ドメイン承認者ID（主キー）';
COMMENT ON COLUMN DomainApprovers.DomainId IS 'ドメインID';
COMMENT ON COLUMN DomainApprovers.UserId IS '承認者ユーザーID';
COMMENT ON COLUMN DomainApprovers.AssignedAt IS '割り当て日時（タイムゾーン付き）';
COMMENT ON COLUMN DomainApprovers.UpdatedBy IS '最終更新者ユーザーID';
COMMENT ON COLUMN DomainApprovers.UpdatedAt IS '最終更新日時（タイムゾーン付き）';
COMMENT ON COLUMN DomainApprovers.IsDeleted IS '論理削除フラグ（false:有効、true:削除済み）';

-- ===============================================
-- 8. 関連ユビキタス言語管理テーブル
-- ===============================================

-- RelatedUbiquitousLang: 正式ユビキタス言語間の関連性管理
CREATE TABLE RelatedUbiquitousLang (
    RelatedUbiquitousLangId BIGSERIAL PRIMARY KEY, -- 関連ユビキタス言語ID（主キー）
    SourceFormalUbiquitousLangId BIGINT NOT NULL,  -- 関連元正式ユビキタス言語ID
    TargetFormalUbiquitousLangId BIGINT NOT NULL,  -- 関連先正式ユビキタス言語ID
    RelationType VARCHAR(50) NOT NULL,             -- 関連タイプ
    RelationDescription TEXT,                      -- 関連説明
    UpdatedBy BIGINT NOT NULL,                     -- 最終更新者ID
    UpdatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),  -- 最終更新日時
    IsDeleted BOOLEAN NOT NULL DEFAULT false       -- 論理削除フラグ
);

-- RelatedUbiquitousLangテーブルコメント
COMMENT ON TABLE RelatedUbiquitousLang IS '正式ユビキタス言語間の関連性と関連タイプを管理';
COMMENT ON COLUMN RelatedUbiquitousLang.RelatedUbiquitousLangId IS '関連ユビキタス言語ID（主キー）';
COMMENT ON COLUMN RelatedUbiquitousLang.SourceFormalUbiquitousLangId IS '関連元正式ユビキタス言語ID';
COMMENT ON COLUMN RelatedUbiquitousLang.TargetFormalUbiquitousLangId IS '関連先正式ユビキタス言語ID';
COMMENT ON COLUMN RelatedUbiquitousLang.RelationType IS '関連タイプ（例：類義語、対義語、上位概念、下位概念）';
COMMENT ON COLUMN RelatedUbiquitousLang.RelationDescription IS '関連説明（任意）';
COMMENT ON COLUMN RelatedUbiquitousLang.UpdatedBy IS '最終更新者ユーザーID';
COMMENT ON COLUMN RelatedUbiquitousLang.UpdatedAt IS '最終更新日時（タイムゾーン付き）';
COMMENT ON COLUMN RelatedUbiquitousLang.IsDeleted IS '論理削除フラグ（false:有効、true:削除済み）';

-- ===============================================
-- 9. ドラフトユビキタス言語関連管理テーブル
-- ===============================================

-- DraftUbiquitousLangRelations: ドラフトユビキタス言語間の関連性管理
CREATE TABLE DraftUbiquitousLangRelations (
    DraftUbiquitousLangRelationId BIGSERIAL PRIMARY KEY, -- ドラフトユビキタス言語関連ID（主キー）
    SourceDraftUbiquitousLangId BIGINT NOT NULL,         -- 関連元ドラフトユビキタス言語ID
    TargetDraftUbiquitousLangId BIGINT NOT NULL,         -- 関連先ドラフトユビキタス言語ID
    RelationType VARCHAR(50) NOT NULL,                   -- 関連タイプ
    RelationDescription TEXT,                            -- 関連説明
    UpdatedBy BIGINT NOT NULL,                           -- 最終更新者ID
    UpdatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),        -- 最終更新日時
    IsDeleted BOOLEAN NOT NULL DEFAULT false             -- 論理削除フラグ
);

-- DraftUbiquitousLangRelationsテーブルコメント
COMMENT ON TABLE DraftUbiquitousLangRelations IS 'ドラフトユビキタス言語間の関連性と関連タイプを管理';
COMMENT ON COLUMN DraftUbiquitousLangRelations.DraftUbiquitousLangRelationId IS 'ドラフトユビキタス言語関連ID（主キー）';
COMMENT ON COLUMN DraftUbiquitousLangRelations.SourceDraftUbiquitousLangId IS '関連元ドラフトユビキタス言語ID';
COMMENT ON COLUMN DraftUbiquitousLangRelations.TargetDraftUbiquitousLangId IS '関連先ドラフトユビキタス言語ID';
COMMENT ON COLUMN DraftUbiquitousLangRelations.RelationType IS '関連タイプ（例：類義語、対義語、上位概念、下位概念）';
COMMENT ON COLUMN DraftUbiquitousLangRelations.RelationDescription IS '関連説明（任意）';
COMMENT ON COLUMN DraftUbiquitousLangRelations.UpdatedBy IS '最終更新者ユーザーID';
COMMENT ON COLUMN DraftUbiquitousLangRelations.UpdatedAt IS '最終更新日時（タイムゾーン付き）';
COMMENT ON COLUMN DraftUbiquitousLangRelations.IsDeleted IS '論理削除フラグ（false:有効、true:削除済み）';

-- ===============================================
-- 10. 正式ユビキタス言語履歴テーブル
-- ===============================================

-- FormalUbiquitousLangHistory: 正式ユビキタス言語の変更履歴管理
CREATE TABLE FormalUbiquitousLangHistory (
    FormalUbiquitousLangHistoryId BIGSERIAL PRIMARY KEY,  -- 正式ユビキタス言語履歴ID（主キー）
    FormalUbiquitousLangId BIGINT NOT NULL,               -- 正式ユビキタス言語ID
    ChangeType VARCHAR(20) NOT NULL                       -- 変更タイプ
        CHECK (ChangeType IN ('Created', 'Updated', 'Deleted')),
    ChangeDescription TEXT,                               -- 変更説明
    OldValue JSONB,                                       -- 変更前値（JSON）
    NewValue JSONB,                                       -- 変更後値（JSON）
    ChangedBy BIGINT NOT NULL,                            -- 変更者ID
    ChangedAt TIMESTAMPTZ NOT NULL DEFAULT NOW()          -- 変更日時
);

-- FormalUbiquitousLangHistoryテーブルコメント
COMMENT ON TABLE FormalUbiquitousLangHistory IS '正式ユビキタス言語の変更履歴と監査証跡を管理';
COMMENT ON COLUMN FormalUbiquitousLangHistory.FormalUbiquitousLangHistoryId IS '正式ユビキタス言語履歴ID（主キー）';
COMMENT ON COLUMN FormalUbiquitousLangHistory.FormalUbiquitousLangId IS '正式ユビキタス言語ID';
COMMENT ON COLUMN FormalUbiquitousLangHistory.ChangeType IS '変更タイプ（Created/Updated/Deleted）';
COMMENT ON COLUMN FormalUbiquitousLangHistory.ChangeDescription IS '変更説明（任意）';
COMMENT ON COLUMN FormalUbiquitousLangHistory.OldValue IS '変更前値（JSONB形式）';
COMMENT ON COLUMN FormalUbiquitousLangHistory.NewValue IS '変更後値（JSONB形式）';
COMMENT ON COLUMN FormalUbiquitousLangHistory.ChangedBy IS '変更者ユーザーID';
COMMENT ON COLUMN FormalUbiquitousLangHistory.ChangedAt IS '変更日時（タイムゾーン付き）';

-- ===============================================
-- 11. 関連ユビキタス言語履歴テーブル
-- ===============================================

-- RelatedUbiquitousLangHistory: 関連ユビキタス言語の変更履歴管理
CREATE TABLE RelatedUbiquitousLangHistory (
    RelatedUbiquitousLangHistoryId BIGSERIAL PRIMARY KEY, -- 関連ユビキタス言語履歴ID（主キー）
    RelatedUbiquitousLangId BIGINT NOT NULL,              -- 関連ユビキタス言語ID
    ChangeType VARCHAR(20) NOT NULL                       -- 変更タイプ
        CHECK (ChangeType IN ('Created', 'Updated', 'Deleted')),
    ChangeDescription TEXT,                               -- 変更説明
    OldValue JSONB,                                       -- 変更前値（JSON）
    NewValue JSONB,                                       -- 変更後値（JSON）
    ChangedBy BIGINT NOT NULL,                            -- 変更者ID
    ChangedAt TIMESTAMPTZ NOT NULL DEFAULT NOW()          -- 変更日時
);

-- RelatedUbiquitousLangHistoryテーブルコメント
COMMENT ON TABLE RelatedUbiquitousLangHistory IS '関連ユビキタス言語の変更履歴と監査証跡を管理';
COMMENT ON COLUMN RelatedUbiquitousLangHistory.RelatedUbiquitousLangHistoryId IS '関連ユビキタス言語履歴ID（主キー）';
COMMENT ON COLUMN RelatedUbiquitousLangHistory.RelatedUbiquitousLangId IS '関連ユビキタス言語ID';
COMMENT ON COLUMN RelatedUbiquitousLangHistory.ChangeType IS '変更タイプ（Created/Updated/Deleted）';
COMMENT ON COLUMN RelatedUbiquitousLangHistory.ChangeDescription IS '変更説明（任意）';
COMMENT ON COLUMN RelatedUbiquitousLangHistory.OldValue IS '変更前値（JSONB形式）';
COMMENT ON COLUMN RelatedUbiquitousLangHistory.NewValue IS '変更後値（JSONB形式）';
COMMENT ON COLUMN RelatedUbiquitousLangHistory.ChangedBy IS '変更者ユーザーID';
COMMENT ON COLUMN RelatedUbiquitousLangHistory.ChangedAt IS '変更日時（タイムゾーン付き）';

-- ===============================================
-- 外部キー制約
-- ===============================================

-- Projects テーブル
ALTER TABLE Projects ADD CONSTRAINT FK_Projects_ProjectManager 
    FOREIGN KEY (ProjectManager) REFERENCES Users(UserId);
ALTER TABLE Projects ADD CONSTRAINT FK_Projects_UpdatedBy 
    FOREIGN KEY (UpdatedBy) REFERENCES Users(UserId);

-- Domains テーブル
ALTER TABLE Domains ADD CONSTRAINT FK_Domains_ProjectId 
    FOREIGN KEY (ProjectId) REFERENCES Projects(ProjectId);
ALTER TABLE Domains ADD CONSTRAINT FK_Domains_DomainApprover 
    FOREIGN KEY (DomainApprover) REFERENCES Users(UserId);
ALTER TABLE Domains ADD CONSTRAINT FK_Domains_UpdatedBy 
    FOREIGN KEY (UpdatedBy) REFERENCES Users(UserId);

-- DraftUbiquitousLang テーブル
ALTER TABLE DraftUbiquitousLang ADD CONSTRAINT FK_DraftUbiquitousLang_DomainId 
    FOREIGN KEY (DomainId) REFERENCES Domains(DomainId);
ALTER TABLE DraftUbiquitousLang ADD CONSTRAINT FK_DraftUbiquitousLang_CreatedBy 
    FOREIGN KEY (CreatedBy) REFERENCES Users(UserId);
ALTER TABLE DraftUbiquitousLang ADD CONSTRAINT FK_DraftUbiquitousLang_UpdatedBy 
    FOREIGN KEY (UpdatedBy) REFERENCES Users(UserId);

-- FormalUbiquitousLang テーブル
ALTER TABLE FormalUbiquitousLang ADD CONSTRAINT FK_FormalUbiquitousLang_DomainId 
    FOREIGN KEY (DomainId) REFERENCES Domains(DomainId);
ALTER TABLE FormalUbiquitousLang ADD CONSTRAINT FK_FormalUbiquitousLang_ApprovedBy 
    FOREIGN KEY (ApprovedBy) REFERENCES Users(UserId);
ALTER TABLE FormalUbiquitousLang ADD CONSTRAINT FK_FormalUbiquitousLang_SourceDraftId 
    FOREIGN KEY (SourceDraftId) REFERENCES DraftUbiquitousLang(DraftUbiquitousLangId);
ALTER TABLE FormalUbiquitousLang ADD CONSTRAINT FK_FormalUbiquitousLang_UpdatedBy 
    FOREIGN KEY (UpdatedBy) REFERENCES Users(UserId);

-- UserProjects テーブル
ALTER TABLE UserProjects ADD CONSTRAINT FK_UserProjects_UserId 
    FOREIGN KEY (UserId) REFERENCES Users(UserId);
ALTER TABLE UserProjects ADD CONSTRAINT FK_UserProjects_ProjectId 
    FOREIGN KEY (ProjectId) REFERENCES Projects(ProjectId);
ALTER TABLE UserProjects ADD CONSTRAINT FK_UserProjects_UpdatedBy 
    FOREIGN KEY (UpdatedBy) REFERENCES Users(UserId);

-- DomainApprovers テーブル
ALTER TABLE DomainApprovers ADD CONSTRAINT FK_DomainApprovers_DomainId 
    FOREIGN KEY (DomainId) REFERENCES Domains(DomainId);
ALTER TABLE DomainApprovers ADD CONSTRAINT FK_DomainApprovers_UserId 
    FOREIGN KEY (UserId) REFERENCES Users(UserId);
ALTER TABLE DomainApprovers ADD CONSTRAINT FK_DomainApprovers_UpdatedBy 
    FOREIGN KEY (UpdatedBy) REFERENCES Users(UserId);

-- RelatedUbiquitousLang テーブル
ALTER TABLE RelatedUbiquitousLang ADD CONSTRAINT FK_RelatedUbiquitousLang_SourceFormalUbiquitousLangId 
    FOREIGN KEY (SourceFormalUbiquitousLangId) REFERENCES FormalUbiquitousLang(FormalUbiquitousLangId);
ALTER TABLE RelatedUbiquitousLang ADD CONSTRAINT FK_RelatedUbiquitousLang_TargetFormalUbiquitousLangId 
    FOREIGN KEY (TargetFormalUbiquitousLangId) REFERENCES FormalUbiquitousLang(FormalUbiquitousLangId);
ALTER TABLE RelatedUbiquitousLang ADD CONSTRAINT FK_RelatedUbiquitousLang_UpdatedBy 
    FOREIGN KEY (UpdatedBy) REFERENCES Users(UserId);

-- DraftUbiquitousLangRelations テーブル
ALTER TABLE DraftUbiquitousLangRelations ADD CONSTRAINT FK_DraftUbiquitousLangRelations_SourceDraftUbiquitousLangId 
    FOREIGN KEY (SourceDraftUbiquitousLangId) REFERENCES DraftUbiquitousLang(DraftUbiquitousLangId);
ALTER TABLE DraftUbiquitousLangRelations ADD CONSTRAINT FK_DraftUbiquitousLangRelations_TargetDraftUbiquitousLangId 
    FOREIGN KEY (TargetDraftUbiquitousLangId) REFERENCES DraftUbiquitousLang(DraftUbiquitousLangId);
ALTER TABLE DraftUbiquitousLangRelations ADD CONSTRAINT FK_DraftUbiquitousLangRelations_UpdatedBy 
    FOREIGN KEY (UpdatedBy) REFERENCES Users(UserId);

-- FormalUbiquitousLangHistory テーブル
ALTER TABLE FormalUbiquitousLangHistory ADD CONSTRAINT FK_FormalUbiquitousLangHistory_FormalUbiquitousLangId 
    FOREIGN KEY (FormalUbiquitousLangId) REFERENCES FormalUbiquitousLang(FormalUbiquitousLangId);
ALTER TABLE FormalUbiquitousLangHistory ADD CONSTRAINT FK_FormalUbiquitousLangHistory_ChangedBy 
    FOREIGN KEY (ChangedBy) REFERENCES Users(UserId);

-- RelatedUbiquitousLangHistory テーブル
ALTER TABLE RelatedUbiquitousLangHistory ADD CONSTRAINT FK_RelatedUbiquitousLangHistory_RelatedUbiquitousLangId 
    FOREIGN KEY (RelatedUbiquitousLangId) REFERENCES RelatedUbiquitousLang(RelatedUbiquitousLangId);
ALTER TABLE RelatedUbiquitousLangHistory ADD CONSTRAINT FK_RelatedUbiquitousLangHistory_ChangedBy 
    FOREIGN KEY (ChangedBy) REFERENCES Users(UserId);

-- ===============================================
-- インデックス
-- ===============================================

-- パフォーマンス向上のためのインデックス

-- Users テーブル
CREATE INDEX IDX_Users_Email ON Users(Email) WHERE IsDeleted = false;
CREATE INDEX IDX_Users_IsActive ON Users(IsActive) WHERE IsDeleted = false;
CREATE INDEX IDX_Users_UserRole ON Users(UserRole) WHERE IsDeleted = false;

-- Projects テーブル
CREATE INDEX IDX_Projects_ProjectManager ON Projects(ProjectManager) WHERE IsDeleted = false;
CREATE INDEX IDX_Projects_ProjectStatus ON Projects(ProjectStatus) WHERE IsDeleted = false;
CREATE INDEX IDX_Projects_ProjectName ON Projects(ProjectName) WHERE IsDeleted = false;

-- Domains テーブル
CREATE INDEX IDX_Domains_ProjectId ON Domains(ProjectId) WHERE IsDeleted = false;
CREATE INDEX IDX_Domains_DomainApprover ON Domains(DomainApprover) WHERE IsDeleted = false;
CREATE INDEX IDX_Domains_DomainStatus ON Domains(DomainStatus) WHERE IsDeleted = false;

-- DraftUbiquitousLang テーブル
CREATE INDEX IDX_DraftUbiquitousLang_DomainId ON DraftUbiquitousLang(DomainId) WHERE IsDeleted = false;
CREATE INDEX IDX_DraftUbiquitousLang_Status ON DraftUbiquitousLang(Status) WHERE IsDeleted = false;
CREATE INDEX IDX_DraftUbiquitousLang_CreatedBy ON DraftUbiquitousLang(CreatedBy) WHERE IsDeleted = false;
CREATE INDEX IDX_DraftUbiquitousLang_Term ON DraftUbiquitousLang(Term) WHERE IsDeleted = false;

-- FormalUbiquitousLang テーブル
CREATE INDEX IDX_FormalUbiquitousLang_DomainId ON FormalUbiquitousLang(DomainId) WHERE IsDeleted = false;
CREATE INDEX IDX_FormalUbiquitousLang_ApprovedBy ON FormalUbiquitousLang(ApprovedBy) WHERE IsDeleted = false;
CREATE INDEX IDX_FormalUbiquitousLang_Term ON FormalUbiquitousLang(Term) WHERE IsDeleted = false;

-- UserProjects テーブル
CREATE INDEX IDX_UserProjects_UserId ON UserProjects(UserId) WHERE IsDeleted = false;
CREATE INDEX IDX_UserProjects_ProjectId ON UserProjects(ProjectId) WHERE IsDeleted = false;
CREATE INDEX IDX_UserProjects_Role ON UserProjects(Role) WHERE IsDeleted = false;

-- DomainApprovers テーブル
CREATE INDEX IDX_DomainApprovers_DomainId ON DomainApprovers(DomainId) WHERE IsDeleted = false;
CREATE INDEX IDX_DomainApprovers_UserId ON DomainApprovers(UserId) WHERE IsDeleted = false;

-- RelatedUbiquitousLang テーブル
CREATE INDEX IDX_RelatedUbiquitousLang_SourceFormalUbiquitousLangId ON RelatedUbiquitousLang(SourceFormalUbiquitousLangId) WHERE IsDeleted = false;
CREATE INDEX IDX_RelatedUbiquitousLang_TargetFormalUbiquitousLangId ON RelatedUbiquitousLang(TargetFormalUbiquitousLangId) WHERE IsDeleted = false;

-- DraftUbiquitousLangRelations テーブル
CREATE INDEX IDX_DraftUbiquitousLangRelations_SourceDraftUbiquitousLangId ON DraftUbiquitousLangRelations(SourceDraftUbiquitousLangId) WHERE IsDeleted = false;
CREATE INDEX IDX_DraftUbiquitousLangRelations_TargetDraftUbiquitousLangId ON DraftUbiquitousLangRelations(TargetDraftUbiquitousLangId) WHERE IsDeleted = false;

-- 履歴テーブル
CREATE INDEX IDX_FormalUbiquitousLangHistory_FormalUbiquitousLangId ON FormalUbiquitousLangHistory(FormalUbiquitousLangId);
CREATE INDEX IDX_FormalUbiquitousLangHistory_ChangedAt ON FormalUbiquitousLangHistory(ChangedAt);
CREATE INDEX IDX_RelatedUbiquitousLangHistory_RelatedUbiquitousLangId ON RelatedUbiquitousLangHistory(RelatedUbiquitousLangId);
CREATE INDEX IDX_RelatedUbiquitousLangHistory_ChangedAt ON RelatedUbiquitousLangHistory(ChangedAt);

-- 複合インデックス
CREATE INDEX IDX_UserProjects_UserId_ProjectId ON UserProjects(UserId, ProjectId) WHERE IsDeleted = false;
CREATE INDEX IDX_DomainApprovers_DomainId_UserId ON DomainApprovers(DomainId, UserId) WHERE IsDeleted = false;

-- ===============================================
-- 一意制約
-- ===============================================

-- プロジェクト内でのドメイン名の一意性
CREATE UNIQUE INDEX UQ_Domains_ProjectId_DomainName ON Domains(ProjectId, DomainName) WHERE IsDeleted = false;

-- ドメイン内での用語名の一意性（ドラフト）
CREATE UNIQUE INDEX UQ_DraftUbiquitousLang_DomainId_Term ON DraftUbiquitousLang(DomainId, Term) WHERE IsDeleted = false;

-- ドメイン内での用語名の一意性（正式）
CREATE UNIQUE INDEX UQ_FormalUbiquitousLang_DomainId_Term ON FormalUbiquitousLang(DomainId, Term) WHERE IsDeleted = false;

-- ユーザー・プロジェクト関連の一意性
CREATE UNIQUE INDEX UQ_UserProjects_UserId_ProjectId ON UserProjects(UserId, ProjectId) WHERE IsDeleted = false;

-- ドメイン承認者の一意性
CREATE UNIQUE INDEX UQ_DomainApprovers_DomainId_UserId ON DomainApprovers(DomainId, UserId) WHERE IsDeleted = false;

-- 関連ユビキタス言語の一意性
CREATE UNIQUE INDEX UQ_RelatedUbiquitousLang_SourceTarget ON RelatedUbiquitousLang(SourceFormalUbiquitousLangId, TargetFormalUbiquitousLangId) WHERE IsDeleted = false;

-- ドラフト関連の一意性
CREATE UNIQUE INDEX UQ_DraftUbiquitousLangRelations_SourceTarget ON DraftUbiquitousLangRelations(SourceDraftUbiquitousLangId, TargetDraftUbiquitousLangId) WHERE IsDeleted = false;

-- ===============================================
-- 循環参照制約
-- ===============================================

-- 関連ユビキタス言語の循環参照防止
ALTER TABLE RelatedUbiquitousLang ADD CONSTRAINT CK_RelatedUbiquitousLang_NoSelfReference 
    CHECK (SourceFormalUbiquitousLangId != TargetFormalUbiquitousLangId);

-- ドラフト関連の循環参照防止
ALTER TABLE DraftUbiquitousLangRelations ADD CONSTRAINT CK_DraftUbiquitousLangRelations_NoSelfReference 
    CHECK (SourceDraftUbiquitousLangId != TargetDraftUbiquitousLangId);

-- ===============================================
-- PostgreSQL固有最適化
-- ===============================================

-- JSONB用GINインデックス（高速検索）
CREATE INDEX IDX_FormalUbiquitousLangHistory_OldValue_GIN ON FormalUbiquitousLangHistory USING GIN (OldValue);
CREATE INDEX IDX_FormalUbiquitousLangHistory_NewValue_GIN ON FormalUbiquitousLangHistory USING GIN (NewValue);
CREATE INDEX IDX_RelatedUbiquitousLangHistory_OldValue_GIN ON RelatedUbiquitousLangHistory USING GIN (OldValue);
CREATE INDEX IDX_RelatedUbiquitousLangHistory_NewValue_GIN ON RelatedUbiquitousLangHistory USING GIN (NewValue);

-- パーティションテーブル（履歴テーブルの月単位分割）
-- 注意: 本番環境では履歴データ増加に応じて月単位パーティション推奨

-- ===============================================
-- 統計情報更新
-- ===============================================

-- 統計情報の更新（クエリプランナー最適化）
ANALYZE;

-- ===============================================
-- 完了メッセージ
-- ===============================================

DO $$
BEGIN
    RAISE NOTICE 'ユビキタス言語管理システム データベーススキーマ作成完了';
    RAISE NOTICE '作成テーブル数: 11';
    RAISE NOTICE '作成インデックス数: 20+';
    RAISE NOTICE '外部キー制約数: 16';
    RAISE NOTICE 'PostgreSQL専用最適化: 有効';
END $$;