# Step 01 組織設計・実行記録

## 🎯 Step目的（Why）

**このStepで達成すべきこと**:
- Phase Aユーザー管理UI機能の完全実装（3画面：一覧・登録・編集）
- Issue #52「ユーザー管理機能E2Eテスト10シナリオ」の前提条件（UI実装）整備
- Phase A完成度向上（25% → 62.5%、+37.5%達成）

**Phase全体における位置づけ**:
- **Phase全体の課題**: Phase A完全完成（25% → 100%）+ Phase B完全完成（100%）
- **このStepの役割**: Phase A対応漏れUI実装により、Phase A機能品質保証基盤確立
- **次Stepへの影響**: Step2（認証補助機能UI）と合わせて Phase A完成度100%達成

**関連Issue**: #52（Phase A機能のE2Eテスト実装）
- **Issue現状**: ユーザー管理機能E2Eテスト 0/10シナリオ（未実施）
- **未実施理由**: ユーザー管理Web UI未実装のため実装不可
- **Step1効果**: 本Step完了により、Issue #52の10シナリオ実装が可能になる

---

## 📋 Step概要

- **Step名**: Step01 ユーザー管理UI実装
- **作業特性**: 実装・テスト・統合
- **推定期間**: 9-13時間 + ユーザー確認時間（1-2セッション）
- **開始日**: 2025-11-24
- **Stage構成**: 4 Stages（UI実装→bUnitテスト→ユーザー確認→E2Eテスト）

---

## 🏢 組織設計

### 技術調査判断結果

**判断**: ❌ **技術調査不要**

**理由**:
1. 実装対象が明確（UI設計書3.6-3.8節準拠の3画面実装）
2. 技術パターンが確立済み（Phase A/B2で実績あり）
   - Blazor Server実装パターン（Phase A確立）
   - ASP.NET Core Identity統合（Phase A確立）
   - E2Eテストパターン（Phase B-F2確立）
   - 権限制御パターン（Phase B2確立）
3. 新規技術要素なし（既存パターンの適用のみ）
4. SubAgentで十分対応可能（csharp-web-ui + e2e-test）

### SubAgent構成（確定版）

**Stage構成方式**: UI実装→bUnitテスト→ユーザー確認→E2Eテスト

#### Stage 1: UI実装（6-8時間）

**csharp-web-ui Agent**（3画面を並列実行）:

**Task 1**: Index.razor実装
- ユーザー一覧画面
- URL: `/admin/users`
- 機能: 一覧表示・検索・フィルタ・ページング・編集/削除ボタン（権限制御）
- 参照: UI設計書3.6節

**Task 2**: Create.razor実装
- ユーザー登録画面
- URL: `/admin/users/create`
- 機能: メールアドレス・氏名・初期パスワード・ロール・所属プロジェクト
- 画面遷移: 一覧画面 → 新規登録ボタン → 登録画面（別画面遷移）
- 参照: UI設計書3.7節

**Task 3**: Edit.razor実装
- ユーザー編集画面
- URL: `/admin/users/edit/{id}`
- 機能: 氏名・ロール・所属プロジェクト・ステータス・パスワードリセット
- 画面遷移: 一覧画面 → 編集ボタン → 編集画面（別画面遷移）
- 参照: UI設計書3.8節

**共通作業**: UserManagement.razor（758行）削除

**並列実行**: ✅ 可能（別ファイル、.csproj競合なし）

#### Stage 2: bUnitテスト実装（2-3時間）

**unit-test Agent**（3テストを並列実行）:

**Task 1**: UserListTests.cs
- Index.razor用bUnitテスト
- テスト対象: 一覧表示・検索・フィルタ・ページング・権限制御

**Task 2**: UserCreateTests.cs
- Create.razor用bUnitテスト
- テスト対象: バリデーション・登録処理・画面遷移

**Task 3**: UserEditTests.cs
- Edit.razor用bUnitテスト
- テスト対象: 編集処理・パスワードリセット・権限制御

**並列実行**: ✅ 可能（別ファイル）

**目標カバレッジ**: 80%+

#### Stage 3: ユーザー動作確認・UIレイアウト調整（時間不明）

**実施内容**:
- ユーザー様による手動動作確認
- UIレイアウト確認（3画面）
- 必要に応じてUIレイアウト変更要求への対応

