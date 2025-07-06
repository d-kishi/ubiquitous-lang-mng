# Application層インターフェース設計書（軽量版）

**プロジェクト名**: ユビキタス言語管理システム  
**バージョン**: 1.0  
**作成日**: 2025-07-06  
**最終更新**: 2025-07-06  
**承認者**: プロジェクトオーナー  

## 目次

1. [設計方針](#1-設計方針)
2. [共通型定義](#2-共通型定義)
3. [認証・認可サービス](#3-認証認可サービス)
4. [ユーザー管理サービス](#4-ユーザー管理サービス)
5. [プロジェクト管理サービス](#5-プロジェクト管理サービス)
6. [ドメイン管理サービス](#6-ドメイン管理サービス)
7. [ユビキタス言語管理サービス](#7-ユビキタス言語管理サービス)
8. [承認ワークフローサービス](#8-承認ワークフローサービス)

---

## 1. 設計方針

### 1.1 目的
- **Clean Architecture準拠**: Application層のサービスインターフェースを定義
- **Blazor Server統合**: DIによるサービス注入とUIからの呼び出し
- **F# ↔ C# 境界**: Contracts層における型変換の基準
- **OpenAPI生成準備**: 将来のSwagger自動生成のための基盤

### 1.2 設計原則
- **単一責任原則**: 各サービスは明確な責務を持つ
- **依存関係逆転**: インターフェースによる抽象化
- **非同期処理**: 全メソッドをTask&lt;T&gt;で統一
- **Result型活用**: エラーハンドリングの一貫性

### 1.3 命名規則
- **インターフェース**: `I{ドメイン名}Service`
- **コマンド**: `{動詞}{対象}Command`
- **クエリ**: `{対象}Query` または `{動詞}{対象}Query`
- **DTO**: `{対象}Dto`

---

## 2. 共通型定義

### 2.1 Result型（エラーハンドリング）

```csharp
// Contracts層 - 共通Result型
public class Result<T>
{
    public bool IsSuccess { get; init; }
    public T? Value { get; init; }
    public string? Error { get; init; }
    public string? ErrorCode { get; init; }
    
    public static Result<T> Success(T value) => new() { IsSuccess = true, Value = value };
    public static Result<T> Failure(string error, string? errorCode = null) => 
        new() { IsSuccess = false, Error = error, ErrorCode = errorCode };
}

public class Result
{
    public bool IsSuccess { get; init; }
    public string? Error { get; init; }
    public string? ErrorCode { get; init; }
    
    public static Result Success() => new() { IsSuccess = true };
    public static Result Failure(string error, string? errorCode = null) => 
        new() { IsSuccess = false, Error = error, ErrorCode = errorCode };
}
```

### 2.2 共通列挙型

```csharp
// ユーザーロール
public enum UserRole
{
    SuperUser,
    ProjectManager,
    DomainApprover,
    GeneralUser
}

// ユビキタス言語ステータス
public enum UbiquitousLanguageStatus
{
    Editing,
    PendingApproval,
    Rejected,
    Approved
}

// 承認ステータス
public enum ApprovalStatus
{
    Pending,
    Approved,
    Rejected
}
```

### 2.3 ページング

```csharp
// ページング要求
public record PageQuery
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; } = false;
}

// ページング結果
public record PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
```

---

## 3. 認証・認可サービス

### 3.1 ICurrentUserService

```csharp
/// <summary>
/// 現在のユーザー情報と認可機能を提供
/// </summary>
public interface ICurrentUserService
{
    /// <summary>現在ログイン中のユーザー情報を取得</summary>
    Task<Result<CurrentUserDto?>> GetCurrentUserAsync();
    
    /// <summary>指定ロールを持つかを確認</summary>
    Task<Result<bool>> HasRoleAsync(UserRole role);
    
    /// <summary>プロジェクトへのアクセス権限を確認</summary>
    Task<Result<bool>> CanAccessProjectAsync(long projectId);
    
    /// <summary>ドメインへのアクセス権限を確認</summary>
    Task<Result<bool>> CanAccessDomainAsync(long domainId);
    
    /// <summary>ユビキタス言語の編集権限を確認</summary>
    Task<Result<bool>> CanEditUbiquitousLanguageAsync(long domainId);
    
    /// <summary>ユビキタス言語の承認権限を確認</summary>
    Task<Result<bool>> CanApproveUbiquitousLanguageAsync(long domainId);
}

/// <summary>現在のユーザー情報DTO</summary>
public record CurrentUserDto
{
    public long UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public UserRole Role { get; init; }
    public bool IsActive { get; init; }
    public IReadOnlyList<long> AccessibleProjectIds { get; init; } = Array.Empty<long>();
    public IReadOnlyList<long> AccessibleDomainIds { get; init; } = Array.Empty<long>();
}
```

### 3.2 IAuthenticationService

```csharp
/// <summary>
/// 認証機能を提供
/// </summary>
public interface IAuthenticationService
{
    /// <summary>ユーザーログイン</summary>
    Task<Result<LoginResultDto>> LoginAsync(LoginCommand command);
    
    /// <summary>ユーザーログアウト</summary>
    Task<Result> LogoutAsync();
    
    /// <summary>パスワード変更</summary>
    Task<Result> ChangePasswordAsync(ChangePasswordCommand command);
    
    /// <summary>初期パスワード設定</summary>
    Task<Result> SetInitialPasswordAsync(SetInitialPasswordCommand command);
}

public record LoginCommand
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public bool RememberMe { get; init; } = false;
}

public record LoginResultDto
{
    public bool IsSuccess { get; init; }
    public bool RequirePasswordChange { get; init; }
    public string? RedirectUrl { get; init; }
}

public record ChangePasswordCommand
{
    public string CurrentPassword { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
    public string ConfirmPassword { get; init; } = string.Empty;
}

public record SetInitialPasswordCommand
{
    public long UserId { get; init; }
    public string NewPassword { get; init; } = string.Empty;
    public string ConfirmPassword { get; init; } = string.Empty;
}
```

---

## 4. ユーザー管理サービス

### 4.1 IUserManagementService

```csharp
/// <summary>
/// ユーザー管理機能を提供
/// </summary>
public interface IUserManagementService
{
    /// <summary>ユーザー一覧取得（ページング対応）</summary>
    Task<Result<PagedResult<UserDto>>> GetUsersAsync(GetUsersQuery query);
    
    /// <summary>ユーザー詳細取得</summary>
    Task<Result<UserDetailDto?>> GetUserByIdAsync(long userId);
    
    /// <summary>新規ユーザー作成</summary>
    Task<Result<long>> CreateUserAsync(CreateUserCommand command);
    
    /// <summary>ユーザー情報更新</summary>
    Task<Result> UpdateUserAsync(UpdateUserCommand command);
    
    /// <summary>ユーザー無効化</summary>
    Task<Result> DeactivateUserAsync(long userId);
    
    /// <summary>ユーザー有効化</summary>
    Task<Result> ActivateUserAsync(long userId);
    
    /// <summary>ユーザープロジェクト割り当て更新</summary>
    Task<Result> UpdateUserProjectAssignmentsAsync(UpdateUserProjectAssignmentsCommand command);
}

public record GetUsersQuery : PageQuery
{
    public string? SearchTerm { get; init; }
    public UserRole? Role { get; init; }
    public bool? IsActive { get; init; }
    public long? ProjectId { get; init; }
}

public record UserDto
{
    public long UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public UserRole Role { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public record UserDetailDto : UserDto
{
    public IReadOnlyList<UserProjectDto> Projects { get; init; } = Array.Empty<UserProjectDto>();
    public IReadOnlyList<DomainApproverDto> ApproverDomains { get; init; } = Array.Empty<DomainApproverDto>();
}

public record UserProjectDto
{
    public long ProjectId { get; init; }
    public string ProjectName { get; init; } = string.Empty;
    public DateTime AssignedAt { get; init; }
}

public record DomainApproverDto
{
    public long DomainId { get; init; }
    public string DomainName { get; init; } = string.Empty;
    public long ProjectId { get; init; }
    public string ProjectName { get; init; } = string.Empty;
    public DateTime AssignedAt { get; init; }
}

public record CreateUserCommand
{
    public string Email { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public UserRole Role { get; init; }
    public string? InitialPassword { get; init; }
    public IReadOnlyList<long> ProjectIds { get; init; } = Array.Empty<long>();
}

public record UpdateUserCommand
{
    public long UserId { get; init; }
    public string Name { get; init; } = string.Empty;
    public UserRole Role { get; init; }
}

public record UpdateUserProjectAssignmentsCommand
{
    public long UserId { get; init; }
    public IReadOnlyList<long> ProjectIds { get; init; } = Array.Empty<long>();
}
```

---

## 5. プロジェクト管理サービス

### 5.1 IProjectManagementService

```csharp
/// <summary>
/// プロジェクト管理機能を提供
/// </summary>
public interface IProjectManagementService
{
    /// <summary>プロジェクト一覧取得</summary>
    Task<Result<IReadOnlyList<ProjectDto>>> GetProjectsAsync();
    
    /// <summary>アクセス可能なプロジェクト一覧取得</summary>
    Task<Result<IReadOnlyList<ProjectDto>>> GetAccessibleProjectsAsync();
    
    /// <summary>プロジェクト詳細取得</summary>
    Task<Result<ProjectDetailDto?>> GetProjectByIdAsync(long projectId);
    
    /// <summary>新規プロジェクト作成</summary>
    Task<Result<long>> CreateProjectAsync(CreateProjectCommand command);
    
    /// <summary>プロジェクト情報更新</summary>
    Task<Result> UpdateProjectAsync(UpdateProjectCommand command);
    
    /// <summary>プロジェクト削除</summary>
    Task<Result> DeleteProjectAsync(long projectId);
    
    /// <summary>プロジェクトメンバー管理</summary>
    Task<Result> UpdateProjectMembersAsync(UpdateProjectMembersCommand command);
}

public record ProjectDto
{
    public long ProjectId { get; init; }
    public string ProjectName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public bool IsDeleted { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public int DomainCount { get; init; }
    public int MemberCount { get; init; }
}

public record ProjectDetailDto : ProjectDto
{
    public IReadOnlyList<ProjectMemberDto> Members { get; init; } = Array.Empty<ProjectMemberDto>();
    public IReadOnlyList<DomainSummaryDto> Domains { get; init; } = Array.Empty<DomainSummaryDto>();
}

public record ProjectMemberDto
{
    public long UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public UserRole Role { get; init; }
    public DateTime AssignedAt { get; init; }
}

public record DomainSummaryDto
{
    public long DomainId { get; init; }
    public string DomainName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int UbiquitousLanguageCount { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public record CreateProjectCommand
{
    public string ProjectName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}

public record UpdateProjectCommand
{
    public long ProjectId { get; init; }
    public string ProjectName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}

public record UpdateProjectMembersCommand
{
    public long ProjectId { get; init; }
    public IReadOnlyList<long> MemberUserIds { get; init; } = Array.Empty<long>();
}
```

---

## 6. ドメイン管理サービス

### 6.1 IDomainManagementService

```csharp
/// <summary>
/// ドメイン管理機能を提供
/// </summary>
public interface IDomainManagementService
{
    /// <summary>ドメイン一覧取得（プロジェクト指定）</summary>
    Task<Result<IReadOnlyList<DomainDto>>> GetDomainsByProjectAsync(long projectId);
    
    /// <summary>アクセス可能なドメイン一覧取得</summary>
    Task<Result<IReadOnlyList<DomainDto>>> GetAccessibleDomainsAsync();
    
    /// <summary>ドメイン詳細取得</summary>
    Task<Result<DomainDetailDto?>> GetDomainByIdAsync(long domainId);
    
    /// <summary>新規ドメイン作成</summary>
    Task<Result<long>> CreateDomainAsync(CreateDomainCommand command);
    
    /// <summary>ドメイン情報更新</summary>
    Task<Result> UpdateDomainAsync(UpdateDomainCommand command);
    
    /// <summary>ドメイン削除</summary>
    Task<Result> DeleteDomainAsync(long domainId);
    
    /// <summary>ドメイン承認者管理</summary>
    Task<Result> UpdateDomainApproversAsync(UpdateDomainApproversCommand command);
}

public record DomainDto
{
    public long DomainId { get; init; }
    public long ProjectId { get; init; }
    public string ProjectName { get; init; } = string.Empty;
    public string DomainName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public bool IsDeleted { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public int UbiquitousLanguageCount { get; init; }
    public int PendingApprovalCount { get; init; }
}

public record DomainDetailDto : DomainDto
{
    public IReadOnlyList<DomainApproverDetailDto> Approvers { get; init; } = Array.Empty<DomainApproverDetailDto>();
    public IReadOnlyList<UbiquitousLanguageSummaryDto> RecentUbiquitousLanguages { get; init; } = Array.Empty<UbiquitousLanguageSummaryDto>();
}

public record DomainApproverDetailDto
{
    public long UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public DateTime AssignedAt { get; init; }
}

public record UbiquitousLanguageSummaryDto
{
    public long UbiquitousLanguageId { get; init; }
    public string JapaneseName { get; init; } = string.Empty;
    public string EnglishName { get; init; } = string.Empty;
    public UbiquitousLanguageStatus Status { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public record CreateDomainCommand
{
    public long ProjectId { get; init; }
    public string DomainName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}

public record UpdateDomainCommand
{
    public long DomainId { get; init; }
    public string DomainName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}

public record UpdateDomainApproversCommand
{
    public long DomainId { get; init; }
    public IReadOnlyList<long> ApproverUserIds { get; init; } = Array.Empty<long>();
}
```

---

## 7. ユビキタス言語管理サービス

### 7.1 IUbiquitousLanguageService

```csharp
/// <summary>
/// ユビキタス言語管理機能を提供
/// </summary>
public interface IUbiquitousLanguageService
{
    /// <summary>ユビキタス言語一覧取得（ページング対応）</summary>
    Task<Result<PagedResult<UbiquitousLanguageDto>>> GetUbiquitousLanguagesAsync(GetUbiquitousLanguagesQuery query);
    
    /// <summary>ユビキタス言語詳細取得</summary>
    Task<Result<UbiquitousLanguageDetailDto?>> GetUbiquitousLanguageByIdAsync(long ubiquitousLanguageId);
    
    /// <summary>ドラフト版ユビキタス言語作成</summary>
    Task<Result<long>> CreateDraftAsync(CreateDraftUbiquitousLanguageCommand command);
    
    /// <summary>ドラフト版ユビキタス言語更新</summary>
    Task<Result> UpdateDraftAsync(UpdateDraftUbiquitousLanguageCommand command);
    
    /// <summary>ドラフト版削除</summary>
    Task<Result> DeleteDraftAsync(long draftId);
    
    /// <summary>承認申請</summary>
    Task<Result> SubmitForApprovalAsync(SubmitForApprovalCommand command);
    
    /// <summary>承認申請取り下げ</summary>
    Task<Result> WithdrawApprovalRequestAsync(long draftId);
    
    /// <summary>関連用語の設定</summary>
    Task<Result> UpdateRelatedTermsAsync(UpdateRelatedTermsCommand command);
    
    /// <summary>用語検索</summary>
    Task<Result<IReadOnlyList<UbiquitousLanguageSearchResultDto>>> SearchAsync(SearchUbiquitousLanguageQuery query);
}

public record GetUbiquitousLanguagesQuery : PageQuery
{
    public long? ProjectId { get; init; }
    public long? DomainId { get; init; }
    public UbiquitousLanguageStatus? Status { get; init; }
    public string? SearchTerm { get; init; }
    public DateTime? UpdatedAfter { get; init; }
}

public record UbiquitousLanguageDto
{
    public long UbiquitousLanguageId { get; init; }
    public long DomainId { get; init; }
    public string DomainName { get; init; } = string.Empty;
    public long ProjectId { get; init; }
    public string ProjectName { get; init; } = string.Empty;
    public string JapaneseName { get; init; } = string.Empty;
    public string EnglishName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public UbiquitousLanguageStatus Status { get; init; }
    public string UpdatedByUserName { get; init; } = string.Empty;
    public DateTime UpdatedAt { get; init; }
    public bool IsDraft { get; init; }
}

public record UbiquitousLanguageDetailDto : UbiquitousLanguageDto
{
    public IReadOnlyList<RelatedTermDto> RelatedTerms { get; init; } = Array.Empty<RelatedTermDto>();
    public IReadOnlyList<UbiquitousLanguageHistoryDto> History { get; init; } = Array.Empty<UbiquitousLanguageHistoryDto>();
    public long? DraftId { get; init; }
    public UbiquitousLanguageDto? DraftVersion { get; init; }
}

public record RelatedTermDto
{
    public long RelatedUbiquitousLanguageId { get; init; }
    public string JapaneseName { get; init; } = string.Empty;
    public string EnglishName { get; init; } = string.Empty;
    public string RelationType { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}

public record UbiquitousLanguageHistoryDto
{
    public long HistoryId { get; init; }
    public string JapaneseName { get; init; } = string.Empty;
    public string EnglishName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string UpdatedByUserName { get; init; } = string.Empty;
    public DateTime UpdatedAt { get; init; }
    public string ChangeType { get; init; } = string.Empty;
}

public record CreateDraftUbiquitousLanguageCommand
{
    public long DomainId { get; init; }
    public string JapaneseName { get; init; } = string.Empty;
    public string EnglishName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public long? BasedOnUbiquitousLanguageId { get; init; }
}

public record UpdateDraftUbiquitousLanguageCommand
{
    public long DraftId { get; init; }
    public string JapaneseName { get; init; } = string.Empty;
    public string EnglishName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}

public record SubmitForApprovalCommand
{
    public long DraftId { get; init; }
    public string? SubmissionComment { get; init; }
}

public record UpdateRelatedTermsCommand
{
    public long UbiquitousLanguageId { get; init; }
    public IReadOnlyList<RelatedTermRequest> RelatedTerms { get; init; } = Array.Empty<RelatedTermRequest>();
}

public record RelatedTermRequest
{
    public long RelatedUbiquitousLanguageId { get; init; }
    public string RelationType { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}

public record SearchUbiquitousLanguageQuery
{
    public string SearchTerm { get; init; } = string.Empty;
    public long? ProjectId { get; init; }
    public long? DomainId { get; init; }
    public bool IncludeDrafts { get; init; } = false;
    public int MaxResults { get; init; } = 50;
}

public record UbiquitousLanguageSearchResultDto
{
    public long UbiquitousLanguageId { get; init; }
    public string JapaneseName { get; init; } = string.Empty;
    public string EnglishName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string DomainName { get; init; } = string.Empty;
    public string ProjectName { get; init; } = string.Empty;
    public UbiquitousLanguageStatus Status { get; init; }
    public bool IsDraft { get; init; }
    public double RelevanceScore { get; init; }
}
```

---

## 8. 承認ワークフローサービス

### 8.1 IApprovalWorkflowService

```csharp
/// <summary>
/// 承認ワークフロー機能を提供
/// </summary>
public interface IApprovalWorkflowService
{
    /// <summary>承認待ち一覧取得</summary>
    Task<Result<PagedResult<PendingApprovalDto>>> GetPendingApprovalsAsync(GetPendingApprovalsQuery query);
    
    /// <summary>自分の承認待ち一覧取得</summary>
    Task<Result<IReadOnlyList<PendingApprovalDto>>> GetMyPendingApprovalsAsync();
    
    /// <summary>承認実行</summary>
    Task<Result> ApproveAsync(ApproveCommand command);
    
    /// <summary>却下実行</summary>
    Task<Result> RejectAsync(RejectCommand command);
    
    /// <summary>承認履歴取得</summary>
    Task<Result<IReadOnlyList<ApprovalHistoryDto>>> GetApprovalHistoryAsync(long ubiquitousLanguageId);
    
    /// <summary>バッチ承認</summary>
    Task<Result<BatchApprovalResultDto>> BatchApproveAsync(BatchApprovalCommand command);
}

public record GetPendingApprovalsQuery : PageQuery
{
    public long? DomainId { get; init; }
    public long? ProjectId { get; init; }
    public DateTime? SubmittedAfter { get; init; }
    public string? SubmittedByUserName { get; init; }
}

public record PendingApprovalDto
{
    public long DraftId { get; init; }
    public long DomainId { get; init; }
    public string DomainName { get; init; } = string.Empty;
    public long ProjectId { get; init; }
    public string ProjectName { get; init; } = string.Empty;
    public string JapaneseName { get; init; } = string.Empty;
    public string EnglishName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string SubmittedByUserName { get; init; } = string.Empty;
    public DateTime SubmittedAt { get; init; }
    public string? SubmissionComment { get; init; }
    public bool IsNewTerm { get; init; }
    public long? OriginalUbiquitousLanguageId { get; init; }
}

public record ApproveCommand
{
    public long DraftId { get; init; }
    public string? ApprovalComment { get; init; }
}

public record RejectCommand
{
    public long DraftId { get; init; }
    public string RejectReason { get; init; } = string.Empty;
    public string? RejectComment { get; init; }
}

public record ApprovalHistoryDto
{
    public long HistoryId { get; init; }
    public ApprovalStatus Status { get; init; }
    public string ApproverUserName { get; init; } = string.Empty;
    public DateTime ProcessedAt { get; init; }
    public string? Comment { get; init; }
    public string? RejectReason { get; init; }
}

public record BatchApprovalCommand
{
    public IReadOnlyList<long> DraftIds { get; init; } = Array.Empty<long>();
    public string? Comment { get; init; }
}

public record BatchApprovalResultDto
{
    public int TotalCount { get; init; }
    public int SuccessCount { get; init; }
    public int FailureCount { get; init; }
    public IReadOnlyList<BatchApprovalFailureDto> Failures { get; init; } = Array.Empty<BatchApprovalFailureDto>();
}

public record BatchApprovalFailureDto
{
    public long DraftId { get; init; }
    public string JapaneseName { get; init; } = string.Empty;
    public string Error { get; init; } = string.Empty;
}
```

---

## 実装ガイドライン

### DI登録例

```csharp
// Program.cs
services.AddScoped<ICurrentUserService, CurrentUserService>();
services.AddScoped<IAuthenticationService, AuthenticationService>();
services.AddScoped<IUserManagementService, UserManagementService>();
services.AddScoped<IProjectManagementService, ProjectManagementService>();
services.AddScoped<IDomainManagementService, DomainManagementService>();
services.AddScoped<IUbiquitousLanguageService, UbiquitousLanguageService>();
services.AddScoped<IApprovalWorkflowService, ApprovalWorkflowService>();
```

### Blazor Component使用例

```csharp
@inject IUbiquitousLanguageService UbiquitousLanguageService
@inject ICurrentUserService CurrentUserService

private async Task LoadUbiquitousLanguages()
{
    var query = new GetUbiquitousLanguagesQuery
    {
        DomainId = selectedDomainId,
        PageNumber = currentPage,
        PageSize = 20
    };
    
    var result = await UbiquitousLanguageService.GetUbiquitousLanguagesAsync(query);
    
    if (result.IsSuccess)
    {
        ubiquitousLanguages = result.Value!.Items;
        totalCount = result.Value.TotalCount;
    }
    else
    {
        ShowError(result.Error);
    }
}
```

---

**作成日**: 2025-07-06  
**作成者**: Claude Code  
**承認予定**: プロジェクトオーナー  
**実装開始**: プロジェクト雛形作成時