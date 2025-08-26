# 開発ガイドライン - 2025年8月26日更新

## プロセス遵守絶対原則（ADR_016）
### 違反は一切許容されない重要遵守事項
- **コマンド = 契約**: 一字一句を法的契約として遵守・例外なし
- **承認 = 必須**: 「ユーザー承認」表記は例外なく取得・勝手な判断禁止
- **手順 = 聖域**: 定められた順序の変更禁止・先回り作業禁止

### 禁止行為（重大違反）
- ❌ **承認前の作業開始**: いかなる理由でも禁止
- ❌ **独断での判断**: 「効率化」を理由とした勝手な作業
- ❌ **成果物の虚偽報告**: 実体のない成果物の報告
- ❌ **コマンド手順の無視**: phase-start/step-start等の手順飛ばし

## セッション開始時必須プロセス
### 自動実行Commands
- **セッション開始**: 「セッションを開始します」→ `.claude/commands/session-start.md` 自動実行
- **Phase開始準備**: 「Phase開始準備」→ `.claude/commands/phase-start.md` 自動実行
- **Step開始準備**: 「Step開始」→ `.claude/commands/step-start.md` 自動実行

### Serena MCP必須初期化
1. `mcp__serena__check_onboarding_performed` 実行
2. メモリー状況確認・必要に応じて更新
3. プロジェクト概要・技術詳細メモリー参照

## 開発手法・品質基準
### スクラム開発サイクル（ADR_011）
- **1-2週間スプリント**: 継続的価値提供
- **TDD実践**: Red-Green-Refactorサイクル
- **完全ビルド維持**: 0 Warning, 0 Error状態保持

### SubAgentプール方式（ADR_013）
#### 専門SubAgent一覧
- **unit-test**: TDD・単体テスト設計実装
- **integration-test**: 統合テスト・E2E・WebApplicationFactory
- **spec-compliance**: 仕様準拠監査・要件逸脱防止
- **design-review**: システム設計・アーキテクチャ整合性確認
- **fsharp-domain**: F#ドメインモデル設計
- **fsharp-application**: F#アプリケーションサービス
- **csharp-infrastructure**: EF Core Repository実装
- **csharp-web-ui**: Blazor Server UI実装
- **contracts-bridge**: F#↔C#型変換・相互運用
- **code-review**: コード品質・Clean Architecture準拠

#### SubAgent並列実行原則
- 同一メッセージ内で複数Task tool呼び出し
- 依存関係のないタスクのみ並列化
- 実際の並列実行確認（表示順次は直列実行）

## Clean Architecture実装ガイド
### 層構成
```
Web (C# Blazor Server) → Contracts (C# DTOs/TypeConverters) → Application (F# UseCases) → Domain (F# Models)
                      ↘ Infrastructure (C# EF Core/Repository) ↗
```

### F#/C#境界設計
#### Result型変換パターン
```csharp
public static T MapResult<T>(FSharpResult<T, string> result)
{
    if (FSharpResult.IsOk(result)) return result.ResultValue;
    else throw new DomainException(result.ErrorValue);
}
```

#### TypeConverter基本パターン
```csharp
public class EntityTypeConverter
{
    public static EntityDto ToDto(Entity domain) { /* Domain → DTO */ }
    public static Result<Entity, string> FromDto(EntityDto dto) { /* DTO → Domain */ }
}
```

## Blazor Server開発ガイド
### 初学者対応（ADR_010）
- **詳細コメント必須**: ライフサイクル・StateHasChanged・SignalR接続説明
- **F#概念説明**: パターンマッチング・Option型・Result型の詳細説明

### 認証実装パターン
```razor
@using Microsoft.AspNetCore.Components.Authorization
@attribute [Authorize]

@code {
    protected override async Task OnInitializedAsync()
    {
        // 認証状態確認・初期化処理
    }
}
```

### エラーハンドリング統一
```razor
<ErrorBoundary>
    <ChildContent>@ChildContent</ChildContent>
    <ErrorContent>
        <div class="alert alert-danger">@context.Message</div>
    </ErrorContent>
</ErrorBoundary>
```

## テスト戦略
### TDD実践パターン
1. **Red Phase**: 失敗するテスト作成
2. **Green Phase**: 最小限実装でテスト成功
3. **Refactor Phase**: コード品質改善・テスト継続成功

### 統合テスト基盤
- **WebApplicationFactory**: DI設定・テスト環境分離
- **TestWebApplicationFactoryパターン**: 確立済み基盤活用
- **E2E認証フローテスト**: 全認証シナリオカバー

## URL設計・用語統一
### URL設計原則
- **Blazor形式**: 小文字・ハイフン区切り（`/change-password`）
- **統一性**: 全URLでBlazor Server形式統一

### 用語統一（ADR_003）
- **必須置換**: 「用語」→「ユビキタス言語」
- **対象範囲**: 全コード・ドキュメント・UI表示

## 品質保証・監査
### 品質確認必須項目
- [ ] `dotnet build` 成功（0 Warning, 0 Error）
- [ ] `dotnet test` 全テスト成功
- [ ] 認証フロー完全動作確認
- [ ] 要件定義・設計書準拠確認

### 仕様準拠監査プロセス
- **spec-compliance SubAgent**: 要件逸脱特定・準拠度測定
- **design-review SubAgent**: アーキテクチャ整合性・品質スコア評価
- **目標基準**: 要件準拠90%以上・アーキテクチャ品質85/100以上

## セキュリティ・運用
### セキュリティ実装必須
- **ASP.NET Core Identity**: 認証・認可基盤
- **CSRF防止**: ValidateAntiForgeryToken適用
- **OWASP準拠**: セキュリティベストプラクティス適用

### 運用環境
- **開発環境**: Docker Compose（PostgreSQL + PgAdmin + Smtp4dev）
- **本番品質**: パフォーマンス・セキュリティ・可用性確保

## 技術負債管理（ADR_015）
### GitHub Issues統一管理パターン（確立・実証済み）
- **体系的追跡**: 優先度・影響範囲・解決期限・ラベル管理
- **検索性向上**: GitHub Issues検索・フィルタリング活用
- **可視性確保**: プロジェクト全体での技術負債可視化
- **履歴保全**: 解決過程・決定記録の永続化
- **解決確認**: 実装完了・テスト成功・動作確認の3段階確認

### 管理状況（2025-08-26現在）
- **Issue #5**: ✅ [COMPLIANCE-001] 要件準拠・品質監査（完了）
- **Issue #6**: ✅ [ARCH-001] アーキテクチャ統一（完了）
- **Issue #7**: 🔄 [TECH-006] Blazor Server認証統合課題（Phase A8 Step2対応中）

### 重要実証済み知見
- **技術負債ファイル記述正確性**: SubAgent誤判定防止のため詳細記述必須
- **step-end-reviewプロセス**: 包括的品質確認・新規課題発見効果実証
- **SubAgent前提条件明確化**: 実行前の現状確認・依存関係整理の重要性