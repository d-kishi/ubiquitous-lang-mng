# Claude Agent SDK 技術調査レポート

**作成日**: 2025-10-08
**調査目的**: Claude Agent SDKの概要把握・Claude Codeとの違い・本プロジェクトへの適用可能性評価

---

## 📋 調査サマリー

### Claude Agent SDKとは
- **提供元**: Anthropic公式（MIT License）
- **発表日**: 2025年9月29日（元「Claude Code SDK」から改名）
- **種別**: AIエージェント構築用フレームワーク（Python/TypeScript版）
- **基盤技術**: Claude Codeと同じ「Agent Harness」を使用

### 主要な特徴
- ✅ **自動コンテキスト管理**: Automatic Compactionにより長時間稼働可能
- ✅ **リッチなツールエコシステム**: ファイル操作・Web検索・MCP拡張
- ✅ **In-process SDK MCP Server**: Python関数を簡単にツール化
- ✅ **SubAgents・Hooks**: 並列実行・カスタムロジック挿入

---

## 1. Claude Agent SDKとは何か

### 1.1 概要

Claude Agent SDKは、Anthropicが提供する**AIエージェント構築用の高レベルフレームワーク**です。開発者がプログラムからClaude AIを制御し、複雑なタスクを自動化するエージェントを構築できます。

**キーポイント**:
- Claude Codeと同じ「Agent Harness」技術を基盤とする
- 2025年9月29日に発表（元「Claude Code SDK」から改名）
- Python版・TypeScript版を提供（MIT License）
- 本番環境対応済み（Production-ready）

### 1.2 提供形式

**Python SDK**:
- PyPI: `claude-agent-sdk`
- GitHub: https://github.com/anthropics/claude-agent-sdk-python
- Claude Codeランタイムが必須

**TypeScript SDK**:
- npm: `@anthropic/agent-sdk`
- GitHub: https://github.com/anthropics/claude-agent-sdk-typescript
- Node.js環境で動作

---

## 2. 主な機能・特徴

### 2.1 コア機能

#### (1) 自動コンテキスト管理
- **Automatic Compaction**: 長時間セッションでもコンテキストウィンドウを自動最適化
- 過去のやり取りを要約し、重要な情報のみ保持
- 数時間〜数日間の連続稼働が可能

#### (2) リッチなツールエコシステム
**組み込みツール**:
- ファイル操作（Read/Write/Edit/Glob）
- コード実行（Bash/Python）
- Web検索・WebFetch
- MCP（Model Context Protocol）拡張

**カスタムツール作成**:
```python
from claude_agent_sdk import tool

@tool("calculate_tax", "消費税込み価格を計算", {"price": int})
async def calculate_tax(args):
    total = args["price"] * 1.10
    return {"content": [{"type": "text", "text": f"税込: {total:.0f}円"}]}
```

#### (3) In-process SDK MCP Server
- Python関数を`@tool`デコレータでツール化
- サブプロセス不要（従来のMCPサーバーはサブプロセス必須）
- 低レイテンシ・高パフォーマンス

#### (4) SubAgents（サブエージェント）
- 専門タスクごとにサブエージェントを並列実行
- 親エージェントがサブエージェント結果を統合
- 本プロジェクトのSubAgentプール方式と同じ思想

#### (5) Hooks（フック機能）
- エージェント実行の各段階にカスタムロジック挿入可能
- `on_tool_call`、`on_error`、`on_completion`等

### 2.2 サポート環境

**言語**:
- Python 3.9+
- TypeScript/Node.js 18+

**プラットフォーム**:
- Linux/macOS/Windows
- Docker/Kubernetes対応

**Claude APIキー**:
- Anthropic APIキーが必須
- プロジェクト単位でキー管理可能

---

## 3. Claude APIとの違い