**成果物**: UIレイアウト調整結果（必要時）

#### Stage 4: E2Eテスト実装（1-2時間）

**e2e-test Agent**:

**成果物**: user-management.spec.ts（10シナリオ）

**シナリオ構成**:
1. ユーザー一覧表示（2シナリオ）
   - 正常系: ユーザー一覧画面表示・検索・ソート動作確認
   - 権限別表示: SuperUser/ProjectManagerの表示データ範囲確認

2. ユーザー登録機能（3シナリオ）
   - 正常系: 全項目入力→登録成功→成功メッセージ表示→一覧画面遷移
   - 異常系（メール重複）: 重複エラーメッセージ表示
   - 異常系（必須項目未入力）: バリデーションエラー表示

3. ユーザー編集機能（3シナリオ）
   - 正常系: 氏名・ロール変更→更新成功→成功メッセージ表示
   - パスワードリセット実行: 管理者によるパスワードリセット→対象ユーザー初回ログイン時強制変更
   - 異常系（権限不足）: 権限エラーメッセージ表示

4. ユーザー削除機能（2シナリオ）
   - 正常系: 削除確認ダイアログ→削除実行→成功メッセージ表示
   - 削除済みユーザー表示: 削除済みユーザー表示切替→論理削除確認

**前提条件**: Stage 1-3完了（UI実装・bUnitテスト・ユーザー確認完了）

### 対話的詳細化結果（Section 2.3）

**対話的詳細化プロセス完了日**: 2025-11-24
**step-start Section 2.3パターン適用テスト**: ✅ 成功

#### 実装対象スコープ（ユーザー合意済み）

**UI実装（3画面）**:
- ✅ Index.razor（ユーザー一覧画面） - `/admin/users`
- ✅ Create.razor（ユーザー登録画面） - `/admin/users/create`
- ✅ Edit.razor（ユーザー編集画面） - `/admin/users/edit/{id}`
- ✅ UserManagement.razor（758行）削除 - 仕様乖離のため

**bUnitテスト実装（3テスト）**:
- ✅ UserListTests.cs（Index.razor用）
- ✅ UserCreateTests.cs（Create.razor用）
- ✅ UserEditTests.cs（Edit.razor用）

**E2Eテスト実装（10シナリオ）**:
- ✅ user-management.spec.ts

**サイドメニュー導線**:
- ✅ **確認済み・追加作業不要**
- NavMenu.razor (line 69-73) に既に `/admin/users` へのリンクが実装済み
- 新規実装するIndex.razorのURLと一致

#### SubAgent選択の合意

**Stage構成方式**: UI実装→bUnitテスト→ユーザー確認→E2Eテスト（段階的実施）

**SubAgent構成**:
- **Stage 1**: csharp-web-ui Agent（UI実装）
- **Stage 2**: unit-test Agent（bUnitテスト実装）
- **Stage 3**: ユーザー動作確認・UIレイアウト調整
- **Stage 4**: e2e-test Agent（E2Eテスト実装）

**並列実行**:
- ✅ Stage 1内: 3画面を並列実装可能（別ファイル、.csproj競合なし）
- ✅ Stage 2内: 3テストを並列実装可能（別ファイル）

**推定時間**: 9-13時間 + ユーザー確認時間

#### 成果物・完了基準の合意

**完了基準（Phase_Summary.md準拠）**:
- ✅ UI設計書3.6-3.8節100%実装
- ✅ エンドユーザーがユーザー管理操作可能（一覧・登録・編集・削除）
- ✅ E2Eテスト10シナリオ全Pass
- ✅ bUnitテスト80%+カバレッジ
- ✅ 0 Warning/0 Error維持
- ✅ Issue #52 Close可能状態

**成果物チェックリスト**:
- [ ] Index.razor実装完了
- [ ] Create.razor実装完了
- [ ] Edit.razor実装完了
- [ ] UserManagement.razor削除完了
- [ ] UserListTests.cs実装完了
- [ ] UserCreateTests.cs実装完了
- [ ] UserEditTests.cs実装完了
- [ ] user-management.spec.ts実装完了（10シナリオ）
- [ ] ビルド成功（0 Warning/0 Error）
- [ ] 全テストPass

---

## 📚 Step必須参照ドキュメント

**Phase_Summary.md「Step間成果物参照マトリックス」より**:

