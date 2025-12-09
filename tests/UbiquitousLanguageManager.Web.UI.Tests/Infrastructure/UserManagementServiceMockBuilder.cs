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

    #region GetAllUsersWithIdentityAsync モックセットアップ

    /// <summary>
    /// GetAllUsersWithIdentityAsync成功モックセットアップ
    ///
    /// 【引数】
    /// - userTuples: 返却するユーザーリスト（F# Domain型User * IdentityIdタプル）
    ///
    /// 【戻り値】
    /// FSharpResult&lt;(User * string) list, string&gt;.NewOk
    ///
    /// 【重要】Phase B-F3リファクタリングによりシグネチャ変更
    /// - 旧: GetAllUsersAsync → User list を返す
    /// - 新: GetAllUsersWithIdentityAsync → (User * IdentityId) list を返す
    ///
    /// 【F#初学者向け解説】
    /// - F#タプル型 (User * string) は、C#ではTuple<User, string>として扱います
    /// - タプルは2つの値をペアで保持する不変データ構造です
    /// - F# list型はImmutableなので、ListModule.OfSeqで変換が必要です
    /// </summary>
    public UserManagementServiceMockBuilder SetupGetAllUsersWithIdentitySuccess(List<Tuple<FSharpDomainUser, string>> userTuples)
    {
        // C# List<Tuple<User, string>> → F# list<User * string> への変換
        // F#のlist型はImmutableなので、ListModule.OfSeqで変換が必要
        var fsharpTupleList = Microsoft.FSharp.Collections.ListModule.OfSeq(userTuples);

        // FSharpResult<list<User * string>, string> を作成
        var fsharpResult = FSharpResult<FSharpList<Tuple<FSharpDomainUser, string>>, string>.NewOk(fsharpTupleList);

        _mockService
            .Setup(s => s.GetAllUsersWithIdentityAsync(It.IsAny<object>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(fsharpResult);

        return this;
    }

    /// <summary>
    /// GetAllUsersWithIdentityAsync失敗モックセットアップ
    /// </summary>
    public UserManagementServiceMockBuilder SetupGetAllUsersWithIdentityFailure(string errorMessage)
    {
        var fsharpResult = FSharpResult<FSharpList<Tuple<FSharpDomainUser, string>>, string>.NewError(errorMessage);

        _mockService
            .Setup(s => s.GetAllUsersWithIdentityAsync(It.IsAny<object>(), It.IsAny<string>(), It.IsAny<bool>()))
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
    ///
    /// 【重要】Phase B-F3リファクタリングによりシグネチャ変更
    /// - 旧: UpdateUserAsync(userId: UserId, ...) → UserId型
    /// - 新: UpdateUserAsync(targetIdentityId: string, ...) → string型（IdentityId）
    /// </summary>
    public UserManagementServiceMockBuilder SetupUpdateUserSuccess(FSharpDomainUser updatedUser)
    {
        // FSharpResult<User, string> を作成
        var fsharpResult = FSharpResult<FSharpDomainUser, string>.NewOk(updatedUser);

        _mockService
            .Setup(s => s.UpdateUserAsync(
                It.IsAny<string>(),          // targetIdentityId (変更: UserId → string)
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
                It.IsAny<string>(),          // targetIdentityId (変更: UserId → string)
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

    #region DeleteUserAsync モックセットアップ

    /// <summary>
    /// DeleteUserAsync成功モックセットアップ
    ///
    /// 【戻り値】
    /// FSharpResult&lt;unit, string&gt;.NewOk（F# unit型）
    ///
    /// 【重要】Phase B-F3リファクタリングによりシグネチャ変更
    /// - 旧: DeleteUserAsync(userId: UserId, ...) → UserId型
    /// - 新: DeleteUserAsync(targetIdentityId: string, ...) → string型（IdentityId）
    ///
    /// 【F#初学者向け解説】
    /// - F# unit型は構造体（値型）のため、default(Unit)で有効な値を生成します
    /// - unit型は「戻り値なし」を表す型（C#のvoidに相当しますが、型として扱えます）
    /// </summary>
    public UserManagementServiceMockBuilder SetupDeleteUserSuccess()
    {
        // F# unit型は値型（struct）のため、default(Unit)で生成
        var unitValue = default(Unit);
        var fsharpResult = FSharpResult<Unit, string>.NewOk(unitValue);

        _mockService
            .Setup(s => s.DeleteUserAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(fsharpResult);

        return this;
    }

    /// <summary>
    /// DeleteUserAsync失敗モックセットアップ
    /// </summary>
    public UserManagementServiceMockBuilder SetupDeleteUserFailure(string errorMessage)
    {
        var fsharpResult = FSharpResult<Unit, string>.NewError(errorMessage);

        _mockService
            .Setup(s => s.DeleteUserAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(fsharpResult);

        return this;
    }

    #endregion

    #region GetProjectIdsByEmailAsync モックセットアップ

    /// <summary>
    /// GetProjectIdsByEmailAsync成功モックセットアップ（デフォルト: 空リスト）
    ///
    /// 【引数】
    /// - projectIds: 返却するプロジェクトIDリスト（省略時は空リスト）
    ///
    /// 【戻り値】
    /// FSharpResult&lt;int64 list, string&gt;.NewOk
    ///
    /// 【補足】
    /// - Index.razorの各ユーザーに対してプロジェクト情報を取得する際に使用
    /// - モック未設定の場合、テストで例外が発生する
    /// </summary>
    public UserManagementServiceMockBuilder SetupGetProjectIdsByEmailSuccess(List<long>? projectIds = null)
    {
        // デフォルトは空リスト
        var ids = projectIds ?? new List<long>();
        var fsharpList = Microsoft.FSharp.Collections.ListModule.OfSeq(ids);
        var fsharpResult = FSharpResult<FSharpList<long>, string>.NewOk(fsharpList);

        _mockService
            .Setup(s => s.GetProjectIdsByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(fsharpResult);

        return this;
    }

    /// <summary>
    /// GetProjectIdsByEmailAsync失敗モックセットアップ
    /// </summary>
    public UserManagementServiceMockBuilder SetupGetProjectIdsByEmailFailure(string errorMessage)
    {
        var fsharpResult = FSharpResult<FSharpList<long>, string>.NewError(errorMessage);

        _mockService
            .Setup(s => s.GetProjectIdsByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(fsharpResult);

        return this;
    }

    #endregion

    #region GetProjectIdsByUserIdAsync モックセットアップ

    /// <summary>
    /// GetProjectIdsByUserIdAsync成功モックセットアップ（デフォルト: 空リスト）
    ///
    /// 【引数】
    /// - projectIds: 返却するプロジェクトIDリスト（省略時は空リスト）
    ///
    /// 【戻り値】
    /// FSharpResult&lt;int64 list, string&gt;.NewOk
    ///
    /// 【補足】
    /// - Edit.razorのLoadUserAsync()内でユーザーのプロジェクト割り当て情報を取得
    /// - AssignedProjectIds復元に使用（Phase B-F3 Stage4実装）
    /// </summary>
    public UserManagementServiceMockBuilder SetupGetProjectIdsByUserIdSuccess(List<long>? projectIds = null)
    {
        // デフォルトは空リスト
        var ids = projectIds ?? new List<long>();
        var fsharpList = Microsoft.FSharp.Collections.ListModule.OfSeq(ids);
        var fsharpResult = FSharpResult<FSharpList<long>, string>.NewOk(fsharpList);

        _mockService
            .Setup(s => s.GetProjectIdsByUserIdAsync(It.IsAny<FSharpUserId>()))
            .ReturnsAsync(fsharpResult);

        return this;
    }

    /// <summary>
    /// GetProjectIdsByUserIdAsync失敗モックセットアップ
    /// </summary>
    public UserManagementServiceMockBuilder SetupGetProjectIdsByUserIdFailure(string errorMessage)
    {
        var fsharpResult = FSharpResult<FSharpList<long>, string>.NewError(errorMessage);

        _mockService
            .Setup(s => s.GetProjectIdsByUserIdAsync(It.IsAny<FSharpUserId>()))
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
