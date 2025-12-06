# ホットリロード vs 再起動 判断基準

## 概要

`dotnet watch run` によるホットリロード機能の動作範囲と、再起動が必要なケースを定義する。

## ホットリロード対応（自動反映）

以下の変更は、ファイル保存時に自動的にブラウザに反映される：

### Razor ファイル (.razor)

- HTMLマークアップ変更
- Razor構文（@if, @foreach等）変更
- CSS class変更
- コンポーネントパラメータ変更

**例**:
```razor
<!-- この変更は即座に反映 -->
<div class="container">  →  <div class="container-fluid">
```

### CSS / SCSS

- スタイル変更
- クラス追加・削除
- レイアウト調整

### 静的ファイル (wwwroot/)

- JavaScript
- 画像
- フォント

## Rude Edit（自動再起動）

以下の変更は「Rude Edit」として検出され、`dotnet watch` が自動的にアプリを再起動する：

### C# コード変更

- メソッド追加・削除
- クラス構造変更
- プロパティ追加・削除
- using文追加

**例**:
```csharp
// この変更はRude Edit → 自動再起動
public string NewProperty { get; set; }
```

### Razor コードビハインド変更

- `@code { }` ブロック内のC#コード変更
- イベントハンドラ追加・変更

**挙動**:
1. 変更検出
2. `Rude edit detected, restarting...` メッセージ表示
3. アプリ自動再起動（10-20秒）
4. 再起動完了後、ブラウザリフレッシュで反映

## 手動再起動必須

以下の変更は `restart` コマンドでの手動再起動が必要：

### F# コード変更

- Domain層 (.fs)
- Application層 (.fs)

**理由**: F#ファイルは `dotnet watch` の監視対象外

**対処**:
```bash
docker exec ubiquitous-lang-mng_devcontainer-devcontainer-1 bash .devcontainer/scripts/web-app.sh restart
```

### Program.cs / DI設定

- サービス登録変更
- ミドルウェア設定変更
- 認証設定変更

### 設定ファイル

- appsettings.json
- appsettings.Development.json

### プロジェクト構成

- .csproj / .fsproj 変更
- NuGetパッケージ追加・削除

## 判断フローチャート

```
変更したファイルは？
│
├─ .razor（HTMLのみ）→ ホットリロード（待機不要）
├─ .razor（@code変更）→ Rude Edit（自動再起動待機）
├─ .css / wwwroot/* → ホットリロード（待機不要）
├─ .cs（C#コード）→ Rude Edit（自動再起動待機）
├─ .fs（F#コード）→ 手動 restart 必要
├─ Program.cs → 手動 restart 必要
├─ appsettings.json → 手動 restart 必要
└─ .csproj / .fsproj → 手動 restart 必要
```

## 実践的なガイドライン

### UI調整作業時

1. Razor/CSSのみ変更 → そのまま作業継続（ホットリロード）
2. 変更が反映されない場合 → ブラウザ強制リフレッシュ（Ctrl+Shift+R）

### 機能実装作業時

1. C#コード変更 → Rude Edit待機（10-20秒）
2. F#コード変更 → `restart` コマンド実行
3. 複数ファイル変更 → まとめて変更後に `restart`

### トラブル時

1. 何も反映されない → `status` で起動確認
2. 古い状態が表示 → `restart` で完全再起動
3. エラー発生 → ログ確認（`/tmp/web-app.log`）

---

**作成日**: 2025-12-06