1. **UI設計書**: `Doc/02_Design/UI設計/01_認証・ユーザー管理画面設計.md`
   - 3.6節: ユーザー一覧画面
   - 3.7節: ユーザー登録画面
   - 3.8節: ユーザー編集画面

2. **権限制御仕様**: `Doc/02_Design/権限制御テストマトリックス.md`
   - 4ロール×4機能 = 16パターン

3. **テスト戦略**: `Doc/08_Organization/Rules/テスト戦略ガイド.md`
   - TDD実践セクション（Red-Green-Refactorサイクル）

4. **E2Eテスト仕様**: GitHub Issue #52
   - ユーザー管理10シナリオ実装指針

5. **既存実装確認**: `src/UbiquitousLanguageManager.Web/Pages/Admin/UserManagement.razor`
   - 全体（758行）- 仕様乖離確認・削除対象特定

---

## 🎯 Step成功基準

### 完了基準（Phase_Summary.md準拠）

- ✅ UI設計書3.6-3.8節100%実装
- ✅ エンドユーザーがユーザー管理操作可能（一覧・登録・編集・削除）
- ✅ E2Eテスト10シナリオ全Pass
- ✅ 単体・統合テスト80%+カバレッジ
- ✅ 0 Warning/0 Error維持
- ✅ Issue #52 Close可能状態

### 実装内容詳細（Phase_Summary.md line 84-166より）

#### 1. ユーザー一覧画面実装（3-4h）
- **削除**: `Pages/Admin/UserManagement.razor` (758行)
- **再作成**: `Components/Pages/Admin/Users/Index.razor`
- **URL**: `/admin/users`
- **機能**: 一覧表示・検索・フィルタ・ページング・編集/削除ボタン（権限制御）

#### 2. ユーザー登録画面実装（2-3h）
- **再作成**: `Components/Pages/Admin/Users/Create.razor`
- **URL**: `/admin/users/create`
- **機能**: メールアドレス・氏名・初期パスワード・ロール・所属プロジェクト
- **画面遷移**: 一覧画面 → 新規登録ボタン → 登録画面（別画面遷移）

#### 3. ユーザー編集画面実装（3-4h）
- **再作成**: `Components/Pages/Admin/Users/Edit.razor`
- **URL**: `/admin/users/edit/{id}`
- **機能**: 氏名・ロール・所属プロジェクト・ステータス・パスワードリセット
- **画面遷移**: 一覧画面 → 編集ボタン → 編集画面（別画面遷移）

#### 4. E2Eテスト作成（1-2h）
- **成果物**: `tests/E2E.Tests/user-management.spec.ts`（10シナリオ）
- **シナリオ**:
  - ユーザー一覧表示（2シナリオ）
  - ユーザー登録機能（3シナリオ）
  - ユーザー編集機能（3シナリオ）
  - ユーザー削除機能（2シナリオ）

### 重要な注意事項

**既存実装の削除について**:
- `Pages/Admin/UserManagement.razor`（758行）は**UI設計書と大きく乖離**
  - **UI設計書**: 3画面分離（一覧・登録・編集）+ 別画面遷移
  - **既存実装**: 1画面統合 + モーダル表示
  - **URL設計**: 3つのURL想定 vs 1つのURLのみ
- **判断**: 削除→再作成が最適（8-11h vs 9-12h）

---

## 📊 Step実行記録（随時更新）

### Stage 1: UI実装

**開始日時**: [YYYY-MM-DD HH:MM]

**実施内容**:
- [ ] Task 1: Index.razor実装（ユーザー一覧画面）
- [ ] Task 2: Create.razor実装（ユーザー登録画面）
- [ ] Task 3: Edit.razor実装（ユーザー編集画面）
- [ ] 共通作業: UserManagement.razor削除

**SubAgent実行結果**:
```
[csharp-web-ui Agent実行結果を記録]
- 並列実行: Task 1, 2, 3
- 実行方式: [直列/並列]
- 実行時間: [XX時間XX分]
```

**成果物**:
- [ ] `Components/Pages/Admin/Users/Index.razor`
- [ ] `Components/Pages/Admin/Users/Create.razor`
- [ ] `Components/Pages/Admin/Users/Edit.razor`
- [ ] `Pages/Admin/UserManagement.razor`（削除確認）

**問題・課題**:
```
[発生した問題・課題を記録]
```

