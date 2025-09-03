# Phase A8 Step5 Stage2 実行計画詳細
## テストコード仕様準拠修正（60分）

**作成日**: 2025-09-03  
**前提**: Stage1調査分析完了・根本原因特定済み  
**目標**: 認証関連テスト失敗35件 → 3件以下（92%改善）

## 🎯 Stage2解決対象課題（Stage1特定済み）

### 🔴 課題1: ロックアウト機能テスト（重大仕様違反）
**問題の詳細**:
- **仕様書基準**: 機能仕様書2.1.1「ログイン失敗によるロックアウト機構は設けない」
- **現状問題**: IdentityLockoutTests.csで大量のロックアウト機能テスト実装
- **影響範囲**: 10件テスト失敗・29%影響・仕様書直接違反

**解決内容**:
- IdentityLockoutTests.cs完全削除
- プロジェクト参照・名前空間削除
- 仕様準拠確認テスト追加

### 🟡 課題2: Phase A3スタブテスト残存
**問題の詳細**:
- **根本原因**: Phase A3→A8実装変更に対するテスト未同期
- **現状問題**: 「Phase A3スタブ実装」前提・「エラー期待テスト」の残存
- **影響範囲**: 15件テスト失敗・43%影響・認証基本機能全般

**解決内容**:
```csharp
// 修正パターン例
// 修正前: Assert.False(result.IsSuccess); // Phase A3エラー期待
// 修正後: Assert.True(result.IsSuccess);  // 実装成功期待
// コメント削除: "Phase A3スタブ実装のため〜"
```

### 🟢 課題3: 初期パスワード不整合
**問題の詳細**:
- **根本原因**: "TempPass123!"と"su"の混在・仕様書準拠不完全
- **現状問題**: 複数テストファイルでの初期パスワード不統一
- **影響範囲**: 7件テスト失敗・20%影響・初期認証テスト

**解決内容**:
```csharp
// 全テストファイル統一修正
// "TempPass123!" → "su"（機能仕様書2.0.1準拠）
```

## 🤖 SubAgent実行計画（並行実行最適化）

### 📋 依存関係分析・実行順序最適化

**依存関係マップ**:
```
課題1（ロックアウト削除） ─┐
                        ├─ 独立実行可能（並行）
課題2（Phase A3修正）   ─┤
                        │
課題3（パスワード統一）  ─┘
```

**並行実行グループ設計**:
- **グループA**: 課題1・課題2（依存関係なし・並行実行）
- **グループB**: 課題3（他課題完了後・単独実行）
- **グループC**: 仕様準拠確認（全修正完了後・確認実行）

### 🚀 並行実行グループ1（30分・同時実行）

#### SubAgent A: spec-compliance
**担当課題**: 課題1（ロックアウト機能テスト削除）
**実行内容**:
1. **IdentityLockoutTests.cs完全削除**
   - ファイル削除: `tests/UbiquitousLanguageManager.Tests/Integration/IdentityLockoutTests.cs`
   - プロジェクト参照削除・名前空間整理
   
2. **仕様違反確認・削除**
   - ロックアウト関連テスト全件検索・削除
   - 機能仕様書2.1.1準拠確認

3. **仕様準拠テスト追加**
   ```csharp
   [Fact]
   public async Task MultipleLoginFailures_ShouldNotLockAccount()
   {
       // 10回失敗後も正しいパスワードでログイン可能
       // 仕様書2.1.1「ロックアウト機構は設けない」準拠確認
   }
   ```

**期待効果**: 10件失敗解決（29%改善）  
**作業時間**: 15分  
**成功基準**: ロックアウト関連テスト0件・仕様準拠テスト追加完了

#### SubAgent B: unit-test
**担当課題**: 課題2（Phase A3スタブテスト修正）
**実行内容**:
1. **Phase A3コメント完全削除**
   - 対象ファイル: AuthenticationServiceAutoLoginTests.cs他
   - コメント削除: "Phase A3スタブ実装のため〜"
   - 実装前提への修正

