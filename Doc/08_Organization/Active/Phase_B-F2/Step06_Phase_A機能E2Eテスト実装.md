# Step 06 組織設計・実行記録

## 🎯 Step目的（Why）

**このStepで達成すべきこと**:
- Phase A機能（認証・ユーザー管理）のE2Eテスト完全実装（19シナリオ）
- Playwright Agents自動修復機能動作確認
- E2Eテスト実装効率測定

**Phase全体における位置づけ**:
- **Phase全体の課題**: 技術負債解決・E2Eテスト基盤強化・技術基盤刷新
- **このStepの役割**: E2Eテスト基盤実践段階・Phase A機能品質保証完全化・回帰テスト基盤確立

**関連Issue**: #52
- Phase A（認証・ユーザー管理）機能のE2Eテスト実装
- 認証機能9シナリオ（ログイン・ログアウト・パスワード変更・パスワードリセット）
- ユーザー管理機能10シナリオ（一覧表示・登録・編集・削除）

---

## 📋 Step概要

- **Step名**: Step06 Phase A機能E2Eテスト実装
- **作業特性**: E2Eテスト実装（テスト段階）
- **推定期間**: 1セッション（3-4時間）
- **開始日**: 2025-11-14

---

## 🏢 組織設計

### SubAgent構成

**メインAgent**: e2e-test Agent
- **責務**: Playwright E2Eテスト実装専門
- **活用ツール**:
  - Playwright MCP 21ツール（browser_navigate, browser_snapshot, browser_click等）
  - playwright-e2e-patterns Skill（3パターン自律適用）
  - Blazor Server SignalR対応パターン
  - data-testid属性設計パターン

**Agent選択理由**:
- Phase B-F2 Step3でE2E専用SubAgent新設（ADR_024準拠）
- Playwright MCP完全統合（21ツール直接使用）
- playwright-e2e-patterns Skill自律適用
- Phase B2で93.3%効率化実績（150分 → 10分）

### 並列実行計画

**2ファイル並列作成**:
1. **AuthenticationTests.cs**（9シナリオ）- 認証機能E2Eテスト
2. **UserManagementTests.cs**（10シナリオ）- ユーザー管理機能E2Eテスト

**並列実行理由**:
- 2ファイルは独立（依存関係なし）
- 同時実装可能（e2e-test Agent × 2インスタンス）
- 実装時間短縮効果（3-4時間 → 2-3時間）

### Phase B2成果物活用

**技術基盤**:
- ✅ Playwright MCP + Agents統合基盤（ADR_021 - 推奨度10/10点）
- ✅ playwright-e2e-patterns Skill（3パターン）
  - data-testid属性設計パターン
  - Playwright MCPツール活用パターン
  - Blazor Server SignalR対応パターン
- ✅ data-testid属性実装済み（Phase B2 Step5でPhase A画面に付与完了）
- ✅ 既存E2Eテスト参照可能（UserProjectsTests.cs）

**テスト環境**:
- ✅ テスト専用アカウント準備済み（e2e-test@ubiquitous-lang.local / E2ETest#2025!Secure）
- ✅ E2Eテストプロジェクト存在（tests/UbiquitousLanguageManager.E2E.Tests/）
- ✅ Playwright for .NET構成確認済み

---

## 🎯 Step成功基準

### 機能要件
- ✅ 全19シナリオ実装完了
  - AuthenticationTests.cs: 9シナリオ実装
  - UserManagementTests.cs: 10シナリオ実装
- ✅ 全19シナリオ実行成功（成功率95%以上）

### 品質要件
- ✅ 0 Warning / 0 Error維持
- ✅ Playwright Agents自動修復機能動作確認
- ✅ E2Eテスト実装効率測定（Phase B2比較データ取得）

### 技術基盤
- ✅ playwright-e2e-patterns Skill自律適用確認
- ✅ Playwright MCP 21ツール活用確認
- ✅ Phase A機能品質保証完全化

---

## 📊 Step Stage構成

### Stage 1: 設計・準備（30分）

