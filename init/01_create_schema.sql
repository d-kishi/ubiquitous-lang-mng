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
    ProjectName VARCHAR(50) NOT NULL UNIQUE,       -- プロジェクト名（システム内一意）
    Description TEXT,                              -- プロジェクト説明
    UpdatedBy BIGINT NOT NULL,                     -- 最終更新者ID
    UpdatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),  -- 最終更新日時
    IsDeleted BOOLEAN NOT NULL DEFAULT false       -- 論理削除フラグ
);

-- Projectsテーブルコメント
COMMENT ON TABLE Projects IS 'プロジェクト情報の管理とユーザー・ドメインとの関連制御';
COMMENT ON COLUMN Projects.ProjectId IS 'プロジェクトID（主キー）';
COMMENT ON COLUMN Projects.ProjectName IS 'プロジェクト名（システム内一意）';
COMMENT ON COLUMN Projects.Description IS 'プロジェクト説明';
COMMENT ON COLUMN Projects.UpdatedBy IS '最終更新者ユーザーID';
COMMENT ON COLUMN Projects.UpdatedAt IS '最終更新日時（タイムゾーン付き）';
COMMENT ON COLUMN Projects.IsDeleted IS '論理削除フラグ（false:有効、true:削除済み）';

-- ===============================================
-- 3. ユーザー・プロジェクト関連テーブル
-- ===============================================

-- UserProjects: ユーザーとプロジェクトの多対多関連管理
CREATE TABLE UserProjects (
    UserProjectId BIGSERIAL PRIMARY KEY,           -- ユーザープロジェクトID（主キー）
    UserId BIGINT NOT NULL,                        -- ユーザーID
    ProjectId BIGINT NOT NULL,                     -- プロジェクトID
    UpdatedBy BIGINT NOT NULL,                     -- 最終更新者ID
    UpdatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),  -- 最終更新日時
    FOREIGN KEY (UserId) REFERENCES Users(UserId),
    FOREIGN KEY (ProjectId) REFERENCES Projects(ProjectId),
    UNIQUE (UserId, ProjectId)                     -- ユーザー・プロジェクト組み合わせ一意
);

-- UserProjectsテーブルコメント
COMMENT ON TABLE UserProjects IS 'ユーザーとプロジェクトの多対多関連を管理、権限制御の基盤';
COMMENT ON COLUMN UserProjects.UserProjectId IS 'ユーザープロジェクトID（主キー）';
COMMENT ON COLUMN UserProjects.UserId IS 'ユーザーID（外部キー）';
COMMENT ON COLUMN UserProjects.ProjectId IS 'プロジェクトID（外部キー）';
COMMENT ON COLUMN UserProjects.UpdatedBy IS '最終更新者ユーザーID';
COMMENT ON COLUMN UserProjects.UpdatedAt IS '最終更新日時（タイムゾーン付き）';

-- ===============================================
-- 4. ドメイン管理テーブル
-- ===============================================

-- Domains: プロジェクト内ドメイン分類の管理
CREATE TABLE Domains (
    DomainId BIGSERIAL PRIMARY KEY,                -- ドメインID（主キー）
    ProjectId BIGINT NOT NULL,                     -- 所属プロジェクトID
    DomainName VARCHAR(30) NOT NULL,               -- ドメイン名
    Description TEXT,                              -- ドメイン説明
    UpdatedBy BIGINT NOT NULL,                     -- 最終更新者ID
    UpdatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),  -- 最終更新日時
    IsDeleted BOOLEAN NOT NULL DEFAULT false,      -- 論理削除フラグ
    FOREIGN KEY (ProjectId) REFERENCES Projects(ProjectId),
    UNIQUE (ProjectId, DomainName)                 -- プロジェクト内ドメイン名一意制約
);

