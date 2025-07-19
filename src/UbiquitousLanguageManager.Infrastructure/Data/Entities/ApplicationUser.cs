using Microsoft.AspNetCore.Identity;
using System;

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
    /// ユーザーロール（単一ロール制）
    /// SuperUser / ProjectManager / DomainApprover / GeneralUser
    /// 
    /// 【設計説明】
    /// ASP.NET Core Identity の標準的なロール管理（多対多）とは別に、
    /// シンプルな単一ロール制を採用しています。
    /// 将来的に複数ロールが必要になった場合は、Identity の標準機能に移行します。
    /// </summary>
    public string UserRole { get; set; } = "GeneralUser";

    /// <summary>
    /// F# Domain 層の User エンティティとの連携用ID
    /// Domain 層の User.Id と同じ値を保持し、層間でのマッピングを可能にする
    /// 
    /// 【アーキテクチャ説明】
    /// Clean Architecture の原則により、Domain 層と Infrastructure 層は分離されています。
    /// このプロパティにより、両層のエンティティを関連付けることができます。
    /// </summary>
    public long? DomainUserId { get; set; }
}