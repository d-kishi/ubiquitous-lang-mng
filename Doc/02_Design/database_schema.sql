-- ===============================================
-- ユビキタス言語管理システム データベーススキーマ
-- 
-- 作成日: 2025-06-29
-- 最終更新: 2025-06-29
-- 対象DB: SQLite（開発・テスト用）/ PostgreSQL（本格運用）
-- A5:SQL Mk-2 対応
-- ===============================================

-- ===============================================
-- 1. ユーザー管理テーブル
-- ===============================================

-- Users: システム利用者の認証・権限情報管理
CREATE TABLE Users (
    UserId BIGINT PRIMARY KEY IDENTITY,  -- ユーザーID（主キー）
    Email NVARCHAR(254) NOT NULL UNIQUE,     -- メールアドレス（ログインID）
    PasswordHash NVARCHAR(255) NOT NULL,     -- パスワードハッシュ値
    Name NVARCHAR(50) NOT NULL,              -- ユーザー氏名
    UserRole NVARCHAR(20) NOT NULL           -- ユーザーロール
        CHECK (UserRole IN ('SuperUser', 'ProjectManager', 'DomainApprover', 'GeneralUser')),
    IsActive BIT NOT NULL DEFAULT 1,     -- アクティブフラグ
    IsFirstLogin BIT NOT NULL DEFAULT 1, -- 初回ログインフラグ
    PasswordResetToken NVARCHAR(255),        -- パスワードリセットトークン
    PasswordResetExpiry DATETIME2(7),            -- リセットトークン有効期限
    UpdatedBy BIGINT NOT NULL,               -- 最終更新者ID
    UpdatedAt DATETIME2(7) NOT NULL DEFAULT CURRENT_TIMESTAMP, -- 最終更新日時
    IsDeleted BIT NOT NULL DEFAULT 0,    -- 論理削除フラグ
);

-- Usersテーブルコメント
COMMENT ON TABLE Users IS 'システム利用者の認証・権限情報と基本プロフィールを管理';
COMMENT ON COLUMN Users.UserId IS 'ユーザーID（主キー）';
COMMENT ON COLUMN Users.Email IS 'メールアドレス（ログインID、システム内一意）';
COMMENT ON COLUMN Users.PasswordHash IS 'パスワードハッシュ値（bcrypt）';
COMMENT ON COLUMN Users.Name IS 'ユーザー氏名';
COMMENT ON COLUMN Users.UserRole IS 'ユーザーロール（SuperUser/ProjectManager/DomainApprover/GeneralUser）';
COMMENT ON COLUMN Users.IsActive IS 'アクティブフラグ（1:有効、0:無効）';
COMMENT ON COLUMN Users.IsFirstLogin IS '初回ログインフラグ（1:初回、0:変更済み）';
COMMENT ON COLUMN Users.PasswordResetToken IS 'パスワードリセット用トークン';
COMMENT ON COLUMN Users.PasswordResetExpiry IS 'リセットトークン有効期限（24時間）';
COMMENT ON COLUMN Users.UpdatedBy IS '最終更新者ユーザーID';
COMMENT ON COLUMN Users.UpdatedAt IS '最終更新日時';
COMMENT ON COLUMN Users.IsDeleted IS '論理削除フラグ（0:有効、1:削除済み）';

-- ===============================================
-- 2. プロジェクト管理テーブル
-- ===============================================

-- Projects: プロジェクト情報の管理
CREATE TABLE Projects (
    ProjectId BIGINT PRIMARY KEY IDENTITY, -- プロジェクトID（主キー）
    ProjectName NVARCHAR(50) NOT NULL UNIQUE,   -- プロジェクト名（システム内一意）
    Description NVARCHAR(200),                  -- プロジェクト説明
    UpdatedBy BIGINT NOT NULL,                  -- 最終更新者ID
    UpdatedAt DATETIME2(7) NOT NULL DEFAULT CURRENT_TIMESTAMP, -- 最終更新日時
    IsDeleted BIT NOT NULL DEFAULT 0,       -- 論理削除フラグ
);

