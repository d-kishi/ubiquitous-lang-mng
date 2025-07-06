-- ===============================================
-- ユビキタス言語管理システム 初期データ投入
-- 
-- 作成日: 2025-07-06
-- 目的: 開発・テスト用初期データセットアップ
-- ===============================================

-- ===============================================
-- 1. 初期スーパーユーザー作成
-- ===============================================

-- スーパーユーザー（admin）
INSERT INTO Users (
    Email, 
    PasswordHash, 
    Name, 
    UserRole, 
    IsActive, 
    IsFirstLogin, 
    UpdatedBy
) VALUES (
    'admin@ubiquitous-lang.com',
    '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LeOmcfqxn9TBOzTGS', -- password: admin123
    'システム管理者',
    'SuperUser',
    true,
    true,
    1
);

-- ===============================================
-- 2. サンプルユーザー作成
-- ===============================================

-- プロジェクト管理者
INSERT INTO Users (
    Email, 
    PasswordHash, 
    Name, 
    UserRole, 
    IsActive, 
    IsFirstLogin, 
    UpdatedBy
) VALUES (
    'project.manager@ubiquitous-lang.com',
    '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LeOmcfqxn9TBOzTGS', -- password: admin123
    'プロジェクト管理者',
    'ProjectManager',
    true,
    true,
    1
);

-- ドメイン承認者
INSERT INTO Users (
    Email, 
    PasswordHash, 
    Name, 
    UserRole, 
    IsActive, 
    IsFirstLogin, 
    UpdatedBy
) VALUES (
    'domain.approver@ubiquitous-lang.com',
    '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LeOmcfqxn9TBOzTGS', -- password: admin123
    'ドメイン承認者',
    'DomainApprover',
    true,
    true,
    1
);

-- 一般ユーザー
INSERT INTO Users (
    Email, 
    PasswordHash, 
    Name, 
    UserRole, 
    IsActive, 
    IsFirstLogin, 
    UpdatedBy
) VALUES (
    'general.user@ubiquitous-lang.com',
    '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LeOmcfqxn9TBOzTGS', -- password: admin123
    '一般ユーザー',
    'GeneralUser',
    true,
    true,
    1
);

-- ===============================================
-- 3. サンプルプロジェクト作成
-- ===============================================

-- ECサイト構築プロジェクト
INSERT INTO Projects (
    ProjectName,
    ProjectDescription,
    ProjectManager,
    StartDate,
    EndDate,
    ProjectStatus,
    UpdatedBy
) VALUES (
    'ECサイト構築プロジェクト',
    'オンライン販売システムの構築プロジェクト',
    2, -- project.manager@ubiquitous-lang.com
    '2025-01-01',
    '2025-12-31',
    'Active',
    1
);

-- 顧客管理システム
INSERT INTO Projects (
    ProjectName,
    ProjectDescription,
    ProjectManager,
    StartDate,
    EndDate,
    ProjectStatus,
    UpdatedBy
) VALUES (
    '顧客管理システム',
    'CRM機能を持つ顧客管理システムの開発',
    2, -- project.manager@ubiquitous-lang.com
    '2025-02-01',
    '2025-11-30',
    'Active',
    1
);

-- ===============================================
-- 4. サンプルドメイン作成
-- ===============================================

-- ECサイト - 商品管理ドメイン
INSERT INTO Domains (
    ProjectId,
    DomainName,
    DomainDescription,
    DomainApprover,
    DomainStatus,
    UpdatedBy
) VALUES (
    1, -- ECサイト構築プロジェクト
    '商品管理',
    '商品カタログ、在庫管理、価格設定に関するドメイン',
    3, -- domain.approver@ubiquitous-lang.com
    'Active',
    1
);

-- ECサイト - 注文管理ドメイン
INSERT INTO Domains (
    ProjectId,
    DomainName,
    DomainDescription,
    DomainApprover,
    DomainStatus,
    UpdatedBy
) VALUES (
    1, -- ECサイト構築プロジェクト
    '注文管理',
    '注文処理、決済、配送に関するドメイン',
    3, -- domain.approver@ubiquitous-lang.com
    'Active',
    1
);

-- 顧客管理システム - 顧客情報ドメイン
INSERT INTO Domains (
    ProjectId,
    DomainName,
    DomainDescription,
    DomainApprover,
    DomainStatus,
    UpdatedBy
) VALUES (
    2, -- 顧客管理システム
    '顧客情報管理',
    '顧客の基本情報、連絡先、履歴管理に関するドメイン',
    3, -- domain.approver@ubiquitous-lang.com
    'Active',
    1
);

-- ===============================================
-- 5. ユーザー・プロジェクト関連設定
-- ===============================================

-- プロジェクト管理者をプロジェクトに割り当て
INSERT INTO UserProjects (
    UserId,
    ProjectId,
    Role,
    UpdatedBy
) VALUES 
(2, 1, 'ProjectManager', 1), -- ECサイト構築プロジェクト
(2, 2, 'ProjectManager', 1); -- 顧客管理システム

