using Moq;
using Microsoft.FSharp.Core;
using Microsoft.FSharp.Collections;
using Microsoft.Extensions.Logging;
// Application層のF#型をエイリアスで使用
using AppIUserManagementService = UbiquitousLanguageManager.Application.IUserManagementService;
using IUserRepository = UbiquitousLanguageManager.Application.IUserRepository;
using IAuthenticationService = UbiquitousLanguageManager.Application.IAuthenticationService;
// F# Domain型をエイリアスで使用
using FSharpDomainUser = UbiquitousLanguageManager.Domain.Authentication.User;
using FSharpUserId = UbiquitousLanguageManager.Domain.Common.UserId;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UbiquitousLanguageManager.Web.Tests.Infrastructure;

/// <summary>
/// UserManagementApplicationServiceモック作成ビルダー（Fluent API）
///
/// 【使用例】
/// var mockService = new UserManagementServiceMockBuilder()
///     .SetupGetAllUsersSuccess(testUsers)
///     .SetupCreateUserSuccess(createdUser)
///     .Build();
///
/// Services.AddSingleton(mockService);
/// </summary>
public class UserManagementServiceMockBuilder
{
    private readonly Mock<AppIUserManagementService> _mockService;

    public UserManagementServiceMockBuilder()
    {
        // IUserManagementServiceインターフェースのモック作成
        // 【F#初学者向け解説】
        // Interface型のモックではコンストラクター引数は不要です。
        // F#の明示的インターフェース実装（interface IXxx with）により、
        // C#からはInterface経由でのみアクセス可能になるため、
        // Interface型のモックを作成することでテスト可能になります。
        _mockService = new Mock<AppIUserManagementService>();
    }

    #region GetAllUsersAsync モックセットアップ

    /// <summary>
    /// GetAllUsersAsync成功モックセットアップ
    ///
    /// 【引数】
    /// - users: 返却するユーザーリスト（F# Domain型）
    ///
    /// 【戻り値】
    /// FSharpResult&lt;User list, string&gt;.NewOk
    ///
    /// 【重要】Application層はF# Domain型を使用するため、UserDtoではなくF# Userを受け取ります
    /// </summary>
    public UserManagementServiceMockBuilder SetupGetAllUsersSuccess(List<FSharpDomainUser> users)
    {
        // C# List<User> → F# list<User> への変換
        // F#のlist型はImmutableなので、ListModule.OfSeqで変換が必要
        var fsharpUserList = Microsoft.FSharp.Collections.ListModule.OfSeq(users);

        // FSharpResult<list<User>, string> を作成
        var fsharpResult = FSharpResult<FSharpList<FSharpDomainUser>, string>.NewOk(fsharpUserList);

        _mockService
            .Setup(s => s.GetAllUsersAsync(It.IsAny<object>(), It.IsAny<string>()))
            .ReturnsAsync(fsharpResult);

        return this;
    }

    /// <summary>
    /// GetAllUsersAsync失敗モックセットアップ
    /// </summary>
    public UserManagementServiceMockBuilder SetupGetAllUsersFailure(string errorMessage)
    {
        var fsharpResult = FSharpResult<FSharpList<FSharpDomainUser>, string>.NewError(errorMessage);

        _mockService
            .Setup(s => s.GetAllUsersAsync(It.IsAny<object>(), It.IsAny<string>()))
            .ReturnsAsync(fsharpResult);

        return this;
    }

    #endregion

    #region GetUserByIdAsync モックセットアップ

    /// <summary>
    /// GetUserByIdAsync成功モックセットアップ
    ///
    /// 【引数】
    /// - user: 返却するユーザー（F# Domain型）
    ///
    /// 【戻り値】
    /// FSharpResult&lt;User, string&gt;.NewOk
    /// </summary>
    public UserManagementServiceMockBuilder SetupGetUserByIdSuccess(FSharpDomainUser user)
    {
        // FSharpResult<User, string> を作成
        var fsharpResult = FSharpResult<FSharpDomainUser, string>.NewOk(user);

        _mockService
            .Setup(s => s.GetUserByIdAsync(It.IsAny<FSharpUserId>(), It.IsAny<string>()))
            .ReturnsAsync(fsharpResult);

        return this;
    }