-- Projectsテーブルコメント
COMMENT ON TABLE Projects IS 'プロジェクト情報の管理とユーザー・ドメインとの関連制御';
COMMENT ON COLUMN Projects.ProjectId IS 'プロジェクトID（主キー）';
COMMENT ON COLUMN Projects.ProjectName IS 'プロジェクト名（システム内一意）';
COMMENT ON COLUMN Projects.Description IS 'プロジェクト説明（最大200文字）';
COMMENT ON COLUMN Projects.UpdatedBy IS '最終更新者ユーザーID';
COMMENT ON COLUMN Projects.UpdatedAt IS '最終更新日時';
COMMENT ON COLUMN Projects.IsDeleted IS '論理削除フラグ（0:有効、1:削除済み）';

-- ===============================================
-- 3. ドメイン管理テーブル
-- ===============================================

-- Domains: プロジェクト内ドメイン分類の管理
CREATE TABLE Domains (
    DomainId BIGINT PRIMARY KEY IDENTITY, -- ドメインID（主キー）
    ProjectId BIGINT NOT NULL,                 -- 所属プロジェクトID
    DomainName NVARCHAR(30) NOT NULL,          -- ドメイン名
    Description NVARCHAR(200),                 -- ドメイン説明
    UpdatedBy BIGINT NOT NULL,                 -- 最終更新者ID
    UpdatedAt DATETIME2(7) NOT NULL DEFAULT CURRENT_TIMESTAMP, -- 最終更新日時
    IsDeleted BIT NOT NULL DEFAULT 0,      -- 論理削除フラグ
    FOREIGN KEY (ProjectId) REFERENCES Projects(ProjectId),
    UNIQUE (ProjectId, DomainName)             -- プロジェクト内ドメイン名一意制約
);

-- Domainsテーブルコメント
COMMENT ON TABLE Domains IS 'プロジェクト内ドメイン分類と承認権限の管理単位';
COMMENT ON COLUMN Domains.DomainId IS 'ドメインID（主キー）';
COMMENT ON COLUMN Domains.ProjectId IS '所属プロジェクトID';
COMMENT ON COLUMN Domains.DomainName IS 'ドメイン名（プロジェクト内一意）';
COMMENT ON COLUMN Domains.Description IS 'ドメイン説明（最大200文字）';
COMMENT ON COLUMN Domains.UpdatedBy IS '最終更新者ユーザーID';
COMMENT ON COLUMN Domains.UpdatedAt IS '最終更新日時';
COMMENT ON COLUMN Domains.IsDeleted IS '論理削除フラグ（0:有効、1:削除済み）';

-- ===============================================
-- 4. ドラフトユビキタス言語管理テーブル
-- ===============================================

-- DraftUbiquitousLang: 編集中・承認申請中のユビキタス言語管理
CREATE TABLE DraftUbiquitousLang (
    DraftUbiquitousLangId BIGINT PRIMARY KEY IDENTITY, -- ドラフトユビキタス言語ID（主キー）
    DomainId BIGINT NOT NULL,                    -- 所属ドメインID
    JapaneseName NVARCHAR(30) NOT NULL,          -- 和名（必須）
    EnglishName NVARCHAR(50),                    -- 英名（任意、半角英数・ハイフン・アンダースコア）
    Description NVARCHAR(500),                   -- 意味・説明（改行可能）
    OccurrenceContext NVARCHAR(50),              -- 発生機会
    Remarks NVARCHAR(500),                       -- 備考（改行可能）
    Status NVARCHAR(20) NOT NULL DEFAULT 'Draft' -- 状態
        CHECK (Status IN ('Draft', 'PendingApproval')),
    ApplicantId BIGINT,                          -- 申請者ID
    ApplicationDate DATETIME2(7),                    -- 申請日時
    RejectionReason NVARCHAR(500),               -- 却下理由
    SourceFormalUbiquitousLangId BIGINT,         -- 編集元正式ユビキタス言語ID
    UpdatedBy BIGINT NOT NULL,                   -- 最終更新者ID
    UpdatedAt DATETIME2(7) NOT NULL DEFAULT CURRENT_TIMESTAMP, -- 最終更新日時
    FOREIGN KEY (DomainId) REFERENCES Domains(DomainId),
    FOREIGN KEY (ApplicantId) REFERENCES Users(UserId),
    FOREIGN KEY (SourceFormalUbiquitousLangId) REFERENCES FormalUbiquitousLang(FormalUbiquitousLangId),
);