2. **エラー期待→成功期待修正**
   ```csharp
   // AuthenticationServiceTests.cs修正例
   [Fact]
   public async Task Login_ShouldReturnSuccess_WhenValidCredentials()
   {
       // 修正前: Assert.False(result.IsSuccess); // エラー期待
       // 修正後: Assert.True(result.IsSuccess);  // 成功期待
       var result = await _service.LoginAsync(validRequest);
       Assert.True(result.IsSuccess);
       Assert.NotNull(result.Value);
   }
   ```

3. **TDD原則準拠確認**
   - Red-Green-Refactorサイクル準拠テスト修正
   - テストファースト原則復活確認

**期待効果**: 15件失敗解決（43%改善）  
**作業時間**: 30分  
**成功基準**: Phase A3コメント0件・成功期待テスト100%

### 🔧 並行実行グループ2（15分・単独実行）

#### SubAgent C: integration-test
**担当課題**: 課題3（初期パスワード統一）
**実行内容**:
1. **全テストファイル初期パスワード統一**
   - 検索・置換: "TempPass123!" → "su"
   - 対象: 統合テスト・機能テスト全般
   
2. **TestWebApplicationFactory設定確認**
   - 初期データ設定・初期パスワード"su"確認
   - テスト環境・本番環境設定統一

3. **統合テスト動作確認**
   ```csharp
   [Fact]
   public async Task InitialLogin_WithSuPassword_ShouldSucceed()
   {
       // admin@ubiquitous-lang.com / "su" での確実ログイン
       var loginRequest = new LoginRequest 
       { 
           Email = "admin@ubiquitous-lang.com", 
           Password = "su" 
       };
       var result = await _authService.LoginAsync(loginRequest);
       Assert.True(result.IsSuccess);
   }
   ```

**期待効果**: 7件失敗解決（20%改善）  
**作業時間**: 15分  
**成功基準**: 初期パスワード"su"統一・統合テスト成功

### ✅ 最終確認グループ3（0分・確認のみ）

#### SubAgent D: spec-compliance（再実行）
**担当**: 仕様準拠最終確認
**実行内容**:
1. **機能仕様書2.0-2.1完全準拠確認**
   - ロックアウト機能：完全削除確認
   - 初期パスワード："su"統一確認
   - Phase A3前提：完全除去確認

2. **テスト実行・結果確認**
   - `dotnet test`全実行
   - 失敗件数確認・目標達成確認

## 📊 Stage2合格判定基準

### 🎯 定量基準

| 測定項目 | 現状 | 目標 | 測定方法 | 合格基準 |
|---------|------|------|----------|----------|
| **テスト失敗件数** | 35件 | 3件以下 | `dotnet test`実行 | 92%改善達成 |
| **ロックアウトテスト** | 10件存在 | 0件 | grep検索・ファイル存在確認 | 完全削除確認 |
| **Phase A3コメント** | 複数存在 | 0件 | grep検索・コメント確認 | 完全除去確認 |
| **初期パスワード統一** | 混在状態 | "su"統一 | grep検索・文字列確認 | 100%統一確認 |

### 🎯 定性基準

#### 仕様準拠度確認
- **機能仕様書2.0-2.1完全準拠**: ロックアウト禁止・初期パスワード"su"
- **TDD原則復活**: Red-Green-Refactorサイクル準拠テスト
- **テストファースト原則**: 仕様ベーステスト設計実践

#### 品質基準確認  
- **コード品質**: 0警告0エラー状態維持
- **テストカバレッジ**: 90%以上維持（カバレッジ低下なし）
- **実行時間**: テスト実行時間の大幅改善

### 📏 測定方法詳細

