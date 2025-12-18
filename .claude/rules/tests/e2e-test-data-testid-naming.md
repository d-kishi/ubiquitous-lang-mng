---
paths:
  - tests/**
  - src/UbiquitousLanguageManager.Web/**/*.razor
---

# E2Eテスト data-testid 命名規則

## 概要

E2Eテストで使用する `data-testid` 属性の命名規則。Phase B2 Step5で実証済み。

---

## 🔴 CRITICAL: 命名規則

### 基本テンプレート

```
data-testid="{target}-{type}"
```

| 要素 | 説明 | 例 |
|------|------|-----|
| **{target}** | 対象要素の意味的名称（小文字・ハイフン区切り） | member, username, project |
| **{type}** | 要素タイプ | button, input, list, card, error-message, link, selector |

### 要素タイプ別パターン

| タイプ | パターン | 例 |
|--------|---------|-----|
| **button** | `{action/target}-button` | `member-add-button`, `login-button` |
| **input** | `{field-name}-input` | `username-input`, `password-input` |
| **list** | `{items}-list` | `member-list`, `project-list` |
| **card** | `{item}-card` | `member-card`, `project-card` |
| **error-message** | `{context}-error-message` | `member-error-message`, `login-error-message` |
| **link** | `{destination}-link` | `member-management-link` |
| **selector** | `{item}-selector` | `member-selector`, `role-selector` |

---

## コード例

### Blazorコンポーネント（実装側）

```razor
<!-- ボタン -->
<button data-testid="member-add-button" @onclick="AddMember">追加</button>

<!-- 入力 -->
<input data-testid="username-input" type="text" @bind="Username" />

<!-- リスト + カード -->
<div data-testid="member-list">
    @foreach (var member in Members)
    {
        <div data-testid="member-card">
            <span data-testid="member-name">@member.Name</span>
        </div>
    }
</div>

<!-- エラーメッセージ -->
<div data-testid="member-error-message" class="alert alert-danger">@ErrorMessage</div>
```

### Playwrightテスト（テスト側）

```typescript
// ✅ 正しい: data-testid使用
await page.click('[data-testid="member-add-button"]');
await page.fill('[data-testid="username-input"]', 'testuser');

// ✅ 階層的セレクタ
await page.locator('[data-testid="member-list"]')
          .locator('[data-testid="member-card"]').first()
          .locator('[data-testid="member-delete-button"]')
          .click();
```

---

## ベストプラクティス

### ✅ 推奨パターン

| 項目 | 説明 |
|------|------|
| **階層的命名** | 親要素からの相対セレクタで名前空間衝突回避 |
| **汎用的命名** | `member-card`（動的ID埋め込み禁止） |
| **data-testid専用** | テスト用途専用でUI変更の影響を受けない |

### ❌ 禁止パターン

| 禁止項目 | 理由 |
|----------|------|
| **CSS/XPathセレクタ** | UI変更で即座に破損（`.btn-primary`, `#submit`） |
| **動的ID埋め込み** | テストが脆弱化（`member-@member.Id`） |
| **テキストセレクタ** | 多言語対応で破損（`button:has-text("追加")`） |

---

## 実装ガイド

### Blazorコンポーネント実装時

1. ユーザーインタラクション要素には必ず `data-testid` を付与
2. 命名規則 `{target}-{type}` を遵守
3. E2Eテストで検証が必要な表示要素にも付与

### E2Eテスト実装時

1. セレクタは `[data-testid="xxx"]` 形式のみ使用
2. 相対セレクタを活用（親要素からの探索）
3. テスト可読性を重視した意味的な命名確認

---

## 関連ドキュメント

- **詳細パターン**: `.claude/skills/playwright-e2e-patterns/patterns/data-testid-design.md`
- **Playwright E2E Skill**: `.claude/skills/playwright-e2e-patterns/SKILL.md`

---

**作成日**: 2025-12-18
**抽出元**: `.claude/skills/playwright-e2e-patterns/patterns/data-testid-design.md`