-- DraftUbiquitousLangテーブルコメント
COMMENT ON TABLE DraftUbiquitousLang IS '編集中・承認申請中のユビキタス言語管理とワークフロー制御';
COMMENT ON COLUMN DraftUbiquitousLang.DraftUbiquitousLangId IS 'ドラフトユビキタス言語ID（主キー）';
COMMENT ON COLUMN DraftUbiquitousLang.DomainId IS '所属ドメインID';
COMMENT ON COLUMN DraftUbiquitousLang.JapaneseName IS '和名（必須、最大30文字）';
COMMENT ON COLUMN DraftUbiquitousLang.EnglishName IS '英名（任意、最大50文字、半角英数・ハイフン・アンダースコアのみ）';
COMMENT ON COLUMN DraftUbiquitousLang.Description IS '意味・説明（任意、最大500文字、改行可能）';
COMMENT ON COLUMN DraftUbiquitousLang.OccurrenceContext IS '発生機会（任意、最大50文字）';
COMMENT ON COLUMN DraftUbiquitousLang.Remarks IS '備考（任意、最大500文字、改行可能）';
COMMENT ON COLUMN DraftUbiquitousLang.Status IS '状態（Draft:ドラフト、PendingApproval:承認申請中）';
COMMENT ON COLUMN DraftUbiquitousLang.ApplicantId IS '承認申請者ユーザーID';
COMMENT ON COLUMN DraftUbiquitousLang.ApplicationDate IS '承認申請日時';
COMMENT ON COLUMN DraftUbiquitousLang.RejectionReason IS '却下理由（却下時のみ設定）';
COMMENT ON COLUMN DraftUbiquitousLang.SourceFormalUbiquitousLangId IS '編集元正式ユビキタス言語ID（新規作成時はNULL）';
COMMENT ON COLUMN DraftUbiquitousLang.UpdatedBy IS '最終更新者ユーザーID';
COMMENT ON COLUMN DraftUbiquitousLang.UpdatedAt IS '最終更新日時';

-- ===============================================
-- 5. 正式ユビキタス言語管理テーブル
-- ===============================================

-- FormalUbiquitousLang: 承認済み確定ユビキタス言語の管理
CREATE TABLE FormalUbiquitousLang (
    FormalUbiquitousLangId BIGINT PRIMARY KEY IDENTITY, -- 正式ユビキタス言語ID（主キー）
    DomainId BIGINT NOT NULL,                    -- 所属ドメインID
    JapaneseName NVARCHAR(30) NOT NULL,          -- 和名（必須）
    EnglishName NVARCHAR(50) NOT NULL,           -- 英名（必須）
    Description NVARCHAR(500) NOT NULL,          -- 意味・説明（必須、改行可能）
    OccurrenceContext NVARCHAR(50),              -- 発生機会（任意）
    Remarks NVARCHAR(500),                       -- 備考（任意、改行可能）
    UpdatedBy BIGINT NOT NULL,                   -- 最終更新者ID
    UpdatedAt DATETIME2(7) NOT NULL DEFAULT CURRENT_TIMESTAMP, -- 最終更新日時
    IsDeleted BIT NOT NULL DEFAULT 0,        -- 論理削除フラグ
    FOREIGN KEY (DomainId) REFERENCES Domains(DomainId),
);