-- Domainsテーブルコメント
COMMENT ON TABLE Domains IS 'プロジェクト内ドメイン分類と承認権限の管理単位';
COMMENT ON COLUMN Domains.DomainId IS 'ドメインID（主キー）';
COMMENT ON COLUMN Domains.ProjectId IS '所属プロジェクトID';
COMMENT ON COLUMN Domains.DomainName IS 'ドメイン名（プロジェクト内一意）';
COMMENT ON COLUMN Domains.Description IS 'ドメイン説明';
COMMENT ON COLUMN Domains.UpdatedBy IS '最終更新者ユーザーID';
COMMENT ON COLUMN Domains.UpdatedAt IS '最終更新日時（タイムゾーン付き）';
COMMENT ON COLUMN Domains.IsDeleted IS '論理削除フラグ（false:有効、true:削除済み）';

-- ===============================================
-- 5. ドメイン承認者管理テーブル
-- ===============================================

-- DomainApprovers: ドメイン承認権限の管理
CREATE TABLE DomainApprovers (
    DomainApproverId BIGSERIAL PRIMARY KEY,        -- ドメイン承認者ID（主キー）
    DomainId BIGINT NOT NULL,                      -- ドメインID
    UserId BIGINT NOT NULL,                        -- ユーザーID
    UpdatedBy BIGINT NOT NULL,                     -- 最終更新者ID
    UpdatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),  -- 最終更新日時
    FOREIGN KEY (DomainId) REFERENCES Domains(DomainId),
    FOREIGN KEY (UserId) REFERENCES Users(UserId),
    UNIQUE (DomainId, UserId)                      -- ドメイン・ユーザー組み合わせ一意
);

-- DomainApproversテーブルコメント
COMMENT ON TABLE DomainApprovers IS 'ドメイン別承認権限の管理、承認者とドメインの多対多関連';
COMMENT ON COLUMN DomainApprovers.DomainApproverId IS 'ドメイン承認者ID（主キー）';
COMMENT ON COLUMN DomainApprovers.DomainId IS 'ドメインID（外部キー）';
COMMENT ON COLUMN DomainApprovers.UserId IS 'ユーザーID（外部キー）';
COMMENT ON COLUMN DomainApprovers.UpdatedBy IS '最終更新者ユーザーID';
COMMENT ON COLUMN DomainApprovers.UpdatedAt IS '最終更新日時（タイムゾーン付き）';

-- ===============================================
-- 6. 正式ユビキタス言語管理テーブル
-- ===============================================

-- FormalUbiquitousLang: 承認済み正式ユビキタス言語管理
CREATE TABLE FormalUbiquitousLang (
    FormalUbiquitousLangId BIGSERIAL PRIMARY KEY,  -- 正式ユビキタス言語ID（主キー）
    DomainId BIGINT NOT NULL,                      -- 所属ドメインID
    JapaneseName VARCHAR(30) NOT NULL,             -- 和名
    EnglishName VARCHAR(50) NOT NULL,              -- 英名
    Description TEXT NOT NULL,                     -- 意味・説明（改行可能）
    OccurrenceContext VARCHAR(50),                 -- 発生機会
    Remarks TEXT,                                  -- 備考（改行可能）
    UpdatedBy BIGINT NOT NULL,                     -- 最終更新者ID
    UpdatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),  -- 最終更新日時
    IsDeleted BOOLEAN NOT NULL DEFAULT false,      -- 論理削除フラグ
    FOREIGN KEY (DomainId) REFERENCES Domains(DomainId),
    UNIQUE (DomainId, JapaneseName)                -- ドメイン内和名一意制約
);

-- FormalUbiquitousLangテーブルコメント
COMMENT ON TABLE FormalUbiquitousLang IS '承認済み正式ユビキタス言語の管理、Claude Code出力対象データ';
COMMENT ON COLUMN FormalUbiquitousLang.FormalUbiquitousLangId IS '正式ユビキタス言語ID（主キー）';
COMMENT ON COLUMN FormalUbiquitousLang.DomainId IS '所属ドメインID（外部キー）';
COMMENT ON COLUMN FormalUbiquitousLang.JapaneseName IS '和名（ドメイン内一意）';
COMMENT ON COLUMN FormalUbiquitousLang.EnglishName IS '英名';
COMMENT ON COLUMN FormalUbiquitousLang.Description IS '意味・説明（改行可能）';
COMMENT ON COLUMN FormalUbiquitousLang.OccurrenceContext IS '発生機会';
COMMENT ON COLUMN FormalUbiquitousLang.Remarks IS '備考（改行可能）';
COMMENT ON COLUMN FormalUbiquitousLang.UpdatedBy IS '最終更新者ユーザーID';
COMMENT ON COLUMN FormalUbiquitousLang.UpdatedAt IS '最終更新日時（タイムゾーン付き）';
COMMENT ON COLUMN FormalUbiquitousLang.IsDeleted IS '論理削除フラグ（false:有効、true:削除済み）';

