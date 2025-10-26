-- ===============================================
-- ユビキタス言語管理システム 初期データ投入
-- 
-- 作成日: 2025-07-06
-- 最終更新: 2025-07-16
-- 認証: ASP.NET Core Identity対応
-- 目的: 開発・テスト用初期データセットアップ
-- 用語統一: ADR_003準拠（UbiquitousLang表記）
-- ===============================================

-- ===============================================
-- 1. 初期ユーザー作成（ASP.NET Core Identity）
-- ===============================================

-- システム管理者（SuperUser）
INSERT INTO "AspNetUsers" (
    "Id",
    "UserName",
    "NormalizedUserName",
    "Email",
    "NormalizedEmail",
    "EmailConfirmed",
    "PasswordHash",
    "SecurityStamp",
    "ConcurrencyStamp",
    "Name",
    "IsFirstLogin",
    "InitialPassword",
    "UpdatedBy",
    "UpdatedAt"
) VALUES (
    'admin-001', 
    'admin@ubiquitous-lang.com', 
    'ADMIN@UBIQUITOUS-LANG.COM', 
    'admin@ubiquitous-lang.com', 
    'ADMIN@UBIQUITOUS-LANG.COM', 
    true, 
    NULL, -- 初期パスワード平文管理仕様準拠（機能仕様書2.2.1）
    RANDOM()::TEXT, 
    RANDOM()::TEXT, 
    'システム管理者', 
    true, 
    'su', -- 機能仕様書2.0.1準拠：固定初期パスワード
    'admin-001', 
    NOW()
);

-- プロジェクト管理者（ProjectManager）
INSERT INTO "AspNetUsers" (
    "Id",
    "UserName",
    "NormalizedUserName",
    "Email",
    "NormalizedEmail",
    "EmailConfirmed",
    "PasswordHash",
    "SecurityStamp",
    "ConcurrencyStamp",
    "Name",
    "IsFirstLogin",
    "InitialPassword",
    "UpdatedBy",
    "UpdatedAt"
) VALUES (
    'pm-001', 
    'project.manager@ubiquitous-lang.com', 
    'PROJECT.MANAGER@UBIQUITOUS-LANG.COM', 
    'project.manager@ubiquitous-lang.com', 
    'PROJECT.MANAGER@UBIQUITOUS-LANG.COM', 
    true, 
    NULL, -- 初期パスワード平文管理仕様準拠（機能仕様書2.2.1）
    RANDOM()::TEXT, 
    RANDOM()::TEXT, 
    'プロジェクト管理者', 
    true, 
    'su', -- 機能仕様書2.0.1準拠：固定初期パスワード
    'admin-001', 
    NOW()
);

-- ドメイン承認者（DomainApprover）
INSERT INTO "AspNetUsers" (
    "Id",
    "UserName",
    "NormalizedUserName",
    "Email",
    "NormalizedEmail",
    "EmailConfirmed",
    "PasswordHash",
    "SecurityStamp",
    "ConcurrencyStamp",
    "Name",
    "IsFirstLogin",
    "InitialPassword",
    "UpdatedBy",
    "UpdatedAt"
) VALUES (
    'da-001', 
    'domain.approver@ubiquitous-lang.com', 
    'DOMAIN.APPROVER@UBIQUITOUS-LANG.COM', 
    'domain.approver@ubiquitous-lang.com', 
    'DOMAIN.APPROVER@UBIQUITOUS-LANG.COM', 
    true, 
    NULL, -- 初期パスワード平文管理仕様準拠（機能仕様書2.2.1）
    RANDOM()::TEXT, 
    RANDOM()::TEXT, 
    'ドメイン承認者', 
    true, 
    'su', -- 機能仕様書2.0.1準拠：固定初期パスワード
    'admin-001', 
    NOW()
);