-- FormalUbiquitousLangテーブルコメント
COMMENT ON TABLE FormalUbiquitousLang IS '承認済み確定ユビキタス言語の管理と外部連携用データ提供';
COMMENT ON COLUMN FormalUbiquitousLang.FormalUbiquitousLangId IS '正式ユビキタス言語ID（主キー）';
COMMENT ON COLUMN FormalUbiquitousLang.DomainId IS '所属ドメインID';
COMMENT ON COLUMN FormalUbiquitousLang.JapaneseName IS '和名（必須、最大30文字）';
COMMENT ON COLUMN FormalUbiquitousLang.EnglishName IS '英名（必須、最大50文字）';
COMMENT ON COLUMN FormalUbiquitousLang.Description IS '意味・説明（必須、最大500文字、改行可能）';
COMMENT ON COLUMN FormalUbiquitousLang.OccurrenceContext IS '発生機会（任意、最大50文字）';
COMMENT ON COLUMN FormalUbiquitousLang.Remarks IS '備考（任意、最大500文字、改行可能）';
COMMENT ON COLUMN FormalUbiquitousLang.UpdatedBy IS '最終更新者ユーザーID';
COMMENT ON COLUMN FormalUbiquitousLang.UpdatedAt IS '最終更新日時';
COMMENT ON COLUMN FormalUbiquitousLang.IsDeleted IS '論理削除フラグ（0:有効、1:削除済み）';

-- ===============================================
-- 6. ユーザー・プロジェクト関連テーブル
-- ===============================================

-- UserProjects: ユーザーとプロジェクトの多対多関連
CREATE TABLE UserProjects (
    UserProjectId BIGINT PRIMARY KEY IDENTITY, -- ユーザープロジェクトID（主キー）
    UserId BIGINT NOT NULL,                         -- ユーザーID
    ProjectId BIGINT NOT NULL,                      -- プロジェクトID
    UpdatedBy BIGINT NOT NULL,                      -- 最終更新者ID
    UpdatedAt DATETIME2(7) NOT NULL DEFAULT CURRENT_TIMESTAMP, -- 最終更新日時
    FOREIGN KEY (UserId) REFERENCES Users(UserId),
    FOREIGN KEY (ProjectId) REFERENCES Projects(ProjectId),
    UNIQUE (UserId, ProjectId)                      -- ユーザー・プロジェクト組み合わせ一意制約
);

-- UserProjectsテーブルコメント
COMMENT ON TABLE UserProjects IS 'ユーザーとプロジェクトの多対多関連を管理、権限制御の基盤';
COMMENT ON COLUMN UserProjects.UserProjectId IS 'ユーザープロジェクトID（主キー）';
COMMENT ON COLUMN UserProjects.UserId IS 'ユーザーID';
COMMENT ON COLUMN UserProjects.ProjectId IS 'プロジェクトID';
COMMENT ON COLUMN UserProjects.UpdatedBy IS '最終更新者ユーザーID';
COMMENT ON COLUMN UserProjects.UpdatedAt IS '最終更新日時';

-- DomainApprovers: ドメイン承認者の管理
CREATE TABLE DomainApprovers (
    DomainApproverId BIGINT PRIMARY KEY IDENTITY, -- ドメイン承認者ID（主キー）
    DomainId BIGINT NOT NULL,                         -- ドメインID
    ApproverId BIGINT NOT NULL,                       -- 承認者ユーザーID
    UpdatedBy BIGINT NOT NULL,                        -- 最終更新者ID
    UpdatedAt DATETIME2(7) NOT NULL DEFAULT CURRENT_TIMESTAMP, -- 最終更新日時
    FOREIGN KEY (DomainId) REFERENCES Domains(DomainId),
    FOREIGN KEY (ApproverId) REFERENCES Users(UserId),
    UNIQUE (DomainId, ApproverId)                     -- ドメイン・承認者組み合わせ一意制約
);

