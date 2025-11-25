using System.ComponentModel.DataAnnotations;

namespace UbiquitousLanguageManager.Contracts.DTOs;

/// <summary>
/// ユーザー一覧取得クエリDTO
/// Phase B-F3: ユーザー管理機能拡張
/// ユーザー検索・フィルタリング・ページング・ソート条件の入力データ構造
/// </summary>
public class UserListQueryDto
{
    /// <summary>
    /// 検索キーワード
    /// ユーザー名・メールアドレスの部分一致検索に使用
    /// </summary>
    public string SearchTerm { get; set; } = string.Empty;

    /// <summary>
    /// プロジェクトフィルタ
    /// 指定されたプロジェクトに所属するユーザーのみ取得
    /// null の場合はフィルタなし
    /// </summary>
    public long? ProjectId { get; set; }

    /// <summary>
    /// 論理削除ユーザー表示フラグ
    /// true: 無効化されたユーザーも含めて表示
    /// false: 有効なユーザーのみ表示（デフォルト）
    /// </summary>
    public bool ShowDeleted { get; set; } = false;

    /// <summary>
    /// ページ番号（1始まり）
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "ページ番号は1以上の値を指定してください")]
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// 1ページあたりの表示件数
    /// </summary>
    [Range(1, 100, ErrorMessage = "ページサイズは1から100の範囲で指定してください")]
    public int PageSize { get; set; } = 50;

    /// <summary>
    /// ソート対象カラム
    /// "Name", "Email", "Role", "CreatedAt", "UpdatedAt" のいずれか
    /// </summary>
    public string SortBy { get; set; } = "Name";

    /// <summary>
    /// ソート方向
    /// true: 降順 / false: 昇順（デフォルト）
    /// </summary>
    public bool SortDescending { get; set; } = false;

    /// <summary>
    /// ロールフィルタ
    /// 指定されたロールのユーザーのみ取得
    /// null の場合はフィルタなし
    /// </summary>
    public string? RoleFilter { get; set; }
}
