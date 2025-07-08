using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UbiquitousLanguageManager.Contracts.DTOs;

namespace UbiquitousLanguageManager.Contracts.Interfaces;

/// <summary>
/// Application層への統一アクセスインターフェース
/// Presentation層からApplication層への境界を定義
/// F#のApplication ServiceをC#から利用するための橋渡し役
/// </summary>
public interface IApplicationService
{
    #region ユーザー管理

    /// <summary>
    /// 新規ユーザー登録
    /// ビジネスルールを適用したユーザー作成処理
    /// </summary>
    /// <param name="createUserDto">作成するユーザー情報</param>
    /// <returns>作成されたユーザー情報、またはエラーメッセージ</returns>
    Task<ServiceResult<UserDto>> CreateUserAsync(CreateUserDto createUserDto);

    /// <summary>
    /// ユーザーログイン
    /// 認証処理と初回ログインチェック
    /// </summary>
    /// <param name="loginDto">ログイン情報</param>
    /// <returns>認証されたユーザー情報、またはエラーメッセージ</returns>
    Task<ServiceResult<UserDto>> LoginAsync(LoginDto loginDto);

    /// <summary>
    /// パスワード変更
    /// セキュリティポリシーに準拠したパスワード更新
    /// </summary>
    /// <param name="changePasswordDto">パスワード変更情報</param>
    /// <returns>変更結果、またはエラーメッセージ</returns>
    Task<ServiceResult<bool>> ChangePasswordAsync(ChangePasswordDto changePasswordDto);

    /// <summary>
    /// ユーザー一覧取得
    /// プロジェクト単位でのユーザー一覧取得
    /// </summary>
    /// <param name="projectId">プロジェクトID（null の場合は全ユーザー）</param>
    /// <returns>ユーザー一覧</returns>
    Task<ServiceResult<List<UserDto>>> GetUsersAsync(long? projectId = null);

    /// <summary>
    /// ユーザー詳細取得
    /// 指定されたIDのユーザー情報を取得
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <returns>ユーザー詳細情報、またはエラーメッセージ</returns>
    Task<ServiceResult<UserDto>> GetUserByIdAsync(long userId);

    #endregion

    #region プロジェクト管理

    /// <summary>
    /// 新規プロジェクト作成
    /// ビジネスルールを適用したプロジェクト作成処理
    /// </summary>
    /// <param name="createProjectDto">作成するプロジェクト情報</param>
    /// <returns>作成されたプロジェクト情報、またはエラーメッセージ</returns>
    Task<ServiceResult<ProjectDto>> CreateProjectAsync(CreateProjectDto createProjectDto);

    /// <summary>
    /// プロジェクト一覧取得
    /// アクティブなプロジェクト一覧を取得
    /// </summary>
    /// <returns>プロジェクト一覧</returns>
    Task<ServiceResult<List<ProjectDto>>> GetActiveProjectsAsync();

    /// <summary>
    /// プロジェクト詳細取得
    /// 指定されたIDのプロジェクト情報を取得
    /// </summary>
    /// <param name="projectId">プロジェクトID</param>
    /// <returns>プロジェクト詳細情報、またはエラーメッセージ</returns>
    Task<ServiceResult<ProjectDto>> GetProjectByIdAsync(long projectId);

    /// <summary>
    /// プロジェクト更新
    /// 既存プロジェクトの情報更新
    /// </summary>
    /// <param name="updateProjectDto">更新するプロジェクト情報</param>
    /// <returns>更新されたプロジェクト情報、またはエラーメッセージ</returns>
    Task<ServiceResult<ProjectDto>> UpdateProjectAsync(UpdateProjectDto updateProjectDto);

    #endregion

    #region ドメイン管理

    /// <summary>
    /// 新規ドメイン作成
    /// プロジェクト内でのドメイン作成処理
    /// </summary>
    /// <param name="createDomainDto">作成するドメイン情報</param>
    /// <returns>作成されたドメイン情報、またはエラーメッセージ</returns>
    Task<ServiceResult<DomainDto>> CreateDomainAsync(CreateDomainDto createDomainDto);