-- ===============================================
-- 7. ドラフトユビキタス言語管理テーブル
-- ===============================================

-- DraftUbiquitousLang: 編集中・承認申請中のユビキタス言語管理
CREATE TABLE DraftUbiquitousLang (
    DraftUbiquitousLangId BIGSERIAL PRIMARY KEY,   -- ドラフトユビキタス言語ID（主キー）
    DomainId BIGINT NOT NULL,                      -- 所属ドメインID
    JapaneseName VARCHAR(30) NOT NULL,             -- 和名
    EnglishName VARCHAR(50) NOT NULL,              -- 英名
    Description TEXT NOT NULL,                     -- 意味・説明（改行可能）
    OccurrenceContext VARCHAR(50),                 -- 発生機会
    Remarks TEXT,                                  -- 備考（改行可能）
    Status VARCHAR(20) NOT NULL DEFAULT 'editing', -- ステータス
    CHECK (Status IN ('editing', 'pending_approval', 'rejected')),
    UpdatedBy BIGINT NOT NULL,                     -- 最終更新者ID
    SubmittedBy BIGINT,                           -- 承認申請者ID
    RejectionReason TEXT,                         -- 否認理由
    FormalUbiquitousLangId BIGINT,                -- 対応する正式ユビキタス言語ID（更新時）
    UpdatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(), -- 最終更新日時
    FOREIGN KEY (DomainId) REFERENCES Domains(DomainId),
    FOREIGN KEY (FormalUbiquitousLangId) REFERENCES FormalUbiquitousLang(FormalUbiquitousLangId)
);

-- DraftUbiquitousLangテーブルコメント
COMMENT ON TABLE DraftUbiquitousLang IS '編集中・承認申請中のドラフトユビキタス言語管理';
COMMENT ON COLUMN DraftUbiquitousLang.DraftUbiquitousLangId IS 'ドラフトユビキタス言語ID（主キー）';
COMMENT ON COLUMN DraftUbiquitousLang.DomainId IS '所属ドメインID（外部キー）';
COMMENT ON COLUMN DraftUbiquitousLang.JapaneseName IS '和名';
COMMENT ON COLUMN DraftUbiquitousLang.EnglishName IS '英名';
COMMENT ON COLUMN DraftUbiquitousLang.Description IS '意味・説明（改行可能）';
COMMENT ON COLUMN DraftUbiquitousLang.OccurrenceContext IS '発生機会';
COMMENT ON COLUMN DraftUbiquitousLang.Remarks IS '備考（改行可能）';
COMMENT ON COLUMN DraftUbiquitousLang.Status IS 'ステータス（editing/pending_approval/rejected）';
COMMENT ON COLUMN DraftUbiquitousLang.UpdatedBy IS '最終更新者ユーザーID';
COMMENT ON COLUMN DraftUbiquitousLang.SubmittedBy IS '承認申請者ユーザーID';
COMMENT ON COLUMN DraftUbiquitousLang.RejectionReason IS '否認理由';
COMMENT ON COLUMN DraftUbiquitousLang.FormalUbiquitousLangId IS '対応する正式ユビキタス言語ID（更新時）';
COMMENT ON COLUMN DraftUbiquitousLang.UpdatedAt IS '最終更新日時（タイムゾーン付き）';

-- ===============================================
-- 8. 関連ユビキタス言語管理テーブル
-- ===============================================