-- DomainApproversテーブルコメント
COMMENT ON TABLE DomainApprovers IS 'ドメイン承認権限の管理、承認フローの権限制御に使用';
COMMENT ON COLUMN DomainApprovers.DomainApproverId IS 'ドメイン承認者ID（主キー）';
COMMENT ON COLUMN DomainApprovers.DomainId IS 'ドメインID';
COMMENT ON COLUMN DomainApprovers.ApproverId IS '承認者ユーザーID';
COMMENT ON COLUMN DomainApprovers.UpdatedBy IS '最終更新者ユーザーID';
COMMENT ON COLUMN DomainApprovers.UpdatedAt IS '最終更新日時';

-- ===============================================
-- 7. 関連ユビキタス言語管理テーブル
-- ===============================================

-- RelatedUbiquitousLang: ユビキタス言語間の関連性管理
CREATE TABLE RelatedUbiquitousLang (
    RelatedUbiquitousLangId BIGINT PRIMARY KEY IDENTITY, -- 関連ユビキタス言語ID（主キー）
    SourceUbiquitousLangId BIGINT NOT NULL,             -- 関連元ユビキタス言語ID
    TargetUbiquitousLangId BIGINT NOT NULL,             -- 関連先ユビキタス言語ID
    UpdatedBy BIGINT NOT NULL,                          -- 最終更新者ID
    UpdatedAt DATETIME2(7) NOT NULL DEFAULT CURRENT_TIMESTAMP, -- 最終更新日時
    FOREIGN KEY (SourceUbiquitousLangId) REFERENCES FormalUbiquitousLang(FormalUbiquitousLangId),
    FOREIGN KEY (TargetUbiquitousLangId) REFERENCES FormalUbiquitousLang(FormalUbiquitousLangId),
    UNIQUE (SourceUbiquitousLangId, TargetUbiquitousLangId) -- 同一関連の重複防止
);

-- RelatedUbiquitousLangテーブルコメント
COMMENT ON TABLE RelatedUbiquitousLang IS 'ユビキタス言語間の関連性を管理、意味的な繋がりや類義語関係の表現';
COMMENT ON COLUMN RelatedUbiquitousLang.RelatedUbiquitousLangId IS '関連ユビキタス言語ID（主キー）';
COMMENT ON COLUMN RelatedUbiquitousLang.SourceUbiquitousLangId IS '関連元正式ユビキタス言語ID';
COMMENT ON COLUMN RelatedUbiquitousLang.TargetUbiquitousLangId IS '関連先正式ユビキタス言語ID';
COMMENT ON COLUMN RelatedUbiquitousLang.UpdatedBy IS '最終更新者ユーザーID';
COMMENT ON COLUMN RelatedUbiquitousLang.UpdatedAt IS '最終更新日時';

-- ===============================================
-- 8. ドラフトユビキタス言語関連テーブル
-- ===============================================

-- DraftUbiquitousLangRelations: ドラフトユビキタス言語間の関連性管理
CREATE TABLE DraftUbiquitousLangRelations (
    DraftUbiquitousLangRelationId BIGINT PRIMARY KEY IDENTITY, -- ドラフト関連ID（主キー）
    SourceDraftUbiquitousLangId BIGINT NOT NULL,        -- 関連元ドラフトユビキタス言語ID
    TargetFormalUbiquitousLangId BIGINT NOT NULL,       -- 関連先正式ユビキタス言語ID
    UpdatedBy BIGINT NOT NULL,                          -- 最終更新者ID
    UpdatedAt DATETIME2(7) NOT NULL DEFAULT CURRENT_TIMESTAMP, -- 最終更新日時
    FOREIGN KEY (SourceDraftUbiquitousLangId) REFERENCES DraftUbiquitousLang(DraftUbiquitousLangId),
    FOREIGN KEY (TargetFormalUbiquitousLangId) REFERENCES FormalUbiquitousLang(FormalUbiquitousLangId),
    UNIQUE (SourceDraftUbiquitousLangId, TargetFormalUbiquitousLangId) -- 同一関連の重複防止
);