    /// <summary>
    /// GetUserByIdAsync失敗モックセットアップ
    /// </summary>
    public UserManagementServiceMockBuilder SetupGetUserByIdFailure(string errorMessage)
    {
        var fsharpResult = FSharpResult<FSharpDomainUser, string>.NewError(errorMessage);

        _mockService
            .Setup(s => s.GetUserByIdAsync(It.IsAny<FSharpUserId>(), It.IsAny<string>()))
            .ReturnsAsync(fsharpResult);

        return this;
    }

    #endregion

    #region CreateUserAsync モックセットアップ

    /// <summary>
    /// CreateUserAsync成功モックセットアップ
    ///
    /// 【引数】
    /// - createdUser: 作成されたユーザー（F# Domain型）
    ///
    /// 【戻り値】
    /// FSharpResult&lt;User, string&gt;.NewOk
    /// </summary>
    public UserManagementServiceMockBuilder SetupCreateUserSuccess(FSharpDomainUser createdUser)
    {
        // FSharpResult<User, string> を作成
        var fsharpResult = FSharpResult<FSharpDomainUser, string>.NewOk(createdUser);

        _mockService
            .Setup(s => s.CreateUserAsync(
                It.IsAny<string>(),      // email
                It.IsAny<string>(),      // name
                It.IsAny<string>(),      // password
                It.IsAny<string>(),      // role
                It.IsAny<FSharpList<long>>(),  // assignedProjectIds
                It.IsAny<string>()       // operatorIdentityId
            ))
            .ReturnsAsync(fsharpResult);

        return this;
    }

    /// <summary>
    /// CreateUserAsync失敗モックセットアップ（メール重複）
    /// </summary>
    public UserManagementServiceMockBuilder SetupCreateUserDuplicateEmail(string email)
    {
        var errorMessage = $"Email address '{email}' is already registered.";
        var fsharpResult = FSharpResult<FSharpDomainUser, string>.NewError(errorMessage);

        _mockService
            .Setup(s => s.CreateUserAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<FSharpList<long>>(),
                It.IsAny<string>()
            ))
            .ReturnsAsync(fsharpResult);

