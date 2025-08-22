using System;

namespace UbiquitousLanguageManager.Contracts.Exceptions
{
    /// <summary>
    /// ドメイン固有の例外クラス
    /// F# Domain層からのエラーをC# Infrastructure/Web層で統一処理するために使用
    /// </summary>
    public class DomainException : Exception
    {
        /// <summary>
        /// DomainExceptionを初期化
        /// </summary>
        /// <param name="message">エラーメッセージ</param>
        public DomainException(string message) : base(message)
        {
        }

        /// <summary>
        /// 内部例外を持つDomainExceptionを初期化
        /// </summary>
        /// <param name="message">エラーメッセージ</param>
        /// <param name="innerException">内部例外</param>
        public DomainException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}