#### 1. テスト実行・結果測定（.logディレクトリ活用）
```bash
# .logディレクトリ作成（git管理対象外）
mkdir -p .log/stage2

# .gitignoreに追加（未設定の場合）
echo ".log/" >> .gitignore

# Stage2開始前測定
dotnet test --logger "console;verbosity=detailed" > .log/stage2/before.log

# SubAgent実行後測定（各グループ完了時）
dotnet test --logger "console;verbosity=detailed" > .log/stage2/group1_after.log
dotnet test --logger "console;verbosity=detailed" > .log/stage2/group2_after.log

# Stage2完了後最終測定
dotnet test --logger "console;verbosity=detailed" > .log/stage2/final.log
```

#### 2. 仕様準拠度測定
```bash
# ロックアウトテスト削除確認
find . -name "IdentityLockoutTests.cs" | wc -l  # 0であること
grep -r "lockout" tests/ | wc -l              # 最小限であること

# Phase A3コメント削除確認
grep -r "Phase A3" tests/ | wc -l             # 0であること

# 初期パスワード統一確認
grep -r "TempPass123" tests/ | wc -l          # 0であること
grep -r '"su"' tests/ | wc -l                # 統一確認
```

#### 3. 品質基準測定
```bash
# ビルド・警告確認
dotnet build --verbosity normal > build.log
grep -i "warning\|error" build.log | wc -l   # 0であること

# カバレッジ測定
dotnet test --collect:"XPlat Code Coverage"
```

## ⚠️ Stage2実行時注意事項

### 🚨 必須確認事項（各SubAgent実行前）

#### Stage2開始前チェックリスト
□ **PostgreSQL起動確認**: docker-compose up -d 実行済み  
□ **.logディレクトリ作成**: mkdir -p .log/stage2 実行済み  
□ **git管理対象外設定**: .gitignoreに.log/追加済み  
□ **現状テスト失敗確認**: dotnet test実行・35件失敗確認  
□ **開始前ログ保存**: .log/stage2/before.log保存完了

#### 各SubAgent実行前確認事項
1. **機能仕様書2.0-2.1参照**: 修正時の仕様準拠基準確認
2. **TDD原則確認**: Red-Green-Refactorサイクル理解
3. **影響範囲確認**: 修正による副作用・回帰バグ防止

### 🔄 段階的確認プロセス
```
並行実行グループ1完了 → テスト実行 → 結果確認 → 次グループ実行
並行実行グループ2完了 → テスト実行 → 結果確認 → 最終確認実行
最終確認完了 → 全体テスト実行 → Stage2完了判定
```

### 📋 失敗時対策
- **想定外テスト失敗**: 1件ずつ原因分析・個別対応
- **仕様準拠不足**: spec-compliance Agent追加実行
- **回帰バグ発生**: unit-test Agent部分再実行

## 🎯 Stage2成功効果予測

### 即効性（Stage2完了時）
- **テスト成功率**: 67% → 97%（30ポイント改善）
- **開発効率**: テスト実行時間50%短縮
- **仕様準拠**: 機能仕様書2.0-2.1準拠度100%達成

### 持続性（Phase B1以降）
- **品質基盤**: テストファースト原則復活・仕様準拠文化確立
- **保守性**: Phase A3残骸完全除去・技術負債解消
- **拡張性**: 認証基盤完全安定・新機能追加準備完了

## 📚 Stage2完了後・Stage3準備事項

### Stage3引き継ぎ情報
- **残存課題**: 3件以下のテスト失敗（予想：統合テスト設定・実装確認系）
- **確認必要項目**: InitialDataService動作・認証フロー統合動作
- **最終目標**: テスト100%成功・実ログイン動作確認

### Phase B1移行準備
- **認証基盤安定**: Stage2-3完了によるテスト基盤確立
- **品質基準**: 0警告0エラー・仕様準拠100%達成
- **開発体制**: テストファースト原則・仕様準拠監査体制確立

---

**作成日**: 2025-09-03  
**基準文書**: Stage1調査分析結果・機能仕様書2.0-2.1  
**実行前提**: 4Agent並列調査完了・根本原因特定済み  
**次段階**: Stage3実装修正・Phase B1移行準備