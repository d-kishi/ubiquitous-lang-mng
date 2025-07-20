# チーム2: データ管理・CRUD専門分析

**分析日**: 2025-07-20  
**専門領域**: ユーザーCRUD・検索・フィルタリング・ページング・大量データ処理  

## 🔍 発見された技術課題

### 1. 複雑な検索・フィルタリング要件
**課題**: 氏名部分一致、プロジェクト別フィルタ、論理削除表示切替  
**影響度**: 🟡 中（機能実装）  

**解決アプローチ**:
```csharp
// IQueryableによる動的クエリ構築
public async Task<PagedResult<UserDto>> SearchUsersAsync(UserSearchCriteria criteria)
{
    var query = _context.Users.AsNoTracking();
    
    // 論理削除フィルタ
    if (!criteria.IncludeDeleted)
        query = query.Where(u => !u.IsDeleted);
    
    // 氏名部分一致（pg_trgm使用）
    if (!string.IsNullOrEmpty(criteria.Name))
        query = query.Where(u => EF.Functions.ILike(u.Name, $"%{criteria.Name}%"));
    
    // プロジェクトフィルタ（権限考慮）
    if (criteria.ProjectId.HasValue)
        query = query.Where(u => u.UserProjects.Any(up => up.ProjectId == criteria.ProjectId));
    
    // ページング適用
    var totalCount = await query.CountAsync();
    var items = await query
        .OrderBy(u => u.Name)
        .Skip(criteria.PageIndex * criteria.PageSize)
        .Take(criteria.PageSize)
        .Select(u => new UserDto { /* マッピング */ })
        .ToListAsync();
}
```

### 2. PostgreSQL最適化戦略
**課題**: 部分一致検索のパフォーマンス問題  
**影響度**: 🔴 高（パフォーマンス）  

**解決アプローチ**:
```sql
-- pg_trgm拡張有効化
CREATE EXTENSION IF NOT EXISTS pg_trgm;

-- GINインデックス作成
CREATE INDEX idx_users_name_gin ON AspNetUsers USING gin (Name gin_trgm_ops);
CREATE INDEX idx_users_email_gin ON AspNetUsers USING gin (Email gin_trgm_ops);

-- 複合インデックス（権限フィルタ用）
CREATE INDEX idx_userprojects_composite ON UserProjects (ProjectId, UserId);
```

### 3. 権限ベースデータアクセス制御
**課題**: プロジェクト管理者の表示制限実装  
**影響度**: 🔴 高（セキュリティ）  

**解決アプローチ**:
```csharp
// 権限に基づくクエリフィルタ（UserProjectsテーブル構造を考慮）
private IQueryable<AspNetUser> ApplySecurityFilter(IQueryable<AspNetUser> query, ClaimsPrincipal user)
{
    if (user.IsInRole("SuperUser"))
        return query; // 全ユーザー表示
    
    if (user.IsInRole("ProjectManager"))
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        // UserProjectsテーブルから管理プロジェクトを取得
        var managedProjectIds = _context.UserProjects
            .Where(up => up.UserId == userId)
            .Select(up => up.ProjectId)
            .ToList();
        
        // 同じプロジェクトに所属するユーザーを取得
        return query.Where(u => 
            _context.UserProjects
                .Any(up => up.UserId == u.Id && managedProjectIds.Contains(up.ProjectId)));
    }
    
    // その他は自分のみ
    return query.Where(u => u.Id == user.FindFirst(ClaimTypes.NameIdentifier).Value);
}
```

**注意点**：
- UserProjectsテーブルはUserProjectId（BIGSERIAL）が主キー
- UserId, ProjectIdの複合一意制約があるため重複登録は防げる
- 効率的なクエリのためにIX_UserProjects_ProjectIdインデックスを活用

## 📊 Gemini技術調査結果

### 調査: EF Core + PostgreSQL最適化
**キーポイント**:
- AsNoTracking()による読み取り専用クエリ最適化
- Select射影による転送データ削減
- pg_trgm GINインデックスによる部分一致高速化
- ILike使用でインデックス自動適用

## 🎯 実装推奨事項

### Infrastructure層実装優先順位
1. **UserRepository基本CRUD**: Create, Update, Delete（論理）
2. **検索メソッド実装**: 動的クエリ構築
3. **権限フィルタ統合**: セキュリティ層実装

### パフォーマンス最適化計画
1. **初期**: 基本的なインデックス設定
2. **中期**: pg_trgm導入（1000ユーザー以上）
3. **長期**: クエリ実行計画分析・最適化

### 技術的リスクと対策
- **リスク**: 複雑なJOINによるN+1問題
- **対策**: Include()による適切なEager Loading

---

**分析完了**: 効率的なデータアクセス層実装方針確立