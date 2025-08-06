using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using UbiquitousLanguageManager.Infrastructure.Data;

namespace UbiquitousLanguageManager.Infrastructure.Identity;

/// <summary>
/// カスタムRoleStore実装
/// IdentityRoleClaimテーブルを使用しないRoleStore
/// 
/// 【設計説明】
/// データベース設計書に基づき、ASP.NET Core Identityの
/// RoleClaim関連テーブルを使用しない実装
/// </summary>
public class CustomRoleStore : RoleStore<IdentityRole, UbiquitousLanguageDbContext>
{
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="context">データベースコンテキスト</param>
    /// <param name="describer">エラー説明</param>
    public CustomRoleStore(UbiquitousLanguageDbContext context, IdentityErrorDescriber? describer = null) 
        : base(context, describer)
    {
    }

    /// <summary>
    /// ロールのClaimを取得（RoleClaimテーブルを使用しない）
    /// </summary>
    public override Task<IList<Claim>> GetClaimsAsync(IdentityRole role, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (role == null)
        {
            throw new ArgumentNullException(nameof(role));
        }

        // RoleClaimテーブルを使用せず、空のリストを返す
        return Task.FromResult<IList<Claim>>(new List<Claim>());
    }

    /// <summary>
    /// ロールにClaimを追加（実装しない）
    /// </summary>
    public override Task AddClaimAsync(IdentityRole role, Claim claim, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        // RoleClaimテーブルを使用しないため、何もしない
        return Task.CompletedTask;
    }

    /// <summary>
    /// ロールからClaimを削除（実装しない）
    /// </summary>
    public override Task RemoveClaimAsync(IdentityRole role, Claim claim, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        // RoleClaimテーブルを使用しないため、何もしない
        return Task.CompletedTask;
    }
}