# Step 06 組織設計・実行記録

## 📋 Step概要
- **Step名**: Step06 統合品質保証・完了確認
- **作業特性**: 品質保証・統合テスト・技術負債解消
- **推定期間**: 1セッション（60-90分）
- **開始日**: 2025-08-26
- **完了予定日**: 2025-08-26（同日完了予定）

## 🏢 組織設計

### SubAgent構成
**選択根拠**: Pattern C（品質改善・技術負債解消）
- **csharp-web-ui**: TECH-006修正実装専門（Login.razor・Blazor Server認証統合）
- **integration-test**: 認証フロー統合テスト・TECH-006修正確認専門
- **spec-compliance**: 最終仕様準拠監査・95%達成確認・再帰的修正管理専門
- **条件付きSubAgent**: spec-compliance結果に基づく不適合修正（内容により選択）

### 並列実行計画

#### Phase 1: TECH-006修正（単独実行）
**担当**: **csharp-web-ui**
**実装内容**:
- Login.razorのStateHasChanged()タイミング調整（256行→認証処理完了後）
- Blazor Server認証統合最適化
- HTTPレスポンス開始前のCookie認証処理実現

#### Phase 2: 初回品質監査（並列実行）
1. **spec-compliance**: 仕様準拠度測定
   - 入力: Step1分析結果（spec_compliance_audit.md）
   - 基準: 要件準拠95%以上
   - 出力: 不適合項目リスト・修正必要性判定

2. **integration-test**: 統合テスト実施
   - TECH-006修正確認テスト
   - 全認証フロー統合テスト（ログイン→パスワード変更→管理画面）
   - プロフィール変更機能統合テスト

#### Phase 3: 条件付き修正フェーズ
**spec-compliance結果に基づく分岐実行**:

##### 準拠度95%未満の場合（再帰的修正）
1. **不適合項目分類・SubAgent選択**:
   - Razorファイル不適合 → **csharp-web-ui**
   - F#ドメイン不適合 → **fsharp-domain**
   - TypeConverter不適合 → **contracts-bridge**
   - Repository不適合 → **csharp-infrastructure**

2. **修正実装** (専門SubAgent並列実行)
3. **spec-compliance再監査** (修正確認)
4. **準拠度95%達成まで2-3を繰り返し**

##### 準拠度95%以上の場合
→ Phase 4へ直接移行

#### Phase 4: 最終確認・完了処理
1. **code-review**: アーキテクチャ品質85/100以上達成確認
2. **統合確認**: 全SubAgent成果統合・Phase A7完了判定
3. **GitHub Issues解決**: #5/#6完全クローズ・成果承認

## 🎯 Step成功基準

### 機能要件
- **TECH-006完全解決**: ログイン認証フローエラー完全解消
- **全認証フロー正常動作**: 初回ログイン→パスワード変更→管理画面フロー完全動作
- **認証応答時間**: <500ms（パフォーマンス基準達成）

### 品質要件
- **仕様準拠度95%以上**: 要件定義・設計書との完全整合達成
- **アーキテクチャ品質85/100以上**: Clean Architecture完全準拠
- **ビルド品質**: 0 Warning, 0 Error継続維持
- **テスト品質**: 全テスト成功・統合テスト追加実装成功

### Phase A7完了要件
- **GitHub Issues完全解決**: #5[COMPLIANCE-001]・#6[ARCH-001]クローズ可能状態
- **要件逸脱完全解消**: Step1発見の全5項目解消確認
- **Phase B1移行基盤**: プロジェクト管理機能実装準備完了

## 📊 Step1分析結果活用計画

### 活用する分析結果
1. **spec_compliance_audit.md**: 仕様準拠マトリックス・要件逸脱5項目の解決確認基準
2. **architecture_review.md**: アーキテクチャ品質78/100→85/100達成確認
3. **dependency_analysis.md**: 技術的依存関係・統合テスト実施順序
4. **spec_deviation_analysis.md**: MVC/Blazor混在解消・Pure Blazor Server実現確認

### 実装方針・技術選択の根拠
- **TECH-006修正**: Blazor Server・ASP.NET Core Identity統合ベストプラクティス適用
- **統合テスト**: WebApplicationFactory基盤活用・既存テストパターン継承
- **品質監査**: Step1確立基準・Phase A7成功基準準拠

## 🛠️ 主要実装タスク（次回セッション実施）

### タスク1: TECH-006ログイン認証フローエラー修正
**担当**: **csharp-web-ui**（Blazor Server・Razor専門）
**対象**: `src/UbiquitousLanguageManager.Web/Components/Pages/Auth/Login.razor`
**修正内容**:
- 256行目StateHasChanged()タイミング調整（認証処理完了後に移動）
- 266行目認証処理との順序最適化
- Blazor Server認証ベストプラクティス適用
- HTTPレスポンス開始前のCookie認証処理実現

