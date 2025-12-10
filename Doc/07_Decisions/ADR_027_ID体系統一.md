# ADR_027: ID体系統一（AspNetUsers.Id string型統一）

**作成日**: 2025-12-10
**決定日**: 2025-12-10
**ステータス**: Accepted
**対応Phase**: Phase Issue79
**対応Step**: Step 1-13
**対応Issue**: GitHub Issue #79

---

## 概要

ASP.NET Core IdentityのAspNetUsers.Id（string/GUID文字列）をアプリケーション全体でstringのまま扱う設計に統一し、不安定なGetHashCode()変換およびGuid.TryParse処理を排除する。

---

## 背景・課題

### 課題1: ID型の不整合

ASP.NET Core IdentityはAspNetUsers.Idをstring型（GUID文字列）で管理するが、アプリケーション設計時にF# UserId型をint64と誤設計。これにより以下の「余計な処理」が全層に実装された：

| 層 | 余計な処理 | 問題点 |
|----|-----------|--------|
| Web層 | Guid.TryParse(identityId) | stringをGuidに変換する必要なし |
| Application層 | Guid.GetHashCode() → int64 | 不安定・衝突リスク・逆算不可 |
| Domain層 | UserId of int64 | stringであるべき |

### 課題2: PM権限機能の不具合（Issue #79本質的問題）

GetHashCode()変換の不安定性により、以下の機能が正常動作しなかった：

- **PMがユーザー一覧を閲覧できない**（検索結果が空）
- **PMがプロジェクトフィルタを使用できない**

### 課題3: GetHashCode()の技術的問題

.NET公式ドキュメント記載の問題点：

1. **プロセス間非一致**: 異なるプロセスで同一値を保証しない
2. **バージョン間非一致**: .NETバージョンで値が変わる可能性
3. **衝突リスク**: 異なるGUIDで同一ハッシュ値になる可能性
4. **逆算不可**: ハッシュ値から元のGUIDを復元できない

---

## 決定内容

### 採用方針: AspNetUsers.Idをstringのまま全層で扱う

#### 1. Domain層（Step 6）

```fsharp
// 変更前
type UserId = UserId of int64

// 変更後
type UserId = UserId of string
```

#### 2. Application層（Step 7）

- Commands/QueriesのGuid型フィールド → string型（22箇所）
- GetHashCode()変換削除（27箇所）

#### 3. Infrastructure層（Step 8-9）

- GetHashCode()排除（2箇所）
- 冗長な.ToString()削除（20箇所）

#### 4. Contracts層（Step 9）

- DTO型変更（UserId関連 long→string）
- TypeConverters/AuthenticationMapper修正

#### 5. Web層（Step 10）

- Guid.TryParse削除（8箇所）
- long.TryParse削除（3箇所）

#### 6. InitialData（Step 11）

- 人間可読ID → GUID文字列形式に統一

### スコープ制限（重要な設計判断）

| ID型 | 修正 | 理由 |
|------|------|------|
| **UserId** | int64 → string | ASP.NET Identity GUID対応 |
| **ProjectId** | int64維持 | DBがbigint、変換不要 |
| **DomainId** | int64維持 | DBがbigint、変換不要 |
| **UbiquitousLanguageId** | int64維持 | DBがbigint、変換不要 |

**根拠**: ASP.NET Core IdentityのみがGUID文字列を使用。アプリケーション固有テーブルはbigint（自動採番）で設計されており、変換は不要。

---

## 判断根拠

### 効果

1. **PM権限機能の正常化**: ユーザー一覧・プロジェクトフィルタが正常動作
2. **GetHashCode()リスク排除**: プロセス間一貫性・衝突リスクを解消
3. **設計の簡素化**: 不要な型変換層を排除し、保守性向上
4. **コード品質向上**: 全テスト（384件）パス確認済み

### 削除した処理（定量実績）