| 項目 | Claude API | Claude Agent SDK |
|------|-----------|------------------|
| **用途** | シンプルなテキスト生成 | 複雑なエージェント構築 |
| **ステート管理** | ステートレス | ステートフル（セッション管理） |
| **コンテキスト管理** | 手動 | 自動（Automatic Compaction） |
| **ツール使用** | 手動実装必要 | 組み込み＋カスタム拡張 |
| **SubAgents** | 非対応 | 対応 |
| **難易度** | 低 | 中〜高（非同期プログラミング必要） |

**使い分け**:
- **Claude API**: チャットボット・要約・翻訳等の単発タスク
- **Claude Agent SDK**: コーディング支援・自動化・複雑なワークフロー

---

## 4. 基本的な使用方法

### 4.1 セットアップ（Python版）

#### (1) インストール
```bash
pip install claude-agent-sdk
```

#### (2) Claude Code CLIインストール（必須）
```bash
# macOS/Linux
curl -fsSL https://claude.com/install.sh | sh

# Windows（PowerShell）
irm https://claude.com/install.ps1 | iex
```

#### (3) APIキー設定
```bash
export ANTHROPIC_API_KEY="sk-ant-..."
```

### 4.2 簡単な実装例

#### 例1: 最もシンプルなクエリ
```python
import anyio
from claude_agent_sdk import query

async def main():
    async for message in query(prompt="2 + 2の答えは？"):
        print(message)

anyio.run(main)
```

**出力**:
```
2 + 2の答えは4です。
```

#### 例2: ファイル操作ツール使用
```python
import anyio
from claude_agent_sdk import query

async def main():
    async for message in query(
        prompt="カレントディレクトリのPythonファイルを一覧表示してください",
        tools=["Glob", "Read"]
    ):
        print(message)

anyio.run(main)
```

#### 例3: カスタムツール作成
```python
from claude_agent_sdk import tool, create_sdk_mcp_server, query
import anyio

@tool("calculate_tax", "消費税込み価格を計算", {"price": int})
async def calculate_tax(args):
    total = args["price"] * 1.10
    return {"content": [{"type": "text", "text": f"税込: {total:.0f}円"}]}

server = create_sdk_mcp_server("tax-tools", "1.0.0", [calculate_tax])

async def main():
    async for message in query(
        prompt="1000円の税込価格を計算してください",
        mcp_servers=[server]
    ):
        print(message)

anyio.run(main)
```

**出力**:
```
税込: 1100円
```

### 4.3 セッション管理

```python
from claude_agent_sdk import Session

async def main():
    async with Session() as session:
        # 1つ目のクエリ
        async for message in session.query("Pythonとは何ですか？"):
            print(message)

        # 2つ目のクエリ（前のコンテキスト継続）
        async for message in session.query("その主な用途は？"):
            print(message)
```

---

## 5. 現在のClaude Codeとの関係

### 5.1 Claude Codeとは（復習）

**Claude Code**（本プロジェクトで現在使用中）:
- エンドユーザー向けCLI/IDE拡張
- 対話的にClaude AIを使用
- `.claude/commands/`でカスタムコマンド定義
- `CLAUDE.md`でプロジェクト指示定義

### 5.2 Claude Agent SDKとの違い

| 項目 | Claude Code | Claude Agent SDK |
|------|------------|------------------|
| **対象ユーザー** | エンドユーザー（開発者含む） | 開発者（プログラム制御） |
| **使用方法** | CLI/IDE拡張（対話的） | プログラムからAPI呼び出し |
| **カスタマイズ** | Commands・CLAUDE.md | Python/TypeScriptコード |
| **実行環境** | ターミナル・VSCode | Python/Node.jsプロセス |
| **ユースケース** | 開発作業支援 | 自動化・CI/CD・バッチ処理 |

### 5.3 統合可能性

**両者は同じAgent Harnessを共有**しており、以下の統合が可能:

1. **Claude Code内からSDKを呼び出し**
   - Commandsスクリプト内でPython SDKを実行
   - 例: `/custom-analyze`コマンドで自動コード分析