-- DraftUbiquitousLangRelationsテーブルコメント
COMMENT ON TABLE DraftUbiquitousLangRelations IS 'ドラフトユビキタス言語と正式ユビキタス言語間の関連性管理';
COMMENT ON COLUMN DraftUbiquitousLangRelations.DraftUbiquitousLangRelationId IS 'ドラフト関連ID（主キー）';
COMMENT ON COLUMN DraftUbiquitousLangRelations.SourceDraftUbiquitousLangId IS '関連元ドラフトユビキタス言語ID';
COMMENT ON COLUMN DraftUbiquitousLangRelations.TargetFormalUbiquitousLangId IS '関連先正式ユビキタス言語ID';
COMMENT ON COLUMN DraftUbiquitousLangRelations.UpdatedBy IS '最終更新者ユーザーID';
COMMENT ON COLUMN DraftUbiquitousLangRelations.UpdatedAt IS '最終更新日時';

-- ===============================================
-- 9. 変更履歴テーブル群
-- ===============================================

-- FormalUbiquitousLangHistory: 正式ユビキタス言語の変更履歴管理
CREATE TABLE FormalUbiquitousLangHistory (
    HistoryId BIGINT PRIMARY KEY IDENTITY,      -- 履歴ID（主キー）
    FormalUbiquitousLangId BIGINT NOT NULL,          -- 対象正式ユビキタス言語ID
    DomainId BIGINT NOT NULL,                        -- 所属ドメインID
    JapaneseName NVARCHAR(30) NOT NULL,              -- 和名（必須）
    EnglishName NVARCHAR(50) NOT NULL,               -- 英名（必須）
    Description NVARCHAR(500) NOT NULL,              -- 意味・説明（必須、改行可能）
    OccurrenceContext NVARCHAR(50),                  -- 発生機会（任意）
    Remarks NVARCHAR(500),                           -- 備考（任意、改行可能）
    RelatedUbiquitousLangSnapshot TEXT,              -- 関連ユビキタス言語スナップショット（JSON形式、PostgreSQL移行時JSONB）
    UpdatedBy BIGINT NOT NULL,                       -- 最終更新者ID
    UpdatedAt DATETIME2(7) NOT NULL,                     -- 最終更新日時
    IsDeleted BIT NOT NULL,                      -- 論理削除フラグ
    FOREIGN KEY (FormalUbiquitousLangId) REFERENCES FormalUbiquitousLang(FormalUbiquitousLangId),
    FOREIGN KEY (DomainId) REFERENCES Domains(DomainId),
);

-- FormalUbiquitousLangHistoryテーブルコメント
COMMENT ON TABLE FormalUbiquitousLangHistory IS '正式ユビキタス言語の変更履歴を保持、監査証跡とデータガバナンスを提供';
COMMENT ON COLUMN FormalUbiquitousLangHistory.HistoryId IS '履歴ID（主キー）';
COMMENT ON COLUMN FormalUbiquitousLangHistory.FormalUbiquitousLangId IS '対象正式ユビキタス言語ID';
COMMENT ON COLUMN FormalUbiquitousLangHistory.DomainId IS '所属ドメインID';
COMMENT ON COLUMN FormalUbiquitousLangHistory.JapaneseName IS '和名（必須、最大30文字）';
COMMENT ON COLUMN FormalUbiquitousLangHistory.EnglishName IS '英名（必須、最大50文字）';
COMMENT ON COLUMN FormalUbiquitousLangHistory.Description IS '意味・説明（必須、最大500文字、改行可能）';
COMMENT ON COLUMN FormalUbiquitousLangHistory.OccurrenceContext IS '発生機会（任意、最大50文字）';
COMMENT ON COLUMN FormalUbiquitousLangHistory.Remarks IS '備考（任意、最大500文字、改行可能）';
COMMENT ON COLUMN FormalUbiquitousLangHistory.RelatedUbiquitousLangSnapshot IS '関連ユビキタス言語スナップショット（JSON形式、履歴作成時点での関連ユビキタス言語情報）';
COMMENT ON COLUMN FormalUbiquitousLangHistory.UpdatedBy IS '最終更新者ユーザーID';
COMMENT ON COLUMN FormalUbiquitousLangHistory.UpdatedAt IS '最終更新日時';
COMMENT ON COLUMN FormalUbiquitousLangHistory.IsDeleted IS '論理削除フラグ（0:有効、1:削除済み）';