**実施内容**:
1. Issue #52詳細確認（19シナリオ仕様）
2. 機能仕様書確認
   - 2.1節: 認証機能仕様（ログイン・ログアウト・パスワード変更・パスワードリセット）
   - 2.2節: ユーザー管理機能仕様（一覧表示・登録・編集・削除）
3. 既存E2Eテスト参照（UserProjectsTests.cs実装パターン確認）
4. data-testid属性確認（Phase A画面への付与状況）
5. テスト専用アカウント確認

**成果物**:
- 19シナリオ実装計画
- data-testid属性一覧
- テスト環境確認完了

**完了条件**:
- 19シナリオの詳細仕様理解完了
- 既存E2Eテストパターン理解完了
- テスト環境動作確認完了

---

### Stage 2: AuthenticationTests.cs実装（1-1.5時間）

**実施内容**:
9シナリオ実装:

#### 2.1 ログイン機能（3シナリオ）
1. **正常系**: 正しいメールアドレス・パスワードでログイン成功
2. **異常系（認証失敗）**: 誤ったパスワードでログイン失敗・エラーメッセージ表示
3. **Remember Me機能**: ログイン状態保持チェックボックス動作確認

#### 2.2 ログアウト機能（1シナリオ）
4. **正常系**: ログアウトボタンクリック→ログイン画面遷移→セッション破棄確認

#### 2.3 パスワード変更機能（2シナリオ）
5. **正常系**: 現在パスワード認証→新パスワード設定→成功メッセージ表示
6. **異常系（現在パスワード誤り）**: 認証失敗・エラーメッセージ表示

#### 2.4 パスワードリセット機能（3シナリオ）
7. **リセット申請**: メールアドレス入力→リセットメール送信確認
8. **リセット実行**: リセットリンククリック→新パスワード設定→ログイン画面遷移
9. **異常系（未登録メール）**: エラーメッセージ表示

**SubAgent指示**:
```
e2e-test Agent: AuthenticationTests.cs実装

以下の9シナリオを実装してください：
[上記2.1-2.4の詳細を指示]

参照文書:
- Doc/01_Requirements/機能仕様書.md 2.1節
- tests/UbiquitousLanguageManager.E2E.Tests/UserProjectsTests.cs

Skills適用:
- playwright-e2e-patterns（自律適用）
```

**成果物**:
- AuthenticationTests.cs実装完了（9シナリオ）
- 9シナリオ実行成功確認

**完了条件**:
- 9シナリオ実装完了
- dotnet test実行成功（9/9 passed）
- 0 Warning / 0 Error維持

---

### Stage 3: UserManagementTests.cs実装（1-1.5時間）

**実施内容**:
10シナリオ実装:

#### 3.1 ユーザー一覧表示（2シナリオ）
1. **正常系**: ユーザー一覧画面表示・検索・ソート動作確認
2. **権限別表示**: SuperUser/ProjectManagerの表示データ範囲確認

#### 3.2 ユーザー登録機能（3シナリオ）
3. **正常系**: 全項目入力→登録成功→成功メッセージ表示→一覧画面遷移
4. **異常系（メール重複）**: 重複エラーメッセージ表示
5. **異常系（必須項目未入力）**: バリデーションエラー表示

#### 3.3 ユーザー編集機能（3シナリオ）
6. **正常系**: 氏名・ロール変更→更新成功→成功メッセージ表示
7. **パスワードリセット実行**: 管理者によるパスワードリセット→対象ユーザー初回ログイン時強制変更
8. **異常系（権限不足）**: 権限エラーメッセージ表示

#### 3.4 ユーザー削除機能（2シナリオ）
9. **正常系**: 削除確認ダイアログ→削除実行→成功メッセージ表示
10. **削除済みユーザー表示**: 削除済みユーザー表示切替→論理削除確認

**SubAgent指示**:
```
e2e-test Agent: UserManagementTests.cs実装

以下の10シナリオを実装してください：
[上記3.1-3.4の詳細を指示]

参照文書:
- Doc/01_Requirements/機能仕様書.md 2.2節
- tests/UbiquitousLanguageManager.E2E.Tests/UserProjectsTests.cs

Skills適用:
- playwright-e2e-patterns（自律適用）
```

