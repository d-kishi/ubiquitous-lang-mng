---
paths:
  - src/**
  - Doc/**
---

# 用語表記統一ルール

## 概要

DDD（ドメイン駆動設計）におけるユビキタス言語の概念を正確に表現するための用語表記ルール。

## 基本方針

システム全体において、以下の表記統一ルールを適用する。

## 日本語表記

- ❌ 「用語」
- ✅ 「ユビキタス言語」

## 英語表記（物理名・識別子）

- ❌ 「Term」
- ✅ 「UbiquitousLang」

## 適用範囲

- データベース設計書（論理名・物理名）
- DDLファイル（テーブル名・カラム名・コメント）
- プログラムコード（クラス名・メソッド名・変数名）
- API仕様書
- ユーザーインターフェース
- ドキュメント全般

## 具体例

### テーブル名

```sql
-- ❌ 旧表記
DraftTerms, FormalTerms, RelatedTerms

-- ✅ 新表記
DraftUbiquitousLang, FormalUbiquitousLang, RelatedUbiquitousLang
```

### カラム名

```sql
-- ❌ 旧表記
SourceTermId, TargetTermId, RelatedTermsSnapshot

-- ✅ 新表記
SourceUbiquitousLangId, TargetUbiquitousLangId, RelatedUbiquitousLangSnapshot
```

### JSON構造

```json
// ❌ 旧表記
{
  "relatedTerms": [
    {
      "targetTermId": 456,
      "targetTermName": "顧客"
    }
  ]
}

// ✅ 新表記
{
  "relatedUbiquitousLangs": [
    {
      "targetUbiquitousLangId": 456,
      "targetJapaneseName": "顧客"
    }
  ]
}
```

## 理由

1. **ドメイン概念の正確性**: DDDにおける「ユビキタス言語」は単なる「用語」以上の概念
2. **一貫性の確保**: システム全体で統一された表記により、開発者・利用者の理解を促進
3. **将来の拡張性**: ユビキタス言語固有の機能追加時に適切な命名が可能
4. **ビジネス価値の明確化**: 単なる用語管理ではなく、ユビキタス言語管理システムとしての価値を明確化

---

**抽出元ADR**: ADR_003_用語表記統一ルール.md
**作成日**: 2025-12-17
**Phase**: Issue #83 ルール管理基盤改善

