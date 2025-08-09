# Visual Studio Code インストール手順

## 1. ダウンロード

1. 公式サイトにアクセス: https://code.visualstudio.com/
2. 「Download for Windows」をクリック
3. 「VSCodeUserSetup-x64-{version}.exe」がダウンロードされる

## 2. インストール

1. ダウンロードしたインストーラーを実行
2. ライセンス契約に同意
3. インストール先の選択（デフォルトでOK）
4. スタートメニューフォルダの選択（デフォルトでOK）
5. 追加タスクの選択（以下を推奨）：
   - ☑ デスクトップ上にアイコンを作成する
   - ☑ エクスプローラーのファイル コンテキスト メニューに「Code で開く」アクションを追加する
   - ☑ エクスプローラーのディレクトリ コンテキスト メニューに「Code で開く」アクションを追加する
   - ☑ サポートされているファイルの種類のエディターとして、Code を登録する
   - ☑ PATH への追加（再起動後に使用可能）
6. 「インストール」をクリック
7. インストール完了後、「完了」をクリック

## 3. インストール確認

PowerShellを**新しく開いて**以下のコマンドを実行：

```powershell
code --version
```

バージョン情報が表示されれば成功です。

## 4. 日本語化（オプション）

1. VSCodeを起動
2. 左側のExtensionsアイコン（四角が4つ）をクリック
3. 検索ボックスに「Japanese」と入力
4. 「Japanese Language Pack for Visual Studio Code」を選択
5. 「Install」をクリック
6. インストール完了後、VSCodeを再起動

## 5. 基本設定

1. VSCodeを起動
2. `Ctrl + ,` で設定を開く
3. 以下の項目を確認・設定：

### エディター設定
- Editor: Tab Size → 4
- Editor: Insert Spaces → チェック
- Editor: Format On Save → チェック

### ファイル設定
- Files: Auto Save → afterDelay
- Files: Auto Save Delay → 1000

### ターミナル設定
- Terminal › Integrated › Default Profile: Windows → PowerShell

## 6. PowerShell実行ポリシーの設定

VSCode内でPowerShellスクリプトを実行できるようにする：

```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

確認プロンプトが表示されたら「Y」を入力。

---
作成日: 2025-08-09