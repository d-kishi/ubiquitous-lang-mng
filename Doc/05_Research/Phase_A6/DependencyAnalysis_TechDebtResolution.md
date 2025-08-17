# Phase A6 技術負債修正 依存関係分析結果

**分析日**: 2025-08-14  
**分析者**: dependency-analysis Agent  
**対象**: TECH-002～004修正の影響範囲・依存関係・制約条件

## 分析対象

### 技術負債一覧
- **TECH-002**: 初期スーパーユーザーパスワード仕様不整合（"TempPass123!" vs "su"）
- **TECH-003**: ログイン画面の重複（MVC版 vs Blazor Server版）
- **TECH-004**: 初回ログイン時パスワード変更機能未実装

## 依存関係マップ

### 技術的依存関係

| 依存元 | 依存先 | 依存種別 | 結合度 | リスク |
|-------|-------|----------|--------|--------|
| TECH-002 | appsettings.json/Development.json | 設定ファイル | 弱 | 低 |
| TECH-002 | InitialDataService.cs | 初期化サービス | 中 | 中 |
| TECH-003 | AccountController.cs (MVC) | コントローラー削除 | 強 | 高 |
| TECH-003 | Login.razor (Blazor) | 認証UI統合 | 強 | 高 |
| TECH-003 | Views/Account/Login.cshtml | ビューファイル削除 | 中 | 中 |
| TECH-004 | ChangePassword.razor (未実装) | パスワード変更画面 | 強 | 高 |
| TECH-004 | Login.razor初回判定ロジック | 分岐処理 | 中 | 中 |
| TECH-004 | ApplicationUser.IsFirstLogin | データベースフィールド | 弱 | 低 |

## 実装順序推奨

### Phase 1: 基盤修正（並列実行可能）
1. **TECH-002 Step1**: appsettings.json パスワード修正
   - 影響範囲: 設定ファイルのみ
   - リスク: 低
   - 所要時間: 5-10分

### Phase 2: UI統合・認証フロー修正（順次実行）
2. **TECH-004 Step1**: ChangePassword.razorページ作成
   - 前提条件: なし
   - 依存関係: 独立実装可能
   - 所要時間: 20-30分

3. **TECH-003 Step1**: Blazor認証機能実装
   - 前提条件: TECH-004 Step1完了
   - 依存関係: ChangePassword.razorが必要
   - 所要時間: 25-35分

4. **TECH-003 Step2**: MVC版削除
   - 前提条件: TECH-003 Step1完了
   - 依存関係: Blazor認証動作確認後
   - 所要時間: 10-15分

### Phase 3: 統合テスト・品質確認（順次実行）
5. **統合テスト**: 認証フロー完全確認
   - 前提条件: Phase 1-2完了
   - 所要時間: 15-20分

## 重要な発見事項

### 最高優先度課題
**TECH-004 重要な未実装**: `/change-password`ページが存在しない
- **現状**: Login.razorで`/change-password`にリダイレクトしているが、ページが未実装
- **影響**: 初回ログイン時に404エラーが発生
- **対策**: ChangePassword.razorの緊急実装が必要

### 循環依存・競合リスク
1. **TECH-003 認証フロー依存関係**: Blazor認証とMVC認証の競合リスク
   - **現状**: 両方のログイン画面が存在し、認証フローが重複
   - **影響**: ユーザー混乱・セキュリティリスク
   - **対策**: Blazor認証完全実装後にMVC版削除

2. **設定ファイル不整合**: appsettings.json vs Development.json
   - **現状**: Development.jsonは"su"、appsettings.jsonは"TempPass123!"
   - **影響**: 環境による認証失敗
   - **対策**: 統一パスワード設定

## 制約・前提条件

### 技術制約
1. **Blazor Server認証制約**: 
   - SignalR接続維持が必要
   - StateHasChanged()の適切な呼び出し
   - AuthenticationStateProviderとの連携

2. **ASP.NET Core Identity制約**:
   - UserManager/SignInManagerの適切な使用
   - Cookie認証設定の整合性維持

3. **Clean Architecture制約**:
   - Contracts層を通じたF#↔C#依存関係維持
   - レイヤー間依存関係の逆転原則遵守

### 運用制約
1. **ゼロダウンタイム制約**: 
   - 段階的切り替えによる無停止デプロイ
   - 既存ユーザーセッション維持

2. **テスト制約**:
   - 既存テストケース（220テスト）の影響最小化
   - 新規認証テストケースの追加必要

## リスク分析

### 高リスク
- **TECH-004**: `/change-password`ページ未実装（緊急対応必要）
- **TECH-003**: MVC→Blazor認証切替時の認証失敗

### 中リスク
- **TECH-002**: パスワード不整合による開発環境認証失敗

### 低リスク
- **設定ファイル変更**: シンプルな文字列置換

## 統合テスト・品質確保の観点

### 必須テストケース
1. **TECH-002テスト**: 
   - 初期スーパーユーザー"su"パスワードでのログイン成功
   - Development/Production環境での動作一貫性

2. **TECH-003テスト**:
   - Blazor認証フロー完全動作確認
   - MVC版削除後の404エラー確認
   - セッション維持・Cookie認証確認

3. **TECH-004テスト**:
   - 初回ログイン→パスワード変更フロー
   - IsFirstLoginフラグ更新確認
   - パスワード変更完了後の通常ログインフロー

## 最適実装順序・並列実行可能性

### 並列実行可能
- TECH-002 設定ファイル修正（独立作業）
- TECH-004 ChangePassword.razorページ作成（独立作業）

### 順次実行必須
1. TECH-004 完了 → TECH-003 Blazor認証実装
2. TECH-003 Blazor動作確認 → MVC版削除
3. 全修正完了 → 統合テスト実施

### 推奨時間配分
- **Step1（TECH-002 + TECH-004並列）**: 25-35分
- **Step2（TECH-003順次）**: 35-50分  
- **Step3（統合テスト・品質確認）**: 15-25分
- **合計予想時間**: 75-110分

## SubAgentプール方式適用推奨

1. **dependency-analysis**: 本分析（完了）
2. **csharp-web-ui**: TECH-003 Blazor認証実装・TECH-004 ChangePassword.razor作成
3. **csharp-infrastructure**: TECH-002 設定ファイル修正・初期データサービス更新
4. **unit-test**: 認証関連テストケース更新・新規テスト追加

## 重要事項

**TECH-004 ChangePassword.razorページ未実装が最高優先度課題**として、Step2開始時に緊急対応が必要。