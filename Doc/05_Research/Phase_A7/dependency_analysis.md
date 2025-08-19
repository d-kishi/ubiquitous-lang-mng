# Phase A7 Step1: MVC/Blazor混在アーキテクチャ依存関係分析

## 分析概要

### 分析対象
GitHub Issue #6「MVC/Blazor混在アーキテクチャの統一」に対応した技術的依存関係分析

### 分析日時
2025-08-19

### 分析スコープ
- MVC関連コンポーネントの依存関係特定
- Blazor Server統合における制約・リスク評価
- 安全な移行手順の策定

---

## 依存関係マップ

### 現在のアーキテクチャ構成

```
【エントリーポイント分離パターン】
┌─────────────────┬─────────────────┐
│   MVC側         │   Blazor側      │
├─────────────────┼─────────────────┤
│ HomeController  │ Login.razor     │
│ Views/*.cshtml  │ Logout.razor    │
│ Error処理       │ 管理画面群      │
└─────────────────┴─────────────────┘
       ↓                    ↓
  ┌──────────────────────────────────┐
  │      共有認証基盤               │
  │  - ASP.NET Core Identity       │
  │  - CustomAuthenticationState   │
  │  - FirstLoginRedirectMiddleware │
  └──────────────────────────────────┘
```

### 技術的依存関係詳細

| 依存元 | 依存先 | 依存種別 | 結合度 | リスク |
|-------|-------|----------|--------|--------|
| HomeController | ASP.NET Core Identity | 認証API | 中 | 低 |
| Views/Account/ChangePassword.cshtml | AccountController | 未実装参照 | 高 | **高** |
| MvcBlazorRoutingIntegrationTests | HomeController | テスト依存 | 中 | 中 |
| Program.cs | MapControllerRoute | ルーティング設定 | 中 | 中 |
| FirstLoginRedirectMiddleware | MVC/Blazor両パス | パス制限設定 | 高 | **高** |
| Views/_Layout.cshtml | Bootstrap/共通CSS | UI依存 | 弱 | 低 |

---

## 実装順序分析

### Phase 1: 基盤整備・準備フェーズ
**Priority: 最高 - 実装前必須**

1. **AccountController実装**
   - 課題: `Views/Account/ChangePassword.cshtml`がAccountControllerを参照するが未実装
   - 対応: TECH-004対応のAccountController作成
   - 期間: 1-2日

2. **Blazor版パスワード変更画面作成**
   - 課題: MVCビューからBlazorコンポーネントへの移行
   - 対応: `/change-password`ルートのBlazorコンポーネント作成
   - 期間: 2-3日

### Phase 2: 段階的移行フェーズ
**Priority: 高 - 機能停止リスク最小化**

3. **ルーティング設定更新**
   - 課題: `MapControllerRoute`削除による影響範囲特定
   - 対応: Blazor Serverルーティングへの完全移行
   - 期間: 1日

4. **FirstLoginRedirectMiddleware更新**
   - 課題: MVC/Blazor混在パス制御からBlazor専用への移行
   - 対応: Blazorルーティングに特化したパス制御
   - 期間: 1日

### Phase 3: クリーンアップフェーズ
**Priority: 中 - 技術的負債解消**