-- ===============================================
-- 10. インデックス作成
-- ===============================================

-- Users テーブルインデックス
CREATE INDEX IX_Users_UserRole ON Users(UserRole);
CREATE INDEX IX_Users_IsDeleted ON Users(IsDeleted);
CREATE INDEX IX_Users_Email_Active ON Users(Email) WHERE IsDeleted = 0;

-- Projects テーブルインデックス
CREATE INDEX IX_Projects_IsDeleted ON Projects(IsDeleted);
CREATE INDEX IX_Projects_UpdatedBy ON Projects(UpdatedBy);
CREATE INDEX IX_Projects_Name_Active ON Projects(ProjectName) WHERE IsDeleted = 0;

-- Domains テーブルインデックス
CREATE INDEX IX_Domains_ProjectId ON Domains(ProjectId);
CREATE INDEX IX_Domains_IsDeleted ON Domains(IsDeleted);
CREATE INDEX IX_Domains_Project_Active ON Domains(ProjectId, DomainName) WHERE IsDeleted = 0;

-- DraftUbiquitousLang テーブルインデックス
CREATE INDEX IX_DraftUbiquitousLang_DomainId ON DraftUbiquitousLang(DomainId);
CREATE INDEX IX_DraftUbiquitousLang_Status ON DraftUbiquitousLang(Status);
CREATE INDEX IX_DraftUbiquitousLang_JapaneseName ON DraftUbiquitousLang(JapaneseName);
CREATE INDEX IX_DraftUbiquitousLang_ApplicantId ON DraftUbiquitousLang(ApplicantId);
CREATE INDEX IX_DraftUbiquitousLang_Domain_Status ON DraftUbiquitousLang(DomainId, Status);
CREATE INDEX IX_DraftUbiquitousLang_SourceFormal ON DraftUbiquitousLang(SourceFormalUbiquitousLangId);

-- FormalUbiquitousLang テーブルインデックス
CREATE INDEX IX_FormalUbiquitousLang_DomainId ON FormalUbiquitousLang(DomainId);
CREATE INDEX IX_FormalUbiquitousLang_JapaneseName ON FormalUbiquitousLang(JapaneseName);
CREATE INDEX IX_FormalUbiquitousLang_EnglishName ON FormalUbiquitousLang(EnglishName);
CREATE INDEX IX_FormalUbiquitousLang_IsDeleted ON FormalUbiquitousLang(IsDeleted);
CREATE INDEX IX_FormalUbiquitousLang_Domain_Japanese_Active ON FormalUbiquitousLang(DomainId, JapaneseName) WHERE IsDeleted = 0;
CREATE INDEX IX_FormalUbiquitousLang_Domain_UpdatedAt ON FormalUbiquitousLang(DomainId, UpdatedAt DESC) WHERE IsDeleted = 0;

-- UserProjects テーブルインデックス
CREATE INDEX IX_UserProjects_ProjectId ON UserProjects(ProjectId);
CREATE INDEX IX_UserProjects_UserId ON UserProjects(UserId);