        return this;
    }

    /// <summary>
    /// CreateUserAsync失敗モックセットアップ（バリデーションエラー）
    /// </summary>
    public UserManagementServiceMockBuilder SetupCreateUserValidationError(string errorMessage)
    {
        var fsharpResult = FSharpResult<FSharpDomainUser, string>.NewError(errorMessage);

        _mockService
            .Setup(s => s.CreateUserAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<FSharpList<long>>(),
                It.IsAny<string>()
            ))
            .ReturnsAsync(fsharpResult);

        return this;
    }

    #endregion

    #region UpdateUserAsync モックセットアップ

    /// <summary>
    /// UpdateUserAsync成功モックセットアップ
    ///
    /// 【引数】
    /// - updatedUser: 更新されたユーザー（F# Domain型）
    ///
    /// 【戻り値】
    /// FSharpResult&lt;User, string&gt;.NewOk
    /// </summary>
    public UserManagementServiceMockBuilder SetupUpdateUserSuccess(FSharpDomainUser updatedUser)
    {
        // FSharpResult<User, string> を作成
        var fsharpResult = FSharpResult<FSharpDomainUser, string>.NewOk(updatedUser);

        _mockService
            .Setup(s => s.UpdateUserAsync(
                It.IsAny<FSharpUserId>(),    // userId
                It.IsAny<string>(),          // name
                It.IsAny<string>(),          // role
                It.IsAny<FSharpList<long>>(), // assignedProjectIds
                It.IsAny<bool>(),            // isActive
                It.IsAny<string>()           // operatorIdentityId
            ))
            .ReturnsAsync(fsharpResult);

        return this;
    }

    /// <summary>
    /// UpdateUserAsync失敗モックセットアップ
    /// </summary>
    public UserManagementServiceMockBuilder SetupUpdateUserFailure(string errorMessage)
    {
        var fsharpResult = FSharpResult<FSharpDomainUser, string>.NewError(errorMessage);

        _mockService
            .Setup(s => s.UpdateUserAsync(
                It.IsAny<FSharpUserId>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<FSharpList<long>>(),
                It.IsAny<bool>(),
                It.IsAny<string>()
            ))
            .ReturnsAsync(fsharpResult);

        return this;
    }

    #endregion

    #region DeactivateUserAsync モックセットアップ

    /// <summary>
    /// DeactivateUserAsync成功モックセットアップ
    ///
    /// 【戻り値】
    /// FSharpResult&lt;unit, string&gt;.NewOk（F# unit型）
    ///
    /// 【重要】F# unit型は構造体（値型）のため、default(unit)で有効な値を生成
    /// </summary>
    public UserManagementServiceMockBuilder SetupDeactivateUserSuccess()
    {
        // F# unit型は値型（struct）のため、default(Unit)で生成
        var unitValue = default(Unit);
        var fsharpResult = FSharpResult<Unit, string>.NewOk(unitValue);

        _mockService
            .Setup(s => s.DeactivateUserAsync(It.IsAny<FSharpUserId>(), It.IsAny<string>()))
            .ReturnsAsync(fsharpResult);

        return this;
    }

    /// <summary>
    /// DeactivateUserAsync失敗モックセットアップ
    /// </summary>
    public UserManagementServiceMockBuilder SetupDeactivateUserFailure(string errorMessage)
    {
        var fsharpResult = FSharpResult<Unit, string>.NewError(errorMessage);

        _mockService
            .Setup(s => s.DeactivateUserAsync(It.IsAny<FSharpUserId>(), It.IsAny<string>()))
            .ReturnsAsync(fsharpResult);

        return this;
    }

    #endregion

    #region ActivateUserAsync モックセットアップ

    /// <summary>
    /// ActivateUserAsync成功モックセットアップ
    ///
    /// 【戻り値】
    /// FSharpResult&lt;unit, string&gt;.NewOk（F# unit型）
    /// </summary>
    public UserManagementServiceMockBuilder SetupActivateUserSuccess()
    {
        // F# unit型は値型（struct）のため、default(Unit)で生成
        var unitValue = default(Unit);
        var fsharpResult = FSharpResult<Unit, string>.NewOk(unitValue);

        _mockService
            .Setup(s => s.ActivateUserAsync(It.IsAny<FSharpUserId>(), It.IsAny<string>()))
            .ReturnsAsync(fsharpResult);

        return this;
    }

    /// <summary>
    /// ActivateUserAsync失敗モックセットアップ
    /// </summary>
    public UserManagementServiceMockBuilder SetupActivateUserFailure(string errorMessage)
    {
        var fsharpResult = FSharpResult<Unit, string>.NewError(errorMessage);

        _mockService
            .Setup(s => s.ActivateUserAsync(It.IsAny<FSharpUserId>(), It.IsAny<string>()))
            .ReturnsAsync(fsharpResult);

        return this;
    }

    #endregion

    #region ビルド

    /// <summary>
    /// モックインスタンス取得（IUserManagementService）
    /// </summary>
    public AppIUserManagementService Build() => _mockService.Object;

    /// <summary>
    /// Mockオブジェクト取得（検証用）
    ///
    /// 【使用例】
    /// var mock = builder.BuildMock();
    /// mock.Verify(s => s.GetAllUsersAsync(It.IsAny&lt;object&gt;(), It.IsAny&lt;UserId&gt;()), Times.Once);
    /// </summary>
    public Mock<AppIUserManagementService> BuildMock() => _mockService;

    #endregion
}
