# プロセスクリーンアップスクリプト
# VSCodeでビルドエラーが発生した際に実行

Write-Host "🧹 .NET プロセスをクリーンアップしています..." -ForegroundColor Yellow

# UbiquitousLanguageManager関連のプロセスを終了
Get-Process -Name "UbiquitousLanguageManager*" -ErrorAction SilentlyContinue | ForEach-Object {
    Write-Host "  終了: $($_.ProcessName) (PID: $($_.Id))" -ForegroundColor Red
    Stop-Process -Id $_.Id -Force
}

# dotnet関連のプロセスを確認（終了はしない）
$dotnetProcesses = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue
if ($dotnetProcesses) {
    Write-Host "`n⚠️  実行中のdotnetプロセス:" -ForegroundColor Yellow
    $dotnetProcesses | ForEach-Object {
        Write-Host "  - $($_.ProcessName) (PID: $($_.Id)) - メモリ: $([math]::Round($_.WorkingSet64 / 1MB, 2)) MB"
    }
}

# bin/objフォルダのロックを解除
Write-Host "`n📁 bin/objフォルダのクリーンアップ..." -ForegroundColor Yellow
$folders = @(
    "src\UbiquitousLanguageManager.Web\bin",
    "src\UbiquitousLanguageManager.Web\obj",
    "src\UbiquitousLanguageManager.Infrastructure\bin",
    "src\UbiquitousLanguageManager.Infrastructure\obj"
)

foreach ($folder in $folders) {
    if (Test-Path $folder) {
        try {
            Remove-Item -Path $folder -Recurse -Force -ErrorAction Stop
            Write-Host "  ✅ クリーンアップ完了: $folder" -ForegroundColor Green
        } catch {
            Write-Host "  ❌ クリーンアップ失敗: $folder - $_" -ForegroundColor Red
        }
    }
}

Write-Host "`n✨ クリーンアップ完了!" -ForegroundColor Green
Write-Host "VSCodeでF5キーを押してデバッグを再開してください。" -ForegroundColor Cyan