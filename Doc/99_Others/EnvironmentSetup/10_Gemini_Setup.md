# Gemini 連携セットアップ手順

## 1. 前提条件

- Google Cloud アカウントを持っていること
- Gemini API へのアクセス権があること
- Python 3.8以降がインストールされていること

## 2. Gemini API キーの取得

### Google AI Studio でAPIキーを取得

1. Google AI Studio にアクセス: https://makersuite.google.com/
2. Googleアカウントでログイン
3. 「Get API key」をクリック
4. 「Create API key」をクリック
5. APIキーをコピー

## 3. Gemini CLI ツールのインストール

```powershell
# Google Generative AI パッケージをインストール
pip install google-generativeai

# 追加の依存関係
pip install python-dotenv
```

## 4. 環境変数の設定

### システム環境変数に設定

```powershell
# Gemini APIキーを環境変数に設定
[System.Environment]::SetEnvironmentVariable("GEMINI_API_KEY", "your-gemini-api-key-here", "User")

# 設定確認
$env:GEMINI_API_KEY
```

### プロジェクトの .env ファイルに設定（オプション）

プロジェクトルートに `.env` ファイルを作成：

```env
GEMINI_API_KEY=your-gemini-api-key-here
```

## 5. Gemini 連携スクリプトの作成

### scripts/gemini-search.py

```python
#!/usr/bin/env python3
"""
Gemini API を使用した技術調査・情報検索スクリプト
"""

import os
import sys
import google.generativeai as genai
from dotenv import load_dotenv

# .envファイルから環境変数を読み込み
load_dotenv()

# Gemini API設定
genai.configure(api_key=os.getenv('GEMINI_API_KEY'))

def search_with_gemini(query, context="技術調査"):
    """
    Geminiを使用して情報を検索
    """
    try:
        # モデルの初期化
        model = genai.GenerativeModel('gemini-pro')
        
        # プロンプトの構築
        prompt = f"""
        コンテキスト: {context}
        質問: {query}
        
        以下の観点から回答してください：
        1. 最新の情報とベストプラクティス
        2. 実装例やコードサンプル（該当する場合）
        3. 注意点や落とし穴
        4. 参考リンクや追加リソース
        """
        
        # Geminiに問い合わせ
        response = model.generate_content(prompt)
        
        return response.text
        
    except Exception as e:
        return f"エラーが発生しました: {str(e)}"

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("使用方法: python gemini-search.py '検索クエリ'")
        sys.exit(1)
    
    query = " ".join(sys.argv[1:])
    result = search_with_gemini(query)
    print(result)
```

### scripts/gemini-code-review.py

```python
#!/usr/bin/env python3
"""
Gemini API を使用したコードレビュースクリプト
"""

import os
import sys
import google.generativeai as genai
from dotenv import load_dotenv
from pathlib import Path

load_dotenv()
genai.configure(api_key=os.getenv('GEMINI_API_KEY'))

def review_code_with_gemini(file_path):
    """
    指定されたファイルのコードをレビュー
    """
    try:
        # ファイル読み込み
        with open(file_path, 'r', encoding='utf-8') as f:
            code = f.read()
        
        # モデルの初期化
        model = genai.GenerativeModel('gemini-pro')
        
        # レビュープロンプト
        prompt = f"""
        以下のコードをレビューしてください：
        
        ファイル: {file_path}
        
        ```
        {code}
        ```
        
        レビューポイント：
        1. コードの品質と可読性
        2. パフォーマンスの問題
        3. セキュリティの懸念
        4. ベストプラクティスからの逸脱
        5. 改善提案
        
        Clean Architectureの原則に従っているかも確認してください。
        """
        
        response = model.generate_content(prompt)
        return response.text
        
    except Exception as e:
        return f"エラーが発生しました: {str(e)}"

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("使用方法: python gemini-code-review.py 'ファイルパス'")
        sys.exit(1)
    
    file_path = sys.argv[1]
    if not Path(file_path).exists():
        print(f"ファイルが見つかりません: {file_path}")
        sys.exit(1)
    
    result = review_code_with_gemini(file_path)
    print(result)
```

## 6. PowerShell エイリアスの設定

PowerShellプロファイルに便利なエイリアスを追加：

```powershell
# PowerShellプロファイルを開く
notepad $PROFILE

# 以下を追加
function gemini {
    param($query)
    python "$env:USERPROFILE\Develop\ubiquitous-lang-mng\scripts\gemini-search.py" $query
}

function gemini-review {
    param($file)
    python "$env:USERPROFILE\Develop\ubiquitous-lang-mng\scripts\gemini-code-review.py" $file
}

# プロファイルを再読み込み
. $PROFILE
```

## 7. Claude Code との統合

### .claude/settings.local.json に権限を追加

既存の設定に以下を追加：

```json
{
  "permissions": {
    "allow": [
      // 既存の権限...
      "Bash(gemini:*)",
      "Bash(python*gemini*.py:*)"
    ]
  }
}
```

## 8. 使用方法

### コマンドラインから直接使用

```powershell
# 技術調査
gemini "Blazor ServerとMVCの統合方法"

# コードレビュー
gemini-review "src/UbiquitousLanguageManager.Web/Program.cs"
```

### Claude Code セッション内から使用

```
「geminiコマンドで最新のC# 12の機能について調査してください」
「gemini-reviewでUserRepository.csをレビューしてください」
```

## 9. バッチ処理スクリプト

### scripts/gemini-batch-review.ps1

```powershell
# 複数ファイルを一括レビュー
param(
    [string]$pattern = "*.cs"
)

$files = Get-ChildItem -Recurse -Filter $pattern | Select-Object -First 5

foreach ($file in $files) {
    Write-Host "Reviewing: $($file.FullName)" -ForegroundColor Green
    python scripts/gemini-code-review.py $file.FullName
    Write-Host "---" -ForegroundColor Gray
}
```

## 10. トラブルシューティング

### APIキーエラーの場合

```powershell
# APIキーが正しく設定されているか確認
$env:GEMINI_API_KEY

# 再設定
[System.Environment]::SetEnvironmentVariable("GEMINI_API_KEY", "your-key", "User")
```

### レート制限エラーの場合

Gemini APIには使用制限があります：
- 無料プラン: 60 requests/minute
- 有料プラン: より高い制限

待機時間を設けるか、有料プランへのアップグレードを検討してください。

### SSL証明書エラーの場合

企業プロキシ環境では以下を試してください：

```powershell
# 証明書検証を無効化（開発環境のみ）
$env:PYTHONHTTPSVERIFY = "0"
```

---
作成日: 2025-08-09