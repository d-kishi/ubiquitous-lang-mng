-- E2Eテスト後のadmin@ubiquitous-lang.comユーザー状態復元スクリプト
-- 初期ログイン状態（IsFirstLogin=true, InitialPassword='su', PasswordHash=NULL）に復元
-- 実行方法: docker exec ubiquitous-lang-postgres psql -U ubiquitous_lang_user -d ubiquitous_lang_db -f /scripts/restore-admin-user.sql

-- 現在のadmin@ubiquitous-lang.comを削除
DELETE FROM "AspNetUsers" WHERE "Email" = 'admin@ubiquitous-lang.com';

-- 初期状態の固定値で復元（初回ログイン状態）
INSERT INTO "AspNetUsers" (
    "Id", "Name", "IsFirstLogin", "UpdatedAt", "IsDeleted", 
    "InitialPassword", "PasswordResetToken", "PasswordResetExpiry", "UpdatedBy",
    "UserName", "NormalizedUserName", "Email", "NormalizedEmail", 
    "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
    "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", 
    "LockoutEnd", "LockoutEnabled", "AccessFailedCount"
) VALUES (
    '542c11f0-646a-4a8a-9809-2462174d0516',
    'システム管理者',
    true,
    '2025-09-09 12:55:35.649685+00',
    false,
    'su',
    null,
    null,
    'System',
    'admin@ubiquitous-lang.com',
    'ADMIN@UBIQUITOUS-LANG.COM',
    'admin@ubiquitous-lang.com', 
    'ADMIN@UBIQUITOUS-LANG.COM',
    true,
    null,
    '4IO3KRHLYYKURAO5HNSGE2ZBW2DG7WRF',
    '42315f93-1637-46c3-ab73-ce7fa3fd508b',
    null,
    false,
    false,
    null,
    false,
    0
);

-- 復元結果確認
SELECT "Id", "Email", "IsFirstLogin", "InitialPassword", "PasswordHash" 
FROM "AspNetUsers" 
WHERE "Email" = 'admin@ubiquitous-lang.com';