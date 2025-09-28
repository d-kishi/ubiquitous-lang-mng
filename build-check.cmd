@echo off
echo ================================
echo Contracts層 ProjectDto・TypeConverter実装
echo ビルド確認（Phase B1 Step2）
echo ================================

echo.
echo [1] Contracts層ビルド確認...
dotnet build src\UbiquitousLanguageManager.Contracts --configuration Debug --verbosity minimal

echo.
echo [2] 全体ビルド確認...
dotnet build UbiquitousLanguageManager.sln --configuration Debug --verbosity minimal

echo.
echo [3] ビルド結果確認完了
echo ================================