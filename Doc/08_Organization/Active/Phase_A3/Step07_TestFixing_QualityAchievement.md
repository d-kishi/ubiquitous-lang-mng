# Step7: テスト修正・品質基準達成 組織設計

**対応対象**: Phase A3 Step6完了基準達成のための125件テスト失敗修正  
**作成日**: 2025-07-29  
**対応方針**: ADR_013準拠による組織的対応  
**緊急度**: 🚨 Critical（Step6完了・Phase A3継続の前提条件）  

## 🎯 Step7目的・背景

### **🚨 緊急発見：根本問題特定**
- **マイグレーション完全不整合**: `20250729111951_CorrectInitialMigration.cs`がinitスキーマと全く一致しない
- **真の根本原因判明**: ApplicationUser.csは正確だが、マイグレーションが古い定義から生成
- **影響範囲**: 125件テスト失敗の本質的原因・データベーススキーマとエンティティの不一致

### **発覚した問題状況**
- **Step6完了基準未達成**: テスト成功率69.1%（目標80%未達）
- **125件テスト失敗**: マイグレーション不整合による根本的な問題
- **品質基準不足**: データベーススキーマ層での不整合によるテスト破綻

### **Step7必要性**
**Step6は実装修正完了、Step7でテスト修正・品質達成により真の完了を実現**

#### **Step6 vs Step7の明確な役割分離**
- **Step6**: データベース・エンティティ・機能実装の修正 ✅ 完了
- **Step7**: テストコード修正・品質基準達成による完了基準実現 🔄 実行中

### **Step7と既存組織設計の関係**

#### **既存参照情報**
- **Step6計画**: `/Doc/08_Organization/Active/Phase_A3/Step06_DatabaseDesignImplementationAlignment.md`
- **Step6分析**: `/Doc/05_Research/Phase_A3/Phase6/Step6_IntegratedAnalysisResults.md`
- **データベース調査**: `/Doc/05_Research/Phase_A3/Phase6/DatabaseDesignImplementationAlignment_Investigation.md`
- **実行計画**: `/Doc/05_Research/Phase_A3/Phase6/ActionPlan_DatabaseImplementationAlignment.md`

#### **Step6成果の活用**
Step6で実施された6専門役割分析結果を基盤とし、**テスト修正実行に特化した組織構成**に調整

## 📋 Step7組織設計（6専門役割並列実行）

### **複雑Phase適用根拠**
- **技術領域横断**: ✅ 全テスト層（Domain/Application/Infrastructure/Web）
- **依存関係多数**: ✅ ApplicationUser変更が全層テストに影響
- **品質影響大**: ✅ 125件失敗による品質基準未達・Step6完了阻害

### 1. マイグレーション緊急修正専門家
**🎯 責務**: マイグレーション完全不整合の緊急修正・根本問題解決
```
【緊急実行内容】
- 現在マイグレーション完全削除・真の正確なマイグレーション再作成
- マイグレーションファイルを標準配置（Infrastructure/Data/Migrations）へ移行
- initスキーマ vs 新マイグレーションの100%一致確認
- データベース再作成・ApplicationUser.csとの完全整合実現
- マイグレーション品質検証・不要列完全排除

【🚨 Critical Issues確認済み】
- 配置問題：Infrastructure/Migrations（非標準）→ Infrastructure/Data/Migrations（標準）
- 余計な列：UserRole, DomainUserId, IsActive, CreatedAt, CreatedBy, Role
- 型不整合：UpdatedBy（VARCHAR(450) vs text）
- Phase A3列：PasswordResetToken/PasswordResetExpiry（正常確認済み）

【実行コマンド】
rm -rf Infrastructure/Migrations/* → 
cd Infrastructure → 
dotnet ef migrations add TrueCorrectInitialMigration -o Data/Migrations → 
検証・適用
```

### 2. ApplicationUser・データベース整合確認専門家  
**🎯 責務**: マイグレーション修正後のApplicationUser・DB完全整合確認
```
【実行内容】
- ApplicationUser.cs vs 新マイグレーションの100%一致確認
- initスキーマ・設計書・エンティティ・マイグレーションの4層整合検証
- 不要プロパティ完全排除確認（UserRole, IsActive, CreatedAt等）
- Phase A3プロパティ動作確認（PasswordResetToken/PasswordResetExpiry）

【🚨 確認済み：ApplicationUser.csは正確】
- 設計書100%準拠・Phase A3プロパティ実装済み
- 問題はマイグレーション側の不整合のみ

【検証項目】
- 新マイグレーション生成後の列定義確認
- テスト実行での整合性確認
```