**成果物**:
- UserManagementTests.cs実装完了（10シナリオ）
- 10シナリオ実行成功確認

**完了条件**:
- 10シナリオ実装完了
- dotnet test実行成功（10/10 passed）
- 0 Warning / 0 Error維持

---

### Stage 4: 全19シナリオ実行確認（30分）

**実施内容**:
1. 全E2Eテスト実行（dotnet test）
2. 全19シナリオ成功確認（成功率95%以上）
3. エラー発生時のPlaywright Agents自動修復動作確認
4. 0 Warning / 0 Error確認

**実行コマンド**:
```bash
# DevContainer環境で実行
docker exec ubiquitous-lang-mng_devcontainer-devcontainer-1 dotnet test --filter "FullyQualifiedName~E2E"
```

**成果物**:
- 全19シナリオ実行結果レポート
- Playwright Agents自動修復動作ログ

**完了条件**:
- 全19シナリオ実行成功（95%以上）
- Playwright Agents自動修復機能動作確認完了
- 0 Warning / 0 Error維持

---

### Stage 5: 効果測定・完了処理（30分）

**実施内容**:
1. E2Eテスト実装効率測定
   - 実装時間測定（Phase B2比較）
   - Playwright Agents自動修復効果測定
   - playwright-e2e-patterns Skill適用効果測定
2. step-end-review Command実行
3. Phase_Summary.md更新（Step6完了記録）
4. GitHub Issue #52 Close処理

**効果測定基準**:
- **Phase B2実績**: 3シナリオ実装 10分（93.3%削減効果）
- **Step6期待値**: 19シナリオ実装 2-3時間（同等の効率化）

**成果物**:
- E2Eテスト実装効率測定レポート
- step-end-review実行結果
- Phase_Summary.md更新完了
- GitHub Issue #52 Close完了

**完了条件**:
- 効果測定完了
- step-end-review合格
- ドキュメント更新完了
- GitHub Issue #52 Close完了

---

## 📚 必須参照文書

### Phase B2成果物
- **ADR_021**: `Doc/07_Decisions/ADR_021_Playwright統合戦略.md`
  - Playwright MCP + Agents統合戦略（推奨度10/10点）
  - 93.3%効率化実績（150分 → 10分）

- **playwright-e2e-patterns Skill**: `.claude/skills/playwright-e2e-patterns/`
  - `patterns/data-testid-design.md` - data-testid属性設計パターン
  - `patterns/mcp-tools-usage.md` - Playwright MCPツール活用パターン
  - `patterns/blazor-signalr-e2e.md` - Blazor Server SignalR対応パターン

- **既存E2Eテスト**: `tests/UbiquitousLanguageManager.E2E.Tests/UserProjectsTests.cs`
  - 3シナリオ実装パターン参照
  - Playwright for .NET実装例

### 機能仕様書
- **2.1節**: `Doc/01_Requirements/機能仕様書.md`
  - 認証機能仕様（ログイン・ログアウト・パスワード変更・パスワードリセット）

- **2.2節**: `Doc/01_Requirements/機能仕様書.md`
  - ユーザー管理機能仕様（一覧表示・登録・編集・削除）

### Issue・Phase記録
- **GitHub Issue #52**: Phase A機能E2Eテスト実装詳細
- **Phase A完了記録**: `Doc/08_Organization/Completed/Phase_A*/Phase_Summary.md`
  - Phase A1-A9実装詳細

### ADR・技術決定
- **ADR_024**: Playwright専用SubAgent新設決定
- **ADR_020**: テストアーキテクチャ決定

---

## 📊 Step実行記録（随時更新）

### Session 1（2025-11-14）

**開始時刻**: [記録]

#### Stage 1実行記録

（実施中に更新）

---

## ✅ Step終了時レビュー

（Step完了時に更新）

---

**作成日**: 2025-11-14
**最終更新**: 2025-11-14（Step開始時）
