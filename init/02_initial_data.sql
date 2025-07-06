-- ===============================================
-- ユビキタス言語管理システム 初期データ投入
-- 
-- 作成日: 2025-07-06
-- 最終更新: 2025-07-06
-- 目的: 開発・テスト用初期データセットアップ
-- 用語統一: ADR_003準拠（UbiquitousLang表記）
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
    Description,
    UpdatedBy
) VALUES (
    'ECサイト構築プロジェクト',
    'オンライン販売システムの構築プロジェクト',
    1
);

-- 顧客管理システム
INSERT INTO Projects (
    ProjectName,
    Description,
    UpdatedBy
) VALUES (
    '顧客管理システム',
    'CRM機能を持つ顧客管理システムの開発',
    1
);

-- ===============================================
-- 4. サンプルドメイン作成
-- ===============================================

-- ECサイト - 商品管理ドメイン
INSERT INTO Domains (
    ProjectId,
    DomainName,
    Description,
    UpdatedBy
) VALUES (
    1, -- ECサイト構築プロジェクト
    '商品管理',
    '商品カタログ、在庫管理、価格設定に関するドメイン',
    1
);

-- ECサイト - 注文管理ドメイン
INSERT INTO Domains (
    ProjectId,
    DomainName,
    Description,
    UpdatedBy
) VALUES (
    1, -- ECサイト構築プロジェクト
    '注文管理',
    '注文処理、決済、配送に関するドメイン',
    1
);

-- 顧客管理システム - 顧客情報ドメイン
INSERT INTO Domains (
    ProjectId,
    DomainName,
    Description,
    UpdatedBy
) VALUES (
    2, -- 顧客管理システム
    '顧客情報管理',
    '顧客の基本情報、連絡先、履歴管理に関するドメイン',
    1
);

-- ===============================================
-- 5. ユーザー・プロジェクト関連設定
-- ===============================================

-- プロジェクト管理者をプロジェクトに割り当て
INSERT INTO UserProjects (
    UserId,
    ProjectId,
    UpdatedBy
) VALUES 
(2, 1, 1), -- ECサイト構築プロジェクト
(2, 2, 1); -- 顧客管理システム

-- ドメイン承認者をプロジェクトに割り当て
INSERT INTO UserProjects (
    UserId,
    ProjectId,
    UpdatedBy
) VALUES 
(3, 1, 1), -- ECサイト構築プロジェクト
(3, 2, 1); -- 顧客管理システム

-- 一般ユーザーをプロジェクトに割り当て
INSERT INTO UserProjects (
    UserId,
    ProjectId,
    UpdatedBy
) VALUES 
(4, 1, 1), -- ECサイト構築プロジェクト
(4, 2, 1); -- 顧客管理システム

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
-- 7. サンプルドラフトユビキタス言語作成（ADR_003準拠）
-- ===============================================

-- 商品管理ドメイン - ドラフトユビキタス言語
INSERT INTO DraftUbiquitousLang (
    DomainId,
    JapaneseName,
    EnglishName,
    Description,
    OccurrenceContext,
    Remarks,
    Status,
    UpdatedBy
) VALUES 
(1, '商品', 'Product', '販売対象となる物品またはサービス', 'ECサイトにおける販売アイテム', 'デジタル商品と物理商品を区別する', 'editing', 4),
(1, 'SKU', 'StockKeepingUnit', '商品の識別コード（Stock Keeping Unit）', '在庫管理と商品識別', 'システム内で一意である必要がある', 'editing', 4),
(1, '在庫', 'Inventory', '販売可能な商品の数量', '在庫管理システム', '予約在庫と実在庫を区別', 'editing', 4);

-- 注文管理ドメイン - ドラフトユビキタス言語
INSERT INTO DraftUbiquitousLang (
    DomainId,
    JapaneseName,
    EnglishName,
    Description,
    OccurrenceContext,
    Remarks,
    Status,
    UpdatedBy
) VALUES 
(2, '注文', 'Order', '顧客による商品の購入依頼', '注文処理システム', '注文確定後のキャンセル処理を含む', 'editing', 4),
(2, 'カート', 'ShoppingCart', '注文前の商品一時保管領域', 'オンラインショッピング', 'セッション終了時の保持期間を定義', 'editing', 4);

-- ===============================================
-- 8. サンプル正式ユビキタス言語作成（ADR_003準拠）
-- ===============================================

-- 承認済みユビキタス言語の例
INSERT INTO FormalUbiquitousLang (
    DomainId,
    JapaneseName,
    EnglishName,
    Description,
    OccurrenceContext,
    Remarks,
    UpdatedBy
) VALUES 
(3, '顧客', 'Customer', 'サービスを利用する個人または法人', '顧客管理システム', '見込み客と既存客を区別', 1),
(3, 'リード', 'Lead', '見込み客となる可能性のある潜在顧客', '営業管理', 'マーケティング活動で獲得', 1);

-- ===============================================
-- 9. 関連ユビキタス言語設定（ADR_003準拠）
-- ===============================================

-- 正式ユビキタス言語間の関連性
INSERT INTO RelatedUbiquitousLang (
    SourceFormalUbiquitousLangId,
    TargetFormalUbiquitousLangId,
    RelationType,
    UpdatedBy
) VALUES 
(2, 1, 'child', 1); -- リードが顧客の子概念

-- ===============================================
-- 10. 完了メッセージ
-- ===============================================

DO $$
BEGIN
    RAISE NOTICE 'ユビキタス言語管理システム 初期データ投入完了';
    RAISE NOTICE '作成ユーザー数: 4';
    RAISE NOTICE '作成プロジェクト数: 2';
    RAISE NOTICE '作成ドメイン数: 3';
    RAISE NOTICE '作成ドラフトユビキタス言語数: 5';
    RAISE NOTICE '作成正式ユビキタス言語数: 2';
    RAISE NOTICE 'デフォルトパスワード: admin123';
    RAISE NOTICE '用語統一: ADR_003準拠（UbiquitousLang表記）';
END $$;