-- 一般ユーザー（GeneralUser）
INSERT INTO "AspNetUsers" (
    "Id",
    "UserName",
    "NormalizedUserName",
    "Email",
    "NormalizedEmail",
    "EmailConfirmed",
    "PasswordHash",
    "SecurityStamp",
    "ConcurrencyStamp",
    "Name",
    "IsFirstLogin",
    "InitialPassword",
    "UpdatedBy",
    "UpdatedAt"
) VALUES (
    'gu-001', 
    'general.user@ubiquitous-lang.com', 
    'GENERAL.USER@UBIQUITOUS-LANG.COM', 
    'general.user@ubiquitous-lang.com', 
    'GENERAL.USER@UBIQUITOUS-LANG.COM', 
    true, 
    NULL, -- 初期パスワード平文管理仕様準拠（機能仕様書2.2.1）
    RANDOM()::TEXT, 
    RANDOM()::TEXT, 
    '一般ユーザー', 
    true, 
    'su', -- 機能仕様書2.0.1準拠：固定初期パスワード
    'admin-001', 
    NOW()
);

-- ===============================================
-- 2. ユーザー・ロール関連設定
-- ===============================================

-- システム管理者にSuperUserロール付与
INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
VALUES ('admin-001', 'super-user');

-- プロジェクト管理者にProjectManagerロール付与
INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
VALUES ('pm-001', 'project-manager');

-- ドメイン承認者にDomainApproverロール付与
INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
VALUES ('da-001', 'domain-approver');

-- 一般ユーザーにGeneralUserロール付与
INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
VALUES ('gu-001', 'general-user');

-- ===============================================
-- 3. サンプルプロジェクト作成
-- ===============================================

-- ECサイト構築プロジェクト
INSERT INTO "Projects" (
    "ProjectName",
    "Description",
    "UpdatedBy"
) VALUES (
    'ECサイト構築プロジェクト',
    'オンライン販売システムの構築プロジェクト',
    'admin-001'
);

-- 顧客管理システム
INSERT INTO "Projects" (
    "ProjectName",
    "Description",
    "UpdatedBy"
) VALUES (
    '顧客管理システム',
    'CRM機能を持つ顧客管理システムの開発',
    'admin-001'
);

-- ===============================================
-- 4. サンプルドメイン作成
-- ===============================================

-- ECサイト - 商品管理ドメイン
INSERT INTO "Domains" (
    "ProjectId",
    "DomainName",
    "Description",
    "UpdatedBy"
) VALUES (
    1, -- ECサイト構築プロジェクト
    '商品管理',
    '商品カタログ、在庫管理、価格設定に関するドメイン',
    'admin-001'
);

-- ECサイト - 注文管理ドメイン
INSERT INTO "Domains" (
    "ProjectId",
    "DomainName",
    "Description",
    "UpdatedBy"
) VALUES (
    1, -- ECサイト構築プロジェクト
    '注文管理',
    '注文処理、決済、配送に関するドメイン',
    'admin-001'
);

-- 顧客管理システム - 顧客情報ドメイン
INSERT INTO "Domains" (
    "ProjectId",
    "DomainName",
    "Description",
    "UpdatedBy"
) VALUES (
    2, -- 顧客管理システム
    '顧客情報管理',
    '顧客の基本情報、連絡先、履歴管理に関するドメイン',
    'admin-001'
);

-- ===============================================
-- 5. ユーザー・プロジェクト関連設定
-- ===============================================

-- プロジェクト管理者をプロジェクトに割り当て
INSERT INTO "UserProjects" (
    "UserId",
    "ProjectId",
    "UpdatedBy"
) VALUES 
('pm-001', 1, 'admin-001'), -- ECサイト構築プロジェクト
('pm-001', 2, 'admin-001'); -- 顧客管理システム

-- ドメイン承認者をプロジェクトに割り当て
INSERT INTO "UserProjects" (
    "UserId",
    "ProjectId",
    "UpdatedBy"
) VALUES 
('da-001', 1, 'admin-001'), -- ECサイト構築プロジェクト
('da-001', 2, 'admin-001'); -- 顧客管理システム

-- 一般ユーザーをプロジェクトに割り当て
INSERT INTO "UserProjects" (
    "UserId",
    "ProjectId",
    "UpdatedBy"
) VALUES 
('gu-001', 1, 'admin-001'), -- ECサイト構築プロジェクト
('gu-001', 2, 'admin-001'); -- 顧客管理システム

-- ===============================================
-- 6. ドメイン承認者設定
-- ===============================================

