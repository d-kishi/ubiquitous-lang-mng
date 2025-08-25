using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace UbiquitousLanguageManager.Infrastructure.Data.Entities;

/// <summary>
/// ASP.NET Core Identity 統合用ユーザーエンティティ
/// IdentityUser を継承し、業務固有のプロパティを追加
/// 
/// 【F#初学者向け解説】
/// ASP.NET Core Identity は、認証・承認機能を提供するフレームワークです。
/// IdentityUser クラスを継承することで、標準的な認証機能（ログイン、パスワード管理等）を利用できます。
/// このクラスは C# Infrastructure 層で使用され、F# Domain 層の User エンティティとは分離されています。
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// ユーザー氏名（表示名）
    /// 業務要件により必須項目
    /// </summary>
    public string Name { get; set; } = string.Empty;



    /// <summary>
    /// 初回ログインフラグ
    /// true の場合、ログイン後にパスワード変更を強制する
    /// 
    /// 【Blazor Server初学者向け解説】
    /// このフラグを使用して、初回ログイン時に特別な処理（パスワード変更画面への自動遷移等）を実装します。
    /// Blazor Server では、AuthenticationStateProvider でこのフラグをチェックし、適切な画面遷移を行います。
    /// </summary>
    public bool IsFirstLogin { get; set; } = true;

    /// <summary>
    /// 最終更新日時（UTC）
    /// PostgreSQL の TIMESTAMPTZ 型に対応
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 論理削除フラグ
    /// true の場合、ユーザーは削除されたものとして扱われる
    /// 物理削除は行わず、このフラグで管理する
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// 初期パスワード（一時的保存用）
    /// 管理者がユーザーを作成した際の初期パスワードを一時的に保存
    /// ユーザーが初回ログインしてパスワードを変更したら null にする
    /// 
    /// 【セキュリティ注意】
    /// この項目は一時的な利便性のためのものです。
    /// 初回ログイン完了後は必ず null に更新し、平文パスワードを保持しないようにしてください。
    /// </summary>
    public string? InitialPassword { get; set; }

    /// <summary>
    /// パスワードリセットトークン
    /// パスワードリセット機能で使用する一時的なトークン
    /// 
    /// 【Phase A3機能】
    /// 機能仕様書 2.1.3 パスワードリセット機能に対応
    /// 24時間の有効期限付きでトークンベースのパスワードリセットを実現
    /// </summary>
    public string? PasswordResetToken { get; set; }

    /// <summary>
    /// パスワードリセットトークン有効期限
    /// リセットトークンの有効期限（UTC）
    /// 
    /// 【Phase A3機能】
    /// 機能仕様書 2.1.3 パスワードリセット機能に対応
    /// 24時間後に自動的に無効になるセキュリティ機能
    /// PostgreSQL の TIMESTAMPTZ 型に対応
    /// </summary>
    public DateTime? PasswordResetExpiry { get; set; }

    /// <summary>
    /// 最終更新者ID
    /// データの更新履歴管理用
    /// </summary>
    public string UpdatedBy { get; set; } = string.Empty;

    // ===== 設計書準拠への修正完了（Refactor Phase） =====
    // ASP.NET Core Identity標準機能への移行完了
    // 設計書で定義されていない余計な実装を削除
    // 
    // 【移行完了事項】
    // - UserRole → ASP.NET Core Identity Roles移行（AspNetRoles/AspNetUserRoles使用）
    // - IsActive → 計算プロパティ（!IsDeleted）として残存（設計書では論理削除のみ）
    // - CreatedAt/CreatedBy → 設計書にない実装のため削除
    // - DomainUserId → 設計書にない実装のため削除
    // - Role → エイリアスプロパティのため削除

    /// <summary>
    /// アクティブ状態（計算プロパティ）
    /// 論理削除フラグの逆転値として算出
    /// 設計書では明示的に定義されていないが、UI利便性のため保持
    /// </summary>
    public bool IsActive => !IsDeleted;

    // Navigation properties for business entities
    /// <summary>
    /// このユーザーが参加するプロジェクトとの関連付け
    /// UserProjectエンティティを通じた多対多の関係
    /// </summary>
    public virtual ICollection<UserProject> UserProjects { get; set; } = new List<UserProject>();
    
    /// <summary>
    /// このユーザーがドメイン承認者として設定されている関係
    /// DomainApproverエンティティを通じた多対多の関係
    /// </summary>
    public virtual ICollection<DomainApprover> DomainApprovers { get; set; } = new List<DomainApprover>();
}