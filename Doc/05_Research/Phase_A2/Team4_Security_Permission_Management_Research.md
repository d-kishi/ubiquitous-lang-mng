# チーム4: セキュリティ・権限管理専門分析

**分析日**: 2025-07-20  
**専門領域**: ASP.NET Core Identity拡張・セキュリティ・権限制御・監査  

## 🔍 発見された技術課題

### 1. ASP.NET Core Identity拡張実装
**課題**: カスタムロールとプロジェクトスコープ権限の統合  
**影響度**: 🔴 高（認証基盤）  

**解決アプローチ**:
```csharp
// カスタムClaimsPrincipalFactory
public class CustomClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
{
    private readonly IUserProjectService _userProjectService;
    
    public override async Task<ClaimsPrincipal> CreateAsync(ApplicationUser user)
    {
        var principal = await base.CreateAsync(user);
        var identity = (ClaimsIdentity)principal.Identity;
        
        // グローバル権限の追加
        var roles = await _userManager.GetRolesAsync(user);
        foreach (var role in roles)
        {
            var permissions = GetPermissionsForRole(role);
            foreach (var permission in permissions)
            {
                identity.AddClaim(new Claim("permission", permission));
            }
        }
        
        // プロジェクトスコープ権限の追加
        var userProjects = await _userProjectService.GetUserProjectsAsync(user.Id);
        foreach (var project in userProjects)
        {
            identity.AddClaim(new Claim("project", project.ProjectId.ToString()));
            
            // プロジェクト管理者の場合
            if (roles.Contains("ProjectManager"))
            {
                identity.AddClaim(new Claim("permission", 
                    $"projects.manage.{project.ProjectId}"));
            }
        }
        
        return principal;
    }
    
    private IEnumerable<string> GetPermissionsForRole(string role)
    {
        return role switch
        {
            "SuperUser" => new[] { "users.manage", "projects.manage", "content.edit", "content.view" },
            "ProjectManager" => new[] { "projects.manage", "content.edit", "content.view" },
            "DomainApprover" => new[] { "content.approve", "content.edit", "content.view" },
            "GeneralUser" => new[] { "content.edit", "content.view" },
            _ => new[] { "content.view" }
        };
    }
}
```

### 2. カスタムAuthorization Handler
**課題**: プロジェクトベースの動的権限チェック  
**影響度**: 🔴 高（セキュリティ）  

**解決アプローチ**:
```csharp
// プロジェクトベース権限要件
public class ProjectPermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }
    public ProjectPermissionRequirement(string permission)
    {
        Permission = permission;
    }
}

// Authorization Handler
public class ProjectPermissionHandler : AuthorizationHandler<ProjectPermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ProjectPermissionRequirement requirement)
    {
        // HTTPコンテキストからプロジェクトIDを取得
        if (context.Resource is HttpContext httpContext)
        {
            var projectId = httpContext.Request.RouteValues["projectId"]?.ToString();
            if (string.IsNullOrEmpty(projectId))
            {
                // グローバル権限チェック
                if (context.User.HasClaim("permission", requirement.Permission))
                {
                    context.Succeed(requirement);
                }
            }
            else
            {
                // プロジェクトスコープ権限チェック
                var projectPermission = $"{requirement.Permission}.{projectId}";
                if (context.User.HasClaim("permission", projectPermission) ||
                    context.User.HasClaim("permission", requirement.Permission)) // グローバル権限
                {
                    context.Succeed(requirement);
                }
            }
        }
        
        return Task.CompletedTask;
    }
}

// Program.csでの登録
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ManageUsers", policy =>
        policy.RequireClaim("permission", "users.manage"));
    
    options.AddPolicy("ManageProjects", policy =>
        policy.Requirements.Add(new ProjectPermissionRequirement("projects.manage")));
});

builder.Services.AddScoped<IAuthorizationHandler, ProjectPermissionHandler>();
```

### 3. 監査ログ実装
**課題**: ユーザー操作の追跡とGDPR対応  
**影響度**: 🟡 中（コンプライアンス）  

**解決アプローチ**:
```csharp
// 監査ログサービス
public interface IAuditService
{
    Task LogAsync(string action, string entityType, string entityId, 
                  object oldValues = null, object newValues = null);
}

public class AuditService : IAuditService
{
    private readonly IDbContext _context;
    private readonly ICurrentUserService _currentUser;
    
    public async Task LogAsync(string action, string entityType, string entityId,
                              object oldValues = null, object newValues = null)
    {
        var auditLog = new AuditLog
        {
            UserId = _currentUser.UserId,
            UserName = _currentUser.UserName,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
            NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
            Timestamp = DateTimeOffset.UtcNow,
            IpAddress = _currentUser.IpAddress
        };
        
        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();
    }
}

// 使用例（UserService内）
public async Task UpdateUserAsync(UserUpdateDto dto)
{
    var user = await _userManager.FindByIdAsync(dto.Id);
    var oldValues = new { user.Name, user.Email };
    
    user.Name = dto.Name;
    user.Email = dto.Email;
    
    var result = await _userManager.UpdateAsync(user);
    
    if (result.Succeeded)
    {
        await _auditService.LogAsync("UserUpdated", "User", user.Id,
            oldValues, new { user.Name, user.Email });
    }
}
```

## 📊 Gemini技術調査結果

### 調査: ASP.NET Core Identity拡張
**キーポイント**:
- IUserClaimsPrincipalFactoryでの動的Claims生成
- IAuthorizationRequirement/Handlerでのカスタム認可
- ポリシーベース認可とリソースベース認可の組み合わせ
- 監査ログでのコンプライアンス対応

## 🎯 実装推奨事項

### セキュリティ層実装優先順位
1. **ApplicationUser拡張**: カスタムプロパティ追加
2. **ClaimsPrincipalFactory**: 動的権限生成
3. **Authorization Policies**: 権限ポリシー定義
4. **監査ログ基盤**: 基本的な操作追跡

### セキュリティベストプラクティス
- **最小権限の原則**: デフォルトで権限なし
- **明示的な権限付与**: 必要な権限のみ付与
- **監査証跡**: 全ての重要操作を記録
- **定期的な権限レビュー**: 不要な権限の削除

### 技術的リスクと対策
- **リスク**: Claims肥大化によるCookie制限超過
- **対策**: 必要最小限のClaimsのみ格納、詳細はDBから取得

---

**分析完了**: 堅牢なセキュリティ・権限管理システム実装方針確立