2. **SDK内でClaude Code機能を利用**
   - SDKがClaude Codeのツール（Read/Write/Edit等）を使用
   - Python SDKはClaude Codeランタイムが必須

3. **カスタムMCPサーバーの共有**
   - Claude Codeで定義したMCPサーバーをSDKから利用可能
   - `.claude/settings.json`の`mcpServers`設定を共有

### 5.4 本プロジェクトへの推奨

#### 現状維持（Claude Code継続使用）を推奨

**理由**:
- ✅ 本プロジェクトは**対話的な開発作業が中心**
- ✅ CLAUDE.md・SubAgent・Commands体系が**既に確立済み**
- ✅ F#・Blazor初学者には**Claude Codeのシンプルさが適切**
- ✅ 本番アプリに**エージェント機能を組み込む要件がない**

#### SDK導入を検討すべきシナリオ

以下の要件が発生した場合に検討:

1. **CI/CDパイプライン自動化**
   - 例: PR作成時の自動コードレビュー
   - 例: マージ前のADR準拠チェック自動化

2. **カスタム管理ツール開発**
   - 例: ユビキタス言語の用語整合性チェックツール
   - 例: ドメインモデル変更時の影響範囲分析ツール

3. **定期実行タスク**
   - 例: 週次技術負債レポート自動生成
   - 例: 月次進捗サマリー作成

4. **Webアプリへのエージェント統合**
   - 例: ユビキタス言語管理システムにAI支援機能追加
   - 例: ドメイン専門家向けの自然言語クエリ機能

---

## 6. 主なユースケース

### 6.1 コーディングエージェント

#### (1) SRE診断エージェント
```python
# ログ分析・障害診断・修正提案を自動化
async for message in query(
    prompt="過去1時間のエラーログを分析し、根本原因を特定してください",
    tools=["Bash", "Read", "Grep"]
):
    print(message)
```

#### (2) 自動コードレビュー
```python
# PR差分を解析し、Clean Architecture準拠確認
async for message in query(
    prompt="PR #123のコード変更をレビューし、ADR準拠を確認してください",
    tools=["Bash", "Read", "WebFetch"]
):
    print(message)
```

### 6.2 ビジネスエージェント

#### (1) 法務エージェント
- 契約書レビュー・リスク分析
- 法的要件チェック

#### (2) 財務分析エージェント
- 財務レポート解析
- 予算計画支援

#### (3) カスタマーサポートエージェント
- 問い合わせ自動応答
- FAQナレッジベース検索

### 6.3 リサーチエージェント

```python
# Web検索・要約・レポート作成を自動化
async for message in query(
    prompt=".NET 8の新機能を調査し、本プロジェクトへの適用可能性を評価してください",
    tools=["WebSearch", "WebFetch", "Write"]
):
    print(message)
```

### 6.4 パーソナルアシスタント

- 旅行計画・予約自動化
- スケジュール管理・リマインダー
- メール下書き作成

---

## 7. アーキテクチャ・設計思想

### 7.1 Agent Harness（エージェント基盤）

Claude Code・Claude Agent SDKの両方が使用する共通基盤:

1. **コンテキスト管理**
   - Automatic Compaction（自動要約）
   - 長期メモリー（セッション永続化）

2. **ツール実行エンジン**
   - ツール呼び出し・結果取得の自動化
   - エラーハンドリング・リトライ

3. **セッション管理**
   - ステートフル会話
   - 複数セッション並列実行

### 7.2 MCP（Model Context Protocol）統合

**MCPとは**:
- Anthropicが提唱するツール接続標準プロトコル
- 外部サービス（GitHub、Slack、データベース等）をツール化

**SDK特有機能: In-process SDK MCP Server**:
- 従来: MCPサーバーを別プロセスで起動（起動コスト・通信オーバーヘッド）
- SDK版: Python関数を直接ツール化（低レイテンシ）

