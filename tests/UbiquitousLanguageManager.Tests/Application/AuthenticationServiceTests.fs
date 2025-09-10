namespace UbiquitousLanguageManager.Tests.Application

open Microsoft.Extensions.Logging
open Moq
open UbiquitousLanguageManager.Application
open UbiquitousLanguageManager.Domain
open System.Threading.Tasks
open Xunit
open System

// 🧪 F# Application層認証サービステスト
// Phase A9 Step 1-1: Railway-oriented Programming・Result型・Smart Constructorのテスト

// 📝 【F#初学者向け解説】
// F#でxUnitテストを書く方法：
// 1. [<Fact>]属性でテストメソッドを定義
// 2. Assert.Equal、Assert.Trueなどでアサーション
// 3. task{}でAsyncテストを実装
// 4. Mockライブラリを使用してDependency Injectionテスト

type AuthenticationServiceTests() =
    
    // 🔧 テスト用モック作成ヘルパー
    // 【F#初学者向け解説】
    // F#でのMockライブラリ使用方法。C#とほぼ同じですが、
    // F#の型推論により、より簡潔に記述できます。
    let createMockUserRepository() = Mock<IUserRepository>()
    let createMockAuthService() = Mock<IAuthenticationService>()
    let createMockLogger() = Mock<ILogger<AuthenticationApplicationService>>()
    
    // 🔧 テスト用ユーザー作成ヘルパー
    let createTestUser emailStr nameStr =
        let email = Email.create emailStr |> Result.defaultWith (fun _ -> failwith "テストデータエラー")
        let name = UserName.create nameStr |> Result.defaultWith (fun _ -> failwith "テストデータエラー")
        User.create email name GeneralUser (UserId.create 1L)
    
    // 🔧 テスト用認証リクエスト作成ヘルパー
    let createAuthRequest email password rememberMe = {
        Email = email
        Password = password  
        RememberMe = rememberMe
    }

    // ✅ 正常な認証フローのテスト
    [<Fact>]
    member this.``AuthenticateUserAsync_ValidCredentials_ShouldReturnAuthenticationSuccess``() =
        task {
            // Arrange
            let mockUserRepo = createMockUserRepository()
            let mockAuthService = createMockAuthService()
            let mockLogger = createMockLogger()
            
            let testUser = createTestUser "test@example.com" "テストユーザー"
            let testEmail = Email.create "test@example.com" |> Result.defaultWith (fun _ -> failwith "テスト失敗")
            let authRequest = createAuthRequest "test@example.com" "ValidPassword123" false
            
            // モック設定：ユーザー存在確認
            mockUserRepo
                .Setup(fun x -> x.GetByEmailAsync(It.IsAny<Email>()))
                .ReturnsAsync(Ok (Some testUser))
            
            // モック設定：認証成功
            mockAuthService
                .Setup(fun x -> x.LoginAsync(It.IsAny<Email>(), It.IsAny<string>()))
                .ReturnsAsync(Ok testUser)
            
            // モック設定：ユーザー保存（認証成功記録）
            mockUserRepo
                .Setup(fun x -> x.SaveAsync(It.IsAny<User>()))
                .ReturnsAsync(Ok testUser)
            
            // モック設定：トークン生成
            mockAuthService
                .Setup(fun x -> x.GenerateTokenAsync(It.IsAny<User>()))
                .ReturnsAsync(Ok "test-token-12345")
            
            let authAppService = AuthenticationApplicationService(mockUserRepo.Object, mockAuthService.Object, mockLogger.Object)
            
            // Act
            let! result = authAppService.AuthenticateUserAsync(authRequest)
            
            // Assert
            match result with
            | Ok (AuthenticationSuccess (user, token)) ->
                Assert.Equal("test@example.com", user.Email.Value)
                Assert.Equal("test-token-12345", token)
            | Ok _ -> 
                Assert.True(false, "期待されていない認証結果タイプです")
            | Error error -> 
                Assert.True(false, $"認証が失敗しました: {error}")
        }

    // ❌ 無効なメールアドレスフォーマットテスト
    [<Fact>]
    member this.``AuthenticateUserAsync_InvalidEmailFormat_ShouldReturnValidationError``() =
        task {
            // Arrange
            let mockUserRepo = createMockUserRepository()
            let mockAuthService = createMockAuthService()
            let mockLogger = createMockLogger()
            
            let authRequest = createAuthRequest "invalid-email" "ValidPassword123" false
            let authAppService = AuthenticationApplicationService(mockUserRepo.Object, mockAuthService.Object, mockLogger.Object)
            
            // Act
            let! result = authAppService.AuthenticateUserAsync(authRequest)
            
            // Assert
            // 【F#初学者向け解説】
            // パターンマッチングを使用してResult型の内容を検査します。
            // Error側にValidationErrorが含まれることを確認します。
            match result with
            | Error (ValidationError errorMsg) ->
                Assert.Contains("有効なメールアドレス形式", errorMsg)
            | Ok _ -> 
                Assert.True(false, "バリデーションエラーが期待されましたが、認証が成功しました")
            | Error otherError -> 
                Assert.True(false, $"予期しないエラータイプです: {otherError}")
        }

    // ❌ ユーザー未存在テスト
    [<Fact>]
    member this.``AuthenticateUserAsync_UserNotFound_ShouldReturnUserNotFoundError``() =
        task {
            // Arrange
            let mockUserRepo = createMockUserRepository()
            let mockAuthService = createMockAuthService()
            let mockLogger = createMockLogger()
            
            let authRequest = createAuthRequest "notfound@example.com" "ValidPassword123" false
            
            // モック設定：ユーザー未存在
            mockUserRepo
                .Setup(fun x -> x.GetByEmailAsync(It.IsAny<Email>()))
                .ReturnsAsync(Ok None)
            
            let authAppService = AuthenticationApplicationService(mockUserRepo.Object, mockAuthService.Object, mockLogger.Object)
            
            // Act
            let! result = authAppService.AuthenticateUserAsync(authRequest)
            
            // Assert
            match result with
            | Error (UserNotFound email) ->
                Assert.Equal("notfound@example.com", email.Value)
            | Ok _ -> 
                Assert.True(false, "ユーザー未存在エラーが期待されましたが、認証が成功しました")
            | Error otherError -> 
                Assert.True(false, $"予期しないエラータイプです: {otherError}")
        }

    // 🔒 アカウントロック状態テスト
    [<Fact>]
    member this.``AuthenticateUserAsync_AccountLocked_ShouldReturnAccountLockedError``() =
        task {
            // Arrange
            let mockUserRepo = createMockUserRepository()
            let mockAuthService = createMockAuthService()
            let mockLogger = createMockLogger()
            
            let testUser = createTestUser "locked@example.com" "ロックユーザー"
            // ロックアウト状態のユーザーを作成（30分後解除）
            let lockedUser = { testUser with LockoutEnd = Some (DateTime.UtcNow.AddMinutes(30.0)) }
            
            let authRequest = createAuthRequest "locked@example.com" "ValidPassword123" false
            
            // モック設定：ロックされたユーザー取得
            mockUserRepo
                .Setup(fun x -> x.GetByEmailAsync(It.IsAny<Email>()))
                .ReturnsAsync(Ok (Some lockedUser))
            
            let authAppService = AuthenticationApplicationService(mockUserRepo.Object, mockAuthService.Object, mockLogger.Object)
            
            // Act
            let! result = authAppService.AuthenticateUserAsync(authRequest)
            
            // Assert
            match result with
            | Error (AccountLocked lockoutEnd) ->
                Assert.True(lockoutEnd > DateTime.UtcNow)
            | Ok _ -> 
                Assert.True(false, "アカウントロックエラーが期待されましたが、認証が成功しました")
            | Error otherError -> 
                Assert.True(false, $"予期しないエラータイプです: {otherError}")
        }

    // 🚫 非アクティブアカウントテスト
    [<Fact>]
    member this.``AuthenticateUserAsync_DeactivatedAccount_ShouldReturnAccountDeactivatedError``() =
        task {
            // Arrange
            let mockUserRepo = createMockUserRepository()
            let mockAuthService = createMockAuthService()
            let mockLogger = createMockLogger()
            
            let testUser = createTestUser "deactivated@example.com" "無効ユーザー"
            let deactivatedUser = { testUser with IsActive = false }
            
            let authRequest = createAuthRequest "deactivated@example.com" "ValidPassword123" false
            
            // モック設定：無効化されたユーザー取得
            mockUserRepo
                .Setup(fun x -> x.GetByEmailAsync(It.IsAny<Email>()))
                .ReturnsAsync(Ok (Some deactivatedUser))
            
            let authAppService = AuthenticationApplicationService(mockUserRepo.Object, mockAuthService.Object, mockLogger.Object)
            
            // Act
            let! result = authAppService.AuthenticateUserAsync(authRequest)
            
            // Assert
            match result with
            | Error AccountDeactivated ->
                Assert.True(true)  // 期待通りのエラー
            | Ok _ -> 
                Assert.True(false, "アカウント無効化エラーが期待されましたが、認証が成功しました")
            | Error otherError -> 
                Assert.True(false, $"予期しないエラータイプです: {otherError}")
        }

    // 🔐 初回ログイン要求テスト
    [<Fact>]
    member this.``AuthenticateUserAsync_FirstLogin_ShouldReturnFirstLoginRequired``() =
        task {
            // Arrange
            let mockUserRepo = createMockUserRepository()
            let mockAuthService = createMockAuthService()
            let mockLogger = createMockLogger()
            
            let testUser = createTestUser "firstlogin@example.com" "初回ユーザー"
            let firstLoginUser = { testUser with IsFirstLogin = true }
            
            let authRequest = createAuthRequest "firstlogin@example.com" "ValidPassword123" false
            
            // モック設定：初回ログインユーザー取得
            mockUserRepo
                .Setup(fun x -> x.GetByEmailAsync(It.IsAny<Email>()))
                .ReturnsAsync(Ok (Some firstLoginUser))
            
            // モック設定：認証成功
            mockAuthService
                .Setup(fun x -> x.LoginAsync(It.IsAny<Email>(), It.IsAny<string>()))
                .ReturnsAsync(Ok firstLoginUser)
            
            // モック設定：ユーザー保存
            mockUserRepo
                .Setup(fun x -> x.SaveAsync(It.IsAny<User>()))
                .ReturnsAsync(Ok firstLoginUser)
            
            let authAppService = AuthenticationApplicationService(mockUserRepo.Object, mockAuthService.Object, mockLogger.Object)
            
            // Act
            let! result = authAppService.AuthenticateUserAsync(authRequest)
            
            // Assert
            match result with
            | Ok (FirstLoginRequired user) ->
                Assert.Equal("firstlogin@example.com", user.Email.Value)
                Assert.True(user.IsFirstLogin)
            | Ok (AuthenticationSuccess _) -> 
                Assert.True(false, "初回ログイン要求が期待されましたが、通常認証が成功しました")
            | Error error -> 
                Assert.True(false, $"認証が失敗しました: {error}")
        }

    // 🔐 パスワード変更テスト（成功ケース）
    [<Fact>]
    member this.``ChangeUserPasswordAsync_ValidRequest_ShouldReturnUpdatedUser``() =
        task {
            // Arrange
            let mockUserRepo = createMockUserRepository()
            let mockAuthService = createMockAuthService()
            let mockLogger = createMockLogger()
            
            let testUser = createTestUser "changepass@example.com" "パスワード変更ユーザー"
            let userId = UserId.create 1L
            let currentPassword = "OldPassword123"
            let newPassword = Password.create "NewPassword456" |> Result.defaultWith (fun _ -> failwith "テストデータエラー")
            let expectedPasswordHash = PasswordHash.create "hashed-new-password" |> Result.defaultWith (fun _ -> failwith "テストデータエラー")
            
            // モック設定：ユーザー取得
            mockUserRepo
                .Setup(fun x -> x.GetByIdAsync(userId))
                .ReturnsAsync(Ok (Some testUser))
            
            // モック設定：パスワード変更
            mockAuthService
                .Setup(fun x -> x.ChangePasswordAsync(userId, currentPassword, newPassword))
                .ReturnsAsync(Ok expectedPasswordHash)
            
            // モック設定：ユーザー保存
            mockUserRepo
                .Setup(fun x -> x.SaveAsync(It.IsAny<User>()))
                .ReturnsAsync(Ok testUser)
            
            let authAppService = AuthenticationApplicationService(mockUserRepo.Object, mockAuthService.Object, mockLogger.Object)
            
            // Act
            let! result = authAppService.ChangeUserPasswordAsync(userId, currentPassword, newPassword)
            
            // Assert
            match result with
            | Ok updatedUser ->
                Assert.Equal("changepass@example.com", updatedUser.Email.Value)
            | Error error -> 
                Assert.True(false, $"パスワード変更が失敗しました: {error}")
        }

    // 🔐 ユーザー作成テスト（成功ケース）
    [<Fact>]
    member this.``CreateUserWithPasswordAsync_ValidRequest_ShouldReturnCreatedUser``() =
        task {
            // Arrange
            let mockUserRepo = createMockUserRepository()
            let mockAuthService = createMockAuthService()
            let mockLogger = createMockLogger()
            
            let email = Email.create "newuser@example.com" |> Result.defaultWith (fun _ -> failwith "テストデータエラー")
            let name = UserName.create "新規ユーザー" |> Result.defaultWith (fun _ -> failwith "テストデータエラー")
            let role = GeneralUser
            let password = Password.create "ValidPassword123" |> Result.defaultWith (fun _ -> failwith "テストデータエラー")
            let createdBy = UserId.create 1L
            let expectedUser = createTestUser "newuser@example.com" "新規ユーザー"
            
            // モック設定：既存ユーザー検索（未存在）
            mockUserRepo
                .Setup(fun x -> x.GetByEmailAsync(email))
                .ReturnsAsync(Ok None)
            
            // モック設定：ユーザー作成
            mockAuthService
                .Setup(fun x -> x.CreateUserWithPasswordAsync(email, name, role, password, createdBy))
                .ReturnsAsync(Ok expectedUser)
            
            let authAppService = AuthenticationApplicationService(mockUserRepo.Object, mockAuthService.Object, mockLogger.Object)
            
            // Act
            let! result = authAppService.CreateUserWithPasswordAsync(email, name, role, password, createdBy)
            
            // Assert
            match result with
            | Ok createdUser ->
                Assert.Equal("newuser@example.com", createdUser.Email.Value)
                Assert.Equal("新規ユーザー", createdUser.Name.Value)
            | Error error -> 
                Assert.True(false, $"ユーザー作成が失敗しました: {error}")
        }

    // ❌ 重複メールアドレステスト
    [<Fact>]
    member this.``CreateUserWithPasswordAsync_DuplicateEmail_ShouldReturnValidationError``() =
        task {
            // Arrange
            let mockUserRepo = createMockUserRepository()
            let mockAuthService = createMockAuthService()
            let mockLogger = createMockLogger()
            
            let email = Email.create "existing@example.com" |> Result.defaultWith (fun _ -> failwith "テストデータエラー")
            let name = UserName.create "重複ユーザー" |> Result.defaultWith (fun _ -> failwith "テストデータエラー")
            let role = GeneralUser
            let password = Password.create "ValidPassword123" |> Result.defaultWith (fun _ -> failwith "テストデータエラー")
            let createdBy = UserId.create 1L
            let existingUser = createTestUser "existing@example.com" "既存ユーザー"
            
            // モック設定：既存ユーザー検索（存在）
            mockUserRepo
                .Setup(fun x -> x.GetByEmailAsync(email))
                .ReturnsAsync(Ok (Some existingUser))
            
            let authAppService = AuthenticationApplicationService(mockUserRepo.Object, mockAuthService.Object, mockLogger.Object)
            
            // Act
            let! result = authAppService.CreateUserWithPasswordAsync(email, name, role, password, createdBy)
            
            // Assert
            match result with
            | Error (ValidationError errorMsg) ->
                Assert.Contains("既に使用されています", errorMsg)
            | Ok _ -> 
                Assert.True(false, "重複エラーが期待されましたが、ユーザー作成が成功しました")
            | Error otherError -> 
                Assert.True(false, $"予期しないエラータイプです: {otherError}")
        }

    // 🔍 Railway-oriented Programming統合テスト
    [<Fact>]
    member this.``AuthenticationUseCases_LoginUser_ValidCredentials_ShouldReturnSuccessResult``() =
        task {
            // Arrange
            let mockUserRepo = createMockUserRepository()
            let mockAuthService = createMockAuthService()
            let mockLogger = createMockLogger()
            
            let testUser = createTestUser "railwaytest@example.com" "Railwayテストユーザー"
            let authRequest = createAuthRequest "railwaytest@example.com" "ValidPassword123" false
            
            // モック設定
            mockUserRepo
                .Setup(fun x -> x.GetByEmailAsync(It.IsAny<Email>()))
                .ReturnsAsync(Ok (Some testUser))
            
            mockAuthService
                .Setup(fun x -> x.LoginAsync(It.IsAny<Email>(), It.IsAny<string>()))
                .ReturnsAsync(Ok testUser)
            
            mockUserRepo
                .Setup(fun x -> x.SaveAsync(It.IsAny<User>()))
                .ReturnsAsync(Ok testUser)
            
            mockAuthService
                .Setup(fun x -> x.GenerateTokenAsync(It.IsAny<User>()))
                .ReturnsAsync(Ok "railway-token-12345")
            
            let authAppService = AuthenticationApplicationService(mockUserRepo.Object, mockAuthService.Object, mockLogger.Object)
            
            // Act：関数型スタイルのユースケース使用
            let! result = AuthenticationUseCases.loginUser authAppService authRequest
            
            // Assert
            match result with
            | Ok loginResult ->
                Assert.Equal("railwaytest@example.com", loginResult.User.Email.Value)
                Assert.Equal(Some "railway-token-12345", loginResult.Token)
                Assert.False(loginResult.RequiresPasswordChange)
                Assert.False(loginResult.RequiresEmailConfirmation)
            | Error error -> 
                Assert.True(false, $"Railway-oriented Programming認証が失敗しました: {error}")
        }