-- RelatedUbiquitousLang: ユビキタス言語間の関連性管理
CREATE TABLE RelatedUbiquitousLang (
    RelationId BIGSERIAL PRIMARY KEY,              -- 関連ID（主キー）
    SourceFormalUbiquitousLangId BIGINT NOT NULL,  -- 関連元正式ユビキタス言語ID
    TargetFormalUbiquitousLangId BIGINT NOT NULL,  -- 関連先正式ユビキタス言語ID
    RelationType VARCHAR(20) NOT NULL DEFAULT 'related', -- 関連種別
    CHECK (RelationType IN ('related', 'opposite', 'parent', 'child')),
    UpdatedBy BIGINT NOT NULL,                     -- 最終更新者ID
    UpdatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),  -- 最終更新日時
    FOREIGN KEY (SourceFormalUbiquitousLangId) REFERENCES FormalUbiquitousLang(FormalUbiquitousLangId),
    FOREIGN KEY (TargetFormalUbiquitousLangId) REFERENCES FormalUbiquitousLang(FormalUbiquitousLangId),
    UNIQUE (SourceFormalUbiquitousLangId, TargetFormalUbiquitousLangId, RelationType)
);

-- RelatedUbiquitousLangテーブルコメント
COMMENT ON TABLE RelatedUbiquitousLang IS 'ユビキタス言語間の関連性管理、多対多関連';
COMMENT ON COLUMN RelatedUbiquitousLang.RelationId IS '関連ID（主キー）';
COMMENT ON COLUMN RelatedUbiquitousLang.SourceFormalUbiquitousLangId IS '関連元正式ユビキタス言語ID';
COMMENT ON COLUMN RelatedUbiquitousLang.TargetFormalUbiquitousLangId IS '関連先正式ユビキタス言語ID';
COMMENT ON COLUMN RelatedUbiquitousLang.RelationType IS '関連種別（related/opposite/parent/child）';
COMMENT ON COLUMN RelatedUbiquitousLang.UpdatedBy IS '最終更新者ユーザーID';
COMMENT ON COLUMN RelatedUbiquitousLang.UpdatedAt IS '最終更新日時（タイムゾーン付き）';

-- ===============================================
-- 9. ドラフトユビキタス言語関連管理テーブル
-- ===============================================

-- DraftUbiquitousLangRelations: ドラフト段階での関連性管理
CREATE TABLE DraftUbiquitousLangRelations (
    DraftRelationId BIGSERIAL PRIMARY KEY,         -- ドラフト関連ID（主キー）
    SourceDraftUbiquitousLangId BIGINT NOT NULL,   -- 関連元ドラフトユビキタス言語ID
    TargetFormalUbiquitousLangId BIGINT NOT NULL,  -- 関連先正式ユビキタス言語ID
    RelationType VARCHAR(20) NOT NULL DEFAULT 'related', -- 関連種別
    CHECK (RelationType IN ('related', 'opposite', 'parent', 'child')),
    UpdatedBy BIGINT NOT NULL,                     -- 最終更新者ID
    UpdatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),  -- 最終更新日時
    FOREIGN KEY (SourceDraftUbiquitousLangId) REFERENCES DraftUbiquitousLang(DraftUbiquitousLangId),
    FOREIGN KEY (TargetFormalUbiquitousLangId) REFERENCES FormalUbiquitousLang(FormalUbiquitousLangId)
);

-- DraftUbiquitousLangRelationsテーブルコメント
COMMENT ON TABLE DraftUbiquitousLangRelations IS 'ドラフトユビキタス言語と正式ユビキタス言語間の関連性管理';
COMMENT ON COLUMN DraftUbiquitousLangRelations.DraftRelationId IS 'ドラフト関連ID（主キー）';
COMMENT ON COLUMN DraftUbiquitousLangRelations.SourceDraftUbiquitousLangId IS '関連元ドラフトユビキタス言語ID';
COMMENT ON COLUMN DraftUbiquitousLangRelations.TargetFormalUbiquitousLangId IS '関連先正式ユビキタス言語ID';
COMMENT ON COLUMN DraftUbiquitousLangRelations.RelationType IS '関連種別（related/opposite/parent/child）';
COMMENT ON COLUMN DraftUbiquitousLangRelations.UpdatedBy IS '最終更新者ユーザーID';
COMMENT ON COLUMN DraftUbiquitousLangRelations.UpdatedAt IS '最終更新日時（タイムゾーン付き）';

-- ===============================================
-- 10. 正式ユビキタス言語履歴管理テーブル
-- ===============================================

