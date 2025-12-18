# ファイル管理規約

**目的**: 組織設計ファイルの統一的管理・効率的参照
**適用範囲**: 全Phase・全Step

---

## 📁 ディレクトリ構造

```
/Doc/08_Organization/
├── Guide/                           # 運用ガイド
├── Active/                          # 実行中Phase
│   └── Phase_XX/
│       ├── Phase_Summary.md         # Phase全体概要・総括
│       ├── Step01_Analysis.md       # Step1組織設計（全Phase共通）
│       └── StepXX_[内容].md         # 各Step組織設計
├── Completed/                       # 完了Phase
├── Patterns/                        # 再利用可能パターン
└── Templates/                       # テンプレート
```

---

## 📋 ファイル命名規則

### 命名パターン

| ファイル | 用途 |
|---------|------|
| **Phase_Summary.md** | Phase全体概要・総括（固定名） |
| **Step01_Analysis.md** | Step1組織設計（全Phase共通） |
| **Step02_[内容].md** | Step2以降（Phase固有内容） |

### Phase固有内容例

- **Phase A1**: Step02_Implementation.md, Step03_Testing.md
- **Phase A2**: Step02_Domain_Implementation.md, Step03_Infrastructure.md
- **Phase B1**: Step02_Project_Implementation.md, Step03_Integration.md

---

## 🔄 ファイル管理運用ルール

### Phase開始時

1. **phase-start Command実行** → Phase開始準備
2. **Phaseディレクトリ作成** → `Active/Phase_XX/`
3. **Phase_Summary.md作成** → 概要・成功基準・組織方針
4. **Step01_Analysis.md作成** → Step1組織設計

### Step1終了時

1. **🔴 成果物出力確認（必須）** → `/Doc/05_Research/Phase_XX/` 配下全確認
2. **Step1レビュー記録** → Step01_Analysis.md更新
3. **Phase計画更新** → Phase_Summary.md「全Step実行プロセス」記録
4. **次Step組織設計ファイル作成**

### 各Step終了時

1. **Stepレビュー記録** → StepXX_[内容].md更新
2. **次Step組織設計ファイル作成**

### Phase完了時

1. **Phase総括記録** → Phase_Summary.md「Phase総括レポート」追加
2. **ディレクトリ移動** → Active → Completed

---

## 📊 Step1分析結果記録システム

**記録場所**: `/Doc/05_Research/Phase_XX/`

**ファイル構成**:
- Step1_Analysis_Results.md（統合分析結果）
- Database_Design_Review.md
- [チーム名]_Research.md（各チーム調査結果）

**重要**: 組織構成情報は必ず`Active/Phase_XX/`配下に記録

---

## 🔧 技術負債管理

**記録場所**: `/Doc/10_Debt/Phase_XX_Implementation_Planning.md`

**分類基準**:
| 優先度 | 対象 |
|--------|------|
| 🔴 高 | ユーザー価値直結・セキュリティ必須 |
| 🟡 中 | 運用効率・開発者体験向上 |
| 🟢 低 | 将来拡張・nice-to-have |

---

## 📚 セッション継続時の文脈復元

### Step2以降開始時の必須読み込み

| 対象 | ファイル |
|------|---------|
| **Research** | `/Doc/05_Research/Phase_XX/` 全ファイル |
| **Organization** | `/Doc/08_Organization/Active/Phase_XX/` 全ファイル |

---

## 🎯 アクセス効率最適化

### セッション開始時必読

1. `/CLAUDE.md` - プロジェクト概要
2. `.claude/rules/operations/organization-manual.md` - 実行手順
3. `/Doc/プロジェクト状況.md` - 最新状況
4. `/Doc/04_Daily/` 直近3日の作業記録

### 動的読み込み

| タイミング | 参照ファイル |
|-----------|-------------|
| Step開始時 | `organization-manual.md` |
| Phase計画時 | `Phase特性別テンプレート.md` |
| テスト実装時 | `テスト戦略ガイド.md` |

---

## 🔍 品質保証

- **命名一貫性**: 規約準拠の命名維持
- **内容完全性**: 必須セクションの記録完了
- **更新適時性**: 進捗に応じた適切な更新
- **継続的改善**: Phase毎評価・規約改善

---

**注意**: 運用手順の詳細は `.claude/rules/operations/organization-manual.md` を参照
