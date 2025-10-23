namespace UbiquitousLanguageManager.Web.Components.Common;

/// <summary>
/// ラジオボタンオプション
/// 【Blazor Server初学者向け解説】
/// CustomRadioGroupコンポーネントで使用するオプションデータクラス
/// Value, Label, Iconの3つのプロパティでラジオボタンを定義
/// </summary>
/// <typeparam name="T">値の型（bool、enum等）</typeparam>
public class RadioOption<T>
{
    /// <summary>
    /// ラジオボタンの値
    /// 【Blazor Server初学者向け解説】
    /// default!: Null非許容型のデフォルト値（初期化必須前提）
    /// </summary>
    public T Value { get; set; } = default!;

    /// <summary>
    /// 表示ラベル
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// アイコン（Font Awesome等のCSSクラス）
    /// 【Blazor Server初学者向け解説】
    /// 例: "fas fa-check-circle text-success"
    /// Font AwesomeクラスをHTML i要素のclass属性に設定してアイコン表示
    /// </summary>
    public string Icon { get; set; } = string.Empty;
}