### 3. TDD実行・品質保証専門家
**🎯 責務**: Red-Green-Refactorサイクルによる体系的テスト修正実行
```
【実行内容】
- Red Phase：125件失敗テストの詳細分析・分類
- Green Phase：最小修正による段階的成功実現
- Refactor Phase：テストコード品質向上・保守性確保
- テストカバレッジ80%達成確認

【参照情報】
- Step6 TDD戦略：Red-Green-Refactorサイクル適用方針
- 品質基準：テストカバレッジ80%以上・成功率80%以上
```

### 4. Clean Architecture・層間テスト専門家
**🎯 責務**: 各層テスト修正のアーキテクチャ整合性確保・境界確認
```
【実行内容】
- Domain層テスト：F#純粋関数テストの影響確認・修正
- Application層テスト：F#↔C#境界テストの修正実行
- Infrastructure層テスト：ApplicationUser直接参照修正
- Web層テスト：Blazor認証状態プロバイダー修正

【参照情報】
- Step6分析：Clean Architecture層構成95%完了評価
- F#↔C#境界：Contracts層実装573行の包括的実装
- 層間依存：依存関係逆転原則100%準拠確認
```

### 5. Phase A3機能・統合テスト専門家
**🎯 責務**: Phase A3機能テストの完全動作確認・統合テスト修正
```
【実行内容】
- パスワードリセット統合テスト修正・完全動作確認
- Remember Me機能テスト最終確認
- メール送信機能テスト統合確認
- Phase A3機能100%動作保証実現

【参照情報】
- Step6成果：Phase A3機能85-95%完成評価
- 統合テスト課題：弱いパスワード検証・同時リセット申請問題
- smtp4dev環境：統合テスト基盤準備完了
```

### 6. 品質基準達成・Step完了判定専門家
**🎯 責務**: Step7完了基準達成の客観的判定・Step8準備
```
【実行内容】
- テスト成功率80%達成確認・品質基準クリア判定
- Step6完了基準最終確認（125件→目標50件以下）
- Step7完了基準達成判定・Step8移行可能性確認
- Phase A3全体品質状況確認・継続可能性判定

【参照情報】
- Step6完了基準：データベース設計書100%一致・ApplicationUser100%準拠
- 品質基準：テストカバレッジ80%以上・Clean Architecture100%準拠
- Step8準備：統合テスト・品質保証実行準備
```

## 🔄 Step7実行計画（ADR_013準拠プロセス）

### **Session 1: 緊急マイグレーション修正・専門役割分析（180分）**

#### **🚨 Phase 1-1: 緊急マイグレーション修正（60分）**
```
1. マイグレーション緊急修正専門家：最優先実行
   - 現マイグレーション完全削除（5分）
   - マイグレーション標準配置設定（5分）
   - 新マイグレーション生成・検証（30分）
   - データベース再作成・適用確認（20分）

2. ApplicationUser・データベース整合確認専門家：並行検証
   - マイグレーション配置確認（Infrastructure/Data/Migrations）
   - initスキーマ vs 新マイグレーション一致確認
   - ApplicationUser.cs vs DB整合性確認
```

#### **Phase 1-2: 並列分析実行（30分×4専門役割）**
```
3. TDD実行・品質保証専門家：修正後テスト分析
4. Clean Architecture専門家：層間整合性確認
5. Phase A3機能専門家：統合テスト動作確認
6. 品質基準専門家：完了判定基準確定
```

#### **Phase 1-2: Gemini連携技術調査（30分）**
各専門役割結果のGemini確認・最新情報収集・ベストプラクティス調査

#### **Phase 1-3: 統合解決策策定（60分）**
- 6専門役割分析結果統合・相互依存関係整理
- 修正優先順位決定・実行順序確定
- リスク評価・対策検討

#### **Phase 1-4: 詳細実行計画確定（60分）**
- Session 2-3の具体的作業内容確定
- 成功基準・測定指標明確化
- 完了判定プロセス確立

### **Session 2: Critical Priority修正実行（120分）**