**所要時間**: [実績XX時間]（計画: 6-8時間）

**完了日時**: [YYYY-MM-DD HH:MM]

---

### Stage 2: bUnitテスト実装

**開始日時**: [YYYY-MM-DD HH:MM]

**実施内容**:
- [ ] Task 1: UserListTests.cs実装
- [ ] Task 2: UserCreateTests.cs実装
- [ ] Task 3: UserEditTests.cs実装

**SubAgent実行結果**:
```
[unit-test Agent実行結果を記録]
- 並列実行: Task 1, 2, 3
- 実行方式: [直列/並列]
- 実行時間: [XX時間XX分]
- テストカバレッジ: [XX%]
```

**成果物**:
- [ ] `tests/UbiquitousLanguageManager.Web.UI.Tests/Admin/Users/UserListTests.cs`
- [ ] `tests/UbiquitousLanguageManager.Web.UI.Tests/Admin/Users/UserCreateTests.cs`
- [ ] `tests/UbiquitousLanguageManager.Web.UI.Tests/Admin/Users/UserEditTests.cs`

**テスト結果**:
```
[テスト実行結果を記録]
- 全テスト数: [XX件]
- Pass: [XX件]
- Fail: [XX件]
- カバレッジ: [XX%]
```

**問題・課題**:
```
[発生した問題・課題を記録]
```

**所要時間**: [実績XX時間]（計画: 2-3時間）

**完了日時**: [YYYY-MM-DD HH:MM]

---

### Stage 3: ユーザー動作確認・UIレイアウト調整

**開始日時**: [YYYY-MM-DD HH:MM]

**確認項目**:
- [ ] ユーザー一覧画面の動作確認
- [ ] ユーザー登録画面の動作確認
- [ ] ユーザー編集画面の動作確認
- [ ] サイドメニューからの遷移確認
- [ ] 権限制御の動作確認

**UIレイアウト確認結果**:
```
[ユーザー様による確認結果を記録]
- Index.razor: [OK/要調整]
- Create.razor: [OK/要調整]
- Edit.razor: [OK/要調整]
```

**調整要求事項**:
```
[ユーザー様からの調整要求を記録]
1. [調整内容1]
2. [調整内容2]
...
```

**調整実施内容**:
```
[調整実施内容を記録]
- 調整1: [実施内容]
- 調整2: [実施内容]
...
```

**所要時間**: [実績XX時間]（ユーザー確認時間含む）

**完了日時**: [YYYY-MM-DD HH:MM]

---

### Stage 4: E2Eテスト実装

**開始日時**: [YYYY-MM-DD HH:MM]

**実施内容**:
- [ ] user-management.spec.ts実装（10シナリオ）
  - [ ] ユーザー一覧表示（2シナリオ）
  - [ ] ユーザー登録機能（3シナリオ）
  - [ ] ユーザー編集機能（3シナリオ）
  - [ ] ユーザー削除機能（2シナリオ）

**SubAgent実行結果**:
```
[e2e-test Agent実行結果を記録]
- 実行時間: [XX時間XX分]
- シナリオ実装数: [XX/10]
```

**成果物**:
- [ ] `tests/E2E.Tests/user-management.spec.ts`

**E2Eテスト実行結果**:
```
[E2Eテスト実行結果を記録]
- 全シナリオ数: 10
- Pass: [XX/10]
- Fail: [XX/10]
- 実行時間: [XX秒]
```

**問題・課題**:
```
[発生した問題・課題を記録]
```

**所要時間**: [実績XX時間]（計画: 1-2時間）

**完了日時**: [YYYY-MM-DD HH:MM]

---

### Step1全体サマリ

**総所要時間**: [実績XX時間]（計画: 9-13時間 + ユーザー確認時間）

**成果物完成状況**:
- [ ] UI実装完了（3画面）
- [ ] bUnitテスト完了（3テスト、カバレッジ[XX%]）
- [ ] E2Eテスト完了（10シナリオ、Pass [XX/10]）
- [ ] ビルド成功（0 Warning/0 Error）
- [ ] Issue #52 Close可能状態

**計画vs実績差異分析**:
```
[計画と実績の差異を分析]
- 所要時間差異: [±XX時間]
- 差異要因: [要因分析]
- 教訓: [次Stepへの教訓]
```

---

## ✅ Step終了時レビュー

**Step完了時に更新**