```python
# 従来のMCP（サブプロセス起動）
mcp_servers = {
    "github": {
        "command": "uvx",
        "args": ["mcp-server-github"]
    }
}

# In-process SDK MCP（サブプロセス不要）
from claude_agent_sdk import tool, create_sdk_mcp_server

@tool("my_tool", "説明", {"arg": str})
async def my_tool(args):
    return {"content": [{"type": "text", "text": "結果"}]}

server = create_sdk_mcp_server("my-tools", "1.0.0", [my_tool])
```

### 7.3 SubAgents（サブエージェント）

**設計思想**:
- 複雑なタスクを専門サブエージェントに分割
- 並列実行で効率化
- 親エージェントが結果を統合

**本プロジェクトのSubAgentプール方式との類似性**:
- 両者とも「専門性による責務分離」を重視
- 並列実行による効率化
- SubAgent選択の柔軟性

---

## 8. リスク・考慮事項

### 8.1 技術的リスク

#### (1) Claude Codeへの依存
- **Python SDK**: Claude Codeランタイムが必須
- Claude Code未インストール環境では動作不可
- バージョン互換性管理が必要

#### (2) APIコスト
- ツール使用が多いと課金額増加
- **軽減策**: Prompt Caching活用（繰り返し使用するプロンプトをキャッシュ）

#### (3) セキュリティ
- `permission_mode='acceptEdits'`での意図しない操作リスク
- **対策**: 本番環境では明示的承認モード使用

### 8.2 学習曲線

#### (1) 非同期プログラミング
- Python: `async/await`・`anyio`の理解必要
- TypeScript: `async/await`・Promise の理解必要

#### (2) MCP概念
- MCPサーバー・ツール・プロトコルの理解
- カスタムツール作成には時間がかかる

### 8.3 運用上の考慮事項

#### (1) エラーハンドリング
```python
try:
    async for message in query(prompt="..."):
        print(message)
except Exception as e:
    # エラーログ・リトライ処理
    logger.error(f"Agent error: {e}")
```

#### (2) レート制限
- Anthropic API: 50リクエスト/分（Tier 1）
- 大量実行時は注意

#### (3) セッション管理
- 長時間セッションでもメモリリーク対策
- 定期的なセッションクリア

---

## 9. まとめ

### 9.1 Claude Agent SDKの価値

**強み**:
- ✅ Anthropic公式の高レベルフレームワーク
- ✅ 自動コンテキスト管理・リッチなツールエコシステム
- ✅ Claude Codeとの統合可能性
- ✅ 本番環境対応（Production-ready）

**適用シーン**:
- CI/CD自動化・カスタムツール開発
- 定期実行タスク・Webアプリへのエージェント統合

### 9.2 本プロジェクトへの推奨

**現状**: Claude Code継続使用を推奨

**将来検討**: 以下の要件が発生時にSDK導入を検討
- CI/CD自動化（自動コードレビュー等）
- カスタム管理ツール（用語整合性チェック等）
- 定期実行タスク（週次レポート生成等）

---

## 10. 参考リンク

### 10.1 公式ドキュメント

- **Claude Agent SDK Overview**: https://docs.claude.com/en/api/agent-sdk/overview
- **Python SDK GitHub**: https://github.com/anthropics/claude-agent-sdk-python
- **TypeScript SDK GitHub**: https://github.com/anthropics/claude-agent-sdk-typescript

### 10.2 チュートリアル

- **DataCamp Tutorial**: https://www.datacamp.com/tutorial/how-to-use-claude-agent-sdk
- **Anthropic Engineering Blog**: https://www.anthropic.com/engineering

### 10.3 関連技術

- **Model Context Protocol (MCP)**: https://docs.claude.com/en/docs/model-context-protocol
- **Claude API Documentation**: https://docs.anthropic.com/

---

**調査完了**: 2025-10-08
**次回更新**: SDKバージョンアップ時・本プロジェクトで適用検討時
