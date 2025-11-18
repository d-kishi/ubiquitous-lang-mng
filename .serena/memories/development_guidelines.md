# 開発ガイドライン

**最終更新**: 2025-11-18（**Serenaメモリスリム化実施・Context効率化**）

---

## 🤖 Agent Skills活用

**原則**: Agent Skillsにより詳細パターンを自律的参照（手動参照不要）

**例**: fsharp-csharp-bridge使用時、F#↔C#型変換パターンを自動適用

**詳細**: `.serena/memories/tech_stack_and_conventions.md` 参照

---

## 📋 ADR vs Agent Skills判断基準

**原則**: 「なぜこの決定をしたか」→ADR、「どう実装すべきか」→Skills（30秒判断フロー）

**例**: 歴史的記録が必要か？→YES: ADR作成

**詳細**: `Doc/08_Organization/Rules/ADRとAgent_Skills判断ガイドライン.md` 参照

---

## 🔴 プロセス遵守絶対原則

**原則**: コマンド=契約、承認=必須、手順=聖域（違反は一切許容されない）

**例**: 承認前の作業開始禁止・成果物の虚偽報告禁止

**詳細**: `CLAUDE.md` - ADR_016プロセス遵守絶対原則 参照

---

## 📖 開発プロセス・判断基準

### 3回修正試行ルール

**原則**: 3回修正試行で解決しない場合は方針転換（効率性重視）

**例**: 3回試行後も失敗 → アプローチ変更判断

**詳細**: `Doc/08_Organization/Rules/開発手法詳細ガイド.md` 参照

### Step再実行プロセス

**原則**: Step失敗時は原因分析→Issue記録→再実行判断

**例**: 仕様理解不足 → Issue記録 → 同一Step再実行

**詳細**: `Doc/08_Organization/Rules/開発手法詳細ガイド.md` 参照

### 技術調査時のアーキテクチャ図作成標準

**原則**: 技術調査時はMermaid形式アーキテクチャ図を必ず作成

**例**: Playwright統合調査 → 統合アーキテクチャ図作成（プロセス境界・通信方式明示）

**詳細**: `Doc/08_Organization/Rules/開発手法詳細ガイド.md` 参照

### 品質vs効率トレードオフ判断基準

**原則**: Phase初期=品質優先、Phase後期=効率優先（段階的移行）

**例**: Phase A-B（基盤確立期） → 品質優先、Phase E-F（最適化期） → 効率優先

**詳細**: `Doc/08_Organization/Rules/開発手法詳細ガイド.md` 参照

### Step目的の明確化プロセス

**原則**: Step開始前に目的・成果物を明確化（曖昧なまま開始禁止）

**例**: Step目的不明確 → ユーザー確認必須

**詳細**: `Doc/08_Organization/Rules/開発手法詳細ガイド.md` 参照

### VSCode拡張機能更新時の検証プロセス

**原則**: VS Code拡張機能更新時は影響範囲確認→検証→段階的適用

**例**: C#拡張更新 → ビルド検証 → 問題なければ適用

**詳細**: `Doc/08_Organization/Rules/開発手法詳細ガイド.md` 参照

---

## 🧪 E2Eテスト運用

### E2Eテスト実装タイミング原則

**原則**: 機能実装完了後すぐにE2Eテスト作成（後回し禁止）

**例**: 認証機能実装完了 → 即座にE2Eテスト実装

**詳細**: `Doc/08_Organization/Rules/Playwright_運用統合ガイドライン.md` 参照

### Playwright Test Agents活用指針

**原則**: MainAgentオーケストレーション型（新規/大規模）、e2e-testスタンドアロン型（メンテナンス/小規模）

**例**: 新機能E2E → パターンA（planner→generator→e2e-test→healer）

**詳細**: `Doc/08_Organization/Rules/Playwright_運用統合ガイドライン.md` 参照

### E2Eテスト作成前の前提条件確認プロセス

**原則**: E2Eテスト作成前に前提条件確認（機能実装完了・データ準備・環境構築）

**例**: 機能実装未完了 → E2E作成延期

**詳細**: `Doc/08_Organization/Rules/Playwright_運用統合ガイドライン.md` 参照

---

## 🏗️ アーキテクチャ・設計原則

### namespace設計原則

**原則**: Clean Architecture準拠のnamespace階層化（レイヤー分離原則）

**例**: Domain/Application/Contracts/Infrastructure/Web

**詳細**: `.claude/skills/clean-architecture-guardian/` 参照

---

## 🎯 初学者対応・コメント規約

### Blazor Server・F#初学者対応

**原則**: プロジェクトオーナーが初学者のため詳細なコメント必須

**例**: Blazor: StateHasChanged説明、F#: Option型概念説明

**詳細**: `CLAUDE.md` - Blazor Server・F#初学者対応 参照

---

## 🔴 メインエージェント責務

### メインエージェント必須遵守事項

**原則**: エラー修正は責務分担原則に基づきSubAgentへ委託（効率性より責務遵守優先）

**例**: F# Domain層エラー → fsharp-domain Agent委託

**詳細**: `CLAUDE.md` - メインエージェント必須遵守事項 参照

---

## 🧪 テストアーキテクチャ

### 新規テストプロジェクト作成時の必須確認事項

**原則**: ADR_020準拠のレイヤー×テストタイプ分離方式（命名規則・参照関係原則確認必須）

**例**: UbiquitousLanguageManager.{Layer}.{TestType}.Tests

**詳細**: `CLAUDE.md` + ADR_020 参照

---

**最終更新**: 2025-11-18（**Serenaメモリスリム化実施・Context効率化・-60%削減**）