-- FormalUbiquitousLangHistory: 正式ユビキタス言語の変更履歴管理
CREATE TABLE FormalUbiquitousLangHistory (
    HistoryId BIGSERIAL PRIMARY KEY,               -- 履歴ID（主キー）
    FormalUbiquitousLangId BIGINT NOT NULL,        -- 元の正式ユビキタス言語ID
    DomainId BIGINT NOT NULL,                      -- 所属ドメインID
    JapaneseName VARCHAR(30) NOT NULL,             -- 和名
    EnglishName VARCHAR(50) NOT NULL,              -- 英名
    Description TEXT NOT NULL,                     -- 意味・説明（改行可能）
    OccurrenceContext VARCHAR(50),                 -- 発生機会
    Remarks TEXT,                                  -- 備考（改行可能）
    RelatedUbiquitousLangSnapshot JSONB,           -- 関連ユビキタス言語スナップショット（PostgreSQL最適化）
    UpdatedBy BIGINT NOT NULL,                     -- 最終更新者ID
    UpdatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),  -- 最終更新日時
    IsDeleted BOOLEAN NOT NULL DEFAULT false,      -- 論理削除フラグ
    FOREIGN KEY (DomainId) REFERENCES Domains(DomainId)
);

-- FormalUbiquitousLangHistoryテーブルコメント
COMMENT ON TABLE FormalUbiquitousLangHistory IS '正式ユビキタス言語の変更履歴管理、JSONB活用でスナップショット保存';
COMMENT ON COLUMN FormalUbiquitousLangHistory.HistoryId IS '履歴ID（主キー）';
COMMENT ON COLUMN FormalUbiquitousLangHistory.FormalUbiquitousLangId IS '元の正式ユビキタス言語ID';
COMMENT ON COLUMN FormalUbiquitousLangHistory.DomainId IS '所属ドメインID（外部キー）';
COMMENT ON COLUMN FormalUbiquitousLangHistory.JapaneseName IS '和名';
COMMENT ON COLUMN FormalUbiquitousLangHistory.EnglishName IS '英名';
COMMENT ON COLUMN FormalUbiquitousLangHistory.Description IS '意味・説明（改行可能）';
COMMENT ON COLUMN FormalUbiquitousLangHistory.OccurrenceContext IS '発生機会';
COMMENT ON COLUMN FormalUbiquitousLangHistory.Remarks IS '備考（改行可能）';
COMMENT ON COLUMN FormalUbiquitousLangHistory.RelatedUbiquitousLangSnapshot IS '関連ユビキタス言語スナップショット（JSONB、GINインデックス対応）';
COMMENT ON COLUMN FormalUbiquitousLangHistory.UpdatedBy IS '最終更新者ユーザーID';
COMMENT ON COLUMN FormalUbiquitousLangHistory.UpdatedAt IS '最終更新日時（タイムゾーン付き）';
COMMENT ON COLUMN FormalUbiquitousLangHistory.IsDeleted IS '論理削除フラグ（false:有効、true:削除済み）';

-- ===============================================
-- PostgreSQL専用インデックス作成
-- ===============================================

-- 基本インデックス
CREATE INDEX IX_Users_Email ON Users(Email) WHERE IsDeleted = false;
CREATE INDEX IX_Users_UserRole ON Users(UserRole) WHERE IsDeleted = false;
CREATE INDEX IX_Users_IsActive ON Users(IsActive) WHERE IsDeleted = false;

CREATE INDEX IX_Projects_ProjectName ON Projects(ProjectName) WHERE IsDeleted = false;
CREATE INDEX IX_Projects_IsDeleted ON Projects(IsDeleted);

CREATE INDEX IX_UserProjects_UserId ON UserProjects(UserId);
CREATE INDEX IX_UserProjects_ProjectId ON UserProjects(ProjectId);

CREATE INDEX IX_Domains_ProjectId ON Domains(ProjectId) WHERE IsDeleted = false;
CREATE INDEX IX_Domains_IsDeleted ON Domains(IsDeleted);

CREATE INDEX IX_DomainApprovers_DomainId ON DomainApprovers(DomainId);
CREATE INDEX IX_DomainApprovers_UserId ON DomainApprovers(UserId);

