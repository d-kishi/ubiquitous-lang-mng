# Step 10 組織設計・実行記録

## 🎯 Step目的（Why）

**このStepで達成すべきこと**:
- Web層22件のビルドエラー解消
- Web層の「余計な処理」（Guid.TryParse、long.TryParse）削除
- Identity ID（string型）をstringのまま扱う設計への統一

**Phase全体における位置づけ**:
- **Phase全体の課題**: AspNetUsers.Id（string/GUID文字列）をアプリケーション全体でstringのまま扱う
- **このStepの役割**: Web層（Blazor Serverコンポーネント）の型統一・余計な変換処理の削除

**関連Issue**: [GitHub Issue #79](https://github.com/d-kishi/ubiquitous-lang-mng/issues/79)

---

## 📋 Step概要

| 項目 | 内容 |
|------|------|
| **Step名** | Step 10: Web層修正 |
| **作業特性** | 実装・リファクタリング |
| **推定工数** | 2-3時間 |
| **ゴール寄与率** | 10%（Phase全体75%→85%） |
| **開始日** | 2025-12-08 |

---

## 🛠 修正方針（ユーザー合意済み）

### 採用方針: 根本修正アプローチ

**Issue #79の本質に沿った修正**:
- `currentUserId`等をGuid型 → string型に変更
- `Guid.TryParse` 7箇所削除（Identity IDをそのまま使用）
- `long.TryParse` 3箇所削除（不要なlong変換を削除）
- 22ビルドエラー解消（型変更で多くは自動解消）

**あるべき姿**:
```csharp
// Before（余計な処理）
private Guid currentUserId;
Guid.TryParse(identityId, out var userId);
new SomeCommand(..., userId: currentUserId.ToString(), ...)

// After（根本修正）
private string currentUserId = string.Empty;
currentUserId = identityId;  // 変換不要
new SomeCommand(..., userId: currentUserId, ...)  // .ToString()不要
```

---

## 🏢 組織設計

### SubAgent構成（2並列実行）

| SubAgent | 役割 | 担当ファイル |
|----------|------|-------------|
| **csharp-web-ui (Agent 1)** | ProjectManagement系修正 | ProjectMembers.razor, ProjectList.razor, ProjectCreate.razor, ProjectEdit.razor（4ファイル） |
| **csharp-web-ui (Agent 2)** | Admin/Users系 + Service修正 | Index.razor, Create.razor, Edit.razor, BlazorAuthenticationService.cs（4ファイル） |

### 並列実行によるメリット

- **時間短縮**: 2-3時間 → 1-1.5時間
- **機能グループ明確**: 各Agentの責務が明確
- **独立性**: 8ファイルは互いに依存関係なし

### Stage構成

```
Stage 1: Web層根本修正（1-1.5時間）【2並列実行】

  [Agent 1: ProjectManagement系]
  - ProjectMembers.razor: currentUserId string化、Guid.TryParse削除
  - ProjectList.razor: currentUserId string化、Guid.TryParse削除
  - ProjectCreate.razor: currentUserId string化、Guid.TryParse削除
  - ProjectEdit.razor: currentUserId string化、Guid.TryParse削除

  [Agent 2: Admin/Users系 + Service]
  - Index.razor: long.TryParse削除、Guid.TryParse削除
  - Create.razor: Guid.TryParse削除
  - Edit.razor: long.TryParse削除、Guid.TryParse削除
  - BlazorAuthenticationService.cs: long→string変換修正

Stage 2: ビルド確認・品質確認（0.5時間）
  - 全層統合ビルド（0 Error, 0 Warning確認）
  - 動作確認（必要に応じて）
```

---

## 📝 修正対象詳細

### 必須参照資料

- `Research/Step10_Web層修正対象一覧.md`（詳細な行番号・修正パターン）

### 修正対象ファイル一覧（8ファイル）

| # | ファイル | パス | 主要修正内容 |
|---|---------|------|-------------|
| 1 | BlazorAuthenticationService.cs | Services/ | long→string変換修正 |
| 2 | ProjectMembers.razor | Components/Projects/ | currentUserId string化、Guid.TryParse削除 |
| 3 | ProjectList.razor | Components/Pages/ProjectManagement/ | currentUserId string化、Guid.TryParse削除 |
| 4 | Index.razor (Users) | Components/Pages/Admin/Users/ | long.TryParse削除、Guid.TryParse削除 |
| 5 | ProjectCreate.razor | Components/Pages/ProjectManagement/ | currentUserId string化、Guid.TryParse削除 |
| 6 | ProjectEdit.razor | Components/Pages/ProjectManagement/ | currentUserId string化、Guid.TryParse削除 |
| 7 | Create.razor (Users) | Components/Pages/Admin/Users/ | Guid.TryParse削除 |
| 8 | Edit.razor (Users) | Components/Pages/Admin/Users/ | long.TryParse削除、Guid.TryParse削除 |

### 削除対象（波及効果対応）

| 対象 | 箇所数 | 対応内容 |
|------|--------|----------|
| **Guid.TryParse** | 7箇所 | 削除（Identity IDをそのまま使用） |
| **long.TryParse** | 3箇所 | 削除（不要なlong変換を排除） |
| **Guid型変数宣言** | 5箇所 | string型に変更 |

### Guid.TryParse削除対象（7箇所）

| ファイル | 行番号 | 対応 |
|---------|--------|------|
| ProjectMembers.razor | 220 | 削除、Identity IDをそのまま使用 |
| ProjectMemberSelector.razor | 146 | 削除、Identity IDをそのまま使用 |
| ProjectCreate.razor | 185 | 削除、Identity IDをそのまま使用 |
| ProjectList.razor | 297 | 削除、Identity IDをそのまま使用 |
| ProjectEdit.razor | 275 | 削除、Identity IDをそのまま使用 |
| Create.razor (Users) | 471 | 削除、Identity IDをそのまま使用 |
| Edit.razor (Users) | 599 | 削除、Identity IDをそのまま使用 |

### long.TryParse削除対象（3箇所）

| ファイル | 行番号 | 対応 |
|---------|--------|------|
| Index.razor (Users) | 343 | 削除、stringのまま使用 |
| Edit.razor (Users) | 802 | 削除、stringのまま使用 |
| CustomAuthenticationStateProvider.cs | 169, 176 | 確認（影響あれば対応） |

---

## 🎯 Step成功基準

### 必須基準

- [ ] 22ビルドエラー全て解消
- [ ] Web層ビルド: 0 Error(s), 0 Warning(s)
- [ ] 全層統合ビルド成功
- [ ] Guid.TryParse 7箇所削除完了
- [ ] long.TryParse 3箇所削除完了
- [ ] currentUserId等がstring型に変更完了

### 品質基準

- [ ] Clean Architecture準拠性維持
- [ ] Issue #79の方針に沿った実装（Identity IDをstringのまま扱う）

---

## 📊 Step実行記録（随時更新）

### Stage 1: Web層根本修正

**開始時刻**: 2025-12-08
**終了時刻**: 2025-12-08
**実施内容**:

**2並列実行完了**:
- **Agent 1（ProjectManagement系4ファイル）**:
  - ProjectMembers.razor: currentUserId string化、Guid.TryParse削除
  - ProjectList.razor: currentUserId string化、Guid.TryParse削除
  - ProjectCreate.razor: currentUserId string化、Guid.TryParse削除
  - ProjectEdit.razor: currentUserId string化、Guid.TryParse削除
- **Agent 2（Admin/Users系+Service）**:
  - Index.razor: long.TryParse削除、Guid.TryParse削除
  - Create.razor: Guid.TryParse削除
  - Edit.razor: long.TryParse削除、Guid.TryParse削除
  - BlazorAuthenticationService.cs: long→string変換修正

**追加修正**:
- ProjectMemberSelector.razor: EventCallback型修正（Guid?→string?）、Guid.TryParse削除

### Stage 2: ビルド確認・品質確認

**開始時刻**: 2025-12-08
**終了時刻**: 2025-12-08
**実施内容**:

- 全src/層ビルド確認: **0 Error(s), 0 Warning(s)**
- テストコードエラー: 78件（Step 12スコープ）

---

## ✅ Step終了時レビュー

### 完了チェックリスト

- [x] BlazorAuthenticationService.cs 修正完了
- [x] ProjectMembers.razor 修正完了
- [x] ProjectList.razor 修正完了
- [x] Index.razor (Users) 修正完了
- [x] ProjectCreate.razor 修正完了
- [x] ProjectEdit.razor 修正完了
- [x] Create.razor (Users) 修正完了
- [x] Edit.razor (Users) 修正完了
- [x] Guid.TryParse 8箇所削除完了（当初予定7箇所+ProjectMemberSelector1箇所）
- [x] long.TryParse 3箇所削除完了
- [x] 全層統合ビルド成功（src/: 0 Error, 0 Warning）

### ビルド結果

```
Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:08.03
```

**各層ビルド結果**:
- UbiquitousLanguageManager.Domain: ✅ OK
- UbiquitousLanguageManager.Application: ✅ OK
- UbiquitousLanguageManager.Contracts: ✅ OK
- UbiquitousLanguageManager.Infrastructure: ✅ OK
- UbiquitousLanguageManager.Web: ✅ OK

### 次Step申し送り事項

1. **Step 11**: 初期データSQL修正（UserId型統一対応）
2. **Step 12**: テストコード修正（78件のエラー対応）
   - Domain.Unit.Tests: F# UserId型修正
   - Contracts.Unit.Tests: TypeConverters/AuthenticationMapper修正
   - Infrastructure.Unit.Tests: NotificationService/AuthenticationService修正

---

**作成日**: 2025-12-08
**最終更新**: 2025-12-08