### タスク2: 統合テスト追加実装
**担当**: integration-test
**追加テスト**:
- 全認証フロー統合テスト（ログイン→パスワード変更→管理画面）
- プロフィール変更機能統合テスト
- 初回ログイン機能統合テスト

### タスク3: 最終仕様準拠監査
**担当**: spec-compliance
**監査範囲**:
- 要件準拠度95%達成確認
- UI設計書8画面完全準拠確認
- 全要件逸脱解消確認

### タスク4: アーキテクチャ品質最終評価
**担当**: code-review
**評価範囲**:
- Clean Architecture完全準拠（85/100以上）
- Pure Blazor Serverアーキテクチャ実現
- Phase B1移行基盤品質評価

## 🔴 SubAgent指示強化策（仕様準拠必須）

### 1. 仕様準拠必須情報の提供
**全SubAgent実行時に以下を必須提供**:
```yaml
必須情報:
- 要件定義書該当条項: [具体的条項番号・要求内容]
- UI設計書該当画面: [画面No・項目名・期待実装]
- ADR準拠要件: [ADR_003用語統一等・関連ADR番号]
- 過去の逸脱防止: [Step5での教訓・回避すべき独自判断]
- 期待される実装: [仕様書記載の具体的実装内容]
```

### 2. 仕様逸脱防止警告（強制）
**全SubAgent実行時の必須警告**:
```yaml
警告: 以下の仕様逸脱を絶対に回避すること
- "用語"使用禁止（"ユビキタス言語"使用厳守）
- 独自判断による機能追加・変更禁止
- 設計書記載外のUI要素・項目追加禁止  
- URLパス・ルーティング独自変更禁止
- 仕様書未記載の"便利機能"追加禁止
```

### 3. 不適合修正時の追加指示
**spec-compliance不適合発見時の修正指示**:
```yaml
修正指示:
- 不適合内容: [spec-compliance検出の具体的内容]
- 修正箇所: [具体的ファイルパス・行番号・対象要素]
- 仕様書期待値: [要件定義・設計書記載の正しい実装]
- 検証方法: [修正確認の具体的方法]
- 影響範囲確認: [他機能への影響・回帰テスト範囲]
- 承認条件: [MainAgentによる修正承認基準]
```

## 📚 実装時参照資料

### 技術詳細
- `/Doc/10_Debt/Technical/TECH-006_ログイン認証フローエラー.md`
- Serena MCP memory `phase_a7_technical_details`

### 分析結果・仕様準拠基準
- `/Doc/05_Research/Phase_A7/spec_compliance_audit.md` - **必須参照**
- `/Doc/05_Research/Phase_A7/architecture_review.md`
- `/Doc/05_Research/Phase_A7/dependency_analysis.md`

### 実装対象
- `src/UbiquitousLanguageManager.Web/Components/Pages/Auth/Login.razor`
- `src/UbiquitousLanguageManager.Web/Services/AuthenticationService.cs`
- `src/UbiquitousLanguageManager.Tests/Integration/`

## 📊 Step実行記録（随時更新）

### 組織設計完了（2025-08-26）
- ✅ Step特性判定完了：品質保証・統合テスト・技術負債解消
- ✅ SubAgent選択完了：csharp-web-ui・integration-test・spec-compliance・条件付きSubAgent
- ✅ 並列実行計画策定完了：4Phase構成・再帰的修正プロセス・仕様準拠強化体制
- ✅ Step成功基準設定完了：仕様準拠95%・アーキテクチャ85/100・GitHub Issues解決

### 修正版組織設計（2025-08-26）
- ✅ **TECH-006修正担当変更**: integration-test → **csharp-web-ui**（Razor専門）
- ✅ **再帰的修正プロセス追加**: spec-compliance結果に基づく条件分岐・不適合修正サイクル
- ✅ **仕様準拠強化策追加**: 必須情報提供・逸脱防止警告・修正時詳細指示

### 次回セッション実行予定（2025-08-26）
**Phase別実行計画**:
- [ ] **Phase 1**: TECH-006修正実装（csharp-web-ui）
- [ ] **Phase 2**: 初回品質監査（spec-compliance + integration-test並列）
- [ ] **Phase 3**: 条件付き修正フェーズ（spec-compliance結果により分岐）
- [ ] **Phase 4**: 最終確認・完了処理（code-review + GitHub Issues解決）

**推定所要時間**:
- 基本ケース（準拠度95%達成済み）: 60分
- 修正必要ケース（1回修正）: 90分  
- 最大ケース（2回修正）: 120分

## ✅ Step終了時レビュー
[Step完了時に更新]

---

**Step責任者**: MainAgent  
**組織設計日**: 2025-08-26  
**実装開始予定**: 次回セッション（2025-08-26）