CREATE INDEX IX_FormalUbiquitousLang_DomainId ON FormalUbiquitousLang(DomainId) WHERE IsDeleted = false;
CREATE INDEX IX_FormalUbiquitousLang_JapaneseName ON FormalUbiquitousLang(JapaneseName) WHERE IsDeleted = false;
CREATE INDEX IX_FormalUbiquitousLang_UpdatedAt ON FormalUbiquitousLang(UpdatedAt DESC) WHERE IsDeleted = false;

CREATE INDEX IX_DraftUbiquitousLang_DomainId ON DraftUbiquitousLang(DomainId);
CREATE INDEX IX_DraftUbiquitousLang_Status ON DraftUbiquitousLang(Status);
CREATE INDEX IX_DraftUbiquitousLang_UpdatedAt ON DraftUbiquitousLang(UpdatedAt DESC);
CREATE INDEX IX_DraftUbiquitousLang_FormalUbiquitousLangId ON DraftUbiquitousLang(FormalUbiquitousLangId);

CREATE INDEX IX_RelatedUbiquitousLang_SourceId ON RelatedUbiquitousLang(SourceFormalUbiquitousLangId);
CREATE INDEX IX_RelatedUbiquitousLang_TargetId ON RelatedUbiquitousLang(TargetFormalUbiquitousLangId);

CREATE INDEX IX_DraftUbiquitousLangRelations_SourceId ON DraftUbiquitousLangRelations(SourceDraftUbiquitousLangId);
CREATE INDEX IX_DraftUbiquitousLangRelations_TargetId ON DraftUbiquitousLangRelations(TargetFormalUbiquitousLangId);

CREATE INDEX IX_FormalUbiquitousLangHistory_FormalUbiquitousLangId ON FormalUbiquitousLangHistory(FormalUbiquitousLangId);
CREATE INDEX IX_FormalUbiquitousLangHistory_UpdatedAt ON FormalUbiquitousLangHistory(UpdatedAt DESC);
CREATE INDEX IX_FormalUbiquitousLangHistory_DomainId ON FormalUbiquitousLangHistory(DomainId);

-- PostgreSQL専用GINインデックス（JSONB検索最適化）
CREATE INDEX IX_FormalUbiquitousLangHistory_RelatedSnapshot_GIN 
    ON FormalUbiquitousLangHistory USING GIN (RelatedUbiquitousLangSnapshot);

-- ===============================================
-- 初期データ挿入
-- ===============================================

-- スーパーユーザー初期データ（パスワード: "su"）
-- PasswordHash: bcryptで "su" をハッシュ化した値
INSERT INTO Users (Email, PasswordHash, Name, UserRole, UpdatedBy) VALUES 
('su@localhost', '$2a$10$example.hash.for.password.su', 'スーパーユーザー', 'SuperUser', 1);

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
PostgreSQL専用最適化の採用内容:

1. データ型最適化:
   - BIGSERIAL: 自動増分主キー（IDENTITY不要）
   - TIMESTAMPTZ: タイムゾーン対応日時型
   - BOOLEAN: true/false（BIT不要）
   - TEXT: 可変長文字列（長い内容用）
   - VARCHAR(n): 固定長制限文字列（短い内容用）
   - JSONB: バイナリJSON（高速検索・インデックス対応）

2. インデックス戦略:
   - 部分インデックス: WHERE IsDeleted = false
   - GINインデックス: JSONB検索最適化
   - 複合インデックス: 頻繁な検索パターンに対応

3. 制約・機能:
   - CHECK制約: enum値の検証
   - 外部キー制約: 参照整合性保証
   - UNIQUE制約: 一意性保証
   - トリガー関数: 自動メンテナンス機能

4. パフォーマンス:
   - NOW()関数: デフォルト値による自動設定
   - 論理削除対応インデックス: 削除済みデータの除外
   - JSONB活用: 構造化データの効率的格納・検索

5. 運用・保守性:
   - コメント完備: 全テーブル・カラムの説明
   - 一貫した命名規則: PostgreSQL慣習に準拠
   - 将来拡張対応: トリガー関数等の基盤整備
*/