| 処理 | 削除箇所数 |
|------|----------|
| GetHashCode()使用 | 30箇所 |
| Guid.TryParse | 8箇所 |
| long.TryParse | 3箇所 |
| **合計** | **41箇所** |

### E2E検証結果（Step 13実施）

| テストケース | 結果 |
|-------------|------|
| SuperUserログイン | ✅ Pass |
| ProjectManagerログイン | ✅ Pass |
| DomainApproverログイン | ✅ Pass |
| GeneralUserログイン | ✅ Pass |
| PM権限ユーザー一覧表示 | ✅ Pass |

---

## 実施Step履歴

| Step | 内容 | 成果 |
|------|------|------|
| Step 1 | 問題分析 | GetHashCode問題特定、影響調査 |
| Step 2-5 | 過剰スコープ失敗 | 全ID型string化を試行→断念 |
| Step 6 | Domain層修正 | UserId型変更（int64→string） |
| Step 7 | Application層修正 | GetHashCode排除27箇所 |
| Step 8 | Infrastructure層修正 | GetHashCode排除2箇所 |
| Step 9 | Contracts層修正 | TypeConverters修正 |
| Step 9.5 | プロセス改善 | 事前調査プロセス策定 |
| Step 10 | Web層修正 | Guid.TryParse削除8箇所 |
| Step 11 | InitialData修正 | GUID文字列形式統一 |
| Step 12 | テスト修正 | 384 Pass, 21 Skip達成 |
| Step 13 | 統合テスト・完了 | E2E検証、ADR作成 |

---

## リスク評価

### 技術的リスク: 低

- 全テスト（384件）パス確認済み
- E2E全4ロール動作確認済み
- ビルド0 Warning, 0 Error維持

### 互換性リスク: 低

- InitialDataをGUID形式に統一
- 既存データとの互換性は維持
- DB移行不要（AspNetUsers.Idは元からstring型）

### 残存課題

| 課題 | 優先度 | 管理場所 |
|------|--------|---------|
| Skipテスト21件 | 中 | GitHub Issue #82 |
| data-testid統一化 | 低 | 次Phase検討 |

---

## 学習事項

### 失敗から得た教訓（Step 2-5）

1. **過剰スコープの危険性**: 全ID型のstring化は不要な設計複雑化を招いた
2. **DB設計の尊重**: ASP.NET Identity（string）とアプリ固有テーブル（long）の違いを尊重すべき
3. **事前調査の重要性**: 型変更は影響範囲が広く、網羅的な事前調査が必須

### プロセス改善（Step 9.5）

型変更を伴うStep計画時の事前調査プロセスを強化：

1. **影響範囲調査**: 全プロジェクト横断でのシンボル検索
2. **依存関係分析**: 変更対象シンボルの参照元特定
3. **修正順序決定**: 依存関係に基づく修正順序策定
4. **工数見積り**: 修正箇所数に基づく正確な見積り

---

## 関連情報

### ドキュメント

- **Phase_Summary.md**: 全Step実行記録・成果物参照マトリックス
- **Research/GetHashCode使用箇所一覧.md**: 30箇所の詳細
- **Research/ID変換フロー図.md**: Mermaid形式フロー図
- **Research/InitialData再設計書.md**: GUID変換マッピング

### GitHub Issues

- **Issue #79**: ID体系統一リファクタリング（本ADR対応Issue）
- **Issue #82**: Skipテスト21件の技術負債管理

### 関連ADR

- **ADR_016**: プロセス遵守違反防止策（Step実行時のプロセス遵守）
- **ADR_020**: テストアーキテクチャ決定（テスト構成の基盤）

---

## 承認記録

**決定者**: プロジェクトオーナー
**承認日**: 2025-12-10（Phase Issue79 Step 13完了時）
**承認コメント**: E2Eテスト全パス確認、PM権限機能正常動作確認

---

**最終更新**: 2025-12-10（Phase Issue79 Step 13）