-- DomainApprovers テーブルインデックス
CREATE INDEX IX_DomainApprovers_DomainId ON DomainApprovers(DomainId);
CREATE INDEX IX_DomainApprovers_ApproverId ON DomainApprovers(ApproverId);

-- RelatedUbiquitousLang テーブルインデックス
CREATE INDEX IX_RelatedUbiquitousLang_SourceUbiquitousLangId ON RelatedUbiquitousLang(SourceUbiquitousLangId);
CREATE INDEX IX_RelatedUbiquitousLang_TargetUbiquitousLangId ON RelatedUbiquitousLang(TargetUbiquitousLangId);

-- DraftUbiquitousLangRelations テーブルインデックス
CREATE INDEX IX_DraftUbiquitousLangRelations_SourceDraft ON DraftUbiquitousLangRelations(SourceDraftUbiquitousLangId);
CREATE INDEX IX_DraftUbiquitousLangRelations_TargetFormal ON DraftUbiquitousLangRelations(TargetFormalUbiquitousLangId);

-- FormalUbiquitousLangHistory テーブルインデックス
CREATE INDEX IX_FormalUbiquitousLangHistory_FormalUbiquitousLangId ON FormalUbiquitousLangHistory(FormalUbiquitousLangId);
CREATE INDEX IX_FormalUbiquitousLangHistory_UpdatedAt ON FormalUbiquitousLangHistory(UpdatedAt DESC);
CREATE INDEX IX_FormalUbiquitousLangHistory_Formal_UpdatedAt ON FormalUbiquitousLangHistory(FormalUbiquitousLangId, UpdatedAt DESC);


-- ===============================================
-- 11. 初期データ挿入
-- ===============================================

-- 初期スーパーユーザー作成（設定ファイルから読み込み想定）
-- 実際の初期化時にアプリケーションコードで実行
/*
INSERT INTO Users (Email, PasswordHash, Name, UserRole, IsActive, IsFirstLogin, UpdatedBy)
VALUES (
    '[設定ファイルから読み込み]',
    '[ハッシュ化された"su"]',
    '[設定ファイルから読み込み]',
    'SuperUser',
    1,
    1,
    1  -- 自分自身を最終更新者とする
);
*/

-- ===============================================
-- 12. PostgreSQL移行用の変更点
-- ===============================================

/*
PostgreSQL移行時の主な変更点:

1. データ型変更
   - BIGINT → BIGSERIAL (PRIMARY KEY IDENTITY、64bit整数)
   - BIGINT (FK) → BIGINT (外部キー、64bit整数)
   - NVARCHAR → VARCHAR
   - BIT → BOOLEAN (PostgreSQLネイティブサポート)
   - DATETIME2(7) → TIMESTAMP
   - TEXT → JSONB (RelatedUbiquitousLangSnapshot列)

2. 制約・インデックス
   - CHECK制約の構文確認
   - 部分インデックス（WHERE句）の構文確認
   - UNIQUE制約の動作確認

3. 関数・機能
   - CURRENT_TIMESTAMP → NOW() または CURRENT_TIMESTAMP
   - 自動増分の仕組み（SERIAL型）
   - JSONB型の活用（FormalUbiquitousLangHistoryの関連ユビキタス言語スナップショット格納）

4. パフォーマンス最適化
   - インデックス戦略の見直し
   - パーティショニングの検討（FormalUbiquitousLangHistory等）
   - 統計情報の更新設定
*/

-- ===============================================
-- 完了: A5:SQL Mk-2 対応データベーススキーマ（10テーブル構成）
-- 変更内容:
-- - RelatedUbiquitousLangHistoryテーブル削除
-- - FormalUbiquitousLangHistoryにRelatedUbiquitousLangSnapshot列追加（JSON/JSONB型）
-- - 用語表記統一ルール適用（ADR_003準拠）
-- ===============================================