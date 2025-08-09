# Git for Windows インストール手順

## 1. インストーラーのダウンロード

1. 公式サイトにアクセス: https://git-scm.com/download/win
2. 「64-bit Git for Windows Setup」をクリックしてダウンロード

## 2. インストール

1. ダウンロードしたインストーラーを実行
2. 以下の設定でインストールを進める：

### インストールオプション
- **Select Components**: デフォルトのままでOK
- **Default editor**: お好みのエディタ（推奨: Use Visual Studio Code as Git's default editor）
- **PATH environment**: 「Git from the command line and also from 3rd-party software」を選択
- **HTTPS transport backend**: 「Use the OpenSSL library」を選択
- **Line ending conversions**: 「Checkout Windows-style, commit Unix-style line endings」を選択
- **Terminal emulator**: 「Use Windows' default console window」を選択
- **Default branch name**: 「Let Git decide」を選択
- **Git Pull behavior**: 「Default (fast-forward or merge)」を選択
- **Credential helper**: 「Git Credential Manager」を選択
- **Extra options**: デフォルトのままでOK

3. 「Install」をクリックしてインストール開始
4. インストール完了後、「Finish」をクリック

## 3. インストール確認

PowerShellを開いて以下のコマンドを実行：

```powershell
git --version
```

バージョン情報が表示されれば成功です。

## 4. 初期設定

PowerShellで以下のコマンドを実行（自分の情報に置き換えてください）：

```powershell
git config --global user.name "Your Name"
git config --global user.email "your.email@example.com"
```

## 5. 改行コード設定（Windows環境用）

```powershell
git config --global core.autocrlf true
```

---
作成日: 2025-08-09