-- ドメイン承認者をドメインに割り当て
INSERT INTO "DomainApprovers" (
    "DomainId",
    "ApproverId",
    "UpdatedBy"
) VALUES 
(1, 'da-001', 'admin-001'), -- 商品管理ドメイン
(2, 'da-001', 'admin-001'), -- 注文管理ドメイン
(3, 'da-001', 'admin-001'); -- 顧客情報管理ドメイン

-- ===============================================
-- 7. サンプルドラフトユビキタス言語作成（ADR_003準拠）
-- ===============================================

-- 商品管理ドメイン - ドラフトユビキタス言語
INSERT INTO "DraftUbiquitousLang" (
    "DomainId",
    "JapaneseName",
    "EnglishName",
    "Description",
    "OccurrenceContext",
    "Remarks",
    "Status",
    "UpdatedBy"
) VALUES 
(1, '商品', 'Product', '販売対象となる物品またはサービス', 'ECサイトにおける販売アイテム', 'デジタル商品と物理商品を区別する', 'Draft', 'gu-001'),
(1, 'SKU', 'StockKeepingUnit', '商品の識別コード（Stock Keeping Unit）', '在庫管理と商品識別', 'システム内で一意である必要がある', 'Draft', 'gu-001'),
(1, '在庫', 'Inventory', '販売可能な商品の数量', '在庫管理システム', '予約在庫と実在庫を区別', 'Draft', 'gu-001');

-- 注文管理ドメイン - ドラフトユビキタス言語
INSERT INTO "DraftUbiquitousLang" (
    "DomainId",
    "JapaneseName",
    "EnglishName",
    "Description",
    "OccurrenceContext",
    "Remarks",
    "Status",
    "UpdatedBy"
) VALUES 
(2, '注文', 'Order', '顧客による商品の購入依頼', '注文処理システム', '注文確定後のキャンセル処理を含む', 'Draft', 'gu-001'),
(2, 'カート', 'ShoppingCart', '注文前の商品一時保管領域', 'オンラインショッピング', 'セッション終了時の保持期間を定義', 'Draft', 'gu-001');

-- ===============================================
-- 8. サンプル正式ユビキタス言語作成（ADR_003準拠）
-- ===============================================

-- 承認済みユビキタス言語の例
INSERT INTO "FormalUbiquitousLang" (
    "DomainId",
    "JapaneseName",
    "EnglishName",
    "Description",
    "OccurrenceContext",
    "Remarks",
    "UpdatedBy"
) VALUES 
(3, '顧客', 'Customer', 'サービスを利用する個人または法人', '顧客管理システム', '見込み客と既存客を区別', 'admin-001'),
(3, 'リード', 'Lead', '見込み客となる可能性のある潜在顧客', '営業管理', 'マーケティング活動で獲得', 'admin-001');

-- ===============================================
-- 9. 関連ユビキタス言語設定（ADR_003準拠）
-- ===============================================

-- 正式ユビキタス言語間の関連性
INSERT INTO "RelatedUbiquitousLang" (
    "SourceUbiquitousLangId",
    "TargetUbiquitousLangId",
    "UpdatedBy"
) VALUES 
(2, 1, 'admin-001'); -- リードが顧客に関連

-- ===============================================
-- 10. 完了メッセージ
-- ===============================================

DO $$
BEGIN
    RAISE NOTICE 'ユビキタス言語管理システム 初期データ投入完了';
    RAISE NOTICE '作成ユーザー数: 4';
    RAISE NOTICE '作成ロール数: 4';
    RAISE NOTICE '作成プロジェクト数: 2';
    RAISE NOTICE '作成ドメイン数: 3';
    RAISE NOTICE '作成ドラフトユビキタス言語数: 5';
    RAISE NOTICE '作成正式ユビキタス言語数: 2';
    RAISE NOTICE 'デフォルトパスワード: su（機能仕様書2.0.1準拠）';
    RAISE NOTICE '認証システム: ASP.NET Core Identity';
    RAISE NOTICE '用語統一: ADR_003準拠（UbiquitousLang表記）';
    RAISE NOTICE '初期パスワード管理: US-005準拠（InitialPassword保存）';
END $$;
