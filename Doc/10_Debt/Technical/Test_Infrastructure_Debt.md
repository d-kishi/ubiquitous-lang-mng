# テストインフラ包括修正計画

**作成日**: 2025-07-26  
**緊急度**: 高（PhaseA4以降のTDD実践に影響）  
**影響範囲**: 全テストプロジェクト  
**工数見積**: 4-6時間（調査2時間＋修正2-4時間）  

## 🚨 **背景・現状**

### 問題概要
- **テストビルドエラー**: 121個のコンパイルエラー発生
- **TDD実践阻害**: PhaseA4以降のテストファースト開発が実行不可
- **品質担保不可**: 継続的テスト実行による品質確保ができない状態

### エラー分類
1. **F#↔C#境界問題** (約40件)
   - F# `Unit`型とC#テストコードの型不一致
   - `UserManager<User>`と`UserManager<ApplicationUser>`の混在
   - F# Domain型とC# Infrastructure型のマッピング問題

2. **未実装サービスメソッド** (約60件)
   - `AuthenticationService`の未実装メソッド（`RequestPasswordResetAsync`等）
   - `INotificationService`の未実装メソッド
   - `ApplicationDbContext`の参照エラー

3. **テスト環境設定** (約21件)
   - SmtpSettings古いプロパティ名（`UseSsl`→`EnableSsl`等）
   - パッケージ依存関係の不整合
   - モック設定の型不一致

## 🔍 **PhaseA3終了後必須調査項目**

### **1. アーキテクチャ境界設計の詳細調査** 🏗️
**調査範囲**: F#↔C#境界の実装状況把握

**必要情報**:
- **F#実装サービス一覧**
  - `UbiquitousLanguageManager.Application`配下の実装状況
  - `UbiquitousLanguageManager.Domain`配下の型定義
  - F#サービスのC#からの利用パターン

- **C#実装サービス一覧**
  - `UbiquitousLanguageManager.Infrastructure`配下の実装状況
  - `UbiquitousLanguageManager.Web`配下のBlazorコンポーネント
  - ASP.NET Core Identity関連サービス

- **型変換戦略の実装状況**
  - F# Result型 vs C# ResultDto型の使い分け実態
  - Contracts層での型変換実装状況
  - 依存関係注入（DI）設定の実装

**調査方法**:
```bash
# PhaseA1～A3実装記録読み込み
/Doc/04_Daily/2025-07/ 配下全ファイル
/Doc/02_Design/ 配下設計書

# ソースコード構造調査
dotnet sln list
find . -name "*.fs" -o -name "*.cs" | head -20
```

### **2. 現在の実装状況詳細調査** 🔧
**調査範囲**: エラー原因となっているサービス・クラスの実装状況

**必要情報**:
- **AuthenticationServiceの実装範囲**
  - 実装済みメソッド vs 未実装メソッド
  - テストが期待しているインターフェース vs 実際の実装
  - Phase別の実装計画・意図

- **ApplicationDbContextの実装状況**
  - Entity Framework設定
  - Connection String設定
  - テスト用In-Memory DB設定

- **INotificationServiceの設計意図**
  - メール送信サービスとの関係
  - 実装予定・優先度

**調査方法**:
```bash
# 実装状況確認
grep -r "AuthenticationService" src/
grep -r "ApplicationDbContext" src/
grep -r "INotificationService" src/
```

### **3. テスト戦略・方針の現状確認** 🧪
**調査範囲**: テスト設計方針の確認・統一

**必要情報**:
- **テストレベルの境界定義**
  - `/Doc/08_Organization/Rules/テスト戦略ガイド.md`の詳細
  - 単体テスト vs 統合テストの責任範囲
  - F#コードのテスト戦略（F#で書くか、C#で書くか）

- **モッキング戦略の統一**
  - NSubstitute vs Moq の使い分け方針確認
  - ASP.NET Core Identity関連のモック戦略
  - F#サービスのモック作成方法

- **テストデータ管理方針**
  - DatabaseFixture設計意図
  - テスト間のデータ分離戦略

**調査方法**:
```bash
# テスト戦略確認
/Doc/08_Organization/Rules/テスト戦略ガイド.md
/Doc/07_Decisions/ 配下テスト関連ADR
```