#### **Infrastructure層テスト修正（60分）**
- `UserRepositoryIntegrationTests.cs`完全修正
- `AuthenticationServiceTests.cs`系修正
- `AuditLoggingTests.cs`修正
- 中間テスト実行・成功確認

#### **Application層テスト修正（60分）**
- `UserManagementUseCaseTests.cs`修正
- `UserApplicationServiceTests.cs`修正
- F#↔C#境界テスト修正
- 段階的成功率向上確認

### **Session 3: High Priority修正・最終品質保証（120分）**

#### **Contracts/Web層テスト修正（60分）**
- `TypeConvertersExtensionsTests.cs`修正
- Web層`AuthenticationServiceTests.cs`修正
- 統合テスト修正・Phase A3機能確認

#### **最終品質保証・完了確認（60分）**
- テスト成功率80%達成確認
- Step6完了基準最終確認
- Step7完了判定・Step8準備完了確認

## 📊 成功基準・完了判定

### **Step7技術完了基準**
1. ✅ **テスト成功率**: 80%以上達成（現状69.1% → 目標80%+）
2. ✅ **テスト失敗数**: 125件 → 50件以下達成
3. ✅ **Phase A3機能**: 統合テスト100%成功
4. ✅ **ビルド状態**: 0警告・0エラー維持

### **Step7品質完了基準**
1. ✅ **設計書準拠**: ApplicationUser 100%準拠維持
2. ✅ **Clean Architecture**: 原則100%準拠維持
3. ✅ **TDD実践**: Red-Green-Refactorサイクル完全適用
4. ✅ **テストカバレッジ**: 80%以上維持

### **Step6完了基準同時達成**
1. ✅ データベーススキーマ設計書100%一致
2. ✅ ApplicationUser設計書100%準拠
3. ✅ Phase A3機能100%実装・動作確認
4. ✅ 全テスト成功・品質基準達成

## 🎯 Step8準備・Phase A3完了への道筋

### **Step7完了による効果**
- **Step6真の完了実現**: 品質基準達成による完了基準クリア
- **Phase A3基盤完成**: 全機能・テスト・品質の統合達成
- **Step8実行準備**: 統合テスト・最終品質保証への完璧な準備

### **Step8実行内容（予定）**
- Phase A3全機能統合テスト・E2Eテスト
- 否定的仕様検証・パフォーマンス測定
- Phase A3完了宣言・Phase A4準備

## 📚 参照ドキュメント・依存関係

### **Step6関連（継承・活用）**
- `/Doc/08_Organization/Active/Phase_A3/Step06_DatabaseDesignImplementationAlignment.md` - 基本組織設計
- `/Doc/05_Research/Phase_A3/Phase6/Step6_IntegratedAnalysisResults.md` - 6専門役割分析結果
- `/Doc/05_Research/Phase_A3/Phase6/DatabaseDesignImplementationAlignment_Investigation.md` - 詳細調査結果
- `/Doc/05_Research/Phase_A3/Phase6/ActionPlan_DatabaseImplementationAlignment.md` - 実行計画

### **設計・仕様書（絶対的真実）**
- `/Doc/02_Design/データベース設計書.md` - データベース設計絶対基準
- `/Doc/01_Requirements/機能仕様書.md` - Phase A3機能要件
- `/init/01_create_schema.sql` - 正しいスキーマ定義

### **ADR・組織運用**
- `/Doc/07_Decisions/ADR_013_組織管理サイクル運用規則.md` - 組織運用規則
- `/Doc/08_Organization/Rules/組織管理運用マニュアル.md` - Step実行手順
- `/Doc/08_Organization/Rules/テスト戦略ガイド.md` - TDD詳細実践

## 🚀 実行開始準備

### **実行準備完了状況**
- ✅ Step6成果・課題完全把握完了
- ✅ 125件テスト失敗詳細分析準備完了
- ✅ 6専門役割組織設計完了
- ✅ ADR_013準拠実行プロセス確立完了
- ✅ 成功基準・完了判定基準明確化完了

### **次回アクション**
**Session 1開始**: 6専門役割並列分析・統合計画策定（180分）

---

**記録日時**: 2025-07-29  
**設計責任者**: Claude Code（ADR_013準拠組織設計）  
**承認状況**: 実行準備完了・Session 1開始待ち  
**Step7目標**: 125件テスト失敗修正による品質基準達成・Step6完了基準実現