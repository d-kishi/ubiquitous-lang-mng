# 開発ガイドライン

## 開発方針・原則

### 基本原則（ADR_016準拠）
- **プロセス遵守絶対原則**: コマンド=契約として一字一句遵守
- **承認=必須**: ユーザー承認表記は例外なく取得
- **手順=聖域**: 定められた順序の変更禁止
- **品質基準**: 0 Warning, 0 Error状態維持必須

### Clean Architecture遵守
- **F# Domain/Application層**: 関数型プログラミングパターン適用
- **C# Infrastructure/Web層**: オブジェクト指向パターン適用
- **Contracts層**: F#↔C#境界の型変換（TypeConverter基盤580行確立済み）
- **依存関係**: Web→Contracts→Application→Domain, Infrastructure→Domain

### 実装品質基準
- **テストファースト**: TDD Red-Green-Refactorサイクル実践
- **カバレッジ**: 95%以上維持（現在220テスト）
- **コメント**: Blazor Server・F#初学者向け詳細説明必須（ADR_010）
- **用語統一**: 「用語」ではなく「ユビキタス言語」使用（ADR_003）

## フェーズベース開発体制

### Phase実行プロセス（確立済み）
1. **phase-start Command**: Phase枠組み確立・Phase_Summary.md作成
2. **SubAgent並列分析**: 4領域同時分析による包括的現状把握
3. **step-start Command**: Step開始準備・前提条件確認
4. **実装実行**: プロダクト精度最優先・十分な時間確保
5. **step-end-review Command**: 品質確認・テスト実行
6. **phase-end Command**: Phase総括・次Phase準備

### SubAgentプール方式（ADR_013）
- **並列実行**: 複数Agent同時実行による効率化
- **専門特化**: spec-compliance, design-review, dependency-analysis, tech-research
- **統合分析**: 各Agent結果の統合による確度向上
- **適用実績**: Phase A9計画策定で4SubAgent並列分析成功

### Commands体系活用
- **自動実行Commands**: session-start, session-end, phase-start等
- **品質チェック**: spec-compliance-check, tdd-practice-check
- **レビュー**: step-end-review（包括的品質・進捗確認）

## 技術実装ガイドライン

### F# Domain/Application層
- **Railway-oriented Programming**: Result型・Option型活用
- **Smart Constructor Pattern**: ドメインモデル不正値排除
- **Async by Design**: 非同期処理の一貫した実装
- **パターンマッチング**: 網羅的ケース分岐・コンパイラ支援活用

### Blazor Server実装
- **StateHasChanged**: 状態変更時の明示的UI更新
- **ライフサイクル**: OnInitializedAsync等の適切な実装
- **SignalR統合**: リアルタイム機能での活用
- **認証統合**: ASP.NET Core Identity完全統合

### TypeConverter基盤
- **F#↔C#境界**: 580行の型変換基盤活用
- **認証特化型変換**: IAuthenticationService統合対応
- **自動変換**: AutoMapperパターン回避・明示的変換

### データベース設計
- **Entity Framework Core**: Code-First移行管理
- **PostgreSQL**: Docker Container活用
- **Repository Pattern**: Infrastructure層での抽象化
- **Identity統合**: ASP.NET Core Identity完全統合

## プロダクト精度重視方針

### 時間見積もり原則（Phase A9確立）
- **プロダクト精度 > 時間効率**: 品質妥協・機能削減回避
- **十分な実装時間**: Phase A9で240分→420分修正実績
- **段階的品質確認**: 各Step完了時のテスト・ビルド・動作確認必須
- **リスク軽減**: 時間に追われた品質低下の事前防止

### 実装参照情報整備
- **Phase_Summary.md**: 実装時必須参照情報完備
- **技術調査レポート**: 実装例・パターン・制約情報提供
- **統合分析サマリー**: 全体方針・重点確認事項整理
- **依存関係分析**: リスク軽減策・実装順序推奨

## 課題・技術負債管理

### GitHub Issues統合管理（ADR_015）
- **技術負債**: TECH-XXX形式での管理
- **プロセス改善**: PROC-XXX形式での管理
- **コミュニケーション**: COM-XXX形式での管理
- **Issue更新**: 解決・進捗・新発見の即時反映

### 技術負債解消状況（2025-09-07現在）
- **TECH-002**: 初期パスワード不整合 → 完全解決（Phase A8）
- **TECH-006**: Headers read-onlyエラー → 完全解決（Phase A8）
- **TECH-007**: 仕様準拠チェック機構 → 完全解決（spec-compliance Agent改善）
- **継続監視**: 新規技術負債の発生防止・早期発見

## コミュニケーション・記録管理

### セッション記録体系
- **日次記録**: /Doc/04_Daily/YYYY-MM/YYYY-MM-DD.md
- **プロジェクト状況**: /Doc/プロジェクト状況.md（次回推奨範囲更新）
- **ADR記録**: 重要決定の構造化記録
- **週次総括**: 週末セッション時の統合振り返り

### Serenaメモリー管理
- **5種類メモリー**: project_overview, development_guidelines, tech_stack_and_conventions, session_insights, task_completion_checklist
- **セッション終了時更新**: session-end Commandでの必須更新
- **次回参照準備**: 更新内容の実用性・参照可能性確保

### 品質評価・改善サイクル
- **目的達成度**: 定量評価（100%/80%/60%等）
- **時間効率測定**: 予定vs実際の比較・改善要因特定
- **適用手法効果**: SubAgent・Commands効果の具体的測定
- **プロセス改善**: 次回セッション最適化提案

## フレームワーク制約の現実的判断（Phase A9確立）

### ASP.NET Core Identity統合制約
- **構造的制約**: UserManager・SignInManagerの必然的依存
- **セキュリティ制約**: パスワードハッシュ化・トークン管理の内部実装依存
- **現実的最適解**: Infrastructure層18-19/20点が投資対効果考慮した最適解
- **技術的妥当性**: 完全自作vs実用性のトレードオフ理解

### UI設計準拠vs機能実装バランス
- **認証画面**: 95/100点の優秀品質（Phase A8成果）
- **管理画面**: Phase B1での統合実装推奨
- **準拠戦略**: Clean Architecture改善→UI準拠改善の段階実施

---

**最終更新**: 2025-09-07（Phase A9計画策定完了）  
**適用フェーズ**: Phase A9以降  
**重点方針**: プロダクト精度最優先・十分な実装時間確保・品質妥協回避