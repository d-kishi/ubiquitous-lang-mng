# Development Guidelines - 開発ガイドライン

## 🎯 基本開発方針

### Clean Architecture準拠（ADR準拠）
- **依存方向**: Web→Contracts→Application→Domain + Infrastructure→Application
- **F#/C#ハイブリッド**: Domain/Application(F#) + Infrastructure/Web/Contracts(C#)
- **境界統合**: TypeConverter基盤・66テストケース成功実証

### 品質基準（絶対遵守）
- **0 Warning, 0 Error**: 必須維持・例外なし
- **テスト成功**: 106/106テスト成功継続必須
- **Clean Architectureスコア**: 94/100点維持・継続改善

## 🤖 SubAgent活用パターン

### 実証済み効果的パターン
1. **csharp-web-ui**: Blazor Component・Razor・フロントエンドUI・リアルタイム機能
2. **csharp-infrastructure**: EF Core・Repository・データベースアクセス・外部サービス連携
3. **fsharp-application**: F#ドメインモデル・ビジネスロジック・関数型プログラミング
4. **fsharp-domain**: F#ドメインモデル設計・Railway-oriented Programming適用
5. **contracts-bridge**: F#↔C#型変換・TypeConverter・双方向データ変換

### Phase A9 Step 1成功事例（2025-09-10）
- **csharp-web-ui**: JsonSerializerService実装・20分・高品質成果
  - DI登録・統一設定・初学者向けコメント完備
  - 技術負債予防・DRY原則完全準拠実現

## 🔧 技術実装パターン

### JsonSerializerService一括管理（2025-09-10確立）
```csharp
// Program.cs DI登録
builder.Services.AddScoped<IJsonSerializerService, JsonSerializerService>();

// Blazor Component利用パターン
@inject IJsonSerializerService JsonSerializer
var parsedResult = JsonSerializer.Deserialize<PasswordChangeApiResponse>(resultJson);
```

**効果**:
- **ConfigureHttpJsonOptions制約解決**: Web API専用設定をBlazor Component統一適用
- **技術負債予防**: 新規Component自動適用・設定漏れ防止
- **保守性向上**: 一箇所変更で全体反映・将来拡張対応

### F# Railway-oriented Programming
```fsharp
type AuthenticationError = 
  | InvalidCredentials
  | UserNotFound
  | PasswordExpired
  | AccountLocked
  | PasswordRequired
  | TooManyAttempts
  | SystemError

type IAuthenticationService =
  abstract member AuthenticateAsync : email:string -> password:string -> Task<Result<AuthenticationResult, AuthenticationError>>
```

### E2Eテストパターン（実証済み）
1. **シナリオ1**: 初回ログイン→パスワード変更
2. **シナリオ2**: パスワード変更後通常ログイン
3. **シナリオ3**: F# Authentication Service統合確認・エラーハンドリング確認

## 📋 開発プロセス

### セッション開始時必須チェック
1. **必読ファイル確認**: プロジェクト状況・Phase計画・前回成果記録
2. **品質状況確認**: ビルド状態・テスト成功率・Clean Architectureスコア
3. **DB状態確認**: admin@ubiquitous-lang.com初期状態・必要に応じて復元
4. **開発環境確認**: docker-compose up -d・dotnet run・https://localhost:5001

### 実装時必須手順
1. **SubAgent選択**: 作業内容に最適な専門Agent選択
2. **段階的実装**: 小さな単位・テスト実行・品質確認サイクル
3. **品質確認**: dotnet build・dotnet test・0警告0エラー確認
4. **E2E確認**: 実際の画面操作・ユーザーフロー動作確認

### セッション終了時必須プロセス
1. **成果記録**: 日次記録作成・具体的成果・学習事項記録
2. **プロジェクト状況更新**: 進捗反映・次回推奨範囲設定
3. **Serenaメモリー更新**: 5種類メモリー更新・次回参照準備
4. **品質最終確認**: ビルド状態・テスト成功・DB状態確認

## 🎯 技術負債管理方針

### 予防優先アプローチ（Phase A9 Step 1実証）
- **問題発見時**: 個別対応ではなく根本解決・システム改善実施
- **DRY原則徹底**: 重複実装排除・設定一元管理・保守性重視
- **将来拡張性**: 新規実装時自動適用・技術負債予防実現

### 解決済み技術負債（参考）
- **TECH-004**: 初回ログイン時パスワード変更未実装 → **完全解決**（2025-09-09）
- **JsonSerializerOptions個別設定**: 重複・設定漏れリスク → **一括管理で根本解決**（2025-09-10）

## 🔍 初学者対応方針（ADR_010準拠）

### Blazor Server初学者向けコメント必須
```razor
@* 【Blazor Server初学者向け解説】 *@
@* パスワード変更コンポーネントの状態管理 *@
@* TECH-006: JavaScript API呼び出しによる認証統合 *@
```

### F#初学者向けコメント必須
```fsharp
// 【F# Railway-oriented Programming】
// Result型を使用したエラーハンドリング
// 成功時: Ok(AuthenticationResult)
// 失敗時: Error(AuthenticationError)
```

## ⚠️ 重要制約・注意点

### DB操作制約
- **E2Eテスト後復元**: `/scripts/restore-admin-user.sql`実行必須
- **初期状態**: admin@ubiquitous-lang.com・初期パスワード'su'・IsFirstLogin=true

### 開発環境制約
- **HTTPS必須**: https://localhost:5001（HTTP非対応）
- **Docker依存**: PostgreSQL・PgAdmin・Smtp4dev要起動
- **ポート統一**: 5001番固定（Issue #16解決済み）

### 実装制約
- **用語統一**: 「用語」ではなく「ユビキタス言語」使用（ADR_003準拠）
- **ADR遵守**: 重要技術決定はADR記録・参照必須
- **コメント必須**: 初学者対応・概念説明・実装理由明記

## 📊 Phase A9実装経験（2025-09-10）

### 成功パターン
1. **問題発見→根本解決**: ConfigureHttpJsonOptions制約→JsonSerializerService一括管理
2. **SubAgent活用**: csharp-web-ui・20分・高品質成果・初学者対応完備
3. **E2E確認**: 3シナリオ・F# Authentication Service統合動作確認
4. **品質向上**: Clean Architecture 89点→94点（+5点達成）

### 学習事項
- **ConfigureHttpJsonOptions**: Web API専用・Blazor Component適用不可
- **JsonSerializerService**: DI経由統一設定・技術負債予防効果
- **Railway-oriented Programming**: F# Authentication Service・適切なエラーハンドリング
- **Clean Architecture効果**: Infrastructure→Application依存方向完全遵守

## 🚀 次回Phase A9 Step 2準備

### 認証処理重複実装統一
- **対象箇所**: Infrastructure/Services/AuthenticationService.cs:64-146・Web/Services/AuthenticationService.cs・Web/Controllers/AuthApiController.cs
- **SubAgent**: csharp-web-ui + csharp-infrastructure
- **目標**: 単一責任原則達成・Infrastructure層認証サービス一本化
- **予想時間**: 120分・アーキテクチャ改善重視