-- ドメイン承認者をプロジェクトに割り当て
INSERT INTO UserProjects (
    UserId,
    ProjectId,
    Role,
    UpdatedBy
) VALUES 
(3, 1, 'DomainApprover', 1), -- ECサイト構築プロジェクト
(3, 2, 'DomainApprover', 1); -- 顧客管理システム

-- 一般ユーザーをプロジェクトに割り当て
INSERT INTO UserProjects (
    UserId,
    ProjectId,
    Role,
    UpdatedBy
) VALUES 
(4, 1, 'GeneralUser', 1), -- ECサイト構築プロジェクト
(4, 2, 'GeneralUser', 1); -- 顧客管理システム

-- ===============================================
-- 6. ドメイン承認者設定
-- ===============================================

-- ドメイン承認者をドメインに割り当て
INSERT INTO DomainApprovers (
    DomainId,
    UserId,
    UpdatedBy
) VALUES 
(1, 3, 1), -- 商品管理ドメイン
(2, 3, 1), -- 注文管理ドメイン
(3, 3, 1); -- 顧客情報管理ドメイン

-- ===============================================
-- 7. サンプルドラフトユビキタス言語作成
-- ===============================================

-- 商品管理ドメイン - ドラフト用語
INSERT INTO DraftUbiquitousLang (
    DomainId,
    Term,
    Definition,
    Context,
    ExampleUsage,
    RelatedConcepts,
    Notes,
    Status,
    CreatedBy,
    UpdatedBy
) VALUES 
(1, '商品', '販売対象となる物品またはサービス', 'ECサイトにおける販売アイテム', '「この商品は在庫切れです」', 'SKU、在庫、価格', 'デジタル商品と物理商品を区別する', 'Draft', 4, 4),
(1, 'SKU', '商品の識別コード（Stock Keeping Unit）', '在庫管理と商品識別', '「SKU: ABC-123-XYZ」', '商品、在庫、バリエーション', 'システム内で一意である必要がある', 'Draft', 4, 4),
(1, '在庫', '販売可能な商品の数量', '在庫管理システム', '「在庫数：50個」', '商品、SKU、入荷、出荷', '予約在庫と実在庫を区別', 'Draft', 4, 4);

-- 注文管理ドメイン - ドラフト用語
INSERT INTO DraftUbiquitousLang (
    DomainId,
    Term,
    Definition,
    Context,
    ExampleUsage,
    RelatedConcepts,
    Notes,
    Status,
    CreatedBy,
    UpdatedBy
) VALUES 
(2, '注文', '顧客による商品の購入依頼', '注文処理システム', '「注文番号：ORD-2025-001」', '顧客、商品、決済', '注文確定後のキャンセル処理を含む', 'Draft', 4, 4),
(2, 'カート', '注文前の商品一時保管領域', 'オンラインショッピング', '「カートに追加」', '商品、セッション、注文', 'セッション終了時の保持期間を定義', 'Draft', 4, 4);

-- ===============================================
-- 8. サンプル正式ユビキタス言語作成
-- ===============================================

-- 承認済み用語の例
INSERT INTO FormalUbiquitousLang (
    DomainId,
    Term,
    Definition,
    Context,
    ExampleUsage,
    RelatedConcepts,
    Notes,
    ApprovedBy,
    SourceDraftId,
    UpdatedBy
) VALUES 
(3, '顧客', 'サービスを利用する個人または法人', '顧客管理システム', '「顧客ID：CUST-001」', '会員、ユーザー、取引先', '見込み客と既存客を区別', 3, null, 3),
(3, 'リード', '見込み客となる可能性のある潜在顧客', '営業管理', '「リード獲得数：100件」', '顧客、見込み客、商談', 'マーケティング活動で獲得', 3, null, 3);

-- ===============================================
-- 9. 関連ユビキタス言語設定
-- ===============================================

-- 正式用語間の関連性
INSERT INTO RelatedUbiquitousLang (
    SourceFormalUbiquitousLangId,
    TargetFormalUbiquitousLangId,
    RelationType,
    RelationDescription,
    UpdatedBy
) VALUES 
(1, 2, '発展関係', 'リードが顧客に発展する可能性がある', 3);

-- ===============================================
-- 10. 完了メッセージ
-- ===============================================

DO $$
BEGIN
    RAISE NOTICE 'ユビキタス言語管理システム 初期データ投入完了';
    RAISE NOTICE '作成ユーザー数: 4';
    RAISE NOTICE '作成プロジェクト数: 2';
    RAISE NOTICE '作成ドメイン数: 3';
    RAISE NOTICE '作成ドラフト用語数: 5';
    RAISE NOTICE '作成正式用語数: 2';
    RAISE NOTICE 'デフォルトパスワード: admin123';
END $$;