5. **MVCコンポーネント削除**
   - HomeController.cs削除
   - Views/*削除
   - MVC関連設定削除
   - 期間: 1日

6. **テスト更新**
   - MvcBlazorRoutingIntegrationTests更新
   - Blazor専用テストへの移行
   - 期間: 1-2日

---

## 循環依存・問題点分析

### 重大な問題点

#### 1. **CRITICAL**: AccountController未実装
```
Views/Account/ChangePassword.cshtml 
    ↓ (参照)
AccountController (未実装)
    ↓ (予想される依存)
ASP.NET Core Identity
```

**影響**: 現在のMVCパスワード変更画面が動作不能  
**対策**: AccountController緊急実装またはBlazor版への完全移行

#### 2. **HIGH**: FirstLoginRedirectMiddleware混在制御
```
FirstLoginRedirectMiddleware
    ↓ (パス制御)
MVC(/Account/ChangePassword) + Blazor(/change-password)
```

**影響**: 認証フローの一貫性欠如  
**対策**: Blazor統一パスへの移行（単一制御点化）

### 軽微な問題点

#### 3. **MEDIUM**: テスト依存関係
```
MvcBlazorRoutingIntegrationTests
    ↓ (テスト対象)
HomeController
```

**影響**: MVC削除時のテスト失敗  
**対策**: Blazor専用テストへの段階的移行

---

## 制約・前提条件

### 技術的制約

1. **ASP.NET Core Identity制約**
   - MVC/Blazor双方対応の認証システム維持必須
   - Cookie認証設定の継続性確保

2. **SignalR接続制約**  
   - Blazor ServerのSignalR Hub維持必須
   - リアルタイム通信機能への影響なし

3. **セッション管理制約**
   - 既存ユーザーセッションの継続性保証
   - ログイン状態の保持

### 運用制約

1. **ダウンタイム最小化**
   - 段階的移行による機能停止期間ゼロ
   - ロールバック可能な実装手順

2. **テストカバレッジ維持**
   - 移行中も品質保証継続
   - 機能回帰防止

---

## リスク分析・対策

### 高リスク項目

#### **RISK-001: AccountController未実装による機能停止**
- **リスク**: 現在のパスワード変更機能が完全に動作不能
- **影響度**: 高（認証システム全体への影響）
- **対策**: 
  1. 緊急でAccountController実装
  2. またはMVC版を無効化してBlazor版のみ使用

#### **RISK-002: ルーティング競合**
- **リスク**: MVC削除時のルーティングエラー
- **影響度**: 中（特定パスでの404エラー）
- **対策**: 段階的ルーティング移行・フォールバック設定

### 中リスク項目

#### **RISK-003: 認証フロー断絶**
- **リスク**: 移行中の認証状態不整合
- **影響度**: 中（ユーザーセッション影響）
- **対策**: カナリア開放・ロールバック体制整備

#### **RISK-004: UI一貫性欠如**
- **リスク**: MVC/Blazor画面の操作感不統一
- **影響度**: 低（UX品質低下）
- **対策**: Bootstrap統一・UI設計指針準拠

### 低リスク項目

#### **RISK-005: 静的リソース参照切れ**
- **リスク**: CSS/JS参照の不整合
- **影響度**: 低（スタイリング問題）
- **対策**: 共有リソースパス検証

---

## 実装推奨手順

### 🚀 **推奨実装シーケンス**

```
Week 1: 基盤整備
├─ Day 1-2: AccountController + TECH-004完全対応
├─ Day 3-4: Blazor版ChangePassword画面実装  
└─ Day 5:   ルーティング整合性確認・テスト

Week 2: 段階的移行
├─ Day 1:   FirstLoginRedirectMiddleware Blazor化
├─ Day 2-3: MVC→Blazor段階的切り替え
├─ Day 4:   統合テスト・品質確認
└─ Day 5:   MVCコンポーネント完全削除

Week 3: 品質保証・監視
├─ Day 1-2: テストスイート全面更新
├─ Day 3-4: 本番環境でのカナリア確認
└─ Day 5:   ドキュメント・ADR更新
```

### 🔄 **並列実行可能作業**

- **並列可能**: Blazor画面実装 + AccountController実装
- **並列可能**: テスト更新 + ドキュメント整備  
- **直列必須**: ルーティング変更 → MVC削除 → 最終テスト

---

## 成功指標・検証項目

### 機能検証項目
- [ ] 初回ログイン→パスワード変更フロー完全動作
- [ ] 通常ログイン→管理画面アクセス正常動作
- [ ] ログアウト機能正常動作
- [ ] セッション管理・タイムアウト動作確認

### 性能検証項目  
- [ ] SignalR接続安定性確認
- [ ] 認証応答時間（目標: <500ms）
- [ ] メモリ使用量削減確認

### セキュリティ検証項目
- [ ] 認証バイパス脆弱性なし
- [ ] CSRF対策維持
- [ ] セッション固定攻撃対策維持

---

## 次Step提言

### 即時実行推奨
1. **AccountController緊急実装**（TECH-004対応）
2. **Blazor版パスワード変更画面作成**

### Phase A7 Step2提案
1. **段階的MVC削除実装**
2. **ルーティング統合・最適化**
3. **統合テスト全面実施**

---

*本分析は Phase A7 Step1 として実施*  
*次ステップ: 依存関係Agent → MainAgent → 実装系Agent連携*
