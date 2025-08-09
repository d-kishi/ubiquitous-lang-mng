# VSCode拡張機能インストール

## 自動インストール方法

PowerShellで以下のコマンドを実行して、必要な拡張機能を一括インストール：

```powershell
# C#/.NET開発関連
code --install-extension ms-dotnettools.csdevkit
code --install-extension ms-dotnettools.csharp
code --install-extension ms-dotnettools.vscode-dotnet-runtime

# F#開発関連
code --install-extension ionide.ionide-fsharp

# Git関連
code --install-extension eamodio.gitlens

# Markdown関連
code --install-extension yzhang.markdown-all-in-one
code --install-extension shd101wyy.markdown-preview-enhanced

# Claude Code関連
code --install-extension anthropic.claude-code

# その他開発ツール
code --install-extension ms-vscode.live-server
code --install-extension prisma.prisma

# 日本語パック（必要に応じて）
code --install-extension ms-ceintl.vscode-language-pack-ja

# GitHub Copilot（ライセンスがある場合）
# code --install-extension github.copilot
# code --install-extension github.copilot-chat

# Flutter/Dart（このプロジェクトでは不要だが、環境に含まれていた場合）
# code --install-extension dart-code.dart-code
# code --install-extension dart-code.flutter
# code --install-extension alexisvt.flutter-snippets

# Go言語（このプロジェクトでは不要だが、環境に含まれていた場合）
# code --install-extension golang.go
```

## 拡張機能の説明

### 必須拡張機能

| 拡張機能 | 説明 |
|---------|------|
| **C# Dev Kit** | C#開発のための包括的な開発キット |
| **C#** | C#言語サポート、IntelliSense、デバッグ機能 |
| **.NET Runtime** | .NETランタイムのインストールと管理 |
| **Ionide for F#** | F#言語サポート、IntelliSense、フォーマット |
| **GitLens** | Git履歴の可視化、Blame表示、差分比較 |
| **Markdown All in One** | Markdownショートカット、目次生成、プレビュー |
| **Markdown Preview Enhanced** | 高機能Markdownプレビュー、Mermaid対応 |
| **Claude Code** | Claude AIとの連携、コード生成支援 |

### オプション拡張機能

| 拡張機能 | 説明 |
|---------|------|
| **Live Server** | HTMLファイルのライブプレビュー |
| **Prisma** | Prisma ORMサポート（将来的な利用のため） |
| **Japanese Language Pack** | VSCodeの日本語化 |
| **GitHub Copilot** | AI補完機能（有料ライセンス必要） |

## インストール確認

VSCodeを再起動後、以下のコマンドで確認：

```powershell
code --list-extensions
```

インストールした拡張機能が一覧に表示されれば成功です。

## 拡張機能の設定

### C#拡張機能の設定

1. VSCodeで `Ctrl + ,` を押して設定を開く
2. 検索ボックスに「omnisharp」と入力
3. 以下を設定：
   - Omnisharp: Enable Roslyn Analyzers → チェック
   - Omnisharp: Enable Editor Config Support → チェック

### F#拡張機能の設定

1. 設定で「FSharp」を検索
2. 以下を設定：
   - FSharp: Suggest Git Ignore → チェック
   - FSharp: Format On Save → チェック

### GitLensの設定

1. 設定で「GitLens」を検索
2. 以下を設定：
   - Gitlens: Current Line: Enabled → チェック
   - Gitlens: Code Lens: Enabled → チェック

---
作成日: 2025-08-09