### **4. 開発環境・パッケージ設定** ⚙️
**調査範囲**: 環境固有設定の確認

**必要情報**:
- **データベース接続設定**
  - PostgreSQL接続設定（テスト用）
  - In-Memory Database設定
  - Entity Framework設定ファイル

- **パッケージ依存関係の整合性**
  - 各.csprojファイルのパッケージバージョン確認
  - テスト用パッケージの統一状況
  - MailKit、FluentAssertions、NSubstitute等

**調査方法**:
```bash
# パッケージ確認
find . -name "*.csproj" -exec grep -H "PackageReference" {} \;
dotnet list package --outdated
```

### **5. PhaseA1～A3技術決定事項** 📋
**調査範囲**: 既存設計の制約事項・意図の確認

**必要情報**:
- **アーキテクチャ決定記録（ADR）**
  - テスト関連の技術決定事項
  - F#↔C#境界での設計制約
  - Clean Architecture適用方針

- **Phase別実装判断**
  - なぜその実装方法を選択したのか
  - 段階的実装での優先度判断
  - 将来拡張を考慮した設計制約

**調査方法**:
```bash
# ADR確認
/Doc/07_Decisions/ 配下全ADR
/Doc/08_Organization/Active/Phase_A1/ 配下
/Doc/08_Organization/Active/Phase_A2/ 配下
```

## 🛠️ **修正アプローチ戦略**

### **Phase 1: 現状把握・分析** (120分)
1. **エラー分類・優先度付け** (30分)
   - 121個エラーの詳細分類・関連性分析
   - 修正優先度マトリックス作成

2. **アーキテクチャ実装状況調査** (60分)
   - F#↔C#境界の実装パターン確認
   - 型変換・DI設定の実装状況確認
   - 未実装サービスの設計意図確認

3. **テスト戦略・環境確認** (30分)
   - テストフレームワーク設定確認
   - モッキング戦略の統一状況確認
   - データベース接続設定確認

### **Phase 2: 段階的修正実行** (120-240分)
1. **基盤修正** (60-120分)
   - パッケージ依存関係修正
   - 基本的な型不一致修正
   - テスト基盤設定修正

2. **アーキテクチャ境界修正** (60-120分)
   - F#↔C#型変換統一
   - インターフェース・実装の整合性確保
   - モック設定統一

3. **統合テスト修正** (30-60分)
   - DatabaseFixture修正
   - 統合テスト環境確認
   - E2Eテスト実行確認

### **Phase 3: 検証・確立** (60分)
1. **全テスト実行確認** (30分)
2. **TDD環境確認** (20分)
3. **ドキュメント更新** (10分)

## 📊 **成功指標・完了基準**

### **定量指標**
- ✅ テストビルドエラー: 0件
- ✅ 全テスト実行成功率: 100%
- ✅ テスト実行時間: 5分以内
- ✅ TDD Red-Green-Refactorサイクル実行可能

### **定性指標**
- ✅ PhaseA4でのTDD実践環境確立
- ✅ 継続的テスト実行による品質担保
- ✅ テスト追加・修正の容易性確保
- ✅ F#↔C#境界でのテスト戦略統一

## 🔗 **依存関係・制約事項**

### **前提条件**
- PhaseA3完了（パスワードリセット機能実装完了）
- PhaseA1～A3実装記録の詳細把握
- 設計書・ADRの包括的理解

### **制約事項**
- 既存の動作している機能への影響最小化
- Clean Architecture原則の維持
- F#↔C#境界設計の継続性確保

### **リスク要因**
- 未知の依存関係によるカスケード修正
- パッケージバージョン整合性問題
- テストデータ・環境設定の複雑性

## 📝 **実行記録（実施時更新）**

### Phase 1実行記録
（PhaseA3完了後に記録）

### Phase 2実行記録
（修正実行時に記録）

### Phase 3実行記録
（検証完了時に記録）

---

**次回更新**: PhaseA3完了後の詳細調査実行時  
**関連ドキュメント**: 
- `/Doc/プロジェクト状況.md` - 簡潔な実施事項記録
- `/Doc/08_Organization/Rules/テスト戦略ガイド.md` - テスト戦略詳細
- `/Doc/07_Decisions/ADR_009_テスト指針.md` - テスト指針ADR