    /// <summary>
    /// プロジェクト別ドメイン一覧取得
    /// 指定されたプロジェクトのドメイン一覧を取得
    /// </summary>
    /// <param name="projectId">プロジェクトID</param>
    /// <returns>ドメイン一覧</returns>
    Task<ServiceResult<List<DomainDto>>> GetDomainsByProjectIdAsync(long projectId);

    /// <summary>
    /// ドメイン詳細取得
    /// 指定されたIDのドメイン情報を取得
    /// </summary>
    /// <param name="domainId">ドメインID</param>
    /// <returns>ドメイン詳細情報、またはエラーメッセージ</returns>
    Task<ServiceResult<DomainDto>> GetDomainByIdAsync(long domainId);

    #endregion

    #region ユビキタス言語管理

    /// <summary>
    /// 新規ユビキタス言語作成（下書き）
    /// ドメイン内での新規用語作成処理
    /// </summary>
    /// <param name="createDto">作成するユビキタス言語情報</param>
    /// <returns>作成されたユビキタス言語情報、またはエラーメッセージ</returns>
    Task<ServiceResult<UbiquitousLanguageDto>> CreateUbiquitousLanguageAsync(CreateUbiquitousLanguageDto createDto);

    /// <summary>
    /// ドメイン別ユビキタス言語一覧取得
    /// 指定されたドメインのユビキタス言語一覧（下書き・正式版両方）を取得
    /// </summary>
    /// <param name="domainId">ドメインID</param>
    /// <returns>ユビキタス言語一覧</returns>
    Task<ServiceResult<List<UbiquitousLanguageDto>>> GetUbiquitousLanguagesByDomainIdAsync(long domainId);

    /// <summary>
    /// 承認申請処理
    /// 下書きから承認フローへの移行
    /// </summary>
    /// <param name="submitDto">承認申請情報</param>
    /// <returns>申請結果、またはエラーメッセージ</returns>
    Task<ServiceResult<UbiquitousLanguageDto>> SubmitForApprovalAsync(SubmitForApprovalDto submitDto);

    /// <summary>
    /// 承認・却下処理
    /// 申請された用語の承認・却下決定
    /// </summary>
    /// <param name="approvalDto">承認・却下情報</param>
    /// <returns>処理結果、またはエラーメッセージ</returns>
    Task<ServiceResult<UbiquitousLanguageDto>> ProcessApprovalAsync(ApprovalDto approvalDto);

    #endregion
}

/// <summary>
/// Application層からの処理結果を表現するクラス
/// F#のResult型をC#で使いやすくラップ
/// </summary>
/// <typeparam name="T">成功時のデータ型</typeparam>
public class ServiceResult<T>
{
    /// <summary>
    /// 処理が成功したかどうか
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// 成功時のデータ
    /// </summary>
    public T? Data { get; init; }

    /// <summary>
    /// エラーメッセージ
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// 検証エラーの詳細情報
    /// </summary>
    public Dictionary<string, string> ValidationErrors { get; init; } = new();

    /// <summary>
    /// 成功結果の作成
    /// </summary>
    public static ServiceResult<T> Success(T data) => new()
    {
        IsSuccess = true,
        Data = data
    };

    /// <summary>
    /// エラー結果の作成
    /// </summary>
    public static ServiceResult<T> Error(string errorMessage) => new()
    {
        IsSuccess = false,
        ErrorMessage = errorMessage
    };

    /// <summary>
    /// 検証エラー結果の作成
    /// </summary>
    public static ServiceResult<T> ValidationError(Dictionary<string, string> validationErrors) => new()
    {
        IsSuccess = false,
        ErrorMessage = "入力値に問題があります",
        ValidationErrors = validationErrors
    };
}