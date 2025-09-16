/**
 * TECH-006 Headers read-onlyエラー解決: 認証API呼び出しヘルパー
 * 
 * このスクリプトは、Blazor ServerコンポーネントからAPI Controllerを
 * 安全に呼び出すためのヘルパー関数を提供します。
 * 
 * 【HTTPコンテキスト分離効果】
 * - 新しいHTTPリクエスト: SignalRとは独立したAPI呼び出し
 * - CSRF保護: Antiforgeryトークンの自動取得・設定
 * - Cookie設定: レスポンスヘッダーでのCookie操作が安全に実行可能
 */

/**
 * CSRFトークンを取得する
 * @returns {Promise<string>} CSRFトークン
 */
async function getCsrfToken() {
  try {
    const response = await fetch('/api/auth/csrf-token', {
      method: 'GET',
      credentials: 'same-origin'
    });

    if (!response.ok) {
      throw new Error('CSRFトークンの取得に失敗しました');
    }

    const data = await response.json();
    return data.token;
  } catch (error) {
    console.error('CSRF Token取得エラー:', error);
    throw error;
  }
}

/**
 * 認証APIへのHTTPリクエストを実行する（簡素化）
 * @param {string} url APIエンドポイントURL
 * @param {Object} data リクエストデータ
 * @returns {Promise<Object>} HTTPレスポンス情報とAPIデータ
 */
async function authApiRequest(url, data) {
  try {
    // CSRFトークンを取得
    const csrfToken = await getCsrfToken();

    const response = await fetch(url, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'X-CSRF-TOKEN': csrfToken
      },
      body: JSON.stringify(data),
      credentials: 'same-origin'  // Cookie送信を有効化
    });

    // APIは常にJSONを返す前提（AuthApiResponse型）
    const result = await response.json();

    return {
      ok: response.ok,
      status: response.status,
      data: result
    };
  } catch (error) {
    console.error('Auth API通信エラー:', error);
    throw new Error(`認証API通信に失敗しました: ${error.message}`);
  }
}

/**
 * ログインAPI呼び出し
 * @param {string} email メールアドレス
 * @param {string} password パスワード
 * @param {boolean} rememberMe ログイン状態保持フラグ
 * @returns {Promise<Object>} ログイン結果
 */
window.authApi = {
  /**
   * ログイン実行
   * 成功時は自動的にリダイレクトを実行
   */
  async login(email, password, rememberMe = false) {
    try {
      const response = await authApiRequest('/api/auth/login', {
        email: email,
        password: password,
        rememberMe: rememberMe
      });

      if (response.data.success && response.data.redirectUrl) {
        // ログイン成功時は自動リダイレクト
        console.log('ログイン成功 - リダイレクト中:', response.data.redirectUrl);
        window.location.href = response.data.redirectUrl;
      }

      return response.data;
    } catch (error) {
      return {
        success: false,
        message: error.message || 'ログイン処理でエラーが発生しました。'
      };
    }
  },

  /**
   * パスワード変更実行
   */
  async changePassword(currentPassword, newPassword, confirmPassword) {
    try {
      const response = await authApiRequest('/api/auth/change-password', {
        currentPassword: currentPassword,
        newPassword: newPassword,
        confirmPassword: confirmPassword
      });

      return response.data;
    } catch (error) {
      return {
        success: false,
        message: error.message || 'パスワード変更処理でエラーが発生しました。'
      };
    }
  },

  /**
   * ログアウト実行
   * Blazor側でリダイレクト制御するため、レスポンスのみ返す
   */
  async logout() {
    try {
      const response = await authApiRequest('/api/auth/logout', {});
      console.log('ログアウト処理完了:', response.data);
      return response.data;
    } catch (error) {
      return {
        success: false,
        message: error.message || 'ログアウト処理でエラーが発生しました。'
      };
    }
  },

  /**
   * ページリダイレクト（Blazor Server対応）
   */
  redirectTo(url) {
    if (url) {
      // Blazor Serverのナビゲーションマネージャーが存在する場合はそれを使用
      if (window.DotNet && window.DotNet.invokeMethod) {
        try {
          window.DotNet.invokeMethod('NavigateToUrl', url);
          return;
        } catch (e) {
          console.warn('Blazor navigation failed, using window.location:', e);
        }
      }

      // フォールバック: 通常のページ遷移
      window.location.href = url;
    }
  }
};

// デバッグ用ログ出力（開発環境のみ）
if (window.location.hostname === 'localhost') {
  console.log('🔐 TECH-006 認証API JavaScript ヘルパー が読み込まれました');
  console.log('使用可能な関数: authApi.login(), authApi.changePassword(), authApi.logout()');
}