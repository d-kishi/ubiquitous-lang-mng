# タスク完了時のチェックリスト

## 必須チェック項目（すべてのタスク完了時）

### 1. ビルド品質
- [ ] `dotnet build` で 0 Warning, 0 Error を確認
- [ ] 全プロジェクトがビルド成功することを確認

### 2. テスト品質  
- [ ] `dotnet test` で全テスト成功を確認
- [ ] 新規実装がある場合は対応するテストを作成
- [ ] TDD Red-Green-Refactor サイクル実践

### 3. 動作確認
- [ ] アプリケーション起動確認 (`dotnet run --project src/UbiquitousLanguageManager.Web`)
- [ ] 主要機能の動作確認
- [ ] 実装した機能のE2E確認

### 4. コード品質
- [ ] 初学者向けコメントの追加（F#・Blazor Server）
- [ ] Clean Architecture原則の遵守確認
- [ ] 用語統一チェック（「ユビキタス言語」使用）

### 5. 文書化（必要な場合）
- [ ] 重要な技術決定はADRとして記録
- [ ] 実装変更があった場合は設計書更新

## Phase完了時の追加チェック
- [ ] 統合テスト実行・成功確認
- [ ] セキュリティ機能動作確認
- [ ] パフォーマンス確認
- [ ] ユーザビリティ確認

## 緊急時・トラブル対応
- データベース再起動: `docker-compose restart postgres`
- 全コンテナ再起動: `docker-compose down && docker-compose up -d`
- キャッシュクリア: `dotnet clean && dotnet build`